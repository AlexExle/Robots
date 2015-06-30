using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;

namespace Strategies
{
    public enum day
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    class DayStatisticHandler
    {
        Dictionary<day, DayStatistic> statictic;

        public DayStatisticHandler()
        {
            statictic = new Dictionary<day, DayStatistic>();            
        }
    }

    class DayStatistic
    {
            public double profit;
            public int loosePositions;
            public int winPositions;
            public int dayWithoutChanges;
    }


}
