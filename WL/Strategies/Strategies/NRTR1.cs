using System;
using System.Collections.Generic;
using System.Linq;
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
                     
            NRTR_PercentHelper helper = new NRTR_PercentHelper();
            int firstValidValue = 1;
         
            firstValidValue = Math.Max(firstValidValue, nrtr.FirstValidValue);

            for (int bar = 1; bar < Bars.Count; bar++)
            {               
                if (bar > firstValidValue)
                {
                    //Вход
                    if (IsLastPositionActive == false)
                    {
                        if (nrtr[bar] > High[bar])
                        {
                            Position Pos = BuyAtStop(bar + 1, nrtr[bar]);
                        }

                        if (nrtr[bar] < Low[bar])
                        {
                            Position Pos = ShortAtStop(bar + 1, nrtr[bar]);
                        }                     
                    }
                    else
                    {
                        Position Pos = LastPosition;

                        if (Pos.PositionType == PositionType.Long)
                        {
                            //Выход или сдвигаем тралинг
                            SellAtStop(bar + 1, Pos, nrtr[bar]);
                                
                        }
                        else
                        {
                            CoverAtStop(bar + 1, Pos, nrtr[bar]);                          
                        }
                    }
                }
            }

            PlotSeries(PricePane, nrtr >> 1, Color.Green, LineStyle.Dotted, 1);           
        }
    }
}
