using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ClassLibrary1.Binders.EnterBindings
{
    class TrendPullbackImpulse : AEnterStrategy
    {
        
        Trend trendStrategy;
        Pullback pullbackStrategy;
        Impulse impulseStrategy;

        public TrendPullbackImpulse(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            trendStrategy = new Trend(strategyInstance);
            pullbackStrategy = new Pullback(strategyInstance);
            impulseStrategy = new Impulse(strategyInstance);
        }


        public override EnterSignalType GenerateSignal(int bar)
        {
            EnterSignalType trandAndImpulse = trendStrategy.GenerateSignal(bar) & impulseStrategy.GenerateSignal(bar);
            if (trandAndImpulse == EnterSignalType.Up && pullbackStrategy.GenerateSignal(bar) == EnterSignalType.Down)
                return EnterSignalType.Up;
            if (trandAndImpulse == EnterSignalType.Down && pullbackStrategy.GenerateSignal(bar) == EnterSignalType.Up)
                return EnterSignalType.Down;
            return EnterSignalType.None;
           
        }
    }
}
