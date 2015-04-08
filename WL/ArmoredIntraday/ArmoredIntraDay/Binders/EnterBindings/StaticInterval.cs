using ArmoredIntradaySpace;
using ArmoredIntradaySpace.Binders;
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
        int multiplier = 1000;
        public double Parameter = 0;
        public StaticInterval(ArmoredIntraday strategy)
            : base(strategy as WealthScript)
        {
            Parameter = multiplier * strategy._enterParameter.Value;
        }

        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;
            ArmoredIntraday inst = si as ArmoredIntraday;
            if (inst.Close[bar] > CalcPrice(inst) + inst.CentralSrikePoint)
            {
                price = CalcPrice(inst) + inst.CentralSrikePoint;
                return EnterSignalType.Up;
            }
            if (inst.Close[bar] < inst.CentralSrikePoint - CalcPrice(inst))
            {
                price = CalcPrice(inst) - inst.CentralSrikePoint;
                return EnterSignalType.Down;
            }           
            return EnterSignalType.None;
        }

        protected virtual double CalcPrice(ArmoredIntraday inst)
        {
            return ((inst.ActivePositions.Count + 1) * Parameter);
        }        
    }
}
