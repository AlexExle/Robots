using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;

namespace ArmorediIntraday.Binders.EnterBindings
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
            trendEma = EMA.Series(StrategyInstance.Bars.Close, TrendPeriod, EMACalculation.Legacy);
            StrategyInstance.PlotSeries(StrategyInstance.PricePane, trendEma, Color.Red, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, trendEma.FirstValidValue);

            fastEma = EMA.Series(StrategyInstance.Bars.Close, FastPeriod, EMACalculation.Legacy);
            StrategyInstance.PlotSeries(StrategyInstance.PricePane, fastEma, Color.Green, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, fastEma.FirstValidValue);
        }

        public override EnterSignalType GenerateSignal(int bar)
        {

            if (fastEma[bar] > trendEma[bar] && fastEma[bar-1] <= trendEma[bar-1]) // Пересечение сверху вниз
               return EnterSignalType.Up;
            if (fastEma[bar] < trendEma[bar] && fastEma[bar - 1] >= trendEma[bar - 1]) // Пересечение снизу вверх
               return EnterSignalType.Down;
           return EnterSignalType.None;
        }
    }
}