using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script.Handlers;
using Community.Indicators;
using WealthLab;
using TSLab.Script;

namespace WLIndicators
{
    [HandlerName("NRTR_WATR")]
    [HandlerCategory("WealthLab.Indicators")]
    public class NRTR_WATR_Indicator : IBar2DoubleHandler
    {
        [HandlerParameter(true, "60", Min = "10" , Max = "200", Name = "Period", Step = "10")]
        public int AtrPeriod { get; set; }
        [HandlerParameter(true, "4", Min = "0.10", Max = "8", Name = "Multy", Step = "0.10")]
        public double Multiplier { get; set; }

        public IList<double> Execute(TSLab.Script.ISecurity source)
        {
            var bars = new WealthLab.Bars(source.Symbol, TScaleToWLScale(source), source.Interval);

            foreach (var bar in source.Bars)
            {
                bars.Add(bar.Date, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
            }

            DataSeries indicator = NRTR_WATR.Series(bars, AtrPeriod, Multiplier);

            IList<double> result = new List<double>();

            for (int bar = 0; bar < indicator.Count; bar++)
                result.Add(indicator[bar]);
            return result;           
        }

        public static BarScale TScaleToWLScale(ISecurity source)
        {
            switch (source.IntervalBase)
            {
                case TSLab.DataSource.DataIntervals.DAYS:
                    return BarScale.Daily;
                    
                case TSLab.DataSource.DataIntervals.MINUTE:
                    return BarScale.Minute;
                    
                case TSLab.DataSource.DataIntervals.SECONDS:
                    return BarScale.Second;
                    
                case TSLab.DataSource.DataIntervals.TICK:
                    return BarScale.Tick;                    
                default:
                    return BarScale.Minute;
                    
            }
        }      
    }
}
