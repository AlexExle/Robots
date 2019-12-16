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
        private MIN minIndicator;        
        private EMA fastEma;
        private EMA slowEma;
        private double basePrice;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "ATR Канал, минимум по пересечению EMA умноженный на ATR";
                Name = "ATR Channel";
                Period = 400;
                Multiplier = 2;
                SlowMA = 23;
                FastMA = 45;
                MinimumPeriod = 3;

                IsSuspendedWhileInactive = true;
                //AddPlot(Brushes.DarkCyan, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameATR);
				AddPlot(Brushes.Red, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
                AddPlot(Brushes.Blue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
            
                
            }
           
            else if (State == State.DataLoaded)
            {
                atr = ATR(Period);
                minIndicator = MIN(MinimumPeriod);
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
                basePrice = input0;
            }
            else
            {
               if (CrossAbove(fastEma,slowEma,1))
               {
                    basePrice = minIndicator[0];
               }
               HighBorder[0] =((double) Math.Round((basePrice + (atr[0] * Multiplier * 2)) / 10d, 3) * 10);
               LowBorder[0] = ((double) Math.Round((basePrice - (atr[0] * Multiplier * 2)) / 10d, 3) * 10);                
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

        [Range(0, 10), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Period for MIN", GroupName = "NinjaScriptParameters", Order = 4)]
        public int MinimumPeriod
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
		public AtrChannel AtrChannel(int period, double multiplier, int slowMA, int fastMA, int minimumPeriod)
		{
			return AtrChannel(Input, period, multiplier, slowMA, fastMA, minimumPeriod);
		}

		public AtrChannel AtrChannel(ISeries<double> input, int period, double multiplier, int slowMA, int fastMA, int minimumPeriod)
		{
			if (cacheAtrChannel != null)
				for (int idx = 0; idx < cacheAtrChannel.Length; idx++)
					if (cacheAtrChannel[idx] != null && cacheAtrChannel[idx].Period == period && cacheAtrChannel[idx].Multiplier == multiplier && cacheAtrChannel[idx].SlowMA == slowMA && cacheAtrChannel[idx].FastMA == fastMA && cacheAtrChannel[idx].MinimumPeriod == minimumPeriod && cacheAtrChannel[idx].EqualsInput(input))
						return cacheAtrChannel[idx];
			return CacheIndicator<AtrChannel>(new AtrChannel(){ Period = period, Multiplier = multiplier, SlowMA = slowMA, FastMA = fastMA, MinimumPeriod = minimumPeriod }, input, ref cacheAtrChannel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AtrChannel AtrChannel(int period, double multiplier, int slowMA, int fastMA, int minimumPeriod)
		{
			return indicator.AtrChannel(Input, period, multiplier, slowMA, fastMA, minimumPeriod);
		}

		public Indicators.AtrChannel AtrChannel(ISeries<double> input , int period, double multiplier, int slowMA, int fastMA, int minimumPeriod)
		{
			return indicator.AtrChannel(input, period, multiplier, slowMA, fastMA, minimumPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AtrChannel AtrChannel(int period, double multiplier, int slowMA, int fastMA, int minimumPeriod)
		{
			return indicator.AtrChannel(Input, period, multiplier, slowMA, fastMA, minimumPeriod);
		}

		public Indicators.AtrChannel AtrChannel(ISeries<double> input , int period, double multiplier, int slowMA, int fastMA, int minimumPeriod)
		{
			return indicator.AtrChannel(input, period, multiplier, slowMA, fastMA, minimumPeriod);
		}
	}
}

#endregion
