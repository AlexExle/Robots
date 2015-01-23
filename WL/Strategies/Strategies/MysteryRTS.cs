//RI - 5min

using System;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;
using Community.Indicators;

namespace Droid.WealthLab.Strategies
{


    public class MysteryRTS : WealthScript
    {
        #region Константы

        double deltaSupport = 1; //множитель для верхней границы
        double deltaResist = 1; //множитель для нижней границы
        const int КоличествоСделокВДень = 2;

        #endregion

        #region Объявление параметров торговой системы

        //StrategyParameter _количествоСделокВДень; // Количество сделок в день
        StrategyParameter _процентСтопа; // Процент Стопа
        //StrategyParameter _процентЛимитаОтСтопа; // Процент MA Envelope на Long
        StrategyParameter _deltaSupport;
        StrategyParameter _deltaResist;

        #endregion

        public MysteryRTS()
        {
            #region Инициализация параметров торговой системы

            //_количествоСделокВДень = CreateParameter("Количество Сделок ВД ень", 5, 1, 10, 1);
            _процентСтопа = CreateParameter("Процент Стопа", 25, 10, 60, 5);
            //_процентЛимитаОтСтопа = CreateParameter("Процент Лимита От Стопа", 6, 1, 20, 1);
            _deltaSupport = CreateParameter("delta supp", 1, 0.5, 5, 0.1);
            _deltaResist = CreateParameter("delta res", 1, 0.5, 5, 0.1);

            #endregion
        }

