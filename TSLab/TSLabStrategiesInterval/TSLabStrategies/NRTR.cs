using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers; // для работы с индикаторими и обработчиками
using TSLab.Script.Helpers; // помошники
using TSLab.Script.Optimization; // для оптимизации
using MMG2015.TSLab.Scripts;


namespace TSLabStrategies
{
    public class NRTR : IExternalScript
    {
        public IPosition LastActivePosition = null;

        public OptimProperty period;
        public OptimProperty multiplier;
        public OptimProperty PercentOEquity;

        private WLIndicators.NRTR_WATR_Indicator NRTRWATR1_h = new WLIndicators.NRTR_WATR_Indicator();

        public NRTR()
        {
            period = new OptimProperty(400, 50, 800, 10);
            multiplier = new OptimProperty(2, 0.5, 8, 0.1);
            PercentOEquity = new OptimProperty(30, 5, 50, 5);
        }

        public void Execute(IContext ctx, ISecurity sec)
        {
            int firstValidValue = 0;
            this.NRTRWATR1_h.AtrPeriod = period;
            this.NRTRWATR1_h.Multiplier = multiplier;
            // Make 'NRTRWATR1' item data
            IList<double> NRTR = ctx.GetData("NRTR_WATR", new string[] {
                this.NRTRWATR1_h.AtrPeriod.ToString(), 
                this.NRTRWATR1_h.Multiplier.ToString(), 
                "Источник1"
            }, delegate
            {
                try
                {
                    return this.NRTRWATR1_h.Execute(sec);
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    throw new TSLab.Script.ScriptException("Ошибка при вычислении блока \'NRTRWATR1\'. Индекс за пределам диапазона.");
                }

            });

            firstValidValue = Math.Max(firstValidValue, period);           
            //bool signalBuy = false; bool signalShort = false;

            for (int bar = 0; bar < sec.Bars.Count; bar++)
            {
                // signalBuy = compressedSec.Bars[bar].High > highLevelSeries2[bar];
                // signalShort = compressedSec.Bars[bar].Low < lowLevelSeries2[bar];
                if (bar > firstValidValue)
                {
                    int shares = Math.Max(1, sec.PercentOfEquityShares(bar, sec.CurrentBalance(bar) * PercentOEquity.Value / 100));

                    var positions = sec.Positions.GetActiveForBar(bar);
                    foreach (var position in positions)
                    {
                        if (position.IsLong)
                        {

                            if (NRTR[bar] > sec.Bars[bar].Close)
                            {
                                position.CloseAtMarket(bar + 1, position.EntrySignalName + "_LongExit");
                                sec.Positions.SellAtMarket(bar + 1, shares, "Sell");
                            }
                        }
                        else
                        {
                            if (NRTR[bar] < sec.Bars[bar].Close)
                            {
                                position.CloseAtMarket(bar + 1, position.EntrySignalName + "_ShortExit");
                                sec.Positions.BuyAtMarket(bar + 1, shares, "Buy");
                            }
                        }
                    }

                    if (LastActivePosition == null)
                    {
                        if (NRTR[bar] > sec.Bars[bar].Close)
                        {                            
                            sec.Positions.SellAtMarket(bar + 1, shares, "Sell");
                        }

                        if (NRTR[bar] < sec.Bars[bar].Close)
                        {                         
                            sec.Positions.BuyAtMarket(bar + 1, shares, "Buy");
                        }
                    }

                }
            }

            IPane pricePane = ctx.First;
            pricePane.AddList("sec", sec, CandleStyles.BAR_CANDLE, true, true, true, true, 0x0000a0, PaneSides.RIGHT);
            // Отрисовка PC
            pricePane.AddList("NRTR", NRTR, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
        }
    }
}
