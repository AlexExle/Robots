using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntradaySpace.Binders.EnterBindings
{
    class TrendImpulse : AEnterStrategy
    {

        Trend trendStrategy;
        Impulse impulseStrategy;

        public TrendImpulse(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            trendStrategy = new Trend(strategyInstance);
            impulseStrategy = new Impulse(strategyInstance);
        }


        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;
            return trendStrategy.GenerateSignal(bar, out price) & impulseStrategy.GenerateSignal(bar, out price);
        }
    }
}
