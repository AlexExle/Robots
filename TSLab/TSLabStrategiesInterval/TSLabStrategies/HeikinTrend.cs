using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers; // для работы с индикаторими и обработчиками
using TSLab.Script.Helpers; // помошники
using TSLab.Script.Optimization; // для оптимизации
using TSLab.Script.Realtime;
using MMG2015.TSLab.Scripts; 

namespace TSLabStrategies
{
    public class HeikinTrend : IExternalScript
    {

        public OptimProperty PercentOEquity = new OptimProperty(30, 5, 50, 5);
        public HeikenAshi indicator = new HeikenAshi();

        public void Execute(IContext ctx, ISecurity sec)
        {
            var candles = indicator.Execute(sec);

            if (ctx.IsOptimization)
            {
                return;
            }
            // Make 'ПанельГрафика1' pane
            TSLab.Script.IPane indicatorPane = ctx.CreatePane("ПанельГрафика1", 100D, false);
            indicatorPane.Visible = true;
            indicatorPane.AddList("Heiken Ashi", candles, CandleStyles.BAR_CANDLE, false, new Color(3, 3, 3), PaneSides.LEFT);
        }        
    }

    public class HeikenAshi : IBar2BarHandler
    {

        //  основная функция пересчёта
        public ISecurity Execute(ISecurity source)
        {
            var Bars = new List<Bar>(source.Bars.Count);
            var C = source.ClosePrices;
            var H = source.HighPrices;
            var L = source.LowPrices;
            var O = source.OpenPrices;
            IList<Bar> HA = new List<Bar>(Bars.Count);
            IList<double> haCn = new List<double>(C.Count);
            IList<double> haHn = new List<double>(C.Count);
            IList<double> haLn = new List<double>(C.Count);
            IList<double> haOn = new List<double>(C.Count);
            double haOpen, haHigh, haLow, haClose;

            for (int i = 0; i < C.Count; i++)
            {
                if (i < 2)
                {
                    haOpen = O[i];
                    haClose = C[i];
                    haLow = L[i];
                    haHigh = H[i];
                }
                else
                {
                    haClose = (C[i] + O[i] + L[i] + H[i]) / 4;
                    // вот тут неверно!!! нужно использовать haOpen и haClose !!!
                    // haOpen = (  O[i - 1]   +   C[i - 1]  ) / 2;
                    // должно быть как-то так
                    haOpen = (haOn[i - 1] + haCn[i - 1]) / 2;
                    haLow = Math.Min(L[i], Math.Min(haOpen, haClose));
                    haHigh = Math.Max(H[i], Math.Max(haOpen, haClose));
                }
                haOn.Add(haOpen);
                haCn.Add(haClose);
                haLn.Add(haLow);
                haHn.Add(haHigh);
            }

            int j = 0;
            foreach (var bar in source.Bars)
            {
                var hav = new Bar(bar.Color, bar.Date,
                                  haOn[j],
                                  haHn[j],
                                  haLn[j],
                                  haCn[j],
                                  bar.Volume);
                HA.Add(hav);
                j++;
            }

            return source.CloneAndReplaceBars(HA);
        }
        public IContext Context { get; set; }
    }
}
