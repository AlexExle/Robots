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
    public class Pullback : AEnterStrategy
    {
        public const int PullbackPeriod = 10;
        DataSeries pullbackMA;

        public Pullback(WealthScript strategyInstance)
            : base(strategyInstance)
        {
             pullbackMA = SMA.Series(StrategyInstance.Bars.Close, PullbackPeriod);
             StrategyInstance.PlotSeries(StrategyInstance.PricePane, pullbackMA, Color.Blue, LineStyle.Solid, 1);
             firstValidValue = Math.Max(firstValidValue, pullbackMA.FirstValidValue);            
           
        }

        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;
            if (StrategyInstance.Bars.Close[bar] > pullbackMA[bar]) // Закрытие бара выше быстрой скользящей
                return EnterSignalType.Up;
            if (StrategyInstance.Bars.Close[bar] < pullbackMA[bar]) // Закрытие бара ниже быстрой скользящей
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
