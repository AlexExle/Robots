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
        private StrategyParameter maxTakeProfitPercent;
        private StrategyParameter reverseMode;

        
        double Equity = 200000; //Указываем здесь размер счета
        int SharesInLot = 10; //показываем, сколько акций содержится в лоте (для каждого вида акций свое количество акций в лоте)
        double Shares = 0; //расчетное количество акций (еще не кратное лотам) изначально равняется нулю рассчитывается при входе в позицию.
        double InitialStop = 0; //объявляем переменную, где будет храниться величина стоп-лосса (после расчета) 

        public MyStrategyScript()
        {
            longChannelPer = CreateParameter("Long Channel", 55, 35, 100, 5);
            shortChannelPer = CreateParameter("Short Channel", 21, 3, 33, 3);
            //   exitMode = CreateParameter("Exit Mode", 0, 0, 3, 1);
            //   enterMode = CreateParameter("Enter Mode", 0, 0, 3, 1);
            maxRiskPercent = CreateParameter("Max percent risk", 1, 0, 2.5, 0.25);
            //maxTakeProfitPercent = CreateParameter("Max take profit", 6, 0, 20, 0.25);
            //reverseMode = CreateParameter("Reverse mode", 0, 0, 1, 1);
        }

        protected override void Execute()
        {
            //Obtain periods from parameters
            int longPer = longChannelPer.ValueInt;
            int shortPer = shortChannelPer.ValueInt;
            double maxRisk = maxRiskPercent.Value;
            double maxTakeProfit = 0;//maxTakeProfitPercent.Value;
            int reverseModeInt = 0; //reverseMode.ValueInt;
            // High - массив хаев для всех свечей
            // Low - массив лоувов для всех свечей
            DataSeries H55 = Highest.Series(High, longPer);
            DataSeries L55 = Lowest.Series(Low, longPer);
            DataSeries H21 = Highest.Series(High, shortPer);
            DataSeries L21 = Lowest.Series(Low, shortPer);

            // shift the plotted series to the right one bar to visualize the crossings
            PlotSeries(PricePane, H55 >> 1, Color.Green, LineStyle.Solid, 1);
            PlotSeries(PricePane, L21 >> 1, Color.Red, LineStyle.Dotted, 1);
            PlotSeries(PricePane, H21 >> 1, Color.Blue, LineStyle.Dotted, 1);
            PlotSeries(PricePane, L55 >> 1, Color.Fuchsia, LineStyle.Solid, 1);

            for (int bar = longPer; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position Pos = LastPosition;

                    //custom exit states
                    if (Pos.EntryBar < bar)
                    {
                        if (maxRisk > 0)
                        {
                            if (Pos.PositionType == PositionType.Long)
                            {
                                if (Close[bar] < Pos.EntryPrice && (100 - ((Close[bar] / Pos.EntryPrice) * 100) > maxRisk))
                                {
                                    ExitAtMarket(bar + 1, LastPosition, "Закрываем лонг по привышению риска " + maxRisk.ToString());
                                    continue;
                                }
                            }
                            else
                            {
                                if (Close[bar] > Pos.EntryPrice && ((Close[bar] / Pos.EntryPrice - 1) * 100 > maxRisk))
                                {
                                    ExitAtMarket(bar + 1, LastPosition, "Закрываем шорт по привышению риска " + maxRisk.ToString());
                                    continue;
                                }
                            }
                        }
                    }

                    if (Date[bar].Hour > 18 && Date[bar].Minute > 20)
                    {
                        ExitAtMarket(bar + 1, LastPosition, "В конце дня валим");
                    }

                    if (Pos.PositionType == PositionType.Long)
                        SellAtStop(bar + 1, LastPosition, L21[bar]);
                    else
                        CoverAtStop(bar + 1, LastPosition, H21[bar]);
                }

                if (!IsLastPositionActive)
                {
                    //if (Date[bar].Hour >= 18 && Date[bar].Minute >= 20)
                    //    continue;

                    RiskStopLevel = L21[bar];
                    if (BuyAtStop(bar + 1, H55[bar]) == null)
                    {
                        RiskStopLevel = H21[bar];
                        ShortAtStop(bar + 1, L55[bar]);
                    }

                }
            }
        }

        public void CalcPositionSize()
        {
            //Чтобы добиться кратности можно воспользоваться при программировании следующей логики:
            //int SharesInLot = 10; //показываем, сколько акций содержится в лоте (для каждого вида акций свое количество акций в лоте)
            //int Shares = 118; //каким либо методом рассчитываем, сколько нужно акций купить.
            //SetShareSize(Shares - Shares % SharesInLot); //определяем количество акций, кратное лоту.
            //Выражение: (Shares % SharesInLot) дает в итоге остаток от деления 118 на 10 (т.е. 8) 
            //В итоге получается: 118-8 = 110
            //110 кратно десяти, так что в итоге купится 110 акций (или 11 лотов).

            //Shares = (int)((Equity * 0.03) / (Close[bar] - InitialStop)); //определяем количество акций для покупки (не кратно лоту) в зависимости от риска на одну сделку и отдаленности Стоп-Лос
            //SetShareSize(System.Math.Floor(Shares - Shares % SharesInLot)); //внутри метода определяем количество акций, которое кратно лоту.
        }
    }
}



