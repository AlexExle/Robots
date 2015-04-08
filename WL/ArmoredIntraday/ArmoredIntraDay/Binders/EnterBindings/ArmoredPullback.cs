using ArmoredIntradaySpace.Binders;
using ArmoredIntradaySpace.Binders.EnterBindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntraDay.Binders.EnterBindings
{
    public class ArmoredPullback : AEnterStrategy
    {
        double posInterval = 1000;

        Pullback pullbackStr;

        public ArmoredPullback(WealthScript strategyInstance)
            : base(strategyInstance) 
        {
            pullbackStr = new Pullback(strategyInstance);
            posInterval = ArmoredInstanse._exitParameter.Value * posInterval;
        }
       
        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            EnterSignalType pullbackSignal = pullbackStr.GenerateSignal(bar, out price);

            // Отсекаем противоположенные сигналы
            //if(si.Bars.Close[bar] > ArmoredInstanse.CentralSrikePoint)
            //определить что в этом интервале нет других позиций.
            var priceChecker = price = pullbackStr.pullbackMA[bar];
            List<Position> activePoses = new List<Position>(si.ActivePositions);
            if (activePoses.Find(delegate(Position pos) {
                return  Math.Abs(pos.EntryPrice - priceChecker) <= posInterval;
            }) != null)
                return EnterSignalType.None;
            else
                return pullbackSignal;
        }
    }
}
