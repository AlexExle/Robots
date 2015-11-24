using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using Community.Indicators;
using System.Drawing;

namespace Strategies
{
    class Fractal : WealthScript
    {

        //Create parameters

        StrategyParameter right;
        StrategyParameter left;
        StrategyParameter currentBar;
        StrategyParameter fractal;

        //[HandlerParameter(true, "5", Min = "1", Max = "20", Step = "1")]
        public int Right { get; set; }

        //[HandlerParameter(true, "5", Min = "1", Max = "20", Step = "5")]
        public int Left { get; set; }

        //[HandlerParameter(true, "0", Min = "0", Max = "1", Step = "1")]
        public int CurrentBar { get; set; }

        //[HandlerParameter(true, "0", Min = "0", Max = "1", Step = "1")]
        public int pFractal { get; set; }

        public Fractal()
        {                          //name ; default, start , strop , step
            right = CreateParameter("Right", 5, 20, 1, 1);
            left = CreateParameter("Left", 5, 20, 1, 5);
            currentBar = CreateParameter("CurrentBar", 0, 1, 0, 1);
            fractal = CreateParameter("Fractal", 0, 1, 0, 1);
        }

        protected override void Execute()
        {
            Right = right.ValueInt;
            Left = left.ValueInt;
            CurrentBar = currentBar.ValueInt;
            pFractal = fractal.ValueInt;

            ClearDebug(); // Очистить окно отладки
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            HideVolume(); // Скрыть объемы 

            var fractalBay1 = new DataSeries("fractalBay1");
            var fractalBay2 = new DataSeries("fractalBay2");

            var fractalSell1 = new DataSeries("fractalSell1");
            var fractalSell2 = new DataSeries("fractalSell2");
         
            double frbuyv = 0;
            double frbuyv1 = 0;
            int firstValidValue =0;
            int StartIndex = Left + (Right + CurrentBar);
            int ResCheck = Left + Right;

            GenerateBuyFractalSeries(fractalBay1, fractalBay2, ref frbuyv, ref frbuyv1, firstValidValue, StartIndex, ResCheck);
            frbuyv = 0;
            frbuyv1 = 0;
            GenerateBuyFractalSeries(fractalSell1, fractalSell2, ref frbuyv, ref frbuyv1, firstValidValue, StartIndex, ResCheck);

        }

        private void GenerateBuyFractalSeries(DataSeries fractalBay1, DataSeries fractalBay2, ref double frbuyv, ref double frbuyv1, int firstValidValue, int StartIndex, int ResCheck)
        {
            for (int i = firstValidValue; i < Bars.Count; i++)
            {

                if (i < StartIndex)
                    frbuyv = 0;
                else
                {
                    int Check = 0;
                    int IndFr = i - (CurrentBar + Right);
                    for (int j = (i - StartIndex); j <= (i - CurrentBar); j++)
                    {
                        if (j != IndFr)
                        {
                            if (Bars.High[j] < Bars.High[IndFr])
                                Check++;
                            else
                                break;
                        }
                    }
                    if (Check == ResCheck)
                        frbuyv = Bars.High[IndFr];
                }
                fractalBay1.Add(frbuyv);
            }
            for (int i = (Right + CurrentBar); i < Bars.Count; i++)
            {
                frbuyv1 = fractalBay1[i];
                fractalBay2.Add(frbuyv1);
            }
            for (int i = Bars.Count - (Right + CurrentBar); i < Bars.Count; i++)
            {
                fractalBay2.Add(frbuyv1);
            }

            if (pFractal == 0)
            {
                PlotSeries(PricePane, fractalBay1 >> 1, Color.Green, LineStyle.Dotted, 1);
            }
            else
            {
                PlotSeries(PricePane, fractalBay2 >> 1, Color.Green, LineStyle.Dotted, 1);
            }
        }

        private void GenerateSellFractalSeries(DataSeries fractalBay1, DataSeries fractalBay2, ref double frbuyv, ref double frbuyv1, int firstValidValue, int StartIndex, int ResCheck)
        {
            for (int i = firstValidValue; i < Bars.Count; i++)
            {

                if (i < StartIndex)
                    frbuyv = 0;
                else
                {
                    int Check = 0;
                    int IndFr = i - (CurrentBar + Right);
                    for (int j = (i - StartIndex); j <= (i - CurrentBar); j++)
                    {
                        if (j != IndFr)
                        {
                            if (Bars.Low[j] > Bars.Low[IndFr])
                                Check++;
                            else
                                break;
                        }
                    }
                    if (Check == ResCheck)
                        frbuyv = Bars.Low[IndFr];
                }
                fractalBay1.Add(frbuyv);
            }
            for (int i = (Right + CurrentBar); i < Bars.Count; i++)
            {
                frbuyv1 = fractalBay1[i];
                fractalBay2.Add(frbuyv1);
            }
            for (int i = Bars.Count - (Right + CurrentBar); i < Bars.Count; i++)
            {
                fractalBay2.Add(frbuyv1);
            }

            if (pFractal == 0)
            {
                PlotSeries(PricePane, fractalBay1 >> 1, Color.Red, LineStyle.Dotted, 1);
            }
            else
            {
                PlotSeries(PricePane, fractalBay2 >> 1, Color.Red, LineStyle.Dotted, 1);
            }
        }
    }
}
