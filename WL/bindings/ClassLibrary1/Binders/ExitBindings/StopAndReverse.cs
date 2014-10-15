using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ClassLibrary1.Binders.ExitBindings
{
    public class StopAndReverse : AExitStrategy
    {
        public StopAndReverse(WealthScript strategyInstance, bool isTrend)
            : base(strategyInstance, isTrend)
        { }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        {

        }

        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        {

        }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            if (lastEnterSignal == EnterSignalType.None)
                return false;
            if ((position.PositionType == PositionType.Long && ((lastEnterSignal == EnterSignalType.Down) && isTrend || !(lastEnterSignal == EnterSignalType.Down) && !isTrend)) || // Если находимся в длинной позиции, и пришел сигнал на вход в короткую позицию
                (position.PositionType == PositionType.Short && ((lastEnterSignal == EnterSignalType.Up) && isTrend || !(lastEnterSignal == EnterSignalType.Up) && !isTrend))) // или находимся в короткой позиции, и пришел сигнал на вход в длинную позицию
                return StrategyInstance.ExitAtMarket(bar + 1, position, "SAR"); // то выйти из позиции по рынку
            return false;
        }
    }
}
