using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmorediIntraday.Binders.EnterBindings
{
    public class Impulse : AEnterStrategy
    {
        public Impulse(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            firstValidValue = Math.Max(firstValidValue, 1); ;
        }

        public override EnterSignalType GenerateSignal(int bar)
        {
            if (StrategyInstance.Bars.Close[bar] > StrategyInstance.Bars.High[bar - 1])
                return EnterSignalType.Up;
            if (StrategyInstance.Bars.Close[bar] < StrategyInstance.Bars.Low[bar - 1])
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
