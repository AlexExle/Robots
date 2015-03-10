using ArmorediIntraday.Binders;
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
        public int staticProfit = 250;
        public StaticProfit(WealthScript strategyInstance, bool isTrend)
            : base(strategyInstance, isTrend)
        { }

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
        { }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            bool result = false;
            if (!result)
                result = StrategyInstance.ExitAtLimit(bar + 1, position, position.AutoProfitLevel, "T/P"); // если не вышли, то попробовать выйти по T/P 
            return result;
        }
    }
}
