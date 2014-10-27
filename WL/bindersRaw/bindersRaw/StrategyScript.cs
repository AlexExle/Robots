using System;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;

namespace bindersRaw
{
    /// <summary>
    /// 64 Шаблоноа-обвязки (версия с курса)
    /// </summary>
    class CourseBinders64 : WealthScript
    {
        #region Типы

        /// <summary>
        /// Тип входа
        /// </summary>
        enum EntryType
        {
            /// <summary>
            /// По импульсу
            /// </summary>
            Impulse,
            /// <summary>
            /// По импульсу и тренду
            /// </summary>
            TrendImpulse,
            /// <summary>
            /// По тренду и откату
            /// </summary>
            TrendPullback,
            /// <summary>
            /// По тренду, откату и импульсу
            /// </summary>
            TrendPullbackImpulse
        }

        /// <summary>
        /// Тип выхода
        /// </summary>
        enum ExitType
        {
            /// <summary>
            /// Подтягиваемый стоп T/S (цена и время)
            /// </summary>
            TrailingStop,
            /// <summary>
            /// Переворот SAR (без цены и времени)
            /// </summary>
            StopAndReverse,
            /// <summary>
            /// Риск и доходность S/P (цена)
            /// </summary>
            StopAndProfit,
            /// <summary>
            /// Временной выход Time (время)
            /// </summary>
            Time
        }

        #endregion

        #region Константы

        const int TrendPeriod = 100; // Кол-во баров, по которым определяем тренд
        const int PullbackPeriod = 10; // Кол-во баров, по которым определяем откат
        const double ProfitRisk = 3; // Отношение доходности к риску в технике сопровождения S/P
        const int WaitPeriod = 4; // Ожидание в сделке в технике сопровождения Time

        #endregion

        #region Объявление параметров торговой системы

        StrategyParameter _isLong; // Длинная (1) или короткая (0) позиция
        StrategyParameter _isTrend; // По тренду (1) или флэту (0)
        StrategyParameter _entryType; // Вход по импульсу (0), тренду и импульсу (1), тренду и откату (2), тренду, откату и импульсу (3)
        StrategyParameter _exitType; // Выход по T/S (0), SAR (1), S/P (2), Time (3)

        #endregion

        public CourseBinders64()
        {
            #region Инициализация параметров торговой системы

            _isLong = CreateParameter("Long", 1, 0, 1, 1);
            _isTrend = CreateParameter("Trend", 1, 0, 1, 1);
            _entryType = CreateParameter("Entry Type", 1, 0, 3, 1);
            _exitType = CreateParameter("Exit Type", 2, 0, 3, 1);

            #endregion
        }

