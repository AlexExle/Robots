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

namespace TSLabStrategies
{
    public class ATRTrio : IExternalScript
    {
        public IPosition LastActivePosition = null;

        public OptimProperty EMAPeriod;
        public OptimProperty ProfitFactor;
        public OptimProperty StopFactor;
        public OptimProperty PercentOEquity;
        public OptimProperty ATRPeriod;

        public ATRTrio()
        {
            EMAPeriod = new OptimProperty(300, 100, 1000, 25);
            ATRPeriod = new OptimProperty(400, 50, 800, 10);
            ProfitFactor = new OptimProperty(8.5, 1, 10, 0.5);
            StopFactor = new OptimProperty(2, 1, 5, 0.5);
            PercentOEquity = new OptimProperty(30, 5, 50, 5);
        }

        public void Execute(IContext ctx, ISecurity sec)
        {
            int period = EMAPeriod;            
            int firstValidValue = 0; // Первое значение свечки при которой существуют все индикаторы
           
            IList<double> ema = ctx.GetData("EMA", new[] { period.ToString() }, () => Series.EMA(sec.ClosePrices, period));

            IList<double> atr = ctx.GetData("ATR", new[] { period.ToString() }, () => Series.AverageTrueRange(sec.Bars, ATRPeriod));        

            bool signalBuy = false, signalShort = false; // Сигналы на вход в длинную и короткую позиции
            double entryPrice = 0;
            double stopPrice = 0; // Цены заявок
            int sharesCount = 1; // кол-во контрактов

            firstValidValue = Math.Max(firstValidValue, period);

            // Берем основную панель (Pane)
            IPane pricePane = ctx.First;

            // Отрисовка PC
            pricePane.AddList("EMA", ema, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);

            for (int bar = firstValidValue; bar < sec.Bars.Count; bar++)
            {
                signalBuy = sec.Bars[bar].Open > ema[bar] && sec.Bars[bar].Date.Hour == 13 && sec.Bars[bar].Date.Minute == 00;
                signalShort = sec.Bars[bar].Open < ema[bar] && sec.Bars[bar].Date.Hour == 12 && sec.Bars[bar].Date.Minute == 00;

                LastActivePosition = sec.Positions.GetLastPositionActive(bar);// получить ссылку на последнию позицию

                if (LastActivePosition != null)//if (IsLastPositionActive) //если позиция есть:
                {
                    if (LastActivePosition.IsLong) //если позиция длинная
                    {
                        if (signalShort)
                        {
                            LastActivePosition.CloseAtMarket(bar + 1, "overturnExit");
                        }                     

                        if (sec.Bars[bar].Close <= LastActivePosition.EntryPrice)
                            LastActivePosition.CloseAtStop(bar + 1, LastActivePosition.EntryPrice - atr[bar] * StopFactor, "stop_Long");
                        if (sec.Bars[bar].Close > LastActivePosition.EntryPrice)
                            LastActivePosition.CloseAtProfit(bar + 1, LastActivePosition.EntryPrice + atr[bar] * ProfitFactor, "take_Long");
                    }
                    else //если позиция короткая
                    {
                        if (signalBuy)
                        {
                            LastActivePosition.CloseAtMarket(bar + 1, "overturnExit");
                        }                   

                        if (sec.Bars[bar].Close >= LastActivePosition.EntryPrice)
                        LastActivePosition.CloseAtStop(bar + 1, LastActivePosition.EntryPrice + atr[bar] * StopFactor, "stop_Short");
                        if (sec.Bars[bar].Close < LastActivePosition.EntryPrice)
                        LastActivePosition.CloseAtProfit(bar + 1, LastActivePosition.EntryPrice - atr[bar] * ProfitFactor, "take_Short");
                    }
                }


                if (LastActivePosition == null)
                {
                    int shares = Math.Max(1, sec.PercentOfEquityShares(bar, sec.CurrentBalance(bar) * PercentOEquity.Value / 100));
                    if (signalBuy)
                    {
                        double orderEntry = sec.Bars[bar].Close;
                        sec.Positions.BuyAtPrice(bar + 1, shares, orderEntry, "Buy");

                    }
                    else if (signalShort)
                    {
                        double orderEntry = sec.Bars[bar].Close;               
                        sec.Positions.SellAtPrice(bar + 1, shares, orderEntry, "Sell");
                    }
                }

            }
        }
    }
}
