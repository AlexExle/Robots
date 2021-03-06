﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;
using ArmoredIntradaySpace.Binders;
using ArmoredIntraDay.Binders.ExitBindings;


namespace ArmoredIntradaySpace
{
    public class ArmoredIntraday : WealthScript
    {
        #region Типы

        public  enum EntryType
        {
            Impulse,
            TrendImpulse,
            TrendPullback,
            TrendPullbackImpulse,
            StaticImpulse,
            ExpImpulse,
            ArmoredPullback
        }

        public enum ExitType
        {
            /*TrailingStop,
            StopAndReverse,
            StopAndProfit,
            Time,*/
            StaticProfit,
            AtrProfit,
            FractalProfit
        }

        #endregion

        #region Константы

        /* const int TrendPeriod = 100; // Кол-во баров, по которым определяем тренд
        const int PullbackPeriod = 10; // Кол-во баров, по которым определяем откат
        const double ProfitRisk = 3; // Отношение доходности к риску в технике сопровождения S/P*/
        const int WaitPeriod = 4; // Ожидание в сделке в технике сопровождения Time

        #endregion

        #region Fields

        int maxPositionCount = 4;
        StrategyParameter _isTrend; // По тренду (1) или флэту (0)
        StrategyParameter _entryType; // Вход по импульсу (0), тренду и импульсу (1), тренду и откату (2), тренду, откату и импульсу (3)
        public StrategyParameter _enterParameter;
        public StrategyParameter _exitParameter;
        StrategyParameter _exitType; // Выход по T/S (0), SAR (1), S/P (2), Time (3)
        public int isLong = -1;
        public double CentralSrikePoint = 0; 
        #endregion

        #region Инстансы стартегий

        AEnterStrategy EnterStrategy;
        AExitStrategy ExitStrategy;

        #endregion

        public ArmoredIntraday()
        {
            _isTrend = CreateParameter("Trend", 1, 0, 1, 1);
            _entryType = CreateParameter("Entry Type", 1, 0, 6, 1);
            _enterParameter = CreateParameter("Enter Param", 0.1, 0.1, 1, 0.1);
            _exitParameter = CreateParameter("Exit Param", 0.1, 0.1, 1, 0.1);
             _exitType = CreateParameter("Exit Type", 1, 0, 2, 1);
        }

        protected override void Execute()
        {
            double lastPrice = 0;
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            ClearDebug(); // Очистить окно отладки
            HideVolume(); // Скрыть объемы    
            
            DataSeries strikePoint = new DataSeries(Bars.Close, "Strike");

            #region Переменные для обслуживания позиции
                     
            bool isSignalBuy = false, isSignalShort = false; // Сигналы на вход в длинную и короткую позиции
            EnterSignalType lastSignal = EnterSignalType.None;

            bool isTrend = _isTrend.ValueInt == 1;

            //создаем инстансы стратегий через наш не до паттерн
            EnterStrategy = AEnterStrategy.CreateInstance((EntryType)_entryType.ValueInt, this);
            ExitStrategy =  AExitStrategy.CreateInstance((ExitType)_exitType.ValueInt, this, isTrend);
            CentralSrikePoint = Open[AEnterStrategy.firstValidValue];
            #endregion

            for (int bar = AEnterStrategy.firstValidValue; bar < Bars.Count -1; bar++) // Пробегаемся по всем барам
            {
                if (this.isOptionExperatinDay(bar))
                {
                    CentralSrikePoint = Open[bar];
                }

                if (CentralSrikePoint == 0)
                {
                    strikePoint[bar] = Open[bar];
                    CentralSrikePoint = Open[bar];
                    continue;
                }
                else
                {
                    isLong = CentralSrikePoint >= Open[bar] ? 1 : 0;
                    strikePoint[bar] = CentralSrikePoint;
                   
                    foreach (var pos in ActivePositions.ToArray())
                    {
                        ExitAtLimit(bar , pos, CentralSrikePoint, "Exit at cross strikes");
                    }
                    
                }

                if (strikePoint[bar] != strikePoint[bar - 1])
                {
                    foreach (var pos in ActivePositions.ToArray())
                    {
                        ExitAtMarket(bar, pos, "End of Month");
                    }
                }

                #region Сигналы на вход в позицию и выход из нее

                //!! Стратегия генерирует сигнал
                lastSignal = EnterStrategy.GenerateSignal(bar, out lastPrice);

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

                if (ActivePositions.Count < maxPositionCount) // Если позиции нет
                {
                    Position newPoisiton = null;
                    if (isSignalBuy && isLong != 0) // Если пришел сигнал на покупку и работаем с длинными позициями
                    {
                        if( lastPrice <= 0)
                            newPoisiton = BuyAtMarket(bar + 1, (_isTrend.ValueInt == 1 ? "Tend" : "Flat") + " Long_" + (ActivePositions.Count + 1));
                        else
                            newPoisiton = BuyAtLimit(bar + 1, lastPrice, (_isTrend.ValueInt == 1 ? "Tend" : "Flat") + " Long_" + (ActivePositions.Count + 1));
                    }
                    else if (isSignalShort && isLong != 1) // Если пришел сигнал на короткую продажу и работаем с короткими позициями
                    {
                        if (lastPrice <= 0)
                            newPoisiton = ShortAtMarket(bar + 1, (_isTrend.ValueInt == 1 ? "Tend" : "Flat") + " Short_" + (ActivePositions.Count + 1));
                        else
                            newPoisiton = ShortAtLimit(bar + 1, lastPrice, (_isTrend.ValueInt == 1 ? "Tend" : "Flat") + " Short_" + (ActivePositions.Count + 1));
                    }

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
            return Date[bar-1].Date.Day < 10 && Date[bar].Date.Day >= 10;
        }
    }
}



