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
    public class Trio : IExternalScript
    {
        public IPosition LastActivePosition = null;

        public OptimProperty EMAPeriod;
        public OptimProperty TakeProfit;
        public OptimProperty StopLoss;
        public OptimProperty PercentOEquity;

        public Trio()
        {
            EMAPeriod = new OptimProperty(300, 100, 1000, 25);
            TakeProfit = new OptimProperty(600, 300, 1500, 50);
            StopLoss = new OptimProperty(100, 100, 500, 50);
            PercentOEquity = new OptimProperty(30, 5, 50, 5);
        }

        public void Execute(IContext ctx, ISecurity sec)
        {
            int period = EMAPeriod;
            int takeProfit = TakeProfit;
            int stopLoss = StopLoss;

            int firstValidValue = 0; // Первое значение свечки при которой существуют все индикаторы
           
            IList<double> ema = ctx.GetData("EMA", new[] { period.ToString() }, () => Series.EMA(sec.ClosePrices, period));

            bool signalBuy = false, signalShort = false; // Сигналы на вход в длинную и короткую позиции

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
                            LastActivePosition.CloseAtMarket(bar + 1, "exit by market long");
                        }

                        if (sec.Bars[bar].Low <= LastActivePosition.EntryPrice - stopLoss)
                            LastActivePosition.CloseAtStop(bar + 1, LastActivePosition.EntryPrice - stopLoss, "stop Long");
                        if (sec.Bars[bar].High >= LastActivePosition.EntryPrice + takeProfit)
                            LastActivePosition.CloseAtProfit(bar + 1, LastActivePosition.EntryPrice + takeProfit, "take Long");
                    }
                    else //если позиция короткая
                    {
                        if (signalBuy)
                        {
                            LastActivePosition.CloseAtMarket(bar + 1, "exit by market short");
                        }

                        if (sec.Bars[bar].High >= LastActivePosition.EntryPrice + stopLoss)
                            LastActivePosition.CloseAtStop(bar + 1, LastActivePosition.EntryPrice + stopLoss, "stop Short");
                        if (sec.Bars[bar].Low <= LastActivePosition.EntryPrice - takeProfit)
                            LastActivePosition.CloseAtProfit(bar + 1, LastActivePosition.EntryPrice - takeProfit, "take Short");
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
