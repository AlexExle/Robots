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
    class Trio : AEnterStrategy
    {
        public const int TrendPeriod = 300;
      
        DataSeries trendEma;

        public Trio(WealthScript strategyInstance)
            : base(strategyInstance)
        {
          
            trendEma = EMA.Series(StrategyInstance.Bars.Close, TrendPeriod, EMACalculation.Legacy);
            StrategyInstance.PlotSeries(StrategyInstance.PricePane, trendEma, Color.Red, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, trendEma.FirstValidValue);
        }

        public override EnterSignalType GenerateSignal(int bar)
        {


            if (StrategyInstance.Bars.Close[bar] > trendEma[bar] && StrategyInstance.Bars.Date[bar].Hour == 13 && StrategyInstance.Bars.Date[bar].Minute == 00) // Закрытие бара выше медленной скользящей
                return EnterSignalType.Up;
            if (StrategyInstance.Bars.Close[bar] < trendEma[bar] && StrategyInstance.Bars.Date[bar].Hour == 12 && StrategyInstance.Bars.Date[bar].Minute == 00) // Закрытие бара ниже медленной скользящей
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