        protected override void Execute()
        {

            int firstValidValue = 500; // Первое значение свечки, при котором существуют все индикаторы

            ClearDebug(); // Очистить окно отладки
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            HideVolume(); // Скрыть объемы 
           
            #region Представление цен с необходимой разрядностью

            string pricePattern = "0.";

            for (int i = 0; i < this.Bars.SymbolInfo.Decimals; i++)
                pricePattern += "0";

            #endregion

            #region Индикаторы

            DataSeries Support = Close - Close; //уровень поддержки
            DataSeries Resist = Close - Close; //уровень сопративления
            DataSeries Stop = Close - Close;

            PlotSeries(PricePane, Support, Color.Green, LineStyle.Solid, 2);
            PlotSeries(PricePane, Resist, Color.Yellow, LineStyle.Solid, 2);
            PlotSeries(PricePane, Stop, Color.Red, LineStyle.Solid, 1);

            #endregion

            #region Переменные для обслуживания позиции

            bool signalBuyTrend = false, signalShortTrend = false; // Сигналы на вход в длинную и короткую позиции 
            double entryPrice = 0; // Лимитная цена входа
            bool деньПокупок = false;
            bool деньПродаж = false;
            int КоличествоСделокСегодня = 0;
            double LastDayRange = 0;
            double supportResistRange = 0;
            double ОткрытиеСегоднешнегоДня = 0;
            Bar ВчерашнийДень = new Bar();
            Bar ПозавчерашнийДень = new Bar();
            bool crossResist = false;
            bool crossSupport = false;
            Position перваяПозиция = null;
            Position втораяПозиция = null;
            double TP = 0;
            double STOP = 0;
            double процентСтопа = _процентСтопа.Value;

            

            deltaSupport = _deltaSupport.Value;
            deltaResist = _deltaResist.Value;

            #endregion

            #region Основной цикл
            for (int bar = firstValidValue; bar < Bars.Count; bar++) // Пробегаемся по всем свечкам (кроме последней)
            {
                Stop[bar] = STOP;

                //double lim = Bars.Close[bar];
                //середина текущей свечи
                double barMiddle = (Bars.High[bar] + Bars.Low[bar]) / 2;


                if (Bars.Date[bar].DayOfYear != Bars.Date[bar - 1].DayOfYear) //первая свечка нового дня
                {
                    ОткрытиеСегоднешнегоДня = Bars.Open[bar];
                    КоличествоСделокСегодня = 0;
                    ВчерашнийДень = ИщемДеньНазад(bar, 1);
                    ПозавчерашнийДень = ИщемДеньНазад(bar, 2);
                    //канал вчерашнего дня
                    LastDayRange = ВчерашнийДень.H - ВчерашнийДень.L;
                    crossResist = false;
                    crossSupport = false;
                }

                Support[bar] = ОткрытиеСегоднешнегоДня - LastDayRange / 2.0 * deltaSupport;
                Resist[bar] = ОткрытиеСегоднешнегоДня + LastDayRange / 2.0 * deltaResist;
                supportResistRange = Resist[bar] - Support[bar];

                crossSupport = false;
                crossResist = false;
                if (Bars.Close[bar] < Support[bar]) crossSupport = true;
                if (Bars.Close[bar] > Resist[bar]) crossResist = true;


                #region Фильтр

                деньПокупок = false;
                деньПродаж = false;
                if (Support[bar] > Support[ВчерашнийДень.StartBarDay])
                {
                    SetBarColor(bar, Color.GreenYellow);
                    деньПокупок = true;
                }
                if (Resist[bar] < Resist[ВчерашнийДень.StartBarDay])
                {
                    SetBarColor(bar, Color.Red);
                    деньПродаж = true;
                }
                if ((деньПокупок && деньПродаж) || (!деньПокупок && !деньПродаж))
                {
                    деньПокупок = false;
                    деньПродаж = false;
                    SetBarColor(bar, Color.Black);
                }

                #endregion

                #region Сигналы на вход в позицию и выход из нее

                signalBuyTrend = crossResist &&
                                 //торгуем с 11 до 20
                                 (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute) >= 1100 &&
                                 (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute) <= 2000 &&
                //signalBuyTrend &= деньПокупок &&
                                 КоличествоСделокСегодня < КоличествоСделокВДень;

                signalShortTrend = crossSupport &&
                                   //торгуем с 11 до 20
                                   (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute) >= 1100 &&
                                   (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute) <= 2000 &&
                //signalShortTrend &= деньПродаж &&
                                   КоличествоСделокСегодня < КоличествоСделокВДень;

                //PrintDebug(КоличествоСделокСегодня.ToString());

                #endregion

                #region Сопровождение и выход из позиции

                #region Первая Позиция
                if (перваяПозиция == null) // Если позиции нет
                {
                    if (signalBuyTrend) // При получении сигнала на вход в длинную позицию
                    {
                        RiskStopLevel = barMiddle - supportResistRange * процентСтопа / 100.0;
                        if ((перваяПозиция = BuyAtLimit(bar + 1, barMiddle, "1")) != null)
                        {

                        }
                        if ((втораяПозиция = BuyAtLimit(bar + 1, barMiddle, "2")) != null)
                        {
                            TP = втораяПозиция.EntryPrice + supportResistRange * 50.0 / 100.0;
                            STOP = втораяПозиция.EntryPrice - supportResistRange * процентСтопа / 100.0;
                            КоличествоСделокСегодня++;
                        }

                    }
                    else if (signalShortTrend) // При получении сигнала на вход в короткую позицию
                    {
                        RiskStopLevel = barMiddle + supportResistRange * процентСтопа / 100.0;
                        if ((перваяПозиция = ShortAtLimit(bar + 1, barMiddle, "1")) != null)
                        {

                        }
                        if ((втораяПозиция = ShortAtLimit(bar + 1, barMiddle, "2")) != null)
                        {
                            TP = втораяПозиция.EntryPrice - supportResistRange * 50.0 / 100.0;
                            STOP = втораяПозиция.EntryPrice + supportResistRange * процентСтопа / 100.0;
                            КоличествоСделокСегодня++;
                        }
                    }
                }
                else // Если позиция есть
                {
                    if (перваяПозиция.PositionType == PositionType.Long)
                    {
                        if (Bars.Low[bar] < STOP)
                        {
                            if (SellAtLimit(bar + 1, перваяПозиция, barMiddle, "1"))
                                перваяПозиция = null;
                        }
                        else
                            if (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute >= 2330)
                            {
                                if (SellAtMarket(bar + 1, перваяПозиция, "1"))
                                {
                                    перваяПозиция = null;
                                }
                            }
                    }
                    else
                    {
                        if (Bars.High[bar] > STOP)
                        {
                            if (CoverAtLimit(bar + 1, перваяПозиция, barMiddle, "1"))
                                перваяПозиция = null;
                        }
                        else
                            if (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute >= 2330)
                            {
                                if (CoverAtMarket(bar + 1, перваяПозиция, "1"))
                                {
                                    перваяПозиция = null;
                                }
                            }
                    }
                }
                #endregion
                #region Вторая Позиция
                if (втораяПозиция == null) // Если позиции нет
                {

                }
                else // Если позиция есть
                {
                    if (втораяПозиция.PositionType == PositionType.Long)
                    {
                        if (Bars.Low[bar] < STOP)
                        {
                            if (SellAtLimit(bar + 1, втораяПозиция, barMiddle, "2"))
                                втораяПозиция = null;
                        }
                        else
                            if (SellAtLimit(bar + 1, втораяПозиция, TP, "2"))
                            {
                                втораяПозиция = null;
                                if (перваяПозиция != null) STOP = перваяПозиция.EntryPrice;
                            }
                            else
                                if (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute >= 2330)
                                {
                                    if (SellAtMarket(bar + 1, втораяПозиция, "2"))
                                    {
                                        втораяПозиция = null;
                                    }
                                }
                    }
                    else
                    {
                        if (Bars.High[bar] > STOP)
                        {
                            if (CoverAtLimit(bar + 1, втораяПозиция, barMiddle, "2"))
                                перваяПозиция = null;
                        }
                        else
                            if (CoverAtLimit(bar + 1, втораяПозиция, TP, "2"))
                            {
                                втораяПозиция = null;
                                if (перваяПозиция != null) STOP = перваяПозиция.EntryPrice;
                            }
                            else
                                if (Bars.Date[bar].Hour * 100 + Bars.Date[bar].Minute >= 2330)
                                {
                                    if (CoverAtMarket(bar + 1, втораяПозиция, "2"))
                                    {
                                        втораяПозиция = null;
                                    }
                                }
                    }
                }
                #endregion

                #endregion

            }
            #endregion
        }

