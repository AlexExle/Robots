using Community.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script.Handlers;
using WealthLab;

namespace WLIndicators
{
    public class NRTR_Osc : IBar2DoubleHandler
    {


        [HandlerParameter(true, "60", Min = "10", Max = "200", Name = "Period", Step = "10")]
        public int AtrPeriod { get; set; }
        [HandlerParameter(true, "4", Min = "0.10", Max = "8", Name = "Multy", Step = "0.10")]
        public double Multiplier { get; set; }    

        public IList<double> Execute(TSLab.Script.ISecurity source)
        {
            WealthLab.Bars bars = new WealthLab.Bars(source.Symbol, NRTR_WATR_Indicator.TScaleToWLScale(source), source.Interval);

            foreach (var bar in source.Bars)
            {
                bars.Add(bar.Date, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
            }

            DataSeries indicator = NRTR_WATR.Series(bars, AtrPeriod, Multiplier);
            

            IList<double> result = new List<double>();

            for (int bar = 0; bar < indicator.Count; bar++)
            {
                var res1 = Math.Abs((100 * (bars.Close[bar] - indicator[bar]) / bars.Close[bar]));
                //var sma = new SMA()
                
                //Value1 = AbsValue( (100* (C-$NRTR_DT (K) ) /C ) /K ) ;
                //Value2 = Power(Average(Value1,Smooth),Sharp);
                result.Add(indicator[bar]);
            }
            return result;
        }
    }
}
