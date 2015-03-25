   using ArmorediIntraday;
using ArmorediIntraday.Binders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;

namespace ArmoredIntraDay.Binders.ExitBindings
{
    public class OnlyProfit : AExitStrategy
    {
        public OnlyProfit(WealthScript strategyInstance, bool isTrend)
        {
            var atrPeriod = (int)Math.Round( (strategyInstance as ArmoredIntraday)._exitParameter.Value * 30);
            StrategyInstance = strategyInstance;
            this.isTrend = isTrend;
            atr = ATR.Series(StrategyInstance.Bars, atrPeriod);
            ChartPane atrPane = StrategyInstance.CreatePane(20, false, true);
            StrategyInstance.PlotSeries(atrPane, atr, Color.Red, LineStyle.Solid, 1);
            //Костыль номер 2. как-то не хорошо что стратегия на выход лезет в кишки стратегии на вход. видимо место этой переменной в абстракции более высокого уровня. Например в самом классе CourseBinders64
            AEnterStrategy.firstValidValue = Math.Max(AEnterStrategy.firstValidValue, atr.FirstValidValue); // Пе
        }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        {
            if (position.PositionType == PositionType.Long && isTrend) // Если торгуем длинную позицию по тренду
            {
                position.RiskStopLevel = StrategyInstance.Bars.Close[bar] - atr[bar] * 3;
                position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] + ProfitRisk * atr[bar] * 3;
            }
            else if (position.PositionType == PositionType.Short && isTrend) // Если торгуем короткую позицию по тренду
            {
                position.RiskStopLevel = StrategyInstance.Bars.Close[bar] + atr[bar] * 3;
                position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] - ProfitRisk * atr[bar] * 3;
            }
            else if (position.PositionType == PositionType.Long && !isTrend) // Если торгуем длинную позицию против тренда
            {
                position.RiskStopLevel = StrategyInstance.Bars.Close[bar] - ProfitRisk * atr[bar] * 3;
                position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] + atr[bar] * 3;
            }
            else // Если торгуем короткую позицию против тренда
            {
                position.RiskStopLevel = StrategyInstance.Bars.Close[bar] + ProfitRisk * atr[bar] * 3;
                position.AutoProfitLevel = StrategyInstance.Bars.Close[bar] - atr[bar] * 3;
            }
        }

        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        { }

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            bool result = false;
            if (!result)
                result = StrategyInstance.ExitAtLimit(bar + 1, position, position.AutoProfitLevel, "T/P"); // если не вышли, то попробовать выйти по T/P 
            return result;
        }
    }
}
