using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;


namespace ClassLibrary1
{
    class MyStrategyScript : WealthScript
    {
        //Create parameters
        private StrategyParameter longChannelPer;
        private StrategyParameter shortChannelPer;
        //   private StrategyParameter exitMode;
        //    private StrategyParameter enterMode;
        private StrategyParameter maxRiskPercent;

        private StrategyParameter StepPar;
        private StrategyParameter Offset;

        double fullEquity = 800000; //Указываем здесь весь счета
        double Equity = 400000; //Указываем здесь начальный счета
        int SharesInLot = 1; //показываем, сколько акций содержится в лоте (для каждого вида акций свое количество акций в лоте)
        double Shares = 0; //расчетное количество акций (еще не кратное лотам) изначально равняется нулю рассчитывается при входе в позицию.
        double Rate = 1;

        double CurrLevel = 0;

        double HighLevel = 0;

        double LowLevel = 0;

        int posCount = 0;
        int positionCOunt = 0;
        List<double> values = new List<double>();
        public MyStrategyScript()
        {
            longChannelPer = CreateParameter("Long Channel", 55, 35, 100, 5);
            shortChannelPer = CreateParameter("Short Channel", 21, 3, 33, 3);
            //   exitMode = CreateParameter("Exit Mode", 0, 0, 3, 1);
            //   enterMode = CreateParameter("Enter Mode", 0, 0, 3, 1);
            maxRiskPercent = CreateParameter("Max percent risk", 1, 0, 2.5, 0.25);
            Offset = CreateParameter("Offser", 0, 0, 1000, 10);
            StepPar = CreateParameter("Step", 5, 1, 1000, 10);
            //maxTakeProfitPercent = CreateParameter("Max take profit", 6, 0, 20, 0.25);
            //reverseMode = CreateParameter("Reverse mode", 0, 0, 1, 1);
        }

        protected override void Execute()
        {
            //Obtain periods from parameters
            int longPer = longChannelPer.ValueInt;
            int shortPer = shortChannelPer.ValueInt;
            double maxRisk = maxRiskPercent.Value;

            double startPoint = Open[0];
            
            startPoint = startPoint - (startPoint % 10);
            double previousPoint = 0;
            for (int bar = longPer; bar < Bars.Count; bar++)
            {
                if (startPoint > Low[bar] && startPoint < High[bar])
                {
                    previousPoint = startPoint;
                    startPoint = 0;
                }

            }

            // High - массив хаев для всех свечей
            // Low - массив лоувов для всех свечей
            DataSeries H55 = Highest.Series(High, longPer);
            DataSeries L55 = Lowest.Series(Low, longPer);
            DataSeries H21 = Highest.Series(High, shortPer);
            DataSeries L21 = Lowest.Series(Low, shortPer);

            
            DataSeries newDataSeries = new DataSeries("Test");
            
            CurrLevel = Open[0];
            CurrLevel = CurrLevel - (CurrLevel % StepPar.Value) + Offset.Value;
            newDataSeries.Add(CurrLevel);
            HighLevel = CurrLevel + StepPar.Value;
            LowLevel = CurrLevel + StepPar.Value;
            /*currentPoint = open;
            currentPoint = currentPoint - fmod(currentPoint, step) + offset;
            line[0] = currentPoint;
            lowPoint = currentPoint - step;
            highPoint = currentPoint + step;*/
            
            // shift the plotted series to the right one bar to visualize the crossings           

            for (int bar = 1; bar < Bars.Count; bar++)
            {                

                if (High[bar] >= HighLevel)
                {
                    CurrLevel = HighLevel;
                    LowLevel = HighLevel - StepPar.Value;
                    HighLevel = HighLevel + StepPar.Value;
                }
                else
                {
                    if (Low[bar] <= LowLevel)
                    {
                        CurrLevel = LowLevel;
                        LowLevel = LowLevel - StepPar.Value;
                        HighLevel = LowLevel + StepPar.Value;
                    }
                }
                newDataSeries.Add(CurrLevel);           
            }
            PlotSeries(PricePane, newDataSeries, Color.Green, LineStyle.Solid, 1);
        }

        public void CalcPositionSize(int bar)
        {
            //Чтобы добиться кратности можно воспользоваться при программировании следующей логики:
            //int SharesInLot = 10; //показываем, сколько акций содержится в лоте (для каждого вида акций свое количество акций в лоте)
            //int Shares = 118; //каким либо методом рассчитываем, сколько нужно акций купить.
            //SetShareSize(Shares - Shares % SharesInLot); //определяем количество акций, кратное лоту.
            //Выражение: (Shares % SharesInLot) дает в итоге остаток от деления 118 на 10 (т.е. 8) 
            //В итоге получается: 118-8 = 110
            //110 кратно десяти, так что в итоге купится 110 акций (или 11 лотов).
            var eq = (Equity * Rate);
            if (eq > fullEquity)
                eq = fullEquity;
            Shares = (int)(eq / (Close[bar]));
            SetShareSize(System.Math.Floor(Shares - Shares % SharesInLot)); //внутри метода определяем количество акций, которое кратно лоту.
        }
    }
}



