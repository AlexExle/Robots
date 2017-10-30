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
using TSLab.DataSource;

namespace TSLabStrategies
{
    public class MiMaCrossoverMMAChannel : IExternalScript
    {
        const int C_BLACK = 0x000000;
        const int C_RED = 0xFF0000;

        public OptimProperty fastPeriod;
        public OptimProperty slowPeriod;        
        public OptimProperty PercentOEquity;
        public OptimProperty StopPercent;

        public double stopPrice = 0;

        public MiMaCrossoverMMAChannel()
        {
            fastPeriod = new OptimProperty(50, 1, 100, 1);
            slowPeriod = new OptimProperty(10, 1, 100, 1);
            PercentOEquity = new OptimProperty(30, 5, 50, 5);
            StopPercent = new OptimProperty(20, 10, 80, 10);
        }

        public void Execute(IContext ctx, ISecurity sec)
        {
            IList<double> fastEma = ctx.GetData("EMA", new[] { fastPeriod.ToString() }, () => Series.EMA(sec.ClosePrices, fastPeriod));
            IList<double> slowEma = ctx.GetData("EMA2", new[] { slowPeriod.ToString() }, () => Series.EMA(sec.ClosePrices, slowPeriod));
            CacheObjectMaker<IList<double>> gistGenerator = CreateGist(fastEma, slowEma);
            IList<double> gist = ctx.GetData("GIST", new[] { fastPeriod.ToString(), slowPeriod.ToString() }, gistGenerator);
            Func<int, int, int> getBigger = GetBigger();
            Func<double, double, bool> sameSign = SameSign();
            Func<int, int, double, IList<double>, IList<double>> setValueForRange = SetValueFor();

            var firstValidValue = getBigger(fastPeriod, slowPeriod);
            CacheObjectMaker<IList<double>> upChannelGenerator = UpChannelGen(sec, gist, WasOverturn, setValueForRange, firstValidValue);
            CacheObjectMaker<IList<double>> lowChannelGenerator = LowChannelGen(sec, gist, WasOverturn, setValueForRange, firstValidValue);

            IList<double> upChannel = ctx.GetData("UP", new[] { fastPeriod.ToString(), slowPeriod.ToString() }, upChannelGenerator);
            IList<double> lowChannel = ctx.GetData("LOW2", new[] { fastPeriod.ToString(), slowPeriod.ToString() }, lowChannelGenerator);

            // Берем основную панель (Pane)
            IPane pricePane = ctx.First;
            var gistPane = ctx.CreatePane("Gist", 70D, true);

            // Отрисовка PC
            for (int bar = firstValidValue; bar < sec.Bars.Count; bar++)
            {

            }

            pricePane.AddList("Fast EMA", fastEma, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
            pricePane.AddList("Slow EMA", slowEma, ListStyles.LINE, 0xFF00a0, LineStyles.DASH_DOT, PaneSides.RIGHT);
            pricePane.AddList("Low Channel", lowChannel, ListStyles.LINE, C_RED, LineStyles.DOT, PaneSides.RIGHT);
            pricePane.AddList("High Channel", upChannel, ListStyles.LINE, C_BLACK, LineStyles.DOT, PaneSides.RIGHT);
            gistPane.AddList("Gist", gist, ListStyles.HISTOHRAM, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);

        }

        public CacheObjectMaker<IList<double>> LowChannelGen(ISecurity sec, IList<double> gist, Func<bool, IList<double>, int, bool> wasOwerturn, Func<int, int, double, IList<double>, IList<double>> setValueForRange, int firstValidValue)
        {
            CacheObjectMaker<IList<double>> lowChannelGenerator = () =>
            {
                double localMin = sec.Bars[0].Low;
                double prev = 0;
                int kandle = 0;
                int prevRevert = 0;
                var newVals = new List<double>();
                for (int i = 0; i < sec.Bars.Count; i++)
                {

                    newVals.Add(sec.Bars[i].Open);
                                        
                    if (gist[i] < 0)
                    {
                        if (localMin > sec.Bars[i].Low)
                        {
                            localMin = sec.Bars[i].Low;
                            kandle = i;                            
                        }
                     
                    }
                    if (prev != 0)
                    {
                        newVals[i] = prev;
                    }                    
                    if (i > firstValidValue && wasOwerturn(true, gist, i))
                    {
                     
                        if (prevRevert != 0)
                        {
                            prev = localMin;
                            localMin = sec.Bars[i].Low;
                        }
                        prevRevert = i;                        
                    }
                    
                }
                return newVals;
            };
            return lowChannelGenerator;
        }

        public CacheObjectMaker<IList<double>> UpChannelGen(ISecurity sec, IList<double> gist, Func<bool, IList<double>, int, bool> wasOwerturn, Func<int, int, double, IList<double>, IList<double>> setValueForRange, int firstValidValue)
        {
            CacheObjectMaker<IList<double>> upChannelGenerator = () =>
            {
                double localHigh = sec.Bars[0].High;
                double prev = 0;
                int kandle = 0;
                int prevRevert = 0;
                var newVals = new List<double>();
                for (int i = 0; i < sec.Bars.Count; i++)
                {

                    newVals.Add(sec.Bars[i].High);
                    if (gist[i] > 0)
                    {
                        if (localHigh < sec.Bars[i].High)
                        {
                            localHigh = sec.Bars[i].High;
                            kandle = i;
                        }
                      
                    }
                    if (prev != 0)
                    {
                        newVals[i] = prev;
                    }                  
                    if (i > firstValidValue && i > 0 && wasOwerturn(false, gist, i))
                    {
                        if (prevRevert != 0)
                        {
                            prev = localHigh;
                            localHigh = sec.Bars[i].High;
                        }
                        prevRevert = i;                        
                    }
                }
                return newVals;
            };
            return upChannelGenerator;
        }

        public Func<int, int, double, IList<double>, IList<double>> SetValueFor()
        {
            return (from, to, value, list) =>
            {
                for (int i = from; i <= to; i++)
                {
                    if (list.Count > i)
                        list[i] = value;
                    else
                        list.Add(value);
                }
                return list;
            };
        }

        public Func<double, double, bool> SameSign()
        {
            return (d1, d2) =>
            {
                return (d1 > 0 && d2 > 0) || (d1 < 0 && d2 < 0);
            };
        }

        public bool WasOverturn(bool direction, IList<double> gist, int i)
        {
            return (direction && gist[i] > 0 && gist[i - 1] < 0) || (!direction && gist[i] < 0 && gist[i - 1] > 0);
        }

        public Func<int, int, int> GetBigger()
        {
            return (d1, d2) =>
            {
                return d1 > d2 ? d1 : d2;
            };
        }

        public CacheObjectMaker<IList<double>> CreateGist(IList<double> fastEma, IList<double> slowEma)
        {
            return () =>
            {
                var newVals = new List<double>();
                for (int i = 0; i < fastEma.Count; i++)
                {
                    newVals.Add(fastEma[i] - slowEma[i]);
                }
                return newVals;
            };
        }
    }
}