        protected override void Execute()
        {
            int firstValidValue = 0; // Первое значение свечки, при котором существуют все индикаторы

            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            ClearDebug(); // Очистить окно отладки
            HideVolume(); // Скрыть объемы    

            #region Индикаторы

            // Тренд
            DataSeries trendMA = SMA.Series(Bars.Close, TrendPeriod);
            firstValidValue = Math.Max(firstValidValue, trendMA.FirstValidValue); // Первое значение, на котором существует SMA

            // Откат
            DataSeries pullbackMA = SMA.Series(Bars.Close, PullbackPeriod);
            firstValidValue = Math.Max(firstValidValue, pullbackMA.FirstValidValue); // Первое значение, на котором существует SMA

            // Волатильность
            DataSeries atr = ATR.Series(Bars, PullbackPeriod);
            ChartPane atrPane;
            firstValidValue = Math.Max(firstValidValue, atr.FirstValidValue); // Первое значение, на котором существует ATR

            #endregion

            #region Переменные для обслуживания позиции

            bool isLong = _isLong.ValueInt == 1;
            bool isTrend = _isTrend.ValueInt == 1;
            EntryType entryType = EntryType.Impulse;
            switch (_entryType.ValueInt)
            {
                case 1:
                    entryType = EntryType.TrendImpulse;
                    PlotSeries(PricePane, trendMA, Color.Red, LineStyle.Solid, 1);
                    break;
                case 2:
                    entryType = EntryType.TrendPullback;
                    PlotSeries(PricePane, trendMA, Color.Red, LineStyle.Solid, 1);
                    PlotSeries(PricePane, pullbackMA, Color.Blue, LineStyle.Solid, 1);
                    break;
                case 3:
                    entryType = EntryType.TrendPullbackImpulse;
                    PlotSeries(PricePane, trendMA, Color.Red, LineStyle.Solid, 1);
                    PlotSeries(PricePane, pullbackMA, Color.Blue, LineStyle.Solid, 1);
                    break;
                default:
                    break;
            }
            ExitType exitType = ExitType.StopAndReverse; // SAR
            switch (_exitType.ValueInt)
            {
                case 0: // T/S
                    exitType = ExitType.TrailingStop;
                    atrPane = CreatePane(20, false, true);
                    PlotSeries(atrPane, atr, Color.Red, LineStyle.Solid, 1);
                    break;
                case 2: // S/P
                    exitType = ExitType.StopAndProfit;
                    atrPane = CreatePane(20, false, true);
                    PlotSeries(atrPane, atr, Color.Red, LineStyle.Solid, 1);
                    break;
                case 3: // Time
                    exitType = ExitType.Time;
                    break;
                default:
                    break;
            }

            bool isImpulseUp = false, isImpulseDown = false; // Импульс
            bool isTrendUp = false, isTrendDown = false; // Тренд
            bool isPullbackUp = false, isPullbackDown = false; // Откат
            bool isSignalBuy = false, isSignalShort = false; // Сигналы на вход в длинную и короткую позиции
            bool swapSignals = false; // Вспомогательная переменная для реверса сигналов во флэте

            #endregion

            for (int bar = firstValidValue; bar < Bars.Count; bar++) // Пробегаемся по всем барам
            {
                #region Сигналы на вход в позицию и выход из нее

                // Ударный день
                isImpulseUp = Bars.Close[bar] > Bars.High[bar - 1]; // Закрытие бара выше максимума предыдущего
                isImpulseDown = Bars.Close[bar] < Bars.Low[bar - 1]; // Закрытие бара ниже минимума предыдущего

                // Положение цены и медленной скользящей
                isTrendUp = Bars.Close[bar] > trendMA[bar]; // Закрытие бара выше медленной скользящей
                isTrendDown = Bars.Close[bar] < trendMA[bar]; // Закрытие бара ниже медленной скользящей

                // Положение цены и быстрой скользящей
                isPullbackUp = Bars.Close[bar] > pullbackMA[bar]; // Закрытие бара выше быстрой скользящей
                isPullbackDown = Bars.Close[bar] < pullbackMA[bar]; // Закрытие бара ниже быстрой скользящей

                switch (entryType) // Сигналы по тренду
                {
                    case EntryType.Impulse:
                        isSignalBuy = isImpulseUp;
                        isSignalShort = isImpulseDown;
                        break;
                    case EntryType.TrendImpulse:
                        isSignalBuy = isTrendUp && isImpulseUp;
                        isSignalShort = isTrendDown && isImpulseDown;
                        break;
                    case EntryType.TrendPullback:
                        isSignalBuy = isTrendUp && isPullbackDown;
                        isSignalShort = isTrendDown && isPullbackUp;
                        break;
                    case EntryType.TrendPullbackImpulse:
                        isSignalBuy = isTrendUp && isPullbackDown && isImpulseUp;
                        isSignalShort = isTrendDown && isPullbackUp && isImpulseDown;
                        break;
                    default:
                        break;
                }

                if (!isTrend) // Реверс сигналов, если торгуем во флэте
                {
                    swapSignals = isSignalBuy;
                    isSignalBuy = isSignalShort;
                    isSignalShort = swapSignals;
                }

                #endregion

                #region Сопровождение и выход из позиции

                if (IsLastPositionActive) // Если позиция есть
                {
                    switch (exitType)
                    {
                        case ExitType.TrailingStop:
                            if (isTrend) // Если торгуем по тренду, то двигаем T/S в направлении позиции
                            {
                                if (LastActivePosition.PositionType == PositionType.Long)
                                    LastActivePosition.RiskStopLevel = Math.Max(LastActivePosition.RiskStopLevel, Bars.Close[bar] - atr[bar]); // T/S только повышаем 
                                else
                                    LastActivePosition.RiskStopLevel = Math.Min(LastActivePosition.RiskStopLevel, Bars.Close[bar] + atr[bar]); // T/S только понижаем

                                ExitAtStop(bar + 1, LastActivePosition, LastActivePosition.RiskStopLevel, "T/S"); // то пытаемся выйти по T/S
                            }
                            else // Если торгуем по флэту, то двигаем обратный T/S против направления позиции
                            {
                                if (LastActivePosition.PositionType == PositionType.Long)
                                    LastActivePosition.AutoProfitLevel = Math.Min(LastActivePosition.AutoProfitLevel, Bars.Close[bar] + atr[bar] * ProfitRisk); // T/S только понижаем
                                else
                                    LastActivePosition.AutoProfitLevel = Math.Max(LastActivePosition.AutoProfitLevel, Bars.Close[bar] - atr[bar] * ProfitRisk); // T/S только понижаем

                                ExitAtLimit(bar + 1, LastActivePosition, LastActivePosition.AutoProfitLevel, "Rev T/S"); // то пытаемся выйти по обратному Trailing Stop
                            }
                            break;
                        case ExitType.StopAndReverse:
                            if (LastActivePosition.PositionType == PositionType.Long && isSignalShort || // Если находимся в длинной позиции, и пришел сигнал на вход в короткую позицию
                                LastActivePosition.PositionType == PositionType.Short && isSignalBuy) // или находимся в короткой позиции, и пришел сигнал на вход в длинную позицию
                                ExitAtMarket(bar + 1, LastActivePosition, "SAR"); // то выйти из позиции по рынку
                            break;
                        case ExitType.StopAndProfit:
                            if (!ExitAtStop(bar + 1, LastActivePosition, LastActivePosition.RiskStopLevel, "S/L")) // Попробовать выйти по S/L
                                ExitAtLimit(bar + 1, LastActivePosition, LastActivePosition.AutoProfitLevel, "T/P"); // если не вышли, то попробовать выйти по T/P
                            break;
                        case ExitType.Time:
                            if (bar - LastActivePosition.EntryBar == WaitPeriod) // Если время вышло
                                ExitAtMarket(bar + 1, LastActivePosition, "Exit Time"); // то выйти из позиции по рынку
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Вход в позицию

                if (!IsLastPositionActive) // Если позиции нет
                {
                    if (isSignalBuy && isLong) // Если пришел сигнал на покупку и работаем с длинными позициями
                        BuyAtMarket(bar + 1, entryType.ToString());
                    else if (isSignalShort && !isLong) // Если пришел сигнал на короткую продажу и работаем с короткими позициями
                        ShortAtMarket(bar + 1, entryType.ToString());

                    if (!IsLastPositionActive) // Если не вошли в позицию
                        continue; // то идем проверять условия входа на следующем баре

                    #region Постановка начальных T/S, S/L, T/P

                    if (exitType == ExitType.TrailingStop) // Для технологии сопровождения T/S ставим T/S
                    {
                        if (isTrend)
                        {
                            if (LastActivePosition.PositionType == PositionType.Long) // Если торгуем длинную позицию по тренду
                                LastActivePosition.RiskStopLevel = Bars.Close[bar] - atr[bar]; // ставим T/S на 1 ATR ниже последнего закрытия 
                            else // Если торгуем короткую позицию по тренду
                                LastActivePosition.RiskStopLevel = Bars.Close[bar] + atr[bar]; // ставим T/S на 1 ATR выше последнего закрытия
                        }
                        else
                        {
                            if (LastActivePosition.PositionType == PositionType.Long) // Если торгуем длинную позицию против тренда
                                LastActivePosition.AutoProfitLevel = Bars.Close[bar] + atr[bar] * ProfitRisk; // ставим T/S на ProfitRisk ATR выше последнего закрытия
                            else // Если торгуем короткую позицию против тренда
                                LastActivePosition.AutoProfitLevel = Bars.Close[bar] - atr[bar] * ProfitRisk; // ставим T/S на ProfitRisk ATR ниже последнего закрытия
                        }
                    }
                    else if (exitType == ExitType.StopAndProfit) // Для технологии сопровождения S/P ставим S/L и T/P
                    {
                        if (LastActivePosition.PositionType == PositionType.Long && isTrend) // Если торгуем длинную позицию по тренду
                        {
                            LastActivePosition.RiskStopLevel = Bars.Close[bar] - atr[bar];
                            LastActivePosition.AutoProfitLevel = Bars.Close[bar] + ProfitRisk * atr[bar];
                        }
                        else if (LastActivePosition.PositionType == PositionType.Short && isTrend) // Если торгуем короткую позицию по тренду
                        {
                            LastActivePosition.RiskStopLevel = Bars.Close[bar] + atr[bar];
                            LastActivePosition.AutoProfitLevel = Bars.Close[bar] - ProfitRisk * atr[bar];
                        }
                        else if (LastActivePosition.PositionType == PositionType.Long && !isTrend) // Если торгуем длинную позицию против тренда
                        {
                            LastActivePosition.RiskStopLevel = Bars.Close[bar] - ProfitRisk * atr[bar];
                            LastActivePosition.AutoProfitLevel = Bars.Close[bar] + atr[bar];
                        }
                        else // Если торгуем короткую позицию против тренда
                        {
                            LastActivePosition.RiskStopLevel = Bars.Close[bar] + ProfitRisk * atr[bar];
                            LastActivePosition.AutoProfitLevel = Bars.Close[bar] - atr[bar];
                        }
                    }

                    #endregion
                }

                #endregion

         
            }
        }
    }
}
