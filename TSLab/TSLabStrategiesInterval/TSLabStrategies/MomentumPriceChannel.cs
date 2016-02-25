﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;
using MMG2015.TSLab.Scripts;

namespace TSLabStrategies
{
    public class MomentumPriceChannel : IExternalScript
    {
        private string ENTER_BUY = "ENTER_BUY";
        private string ENTER_SELL = "ENTER_SELL";

        private TSLab.Script.Handlers.Close Close = new TSLab.Script.Handlers.Close();

        private TSLab.Script.Handlers.Momentum MM_h = new TSLab.Script.Handlers.Momentum();
        
        private TSLab.Script.Handlers.Highest high_h = new TSLab.Script.Handlers.Highest();
        
        private TSLab.Script.Handlers.Lowest low_h = new TSLab.Script.Handlers.Lowest();
        
        public TSLab.Script.Optimization.OptimProperty MM_Period = new TSLab.Script.Optimization.OptimProperty(20, 10, 100, 5);
        
        public TSLab.Script.Optimization.OptimProperty high_Period = new TSLab.Script.Optimization.OptimProperty(20, 10, 100, 5);

        public TSLab.Script.Optimization.OptimProperty PercentOEquity = new OptimProperty(30, 5, 50, 5);

        public void Execute(IContext context, ISecurity sec)
        {
              // Initialize 'Закрытие1' item
            this.Close.Context = context;
            // Make 'Закрытие1' item data
            IList<double> CloseData = context.GetData("Закрытие1", new string[] {
                "Источник1"
            }, delegate {
                try {
                    return this.Close.Execute(sec);
                }
                catch (System.ArgumentOutOfRangeException ) {
                    throw new TSLab.Script.ScriptException("Ошибка при вычислении блока \'Закрытие1\'. Индекс за пределам диапазона.");
                }

            });
            MM_h.Period = this.MM_Period;
            // Make 'MM' item data
            IList<double> MomentumData = context.GetData("MM", new string[] {
                this.MM_h.Period.ToString(), 
                "Источник1"
            }, delegate {
                try {
                    return this.MM_h.Execute(CloseData);
                }
                catch (System.ArgumentOutOfRangeException ) {
                    throw new TSLab.Script.ScriptException("Ошибка при вычислении блока \'MM\'. Индекс за пределам диапазона.");
                }

            });

            // Initialize 'high' item
            this.high_h.Context = context;
            this.high_h.Period = this.high_Period;
            // Make 'high' item data
            IList<double> high = context.GetData("high", new string[] {
                this.MM_h.Period.ToString(), 
                this.high_h.Period.ToString(), 
                "Источник1"
            }, delegate
            {
                try
                {
                    return this.high_h.Execute(MomentumData);
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    throw new TSLab.Script.ScriptException("Ошибка при вычислении блока \'high\'. Индекс за пределам диапазона.");
                }

            });
            // Initialize 'low' item
            this.low_h.Context = context;
            this.low_h.Period = this.high_Period;
            // Make 'low' item data
            IList<double> low = context.GetData("low", new string[] {
                this.MM_h.Period.ToString(), 
                this.low_h.Period.ToString(), 
                "Источник1"
            }, delegate
            {
                try
                {
                    return this.low_h.Execute(MomentumData);
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    throw new TSLab.Script.ScriptException("Ошибка при вычислении блока \'low\'. Индекс за пределам диапазона.");
                }

            });

            for (int bar = 1; bar < sec.Bars.Count; bar++)
            {
                IPosition buy = sec.Positions.GetLastActiveForSignal(ENTER_BUY);
                IPosition sell = sec.Positions.GetLastActiveForSignal(ENTER_SELL);

                if (MomentumData[bar - 1] > high[bar - 1])
                {
                    if (sell != null)
                    {
                        sell.CloseAtMarket(bar + 1, sell.EntrySignalName + "Close");
                    }
                    if (buy == null)
                    {
                        int shares = Math.Max(1, sec.PercentOfEquityShares(bar, sec.CurrentBalance(bar) * PercentOEquity.Value / 100));
                        sec.Positions.BuyAtMarket(bar + 1, shares, ENTER_BUY);
                    }
                }


                if (MomentumData[bar - 1] < low[bar - 1])
                {
                    if (buy != null)
                    {
                        buy.CloseAtMarket(bar + 1, sell.EntrySignalName + "Close");
                    }
                    if (sell == null)
                    {
                        int shares = Math.Max(1, sec.PercentOfEquityShares(bar, sec.CurrentBalance(bar) * PercentOEquity.Value / 100));
                        sec.Positions.SellAtMarket(bar + 1, shares, ENTER_SELL);
                    }
                }            
            }

            if (context.IsOptimization)
            {
                return;
            }
        }
    }
}
