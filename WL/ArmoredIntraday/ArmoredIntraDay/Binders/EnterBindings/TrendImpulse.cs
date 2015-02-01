using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmorediIntraday.Binders.EnterBindings
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


        public override EnterSignalType GenerateSignal(int bar)
        {
            return trendStrategy.GenerateSignal(bar) & impulseStrategy.GenerateSignal(bar);
        }
    }
}
