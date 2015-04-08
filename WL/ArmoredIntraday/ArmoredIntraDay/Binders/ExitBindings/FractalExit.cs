using ArmoredIntradaySpace.Binders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmoredIntraDay.Binders.ExitBindings
{
    public class FractalExit : AExitStrategy
    {
        public DataSeries fractalDownSeries;
        public DataSeries fractalUpSeries;

        public FractalExit(WealthScript strategyInstance, bool isTrend)
            : base(strategyInstance, isTrend)
        {
            fractalDownSeries = Community.Indicators.FractalDown.Series(si.Bars.Close);
            fractalUpSeries = Community.Indicators.FractalUp.Series(si.Bars.Close);
            si.PlotSeries(si.PricePane, fractalDownSeries, Color.Red, LineStyle.Dotted, 1);
            si.PlotSeries(si.PricePane, fractalUpSeries, Color.Green, LineStyle.Dotted, 1);
        }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        {
            if (position.PositionType == PositionType.Long)
                position.AutoProfitLevel = fractalUpSeries[bar];
            else
                position.AutoProfitLevel = fractalDownSeries[bar];
        }

        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        {
            
            if (position.PositionType == PositionType.Long)
            {
                if (fractalUpSeries[bar] < position.EntryPrice)
                    position.AutoProfitLevel = Math.Min(position.AutoProfitLevel, fractalUpSeries[bar]);
            }
            else
                if (fractalUpSeries[bar] > position.EntryPrice)
                    position.AutoProfitLevel = Math.Max(position.AutoProfitLevel, fractalDownSeries[bar]);
        }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            bool result = false;           
            result = si.ExitAtLimit(bar + 1, position, position.AutoProfitLevel, "T/P"); // если не вышли, то попробовать выйти по T/P 
            return result;         
        }
    }
}
