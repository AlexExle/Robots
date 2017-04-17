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
    public class ATRChannelTake : IExternalScript
    {
        public IPosition LastActivePosition = null;

        public OptimProperty period;
        public OptimProperty multiplier;
        public OptimProperty takeMultiplier;
        public OptimProperty PercentOEquity;
        
        private string[] timesToUpdateLeveles = { "23:45" };
        public ATRChannelTake()
        {
            period = new OptimProperty(400, 50, 800, 10);
            takeMultiplier = new OptimProperty(2, 1, 20, 1);
            multiplier = new OptimProperty(2, 0.5, 4, 0.1);
            PercentOEquity = new OptimProperty(30, 5, 50, 5);        
        }

        public void Execute(IContext ctx, ISecurity sec)
        {
            List<DateTime> dates = new List<DateTime>();
            foreach(var i in timesToUpdateLeveles)
            {
	            dates.Add(DateTime.Parse(i));
            }

            int firstValidValue = 0;
          
            double calcPrice = 0;

            IList<double> atr = ctx.GetData("ATR", new[] { period.ToString() }, () => Series.AverageTrueRange(sec.Bars, period));

            firstValidValue = Math.Max(firstValidValue, period);
         
            IList<double> highLevelSeries2 = new List<double>();
            IList<double> lowLevelSeries2 = new List<double>();

            //bool signalBuy = false; bool signalShort = false;

            for (int bar = 0; bar < sec.Bars.Count; bar++)
            {               
                if (IsDateInArray(sec.Bars[bar].Date, dates))
                    {                        
                        calcPrice = sec.Bars[bar].Close;
                    }

                    highLevelSeries2.Add((double)Math.Round((calcPrice + (atr[bar] * multiplier.Value * 2)) / 10d, 0)*10);
                    lowLevelSeries2.Add((double)Math.Round((calcPrice - (atr[bar] * multiplier.Value * 2)) / 10d, 0)*10);

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
                                position.CloseAtProfit(bar + 1, position.EntryPrice + atr[position.EntryBarNum] * takeMultiplier.Value, position.EntrySignalName + "take_Long");
                                position.CloseAtStop(bar + 1, lowLevelSeries2[bar], position.EntrySignalName + "stop_Long");
                                if (positions.Count() < 2)
                                    sec.Positions.SellIfLess(bar + 1, shares, lowLevelSeries2[bar], "Sell");
                            }
                            else
                            {
                                position.CloseAtProfit(bar + 1, position.EntryPrice - atr[position.EntryBarNum] * takeMultiplier.Value, position.EntrySignalName + "take_Short");
                                position.CloseAtStop(bar + 1, highLevelSeries2[bar], position.EntrySignalName + "stop_Short");
                                if (positions.Count() < 2)
                                    sec.Positions.BuyIfGreater(bar + 1, shares, highLevelSeries2[bar], "Buy");
                            }
                        }                       
                        
                        if (LastActivePosition == null)
                        {
                            if (sec.Bars[bar - 1].Low < highLevelSeries2[bar])
                                sec.Positions.BuyIfGreater(bar + 1, shares, highLevelSeries2[bar], "Buy");

                            if (sec.Bars[bar - 1].High > lowLevelSeries2[bar])
                                sec.Positions.SellIfLess(bar + 1, shares, lowLevelSeries2[bar], "Sell");
                        }
                      
                    }
            }
		        
            IPane pricePane = ctx.First;
            pricePane.AddList("sec", sec, CandleStyles.BAR_CANDLE, true, true, true, true, 0x0000a0, PaneSides.RIGHT);
            // Отрисовка PC
            pricePane.AddList("High Channel", highLevelSeries2, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
            pricePane.AddList("Low Channel", lowLevelSeries2, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
         
		}

        protected bool IsDateInArray(DateTime date, List<DateTime> array)
        {
            foreach (var dt in array)
            {
                if (date.Minute == dt.Minute && date.Hour == dt.Hour)
                    return true;

            }
            return false;
        }

    }
}
