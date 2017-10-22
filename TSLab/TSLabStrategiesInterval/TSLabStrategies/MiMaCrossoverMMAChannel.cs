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
            CacheObjectMaker<IList<double>> gistGenerator = () =>
                    {
                        var newVals = new List<double>();
                        for (int i = 0; i < fastEma.Count; i++)
                        {
                            newVals.Add(fastEma[i] - slowEma[i]);
                        }
                        return newVals;
                    };
            IList<double> gist = ctx.GetData("GIST", new[] { fastPeriod.ToString(), slowPeriod.ToString() }, gistGenerator);

            Func<int, int, int> getBigger = (d1, d2) =>
            {
                return d1 > d2 ? d1 : d2;
            };

            var firstValidValue = getBigger(fastPeriod, slowPeriod);

            CacheObjectMaker<IList<double>> upChannelGenerator = () =>
            {

                var newVals = new List<double>();
                for (int i = 0; i < fastEma.Count; i++)
                {
                    if (i < firstValidValue)
                        newVals.Add(sec.Bars[i].Close);
                    else
                    {

                    }

                }
                return newVals;
            };

            IList<double> upChannel = ctx.GetData("GIST", new[] { fastPeriod.ToString(), slowPeriod.ToString() }, gistGenerator);



            // Берем основную панель (Pane)
            IPane pricePane = ctx.First;
            var gistPane = ctx.CreatePane("Gist", 70D, true);

            // Отрисовка PC



            for (int bar = firstValidValue; bar < sec.Bars.Count; bar++)
            {

            }

            pricePane.AddList("Fast EMA", fastEma, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
            pricePane.AddList("Slow EMA", slowEma, ListStyles.LINE, 0xFF00a0, LineStyles.DASH_DOT, PaneSides.RIGHT);
            gistPane.AddList("Gist", gist, ListStyles.HISTOHRAM, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);

        }
    }
}
