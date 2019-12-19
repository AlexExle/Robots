using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class AtrChannel : Indicator
    {
        private ATR atr;
        private EMA fastEma;
        private EMA slowEma;
        private double basePrice;
        private double minForCross;
        private double maxForCross;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "ATR Канал, минимум по пересечению EMA умноженный на ATR";
                Name = "ATR Channel";
                Period = 400;
                Multiplier = 1.6;
                SlowMA = 20;
                FastMA = 5;

                IsSuspendedWhileInactive = true;
                //AddPlot(Brushes.DarkCyan, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameATR);
				AddPlot(Brushes.Red, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
                AddPlot(Brushes.Blue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
                AddPlot(Brushes.Chocolate, "Center");

            }
           
            else if (State == State.DataLoaded)
            {
                atr = ATR(Period);
                fastEma = EMA(FastMA);
                slowEma = EMA(SlowMA);
                basePrice = 0;
            }
        }

        protected override void OnBarUpdate()
        {
            double input0 = Input[0];

            if (CurrentBar == 0)
            {                
                HighBorder[0] = input0;
                LowBorder[0] = input0;
                Center[0] = input0;
                basePrice = input0;
                minForCross = Low[0];
                maxForCross = High[0];
            }
            else
            {
               if (CrossAbove(fastEma,slowEma,1))
               {
                    basePrice = minForCross;
                    maxForCross = High[0];
               }

               if (CrossBelow(fastEma, slowEma, 1))
               {
                   minForCross = Low[0]; 
               }

               if (fastEma[0] > slowEma[0] && maxForCross < High[0])
               {
                   maxForCross = High[0];
               }

               if (fastEma[0] < slowEma[0] && minForCross > Low[0])
               {
                   minForCross = Low[0];
               }

               Center[0] = basePrice;
               HighBorder[0] =((double) Math.Round((basePrice + (atr[0] * Multiplier * 2)) / 10d, 1) * 10);
               LowBorder[0] = ((double) Math.Round((basePrice - (atr[0] * Multiplier * 2)) / 10d, 1) * 10);                
            }
        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> HighBorder
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LowBorder
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Center
        {
            get { return Values[2]; }
        }



        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "ATR Period", GroupName = "NinjaScriptParameters", Order = 0)]
        public int Period
        { get; set; }

        [Range(0, 10d), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Channel Multiplier", GroupName = "NinjaScriptParameters", Order = 1)]
        public double Multiplier
        { get; set; }

        [Range(0, 100), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SlowEMA Period", GroupName = "NinjaScriptParameters", Order = 2)]
        public int SlowMA
        { get; set; }

        [Range(0, 100), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "FastEMA Period", GroupName = "NinjaScriptParameters", Order = 3)]
        public int FastMA
        { get; set; }

        #endregion


    }

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AtrChannel[] cacheAtrChannel;
		public AtrChannel AtrChannel(int period, double multiplier, int slowMA, int fastMA)
		{
			return AtrChannel(Input, period, multiplier, slowMA, fastMA);
		}

		public AtrChannel AtrChannel(ISeries<double> input, int period, double multiplier, int slowMA, int fastMA)
		{
			if (cacheAtrChannel != null)
				for (int idx = 0; idx < cacheAtrChannel.Length; idx++)
					if (cacheAtrChannel[idx] != null && cacheAtrChannel[idx].Period == period && cacheAtrChannel[idx].Multiplier == multiplier && cacheAtrChannel[idx].SlowMA == slowMA && cacheAtrChannel[idx].FastMA == fastMA && cacheAtrChannel[idx].EqualsInput(input))
						return cacheAtrChannel[idx];
			return CacheIndicator<AtrChannel>(new AtrChannel(){ Period = period, Multiplier = multiplier, SlowMA = slowMA, FastMA = fastMA }, input, ref cacheAtrChannel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AtrChannel AtrChannel(int period, double multiplier, int slowMA, int fastMA)
		{
			return indicator.AtrChannel(Input, period, multiplier, slowMA, fastMA);
		}

		public Indicators.AtrChannel AtrChannel(ISeries<double> input , int period, double multiplier, int slowMA, int fastMA)
		{
			return indicator.AtrChannel(input, period, multiplier, slowMA, fastMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AtrChannel AtrChannel(int period, double multiplier, int slowMA, int fastMA)
		{
			return indicator.AtrChannel(Input, period, multiplier, slowMA, fastMA);
		}

		public Indicators.AtrChannel AtrChannel(ISeries<double> input , int period, double multiplier, int slowMA, int fastMA)
		{
			return indicator.AtrChannel(input, period, multiplier, slowMA, fastMA);
		}
	}
}

#endregion
