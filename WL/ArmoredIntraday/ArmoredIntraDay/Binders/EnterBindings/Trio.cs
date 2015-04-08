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
    class Trio : AEnterStrategy
    {
        public const int TrendPeriod = 300;
      
        DataSeries trendEma;

        public Trio(WealthScript strategyInstance)
            : base(strategyInstance)
        {
          
            trendEma = EMA.Series(si.Bars.Close, TrendPeriod, EMACalculation.Legacy);
            si.PlotSeries(si.PricePane, trendEma, Color.Red, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, trendEma.FirstValidValue);
        }

        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;

            if (si.Bars.Close[bar] > trendEma[bar] && si.Bars.Date[bar].Hour == 13 && si.Bars.Date[bar].Minute == 00) // Закрытие бара выше медленной скользящей
                return EnterSignalType.Up;
            if (si.Bars.Close[bar] < trendEma[bar] && si.Bars.Date[bar].Hour == 12 && si.Bars.Date[bar].Minute == 00) // Закрытие бара ниже медленной скользящей
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
