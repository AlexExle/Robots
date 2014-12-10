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

                LastActivePosition = sec.Positions.GetLastPositionActive(bar);// получить ссылку на последнию позицию
               
                //sec.Positions.GetLastPositionClosed
                if (LastActivePosition != null)//if (IsLastPositionActive) //если позиция есть:
                {
                    if (LastActivePosition.IsLong)
                    {
                        if (posSizer > baseSize)
                        {
                            LastActivePosition.CloseAtStop(bar + 1, LowLevel, "stop Long");
                        }
                    }
                    else
                    {
                        LastActivePosition.CloseAtStop(bar + 1, HighLevel, "stop Short");
                    }
                }

                if (LastActivePosition == null)
                {
                    if (sec.Positions.GetLastPositionClosed(bar) == null)
                    {
                        sec.Positions.BuyIfGreater(bar + 1, posSizer, HighLevel, "Buy");
                        sec.Positions.SellIfLess(bar + 1, posSizer, LowLevel, "Sell");
                    }
                    else
                    {
                        
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
            pricePane.AddList("Stairway", stairwayIndicator, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
        }
    }
}
