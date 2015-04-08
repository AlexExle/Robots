using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntradaySpace.Binders.EnterBindings
{
    public class Impulse : AEnterStrategy
    {
        public Impulse(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            firstValidValue = Math.Max(firstValidValue, 1); ;
        }

        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;
            if (si.Bars.Close[bar] > si.Bars.High[bar - 1])
                return EnterSignalType.Up;
            if (si.Bars.Close[bar] < si.Bars.Low[bar - 1])
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
