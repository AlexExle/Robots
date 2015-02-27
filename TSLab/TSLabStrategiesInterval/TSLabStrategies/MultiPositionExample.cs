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
        private Compress KandleCompresser = new Compress();

        public MultiPositionExample()
        {
        }

        public void Execute(IContext ctx, TSLab.Script.ISecurity sec)
        {
			KandleCompresser.Interval = 300;
            ISecurity cSec = KandleCompresser.Execute(sec);
            for (int bar = 0; bar < cSec.Bars.Count; bar++)
            {
                List<IPosition> activePositions = new List<IPosition>(sec.Positions.GetActiveForBar(bar));

                sec.Positions.SellAtPrice(bar, 1, 90500, "EnterToShort1");
                sec.Positions.SellAtPrice(bar, 1, 91500, "EnterToShort2");
                sec.Positions.SellAtPrice(bar, 1, 92000, "EnterToShort3");
                sec.Positions.SellAtPrice(bar, 1, 92500, "EnterToShort4");

                foreach (IPosition activePosition in activePositions)
                {
                    activePosition.CloseAtPrice(bar, activePosition.EntryPrice + 300 * (activePosition.IsLong ? 1 : -1), "ExitProfit_" + activePosition.EntrySignalName);
                }
            }

            IPane pricePane = ctx.First;
            pricePane.AddList("Compressed", cSec, CandleStyles.BAR_CANDLE, true, true, true, true, 0x0000a0, PaneSides.RIGHT);
        }
    }
}
