using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ClassLibrary1.Binders.ExitBindings
{
    public class TimeExit : AExitStrategy
    {
        const int WaitPeriod = 4; 

        public TimeExit(WealthScript strategyInstance, bool isTrend)
            : base(strategyInstance, isTrend)
        { }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        { }

        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        { }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            if (bar - position.EntryBar == WaitPeriod) // Если время вышло
              return StrategyInstance.ExitAtMarket(bar + 1, position, "Exit Time"); // то выйти из позиции по рынку
            return false;
        }
    }
}
