using ArmoredIntradaySpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmoredIntraDay.Binders.EnterBindings
{
    public class ExpInterval : StaticInterval
    {
        public ExpInterval(ArmoredIntraday strategy)
            : base(strategy)
        {  }

        protected override double CalcPrice(ArmoredIntradaySpace.ArmoredIntraday inst)
        {
            return ((inst.ActivePositions.Count + 1) * Parameter * Math.Pow(2, inst.ActivePositions.Count));         
        }
    }
}
