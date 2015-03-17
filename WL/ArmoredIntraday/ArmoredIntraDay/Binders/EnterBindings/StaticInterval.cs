using ArmorediIntraday.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntraDay.Binders.EnterBindings
{
    public class StaticInterval : AEnterStrategy
    {
        public StaticInterval(WealthScript strategy):base(strategy)
        {
            
        }

        public override EnterSignalType GenerateSignal(int bar)
        {
            
        }
    }
}
