using System;
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
    public class Fractal : IExternalScript
    {
        public IPosition LastActivePosition = null;
    
        public OptimProperty Shares;
        public OptimProperty MaxPositions;
        public OptimProperty LeftRight;
        public OptimProperty FractalParam;
        public OptimProperty CurrentBarParam;
        /// <summary>
        /// -1 - only short positions
        /// 0 - both positions
        /// 1 - only long positions
        /// </summary>
        public OptimProperty Direction;

        public FractalBuyValue_.FractalBuyValue fractalBuy;
        public FractalSellValue_.FractalSellValue fractalSell;

        public int Leg
        {
            get
            {
                return LeftRight;
            }
        }

        public int FractalVal
        {
            get
            {
                return FractalParam;
            }
        }

        public int CurrentBar
        {
            get
            {
                return CurrentBarParam;
            }
        }

        public Fractal()
        {
            fractalBuy = new FractalBuyValue_.FractalBuyValue();
            fractalSell = new FractalSellValue_.FractalSellValue();
            Shares = new OptimProperty(1, 1, 1, 1);
            MaxPositions = new OptimProperty(4, 1, 4, 1);
            LeftRight = new OptimProperty(5, 1, 20, 1);
            FractalParam = new OptimProperty(5, 1, 20, 1);
            CurrentBarParam = new OptimProperty(0, 0, 1, 1);
        }

        public void Execute(IContext ctx, TSLab.Script.ISecurity sec)
        {
            fractalBuy.Context = ctx;
            fractalBuy.Left = Leg;
            fractalBuy.Right = Leg;
            fractalBuy.Fractal = FractalVal;
            fractalBuy.CurrentBar = CurrentBar;

            fractalSell.Context = ctx;
            fractalSell.Left = Leg;
            fractalSell.Right = Leg;
            fractalSell.Fractal = FractalVal;
            fractalSell.CurrentBar = CurrentBar;

            IList<double> buyFractal = ctx.GetData
            (
                "Фрактал на покупку", //вводим название нового индикатора
                new[] { Leg.ToString(), FractalVal.ToString(), CurrentBar.ToString() },
                delegate
                {
                    try
                    {
                        return fractalBuy.Execute(sec);
                    }
                    catch (Exception e)
                    {

                        throw new TSLab.Script.ScriptException("Ошибка при вычислении блока \'fractalBuy\'." + e.ToString());
                    }
                } // именно здесь расчитывается индикатор
            );

            IList<double> sellFractal = ctx.GetData
            (
                "Фрактал на продажу", //вводим название нового индикатора
                new[] { Leg.ToString(), FractalVal.ToString(), CurrentBar.ToString() },
                delegate
                {
                    try
                    {
                        return fractalSell.Execute(sec);
                    }
                    catch (Exception e)
                    {

                        throw new TSLab.Script.ScriptException("Ошибка при вычислении блока \'fractalBuy\'." + e.ToString());
                    }
                } // именно здесь расчитывается индикатор
            );

            IPane pricePane = ctx.First;

            // Отрисовка PC
            pricePane.AddList("Фрактал на покупку", buyFractal, ListStyles.LINE, 0xa00000, LineStyles.DASH, PaneSides.RIGHT);
            pricePane.AddList("Фрактал на продажу", sellFractal, ListStyles.LINE, 0x0000a0, LineStyles.DASH, PaneSides.RIGHT);

            int firstValidValue = 0;

            firstValidValue = Math.Max(firstValidValue, Leg);
          
            for (int bar = 1; bar < sec.Bars.Count; bar++)
            {
                List<IPosition> activePositions = new List<IPosition>(sec.Positions.GetActiveForBar(bar));
                foreach (IPosition activePosition in activePositions)
                {
                    activePosition.CloseAtPrice(bar + 1, activePosition.IsLong ? buyFractal[bar] : sellFractal[bar], GenerateSignalName(false, !activePosition.IsLong, activePosition.IsLong ? buyFractal[bar] : sellFractal[bar]));
                }

                if (activePositions.Count < MaxPositions)
                {
                    if (!IsPositionexistForFractal(buyFractal[bar], false, bar, sec, activePositions))
                    {
                        sec.Positions.SellAtPrice(bar + 1, Shares, buyFractal[bar], GenerateSignalName(true, false, buyFractal[bar]));
                    }

                    if (!IsPositionexistForFractal(sellFractal[bar], true, bar, sec, activePositions))
                    {
                        sec.Positions.BuyAtPrice(bar + 1, Shares, sellFractal[bar], GenerateSignalName(true, true, sellFractal[bar]));
                    }
                }
            }
        }

        public bool IsPositionexistForFractal(double fractalPrice, bool direcition, int bar, ISecurity sec, List<IPosition> list)
        {
            string signalName = GenerateSignalName(true, direcition, fractalPrice);
            return list.FindAll(pos => pos.EntrySignalName.Contains(signalName)).Count > 0;
        }

        public int CountOfIntradayClosedPositions(string signalName, int bar, ISecurity sec, List<IPosition> list)
        {            
            return list.FindAll(pos => pos.EntrySignalName.Contains(signalName) && pos.ExitBar.Date.ToUniversalTime().DayOfYear == DateTime.UtcNow.DayOfYear).Count;
        }

        public string GenerateSignalName(bool enter, bool direction, double price)
        {
            return (enter ? "Enter_" : "Exit") + (direction ? "Long_" : "Short_") + price.ToString();
        }
    }
}