        #region Ищем День

        Bar ИщемДеньНазад(int Pos, int nDay)
        {
            Bar Day = new Bar();
            int КонецПредДня = 0;

            int i = Pos;
            while (Bars.Date[i].DayOfYear == Bars.Date[i - 1].DayOfYear)
            {
                i--;
            }
            i--;
            КонецПредДня = i;
            Day.EndBarDay = i;
            for (int n = 0; n < nDay; n++)
            {
                Day.H = Bars.High[i];
                Day.L = Bars.Low[i];
                Day.C = Bars.Close[i];
                while (Bars.Date[i].DayOfYear == Bars.Date[i - 1].DayOfYear)
                {
                    if (Bars.High[i] > Day.H) Day.H = Bars.High[i];
                    if (Bars.Low[i] < Day.L) Day.L = Bars.Low[i];
                    i--;
                }
                Day.O = Bars.Open[i];
                Day.StartBarDay = i;
                i--;
            }

            return Day;
        }

        #endregion

        #region Class Bar

        /// <summary>
        /// Class Bar (OHLC)
        /// </summary>
        class Bar
        {
            public double O = 0;
            public double H = 0;
            public double L = 0;
            public double C = 0;
            public int StartBarDay = 0;
            public int EndBarDay = 0;

            public Bar()
            {
            }

            /// <summary>
            /// Bar
            /// </summary>
            /// <param name="O"> Open </param>
            /// <param name="H"> High </param>
            /// <param name="L"> Low </param>
            /// <param name="C"> Close </param>
            public Bar(double O, double H, double L, double C)
            {
                this.O = O;
                this.H = H;
                this.L = L;
                this.C = C;
            }
        }

        #endregion

    }
}


