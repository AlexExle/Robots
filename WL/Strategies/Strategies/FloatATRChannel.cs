using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;


namespace Strategies
{
    public class FloatATRChannel : WealthScript
    {

        //Create parameters

        DataSeries highLevelSeries;
        DataSeries lowLevelSeries;
        DataSeries highLevelSeries2;
        DataSeries lowLevelSeries2;

        double channelStartPoint = 0;

        double highLevel;
        double lowLevel;
        StrategyParameter step;
        StrategyParameter multiplier;
        StrategyParameter stopPercent;
        public FloatATRChannel()
        {
            step = CreateParameter("Period", 25, 25, 500, 25);
            multiplier = CreateParameter("multiplier", 2.5, 0.5, 4, 0.5);
            stopPercent = CreateParameter("stop percent", 0.4, 0.3, 0.8, 0.1);
        }

        protected override void Execute()
        {
            ClearDebug(); // Очистить окно отладки
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            HideVolume(); // Скрыть объемы 
            highLevelSeries = new DataSeries("high");
            lowLevelSeries = new DataSeries("low");
            highLevelSeries2 = new DataSeries("doubleHigh");
            lowLevelSeries2 = new DataSeries("doubleLow");

            int firstValidValue = 1;

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
                if (Bars.IsLastBarOfDay(bar) && Positions.Count == 0)
                {
                    channelStartPoint = Close[bar];
                }

                CalcAtrChannel(atr, bar);

                if (bar > firstValidValue)
                {
                    //Вход
                    if (IsLastPositionActive == false)
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

                        if (Pos.PositionType == PositionType.Long)
                        {
                            //Выход или сдвигаем тралинг
                            if (!SellAtStop(bar + 1, Pos, Pos.RiskStopLevel))
                                Pos.RiskStopLevel = Math.Max(LastActivePosition.RiskStopLevel, Bars.Close[bar] + CalcStopLevel(bar, Pos));
                            else
                                //изменяем точку отсчета для канала
                                channelStartPoint = Pos.ExitPrice;
                        }
                        else
                        {
                            //Выход или сдвигаем тралинг
                            if (!CoverAtStop(bar + 1, Pos, Pos.RiskStopLevel))
                                Pos.RiskStopLevel = Math.Min(LastActivePosition.RiskStopLevel, Bars.Close[bar] + CalcStopLevel(bar, Pos));
                            else
                                //изменяем точку отсчета для канала
                                channelStartPoint = Pos.ExitPrice;
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
    }
}
