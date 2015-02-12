using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;
using ArmorediIntraday.Binders;
using ArmoredIntraDay.Binders.ExitBindings;


namespace ArmorediIntraday
{
    public class ArmoredIntraday : WealthScript
    {
        #region Типы

        public  enum EntryType
        {
            Impulse,
            TrendImpulse,
            TrendPullback,
            TrendPullbackImpulse
        }

        public enum ExitType
        {
            TrailingStop,
            StopAndReverse,
            StopAndProfit,
            Time
        }

        #endregion

        #region Константы

        /* const int TrendPeriod = 100; // Кол-во баров, по которым определяем тренд
        const int PullbackPeriod = 10; // Кол-во баров, по которым определяем откат
        const double ProfitRisk = 3; // Отношение доходности к риску в технике сопровождения S/P*/
        const int WaitPeriod = 4; // Ожидание в сделке в технике сопровождения Time

        #endregion

        #region Объявление параметров торговой системы

        int maxPositionCount = 4;
        StrategyParameter _isTrend; // По тренду (1) или флэту (0)
        StrategyParameter _entryType; // Вход по импульсу (0), тренду и импульсу (1), тренду и откату (2), тренду, откату и импульсу (3)
        //StrategyParameter _exitType; // Выход по T/S (0), SAR (1), S/P (2), Time (3)

        #endregion

        #region Инстансы стартегий

        AEnterStrategy EnterStrategy;
        AExitStrategy ExitStrategy;

        #endregion

        public ArmoredIntraday()
        {
            #region Инициализация параметров торговой системы

            _isTrend = CreateParameter("Trend", 1, 0, 1, 1);
            _entryType = CreateParameter("Entry Type", 1, 0, 3, 1);
           // _exitType = CreateParameter("Exit Type", 2, 0, 3, 1);

            #endregion
        }

        protected override void Execute()
        {
            double CentralSrikePoint = 0; 
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            ClearDebug(); // Очистить окно отладки
            HideVolume(); // Скрыть объемы    

            DataSeries strikePoint = new DataSeries(Bars.Close, "Strike");

            #region Переменные для обслуживания позиции
                     
            bool isSignalBuy = false, isSignalShort = false; // Сигналы на вход в длинную и короткую позиции
            EnterSignalType lastSignal = EnterSignalType.None;
            int isLong = -1;
            bool isTrend = _isTrend.ValueInt == 1; 

            ExitType exitType = ExitType.StopAndProfit; //(ExitType)_exitType.ValueInt;

            //создаем инстансы стратегий через наш не до паттерн
            EnterStrategy = AEnterStrategy.CreateInstance((EntryType)_entryType.ValueInt, this);
            ExitStrategy = new OnlyProfit(this, isTrend);// AExitStrategy.CreateInstance((ExitType)_exitType.ValueInt, this, isTrend);

            #endregion

            for (int bar = AEnterStrategy.firstValidValue; bar < Bars.Count; bar++) // Пробегаемся по всем барам
            {
                if (this.isOptionExperatinDay(bar))
                {
                    CentralSrikePoint = Open[bar];
                }

                if (CentralSrikePoint == 0)
                {
                    strikePoint[bar] = Open[bar];
                    continue;
                }
                else
                {
                    isLong = CentralSrikePoint >= Open[bar] ? 1 : 0;
                    strikePoint[bar] = CentralSrikePoint;
                }

                if (strikePoint[bar] != strikePoint[bar - 1])
                {
                    foreach (var pos in ActivePositions.ToArray())
                    {
                        ExitAtMarket(bar, pos);
                    }
                }

                #region Сигналы на вход в позицию и выход из нее

                //!! Стратегия генерирует сигнал
                lastSignal = EnterStrategy.GenerateSignal(bar);

                //многозначная логика - это вам не хухры-мухры
                if (lastSignal == EnterSignalType.None)
                {
                    isSignalBuy = false;
                    isSignalShort = false;
                }
                else
                {
                    isSignalBuy = (lastSignal == EnterSignalType.Up) && isTrend || !(lastSignal == EnterSignalType.Up) && !isTrend;
                    isSignalShort = (lastSignal == EnterSignalType.Down) && isTrend || !(lastSignal == EnterSignalType.Down) && !isTrend;
                }
             
                #endregion

                #region Сопровождение и выход из позиции


                // !! Поменял местами условие на выход и на вход и обернул в отедльные ифы, чтобы правильно работал SAR - Сначала вышли, потом на той же свечке вошли
                if (IsLastPositionActive) // Если позиция есть
                {
                    foreach (Position pos in ActivePositions.ToArray())
                    {
                        //Тут уже начинают рулить стратегии на Выход
                        ExitStrategy.RecalculateExitConditions(pos, bar);

                        //функционал выхода внедрил в сами стратегии, так как они зависимы от типа.
                        //логично было бы и функционал входа затащить в стратегию входа, но пока он независим. Как говорится, первый принцип программиста как и врача "не навреди" или "нелезь, если само работает" =)
                        //наверное следовало бы объеденить эту функцию и предыдущую в одну.
                        ExitStrategy.TryExit(pos, bar, lastSignal);
                    }
                }

                #endregion

                #region Вход в позицию

                if (ActivePositions.Count <= maxPositionCount) // Если позиции нет
                {
                    Position newPoisiton = null;
                    if (isSignalBuy && isLong != 0) // Если пришел сигнал на покупку и работаем с длинными позициями
                       newPoisiton = BuyAtMarket(bar + 1, _isTrend.ToString() + " Buy");
                    else if (isSignalShort && isLong != 1) // Если пришел сигнал на короткую продажу и работаем с короткими позициями
                       newPoisiton = ShortAtMarket(bar + 1, _isTrend.ToString() + " Short");

                    if (newPoisiton == null) // Если не вошли в позицию
                        continue; // то идем проверять условия входа на следующем баре

                    #region Постановка начальных T/S, S/L, T/P

                    ExitStrategy.InitExitConditions(newPoisiton, bar);              

                    #endregion
                }

                #endregion                

            }
            this.PlotSeries(PricePane, strikePoint, Color.Gold, LineStyle.Solid, 1);
        }

        protected bool isOptionExperatinDay(int bar)
        {
            return Date[bar-1].Date.Day < 15 && Date[bar].Date.Day >= 15;
        }
    }
}



