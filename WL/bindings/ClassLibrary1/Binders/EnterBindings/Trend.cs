using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;

namespace ClassLibrary1.Binders.EnterBindings
{
    public class Trend : AEnterStrategy
    {
        public const int TrendPeriod = 100;
        DataSeries trendMA;

        public Trend(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            trendMA = SMA.Series(StrategyInstance.Bars.Close, TrendPeriod);
            StrategyInstance.PlotSeries(StrategyInstance.PricePane, trendMA, Color.Red, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, trendMA.FirstValidValue);
        }

        public override EnterSignalType GenerateSignal(int bar)
        {

            if(StrategyInstance.Bars.Close[bar] > trendMA[bar]) // Закрытие бара выше медленной скользящей
               return EnterSignalType.Up;
           if (StrategyInstance.Bars.Close[bar] < trendMA[bar]) // Закрытие бара ниже медленной скользящей
               return EnterSignalType.Down;
           return EnterSignalType.None;
        }
    }
}
