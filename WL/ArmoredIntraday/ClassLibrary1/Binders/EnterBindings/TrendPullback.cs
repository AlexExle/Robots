using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ClassLibrary1.Binders.EnterBindings
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


        public override EnterSignalType GenerateSignal(int bar)
        {
            if (trendStrategy.GenerateSignal(bar) == EnterSignalType.Up && pullbackStrategy.GenerateSignal(bar) == EnterSignalType.Down)
                return EnterSignalType.Up;
            if (trendStrategy.GenerateSignal(bar) == EnterSignalType.Down && pullbackStrategy.GenerateSignal(bar) == EnterSignalType.Up)
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
