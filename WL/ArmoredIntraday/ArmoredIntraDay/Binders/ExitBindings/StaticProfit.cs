using ArmoredIntradaySpace;
using ArmoredIntradaySpace.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntraDay.Binders.ExitBindings
{
    public class StaticProfit : AExitStrategy
    {
        public double staticProfit = 250;
        public StaticProfit(ArmoredIntraday strategyInstance, bool isTrend)
            : base(strategyInstance as WealthScript, isTrend)
        {
            staticProfit = 1000 * strategyInstance._exitParameter.Value;
        }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        {
            if (position.PositionType == PositionType.Long && isTrend) // Если торгуем длинную позицию по тренду
            {
                position.AutoProfitLevel = position.EntryPrice + staticProfit;
            }
            else if (position.PositionType == PositionType.Short && isTrend) // Если торгуем короткую позицию по тренду
            {
                position.AutoProfitLevel = position.EntryPrice - staticProfit;
            }
            else if (position.PositionType == PositionType.Long && !isTrend) // Если торгуем длинную позицию против тренда
            {
                position.AutoProfitLevel = position.EntryPrice + staticProfit;
            }
            else // Если торгуем короткую позицию против тренда
            {
                position.AutoProfitLevel = position.EntryPrice - staticProfit;
            }
        }

        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        {
            double barsHeldCounter = Math.Floor((double)((bar - position.EntryBar) / 5));
            if (barsHeldCounter < 5)
            {
                if (position.PositionType == PositionType.Long)
                {

                    position.AutoProfitLevel = position.AutoProfitLevel = position.EntryPrice + staticProfit - (barsHeldCounter * 20);
                }
                else
                    position.AutoProfitLevel = position.AutoProfitLevel = position.EntryPrice - staticProfit + (barsHeldCounter * 20);
            }
        
        }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            bool result = false;
            if (!result)
                result = si.ExitAtLimit(bar + 1, position, position.AutoProfitLevel, "T/P"); // если не вышли, то попробовать выйти по T/P 
            return result;
        }
    }
}
