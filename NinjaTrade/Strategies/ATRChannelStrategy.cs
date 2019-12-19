using NinjaTrader.NinjaScript.Indicators;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;


//This namespace holds strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
    public class ATRChannelStrategy : Strategy
    {
        private AtrChannel ChannelIndicator;
        

        protected override void OnStateChange()
        {
            switch (State)
            {
                case State.SetDefaults:
                    break;
                case State.Configure:
                    break;
                case State.Active:
                    break;
                case State.DataLoaded:
                    break;
                case State.Historical:
                    break;
                case State.Transition:
                    break;
                case State.Realtime:
                    break;
                case State.Terminated:
                    break;
                case State.Finalized:
                    break;
                default:
                    break;
            }

            if (State == State.SetDefaults)
            {

                Period = 400;
                Multiplier = 1.6;
                SlowMA = 20;
                FastMA = 5;
                EquityPercent = 5;
                // This strategy has been designed to take advantage of performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = false;
            }
            else if (State == State.DataLoaded)
            {
                ChannelIndicator = AtrChannel(Period, Multiplier, SlowMA, FastMA);
                
                ChannelIndicator.Plots[0].Brush = Brushes.Goldenrod;
                ChannelIndicator.Plots[1].Brush = Brushes.Red;

                AddChartIndicator(ChannelIndicator);                
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            if (CrossAbove(High, ChannelIndicator.HighBorder, 1))
            {
                ExitShort();
                EnterLong(quantity: CalcPosition(EquityPercent/100));
            }
            else if (CrossBelow(Low, ChannelIndicator.LowBorder, 1))
            {
                ExitLong();
                EnterShort(quantity: CalcPosition(EquityPercent/100));
            }
        }
        
        protected int CalcPosition(double percent)
        {
            double instrumentPrice = Open[0];
            var cash = Account.Get(Cbi.AccountItem.CashValue, Cbi.Currency.UsDollar);
            var position = (cash * percent) / instrumentPrice;
            return (int)System.Math.Floor(position);
        }


        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "ATR Period", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
        public int Period
        { get; set; }

        [Range(0, 10d), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Channel Multiplier", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
        public double Multiplier
        { get; set; }

        [Range(0, 100), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SlowEMA Period", GroupName = "NinjaScriptStrategyParameters", Order = 2)]
        public int SlowMA
        { get; set; }

        [Range(0, 100), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "FastEMA Period", GroupName = "NinjaScriptStrategyParameters", Order = 3)]
        public int FastMA
        { get; set; }

        [Range(0, 10), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Period for MIN", GroupName = "NinjaScriptStrategyParameters", Order = 4)]
        public int MinimumPeriod
        { get; set; }

        [Range(1, 100), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Процент от счета на торговлю", GroupName = "NinjaScriptStrategyParameters", Order = 5)]
        public int EquityPercent
        { get; set; }
    }
}
