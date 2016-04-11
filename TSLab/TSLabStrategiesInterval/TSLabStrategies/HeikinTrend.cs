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
        /// <summary>
        /// Short signal
        /// </summary>
        const string S_S = "Short_";
        /// <summary>
        /// Long signal
        /// </summary>
        const string L_S = "Long_";
        /// <summary>
        /// Exit postfix
        /// </summary>
        const string E_S = "EXIT";
        public OptimProperty PercentOEquity = new OptimProperty(30, 5, 50, 5);
        public OptimProperty RenderMode = new OptimProperty(1, 1, 3, 1);
      
        public IPosition shortPos;
        public IPosition longPos;

        public void Execute(IContext ctx, ISecurity sec)
        {

            ISecurity candles;
            switch ((int)RenderMode)
            {
                case 1:
                    {
                        candles = new HeikenAshi().Execute(sec); 
                    }
                    break;
                case 2:
                    {
                        candles = new HeikenAshiTSLab().Execute(sec);
                    }
                    break;
                case 3:
                    {
                        candles = new LevykinHeikenAshi().Execute(sec);
                    }
                    break;
                default:
                    candles = new HeikenAshi().Execute(sec); 
                    break;  
            }
                 
            
            for (int bar = 1; bar < sec.Bars.Count; bar++)
            {
                GenerateSignal(sec, candles, bar);
            }
            if (!ctx.IsOptimization)
            {
                // Make 'ПанельГрафика1' pane
                TSLab.Script.IPane indicatorPane = ctx.CreatePane("ПанельГрафика1", 100D, false);
                indicatorPane.Visible = true;
                indicatorPane.AddList("Heiken Ashi", candles, CandleStyles.BAR_CANDLE, false, new Color(3, 3, 3), PaneSides.LEFT);
            }
            
           /* else
            {
                int currentBar = sec.Bars.Count - 1;
                GenerateSignal(sec, candles, currentBar);
                // Make 'ПанельГрафика1' pane
                TSLab.Script.IPane indicatorPane = ctx.CreatePane("ПанельГрафика1", 100D, false);
                indicatorPane.Visible = true;
                indicatorPane.AddList("Heiken Ashi", candles, CandleStyles.BAR_CANDLE, false, new Color(3, 3, 3), PaneSides.LEFT);
            }*/                    
        }

        private void GenerateSignal(ISecurity sec, ISecurity candles, int currentBar)
        {

            if (candles.Bars[currentBar].Date.Hour < 18)
            {
                // предыдущая свеча : close больше open
                bool SignalBuy = candles.Bars[currentBar - 1].Close > candles.Bars[currentBar - 1].Open;
                //open текущей меньше open предыдущей
                bool SignalSell = candles.Bars[currentBar].Open < candles.Bars[currentBar - 1].Open;
                //текущий high = open (нет тени сверху) И предылущий close < open
                bool SignalShort = candles.Bars[currentBar].Open == candles.Bars[currentBar].High && candles.Bars[currentBar - 1].Close < candles.Bars[currentBar - 1].Open;
                // предыдущая свеча : close больше open
                bool SignalCover = candles.Bars[currentBar - 1].Close > candles.Bars[currentBar - 1].Open;
                shortPos = sec.Positions.GetLastActiveForSignal(S_S);
                longPos = sec.Positions.GetLastActiveForSignal(L_S);
                if (SignalBuy && longPos == null)
                {
                    int shares = Math.Max(1, sec.PercentOfEquityShares(currentBar, sec.CurrentBalance(currentBar) * PercentOEquity.Value / 100));
                    sec.Positions.BuyAtMarket(currentBar + 1, shares, L_S);
                }
                if (SignalSell && longPos != null)
                {
                    longPos.CloseAtMarket(currentBar + 1, L_S + E_S);
                    //выход из лонга
                }
                if (SignalShort && shortPos == null)
                {
                    int shares = Math.Max(1, sec.PercentOfEquityShares(currentBar, sec.CurrentBalance(currentBar) * PercentOEquity.Value / 100));
                    sec.Positions.SellAtMarket(currentBar + 1, shares, S_S);
                    //вход на шорт
                }
                if (SignalCover && shortPos != null)
                {
                    shortPos.CloseAtMarket(currentBar + 1, S_S + E_S);
                    //выход из шорта
                }
            }
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

    public class HeikenAshiTSLab : IBar2BarHandler
    {
        public ISecurity Execute(ISecurity source)
        {
            var Bars = new List<Bar>(source.Bars.Count);
            var C = source.ClosePrices;
            var H = source.HighPrices;
            var L = source.LowPrices;
            var O = source.OpenPrices;
            IList<Bar> HA = new List<Bar>(Bars.Count);
            IList<double> Cn = new List<double>(C.Count);
            IList<double> Hn = new List<double>(C.Count);
            IList<double> Ln = new List<double>(C.Count);
            IList<double> On = new List<double>(C.Count);
            double Open, High, Low, Close;

            for (int i = 0; i < C.Count; i++)
            {
                if (i < 1)
                {
                    Open = 0;
                    Close = 0;
                    Low = 0;
                    High = 0;
                }
                else
                {
                    Open = (O[i - 1] + C[i - 1]) / 2;
                    Close = (C[i] + O[i] + L[i] + H[i]) / 4;
                    Low = Math.Min(L[i], Math.Min(Open, Close));
                    High = Math.Max(H[i], Math.Max(Open, Close));
                }
                On.Add(Open);
                Cn.Add(Close);
                Ln.Add(Low);
                Hn.Add(High);
            }

            int j = 0;
            foreach (var bar in source.Bars)
            {
                var hav = new Bar(bar.Color, bar.Date,
                                  On[j],
                                  Hn[j],
                                  Ln[j],
                                  Cn[j],
                                  bar.Volume);
                HA.Add(hav);
                j++;
            }

            return source.CloneAndReplaceBars(HA);
        }
        public IContext Context { get; set; }
    }

    public class LevykinHeikenAshi : IBar2BarHandler
    {

        public ISecurity Execute(ISecurity source)
        {
            var Bars = new List<Bar>(source.Bars.Count);
            var C = source.ClosePrices;
            var H = source.HighPrices;
            var L = source.LowPrices;
            var O = source.OpenPrices;
            IList<Bar> HA = new List<Bar>(Bars.Count);
            IList<double> Cn = new List<double>(C.Count);
            IList<double> Hn = new List<double>(C.Count);
            IList<double> Ln = new List<double>(C.Count);
            IList<double> On = new List<double>(C.Count);
            
            //ctx.GetData(

            for (int i = 0; i < C.Count; i++)
            {
                Cn[i] = (O[i] + H[i] + L[i] + C[i]) / 4;
            }
            
            var ema = new EMA();
            ema.Period = 3;
            Cn = ema.Execute(Cn);
            var ama = new AMA();
            ama.Period = 1;
            List<double> list = new List<double>();
            for (int i = 0; i < Cn.Count; i++)
            {
                On.Add(i == 0 ? 0 : Cn[i-1]);
            }
            On = ama.Execute(On);
            for (int i = 0; i < Cn.Count; i++)
            {
               Hn.Add(Math.Max(Cn[i], On[i]));
               Ln.Add(Math.Min(Cn[i], On[i]));
            }           

            int j = 0;
            foreach (var bar in source.Bars)
            {
                var hav = new Bar(bar.Color, bar.Date,
                                  On[j],
                                  Hn[j],
                                  Ln[j],
                                  Cn[j],
                                  bar.Volume);
                HA.Add(hav);
                j++;
            }

            return source.CloneAndReplaceBars(HA);
        }
        public IContext Context { get; set; }
    }

}
