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
    public class Pullback : AEnterStrategy
    {
        public int PullbackPeriod = 30;
        public DataSeries pullbackMA; 

        public Pullback(WealthScript strategyInstance)
            : base(strategyInstance)
        {
            PullbackPeriod = (int)Math.Round(PullbackPeriod * this.ArmoredInstanse._enterParameter.Value);
            pullbackMA = SMA.Series(si.Bars.Close, PullbackPeriod);
            si.PlotSeries(si.PricePane, pullbackMA, Color.Blue, LineStyle.Solid, 1);
            firstValidValue = Math.Max(firstValidValue, pullbackMA.FirstValidValue);            
           
        }

        public override EnterSignalType GenerateSignal(int bar, out double price)
        {
            price = 0;
            if (si.Bars.Close[bar] > pullbackMA[bar]) // Закрытие бара выше быстрой скользящей
                return EnterSignalType.Up;
            if (si.Bars.Close[bar] < pullbackMA[bar]) // Закрытие бара ниже быстрой скользящей
                return EnterSignalType.Down;
            return EnterSignalType.None;
        }
    }
}
