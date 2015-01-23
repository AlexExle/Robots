using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;


namespace Strategies
{

    class TimeImpulse : WealthScript
    {
        //Create parameters

        StrategyParameter fastPeriod;
        StrategyParameter atrPeriod;
        StrategyParameter stopProfitRatio;
        StrategyParameter stopMultiplier;
        public TimeImpulse()
        {
            fastPeriod = CreateParameter("EMA Period", 1000, 100, 1500, 100);
            atrPeriod = CreateParameter("ATR Period", 50, 10, 100, 10);
            stopProfitRatio = CreateParameter("Stop Profit Ratio", 8.5, 3, 10, 0.5);
            stopMultiplier = CreateParameter("stopMultiplier", 2, 1, 4, 1);
        }

        protected override void Execute()
        {

            ClearDebug(); // Очистить окно отладки
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            HideVolume(); // Скрыть объемы 

            //Obtain periods from parameters
            int fastPer = fastPeriod.ValueInt;

            EMA ema = EMA.Series(Close, fastPer, EMACalculation.Legacy);

            ATR atr = ATR.Series(Bars, atrPeriod.ValueInt);

            PlotSeries(PricePane, ema, Color.Green, LineStyle.Solid, 2);

            for (int bar = fastPer; bar < Bars.Count; bar++)
            {
                //Выход по S/P
                if (LastPosition != null && LastPosition.Active)
                {
                    if (LastPosition.PositionType == PositionType.Long)
                    {
                        if (!ExitAtLimit(bar + 1, LastPosition, LastPosition.EntryPrice + atr[LastPosition.EntryBar] * stopProfitRatio.Value, "Long Take Profit"))
                            ExitAtStop(bar + 1, LastPosition, LastPosition.EntryPrice - atr[LastPosition.EntryBar] * stopMultiplier.Value, "Long Stop Loss");
                    }
                    else
                    {
                        if (!ExitAtLimit(bar + 1, LastPosition, LastPosition.EntryPrice - atr[LastPosition.EntryBar] * stopProfitRatio.Value, "Short Take Profit"))
                            ExitAtStop(bar + 1, LastPosition, LastPosition.EntryPrice + atr[LastPosition.EntryBar] * stopMultiplier.Value, "Short Stop Loss");
                    }
                }
                if (Date[bar].Hour == 12 && Date[bar].Minute == 00)
                {
                    if (Close[bar] < ema[bar])
                    {
                        //Выход по перевортному сигналу
                        if (LastPosition != null && LastPosition.Active && LastPosition.PositionType == PositionType.Long)
                        {
                            ExitAtMarket(bar + 1, LastPosition, "Long Traling Stop");
                        }
                        if (LastPosition == null || !LastPosition.Active)
                            ShortAtMarket(bar + 1);
                    }
                }
                if (Date[bar].Hour == 13 && Date[bar].Minute == 00)
                {
                    if (Close[bar] > ema[bar])
                    {
                        //Выход по перевортному сигналу
                        if (LastPosition != null && LastPosition.Active && LastPosition.PositionType == PositionType.Short)
                        {
                            ExitAtMarket(bar + 1, LastPosition, "Short Traling Stop");
                        }
                        if (LastPosition == null || !LastPosition.Active)
                            BuyAtMarket(bar + 1);
                    }
                }
            }

        }
    }
}