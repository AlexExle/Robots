using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers; // для работы с индикаторими и обработчиками
using TSLab.Script.Helpers; // помошники
using TSLab.Script.Optimization; // для оптимизации

namespace TSLabStrategies
{
    public class Stairway : IExternalScript
    {
        public IPosition LastActivePosition = null;
        public IPosition LastClosedPosition = null;

        OptimProperty Offset;
        OptimProperty StepPar;

        public Stairway()
        {
            Offset = new OptimProperty (0, 0, 300, 10);
            StepPar = new OptimProperty(0, 30, 300, 10);
        }

        int posSizer = 1;
        int baseSize = 1;
        public void Execute(IContext ctx, ISecurity sec)
        {

            int firstValidValue = 0;
            IList<double> stairwayIndicator = new  List<double>();
            
            double CurrLevel = sec.Bars[0].Open;
			CurrLevel = CurrLevel - (CurrLevel % StepPar.Value) + Offset.Value;

            stairwayIndicator.Add(CurrLevel);
			double HighLevel = CurrLevel + StepPar.Value;
            double LowLevel = CurrLevel - StepPar.Value;

            bool signalBuy = false; bool signalShort = false;
          
          

            for (int bar = firstValidValue; bar < sec.Bars.Count; bar++)
            {

                signalBuy = sec.Bars[bar].High > HighLevel;
                signalShort = sec.Bars[bar].Low < LowLevel;

                LastClosedPosition  = sec.Positions.GetLastPositionClosed(bar);
                LastActivePosition = sec.Positions.GetLastPositionActive(bar);// получить ссылку на последнию позицию
                               
                if (LastActivePosition != null)
                {
                    if (LastActivePosition.Shares > baseSize)
                    {
                        if (signalBuy && LastActivePosition.IsLong)
                        {
                            LastActivePosition.CloseAtMarket(bar, "CloseDouble");
                            sec.Positions.BuyAtPrice(bar + 1, baseSize, HighLevel, "SingleReBuy");
                        }
                        if (signalShort && LastActivePosition.IsShort)
                        {
                            LastActivePosition.CloseAtMarket(bar, "CloseDouble");
                            sec.Positions.SellAtPrice(bar + 1, baseSize, HighLevel, "SingleReSell");
                        }
                    }
                    if (LastActivePosition.IsLong)
                    {                      
                            LastActivePosition.CloseAtStop(bar + 1, LowLevel, "StopLong");
                    }
                    else
                    {
                        LastActivePosition.CloseAtStop(bar + 1, HighLevel, "StopShort");
                    }
                }

                if (LastActivePosition == null)
                {

                    if (LastClosedPosition != null && LastClosedPosition.Shares == baseSize)
                    {
                        if (signalBuy && LastClosedPosition.IsShort)
                            sec.Positions.BuyAtPrice(bar + 1, baseSize * 2, HighLevel, "DoubleBuy");
                        else
                            if (signalShort && LastClosedPosition.IsLong)
                                sec.Positions.SellAtPrice(bar + 1, baseSize * 2, LowLevel, "DoubleSell");
                    }
                    else
                    {
                        //if (signalBuy)
                            sec.Positions.BuyAtPrice(bar + 1, baseSize , HighLevel, "SingleBuy");
                       /// else
                        //if (signalShort)
                            sec.Positions.SellAtPrice(bar + 1, baseSize , LowLevel, "SingleSell");
                    }
                }

                if (sec.Bars[bar].High >= HighLevel)
                {
                    CurrLevel = HighLevel;
                    LowLevel = CurrLevel - StepPar.Value;
                    HighLevel = CurrLevel + StepPar.Value;


                }
                else
                {
                    if (sec.Bars[bar].Low <= LowLevel)
                    {
                        CurrLevel = LowLevel;
                        LowLevel = CurrLevel - StepPar.Value;
                        HighLevel = CurrLevel + StepPar.Value;
                    }
                }
                stairwayIndicator.Add(CurrLevel);
            }

            IPane pricePane = ctx.First;
            //pricePane.AddList("Stairway", stairwayIndicator, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.Left);
        }
    }
}
