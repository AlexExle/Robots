using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ClassLibrary1.Binders.ExitBindings
{
    public class StopAndProfit : AExitStrategy
    {
        public StopAndProfit(WealthScript strategyInstance, bool isTrend)
            : base(strategyInstance, isTrend)
        {        
        }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        {
            if (position.PositionType == PositionType.Long && isTrend) // Если торгуем длинную позицию по тренду
            {
                position.RiskStopLevel =  StrategyInstance.Bars.Close[bar] - atr[bar]*3;
                position.AutoProfitLevel =  StrategyInstance.Bars.Close[bar] + ProfitRisk * atr[bar]*3;
            }
            else if (position.PositionType == PositionType.Short && isTrend) // Если торгуем короткую позицию по тренду
            {
                position.RiskStopLevel = StrategyInstance.Bars.Close[bar] + atr[bar]*3;
                position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] - ProfitRisk * atr[bar]*3;
            }
            else if (position.PositionType == PositionType.Long && !isTrend) // Если торгуем длинную позицию против тренда
            {
                position.RiskStopLevel = StrategyInstance.Bars.Close[bar] - ProfitRisk * atr[bar]*3;
                position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] + atr[bar]*3;
            }
            else // Если торгуем короткую позицию против тренда
            {
                position.RiskStopLevel = StrategyInstance.Bars.Close[bar] + ProfitRisk * atr[bar]*3;
                position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] - atr[bar]*3;
            }
        }

        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        { }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            bool result =  StrategyInstance.ExitAtStop(bar + 1, position, position.RiskStopLevel, "S/L");// Попробовать выйти по S/L
            if (!result)
                result = StrategyInstance.ExitAtLimit(bar + 1, position, position.AutoProfitLevel, "T/P"); // если не вышли, то попробовать выйти по T/P 
            return result;
        }
    }
}
