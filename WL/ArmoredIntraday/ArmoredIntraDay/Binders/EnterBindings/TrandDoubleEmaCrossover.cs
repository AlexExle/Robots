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
    class TrandDoubleEmaCrossover : AEnterStrategy
    {
        public const int TrendPeriod = 27;
        public const int FastPeriod = 9;
        DataSeries trendEma;
        DataSeries fastEma;

        public TrandDoubleEmaCrossover(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            trendEma = EMA.Series(si.Bars.Close, TrendPeriod, EMACalculation.Legacy);
            si.PlotSeries(si.PricePane, trendEma, Color.Red, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, trendEma.FirstValidValue);

            fastEma = EMA.Series(si.Bars.Close, FastPeriod, EMACalculation.Legacy);
            si.PlotSeries(si.PricePane, fastEma, Color.Green, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, fastEma.FirstValidValue);
        }

        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;

            if (fastEma[bar] > trendEma[bar] && fastEma[bar-1] <= trendEma[bar-1]) // Пересечение сверху вниз
               return EnterSignalType.Up;
            if (fastEma[bar] < trendEma[bar] && fastEma[bar - 1] >= trendEma[bar - 1]) // Пересечение снизу вверх
               return EnterSignalType.Down;
           return EnterSignalType.None;
        }
    }
}