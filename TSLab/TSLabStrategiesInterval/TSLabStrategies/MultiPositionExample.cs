using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TSLabStrategies
{
    public class MultiPositionExample : IExternalScript
    {
        
        public MultiPositionExample()
        {
        }

        public void Execute(IContext ctx, TSLab.Script.ISecurity sec)
        {

            for (int bar = 0; bar < sec.Bars.Count; bar++)
            {
                List<IPosition> activePositions = new List<IPosition>(sec.Positions.GetActiveForBar(bar));

                sec.Positions.SellAtPrice(bar + 1, 1, 91500, "EnterToShort1");
                sec.Positions.SellAtPrice(bar + 1, 1, 92500, "EnterToShort2");
                sec.Positions.SellAtPrice(bar + 1, 1, 93000, "EnterToShort3");
                sec.Positions.SellAtPrice(bar + 1, 1, 94500, "EnterToShort4");

                foreach (IPosition activePosition in activePositions)
                {
                    activePosition.CloseAtPrice(bar+1, activePosition.EntryPrice + 300 * (activePosition.IsLong ? 1 : -1), "ExitProfit_" + activePosition.EntrySignalName);
                }
            }           
        }
    }
}
