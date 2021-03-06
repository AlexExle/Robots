﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ClassLibrary1.Binders.ExitBindings
{
    public class TrailingStop : AExitStrategy
    {
        public TrailingStop(WealthScript strategyInstance, bool isTrend)
            : base(strategyInstance, isTrend)
        { }

        public override void InitExitConditions(Position position, int bar)
        {
            if (isTrend)
            {
                if (position.PositionType == PositionType.Long) // Если торгуем длинную позицию по тренду
                    position.RiskStopLevel = StrategyInstance.Bars.Close[bar] - atr[bar]; // ставим T/S на 1 ATR ниже последнего закрытия 
                else // Если торгуем короткую позицию по тренду
                    position.RiskStopLevel = StrategyInstance.Bars.Close[bar] + atr[bar]; // ставим T/S на 1 ATR выше последнего закрытия
            }
            else
            {
                if (position.PositionType == PositionType.Long) // Если торгуем длинную позицию против тренда
                    position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] + atr[bar] * ProfitRisk; // ставим T/S на ProfitRisk ATR выше последнего закрытия
                else // Если торгуем короткую позицию против тренда
                    position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] - atr[bar] * ProfitRisk; // ставим T/S на ProfitRisk ATR ниже последнего закрытия
            }
        }

        public override void RecalculateExitConditions(Position position, int bar)
        {
            if (isTrend) // Если торгуем по тренду, то двигаем T/S в направлении позиции
            {
                if (position.PositionType == PositionType.Long)
                    position.RiskStopLevel = Math.Max(position.RiskStopLevel, StrategyInstance.Bars.Close[bar] - atr[bar]); // T/S только повышаем 
                else
                    position.RiskStopLevel = Math.Min(position.RiskStopLevel, StrategyInstance.Bars.Close[bar] + atr[bar]); // T/S только понижаем
            }
            else // Если торгуем по флэту, то двигаем обратный T/S против направления позиции
            {
                if (position.PositionType == PositionType.Long)
                    position.AutoProfitLevel = Math.Min(position.AutoProfitLevel, StrategyInstance.Bars.Close[bar] + atr[bar] * ProfitRisk); // T/S только понижаем
                else
                    position.AutoProfitLevel = Math.Max(position.AutoProfitLevel, StrategyInstance.Bars.Close[bar] - atr[bar] * ProfitRisk); // T/S только понижаем              
            }
        }

        public override bool TryExit(Position position, int bar, EnterSignalType lastEnterSignal)
        {
            if (isTrend) // Если торгуем по тренду, то двигаем T/S в направлении позиции
            {
                return StrategyInstance.ExitAtStop(bar + 1, position, position.RiskStopLevel, "T/S"); // то пытаемся выйти по T/S
            }
            else // Если торгуем по флэту, то двигаем обратный T/S против направления позиции
            {
                return StrategyInstance.ExitAtLimit(bar + 1, position, position.AutoProfitLevel, "Rev T/S"); // то пытаемся выйти по обратному Trailing Stop
            }
        }
    }
}
