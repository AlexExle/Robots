using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;


namespace Strategies
{
    public class DayATRChannel : WealthScript
    {

        //Create parameters
        private StrategyParameter step;
        private StrategyParameter multiplier;
        private StrategyParameter stopPercent;

        DataSeries highLevelSeries;
        DataSeries lowLevelSeries;
        DataSeries highLevelSeries2;
        DataSeries lowLevelSeries2;

        double channelStartPoint = 0;

        double highLevel;
        double lowLevel;
        public DayATRChannel()
        {
            step = CreateParameter("Period", 25, 25, 500, 25);
            multiplier = CreateParameter("multiplier", 2, 0.5, 3, 0.5);
            stopPercent = CreateParameter("stop percent", 0.4, 0.3, 0.8, 0.1);
        }

        protected override void Execute()
        {

            highLevelSeries = new DataSeries("high");
            lowLevelSeries = new DataSeries("low");
            highLevelSeries2 = new DataSeries("doubleHigh");
            lowLevelSeries2 = new DataSeries("doubleLow");


            ClearDebug(); // Очистить окно отладки
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            HideVolume(); // Скрыть объемы 
            int firstValidValue = 1;
            List<DateTime> dates = new List<DateTime>();

            int stepVal = step.ValueInt;

            channelStartPoint = Close[0];

            highLevel = channelStartPoint + stepVal;
            lowLevel = channelStartPoint - stepVal;
            lowLevelSeries.Add(lowLevel);
            highLevelSeries.Add(highLevel);
            lowLevelSeries2.Add(lowLevel - stepVal);
            highLevelSeries2.Add(highLevel + stepVal);

            ATR atr = ATR.Series(Bars, stepVal);

            firstValidValue = Math.Max(firstValidValue, atr.FirstValidValue);

            for (int bar = 1; bar < Bars.Count; bar++)
            {
                if (Bars.IsLastBarOfDay(bar))
                {
                    channelStartPoint = Close[bar];
                }

                CalcAtrChannel(atr, bar);

                if (bar > firstValidValue)
                {
                    if (Positions.Count == 0 || (!IsLastPositionActive && LastPosition.ExitDate.Date.CompareTo(Date[bar].Date) != 0))
                    {
                        Position Pos = BuyAtStop(bar + 1, highLevelSeries2[bar]);
                        if (Pos == null)
                        {
                            Pos = ShortAtStop(bar + 1, lowLevelSeries2[bar]);
                        }
                        if (Pos != null)
                            Pos.RiskStopLevel = Pos.EntryPrice + CalcStopLevel(bar, Pos);
                    }
                    else
                    {
                        Position Pos = LastPosition;

                        if (!(Date[bar].Hour == 23 && Date[bar].Minute > 30 && ExitAtMarket(bar + 1, Pos, "End of day")))
                            if (Pos.PositionType == PositionType.Long)
                            {
                                if (!SellAtStop(bar + 1, Pos, Pos.RiskStopLevel))
                                    Pos.RiskStopLevel = Math.Max(Pos.RiskStopLevel, Bars.Close[bar] + CalcStopLevel(bar, Pos));
                            }
                            else
                            {
                                if (!CoverAtStop(bar + 1, LastPosition, Pos.RiskStopLevel))
                                    Pos.RiskStopLevel = Math.Min(Pos.RiskStopLevel, Bars.Close[bar] + CalcStopLevel(bar, Pos));
                            }
                    }
                }
            }

            PlotSeries(PricePane, highLevelSeries >> 1, Color.Green, LineStyle.Dotted, 1);
            PlotSeries(PricePane, lowLevelSeries >> 1, Color.Green, LineStyle.Dotted, 1);
            PlotSeries(PricePane, highLevelSeries2 >> 1, Color.Green, LineStyle.Solid, 2);
            PlotSeries(PricePane, lowLevelSeries2 >> 1, Color.Green, LineStyle.Solid, 2);


        }

        private double CalcStopLevel(int bar, Position Pos)
        {
            return (highLevelSeries2[bar] - lowLevelSeries2[bar]) * stopPercent.Value * (Pos.PositionType == PositionType.Short ? 1 : -1);
        }

        private void CalcAtrChannel(ATR atr, int bar)
        {
            highLevel = channelStartPoint + atr[bar] * multiplier.Value;
            lowLevel = channelStartPoint - atr[bar] * multiplier.Value;
            lowLevelSeries.Add(lowLevel);
            highLevelSeries.Add(highLevel);
            lowLevelSeries2.Add(lowLevel - atr[bar] * multiplier.Value);
            highLevelSeries2.Add(highLevel + atr[bar] * multiplier.Value);
        }

        protected bool IsDateInArray(DateTime date, List<DateTime> array)
        {
            foreach (var dt in array)
            {
                if (date.Minute == dt.Minute && date.Hour == dt.Hour)
                    return true;
            }
            return false;
        }
    }
}
