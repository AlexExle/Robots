using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;

namespace ArmoredIntradaySpace.Binders.EnterBindings
{
    public class Trend : AEnterStrategy
    {
        public const int TrendPeriod = 100;
        DataSeries trendMA;

        public Trend(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            trendMA = SMA.Series(si.Bars.Close, TrendPeriod);
            si.PlotSeries(si.PricePane, trendMA, Color.Red, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, trendMA.FirstValidValue);
        }

        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;

            if(si.Bars.Close[bar] > trendMA[bar]) // Закрытие бара выше медленной скользящей
               return EnterSignalType.Up;
           if (si.Bars.Close[bar] < trendMA[bar]) // Закрытие бара ниже медленной скользящей
               return EnterSignalType.Down;
           return EnterSignalType.None;
        }
    }
}
