﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using Community.Indicators;
using System.Drawing;

namespace Strategies
{
    class NRTR1 : WealthScript
    {

         //Create parameters

        DataSeries nrtr;

        StrategyParameter step;
      

        public NRTR1()
        {
            step = CreateParameter("step", 2.5, 0.5, 4, 0.5);
        }

        protected override void Execute()
        {
            ClearDebug(); // Очистить окно отладки
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            HideVolume(); // Скрыть объемы 
            nrtr = NRTR_Percent.Series(Bars, step.Value);
            var nrtr_osc = new DataSeries("NRTR_OSC");

            NRTR_PercentHelper helper = new NRTR_PercentHelper();
            int firstValidValue = 1;

            firstValidValue = Math.Max(firstValidValue, nrtr.FirstValidValue);
            ChartPane trixPane = CreatePane(20, false, true);

            for (int bar = 1; bar < Bars.Count; bar++)
            {
                nrtr_osc.Add(Math.Abs((100 * (Close[bar] - nrtr[bar]) / Close[bar]))/step.Value);
                if (bar > firstValidValue)
                {
                    //Вход
                    if (IsLastPositionActive == false)
                    {
                        if (Close[bar] > nrtr[bar])
                        {
                            Position Pos = BuyAtMarket(bar + 1);
                        }

                        if (Close[bar] < nrtr[bar])
                        {
                            Position Pos = ShortAtMarket(bar + 1);
                        }
                    }
                    if (IsLastPositionActive == true)
                    {
                        Position Pos = LastPosition;

                        if (Pos.PositionType == PositionType.Long)
                        {
                            if (nrtr[bar] > Close[bar])
                                SellAtMarket(bar + 1, Pos);
                        }
                        else
                        {
                            if (nrtr[bar] < Close[bar])
                                CoverAtMarket(bar + 1, Pos);
                        }
                    }
                }
            }
            PlotSeriesOscillator(trixPane, nrtr_osc, 0, 0, Color.LightPink, Color.LightBlue, Color.Red, LineStyle.Solid, 1);
            PlotSeries(PricePane, nrtr >> 1, Color.Green, LineStyle.Dotted, 1);           
        }
    }
}
