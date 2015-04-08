   using ArmoredIntradaySpace;
using ArmoredIntradaySpace.Binders;
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
            si = strategyInstance;
            this.isTrend = isTrend;
            atr = ATR.Series(si.Bars, atrPeriod);
            ChartPane atrPane = si.CreatePane(20, false, true);
            si.PlotSeries(atrPane, atr, Color.Red, LineStyle.Solid, 1);
            //Костыль номер 2. как-то не хорошо что стратегия на выход лезет в кишки стратегии на вход. видимо место этой переменной в абстракции более высокого уровня. Например в самом классе CourseBinders64
            AEnterStrategy.firstValidValue = Math.Max(AEnterStrategy.firstValidValue, atr.FirstValidValue); // Пе
        }

        public override void InitExitConditions(WealthLab.Position position, int bar)
        {
            if (position.PositionType == PositionType.Long && isTrend) // Если торгуем длинную позицию по тренду
            {
                position.AutoProfitLevel = position.EntryPrice + atr[bar];
            }
            else if (position.PositionType == PositionType.Short && isTrend) // Если торгуем короткую позицию по тренду
            {
                position.AutoProfitLevel = position.EntryPrice - atr[bar];
            }
            else if (position.PositionType == PositionType.Long && !isTrend) // Если торгуем длинную позицию против тренда
            {
                position.AutoProfitLevel = position.EntryPrice + atr[bar];
            }
            else // Если торгуем короткую позицию против тренда
            {
       
                position.AutoProfitLevel = position.EntryPrice - atr[bar];
            }
        }

  
        public override void RecalculateExitConditions(WealthLab.Position position, int bar)
        {
            double barsHeldCounter = Math.Floor((double)((bar - position.EntryBar) / 5));

            if (barsHeldCounter < 5)
            {
                if (position.PositionType == PositionType.Long)
                {

                    position.AutoProfitLevel = position.AutoProfitLevel = position.EntryPrice + atr[position.EntryBar] - (barsHeldCounter * 20);
                }
                else
                    position.AutoProfitLevel = position.AutoProfitLevel = position.EntryPrice - atr[position.EntryBar] + (barsHeldCounter * 20);
                
            }
        }
        

        public override bool TryExit(WealthLab.Position position, int bar, EnterSignalType lastEnterSignal)
        {
            bool result = false;
            if (!result)
                result = si.ExitAtLimit(bar + 1, position, position.AutoProfitLevel, "T/P"); // если не вышли, то попробовать выйти по T/P 
            return result;
        }
    }
}
