using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntradaySpace.Binders.EnterBindings
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


        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;
            EnterSignalType trandAndImpulse = trendStrategy.GenerateSignal(bar, out price) & impulseStrategy.GenerateSignal(bar, out price);
            if (trandAndImpulse == EnterSignalType.Up && pullbackStrategy.GenerateSignal(bar, out price) == EnterSignalType.Down)
                return EnterSignalType.Up;
            if (trandAndImpulse == EnterSignalType.Down && pullbackStrategy.GenerateSignal(bar, out price) == EnterSignalType.Up)
                return EnterSignalType.Down;
            return EnterSignalType.None;
           
        }
    }
}
