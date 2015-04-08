using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntradaySpace.Binders.EnterBindings
{
    class TrendPullback : AEnterStrategy
    {

        Trend trendStrategy;
        Pullback pullbackStrategy;

        public TrendPullback(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            trendStrategy = new Trend(strategyInstance);
            pullbackStrategy = new Pullback(strategyInstance);
        }


        public override EnterSignalType GenerateSignal(int bar, out double price )
        {
            price = 0;
            if (trendStrategy.GenerateSignal(bar, out price) == EnterSignalType.Up && pullbackStrategy.GenerateSignal(bar, out price) == EnterSignalType.Down)
                return EnterSignalType.Up;
            if (trendStrategy.GenerateSignal(bar, out price) == EnterSignalType.Down && pullbackStrategy.GenerateSignal(bar, out price) == EnterSignalType.Up)
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
