using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmorediIntraday.Binders.ExitBindings
{
    class ComplexTrioExit : AExitStrategy
    {

        StopAndProfit sap;
        StopAndReverse sar;
        TrailingStop ts;
        public ComplexTrioExit(WealthScript strategyInstance, bool isTrend) : base(strategyInstance, isTrend)
        {
            sap = new StopAndProfit(strategyInstance, isTrend);
            sar = new StopAndReverse(strategyInstance, isTrend);
            ts = new TrailingStop(strategyInstance, isTrend);
        }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        {
            sap.InitExitConditions(position, bar);
        }

        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        {
            ts.RecalculateExitConditions(position, bar);
        }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            return sar.TryExit(position,bar,lastEnterSignal) || ts.TryExit(position, bar, lastEnterSignal) || sap.TryExit(position, bar, lastEnterSignal);

        }
    }
}
