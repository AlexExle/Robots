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
    public class ArmoredIntaday2 : IExternalScript
    {
        public IPosition LastActivePosition = null;
        int period = 300;
        int direction = 0;

        public OptimProperty Shares;
        public OptimProperty MaxPositions;
        public OptimProperty StrikeLine;
        public OptimProperty EnterPriceStep;
        public OptimProperty TakeProfitSize;
        /// <summary>
        /// -1 - only short positions
        /// 0 - both positions
        /// 1 - only long positions
        /// </summary>
        public OptimProperty Direction;

        public ArmoredIntaday2()
        {
            Shares = new OptimProperty(1, 1, 1, 1);
            MaxPositions = new OptimProperty(4, 4, 4, 1);
            StrikeLine = new OptimProperty(90000, 0, int.MaxValue, 5000);
            TakeProfitSize = new OptimProperty(350, 200, 1000, 50);
            EnterPriceStep = new OptimProperty(500, 200, 1000, 50);
            Direction = new OptimProperty(0, -1, 1, 1);
        }

        public void Execute(IContext ctx, TSLab.Script.ISecurity sec)
        {

            int shares = Shares;
            int sharesCounter = 3;
            int firstValidValue = 0;               

            firstValidValue = Math.Max(firstValidValue, period);

            for (int bar = 1; bar < sec.Bars.Count; bar++)
            {
                List<IPosition> activePositions = new List<IPosition>(sec.Positions.GetActiveForBar(bar));
                if (sec.Bars[bar].Close > StrikeLine.Value)
                    direction = -1;
                if (sec.Bars[bar].Close < StrikeLine.Value)
                    direction = 1;
                int prevDirection = 0;
                if (sec.Bars[bar-1].Close > StrikeLine.Value)
                    prevDirection = -1;
                if (sec.Bars[bar-1].Close < StrikeLine.Value)
                    prevDirection = 1;
                if(prevDirection != 0 && prevDirection != direction)
                {
                    foreach(IPosition activePosition in activePositions)
                    {
                        activePosition.CloseAtMarket(bar, "ExitCrossStrike_" + activePosition.EntrySignalName);
                    }
                }
                else
                {
                    int countNeedToCreate = MaxPositions - activePositions.Count;
                    for (int i = 1; i <= countNeedToCreate; i++)
                    {
                        string signalName;
                        if (direction == -1 && Direction < 1)
                        {
                            signalName = "EnterToShort" + (activePositions.Count + i);
                            int closedPositions = CountOfIntradayClosedPositions(signalName, bar, sec);
                            shares = shares + (int)Math.Floor((double)(closedPositions / sharesCounter)) * shares;                            
                            sec.Positions.SellAtPrice(bar + 1, shares, StrikeLine + CalcPrice(activePositions.Count + i), signalName);
                        }
                        if (direction == 1 && Direction > -1)
                        {
                            signalName = "EnterToLong" + (activePositions.Count + i);
                            int closedPositions = CountOfIntradayClosedPositions(signalName, bar, sec);
                            shares = shares + (int)Math.Floor((double)(closedPositions / sharesCounter)) * shares;                            
                            sec.Positions.BuyAtPrice(bar + 1, shares, StrikeLine - CalcPrice(activePositions.Count + i), signalName);
                        }
                    }

                    foreach (IPosition activePosition in activePositions)
                    {
                        activePosition.CloseAtPrice(bar + 1, activePosition.EntryPrice + TakeProfitSize * (activePosition.IsLong ? 1 : -1), "ExitProfit_" + activePosition.EntrySignalName);
                    }
                }

            }     
        }

        public int CountOfIntradayClosedPositions(string signalName, int bar, ISecurity sec)
        {           
            List<IPosition> list = new List<IPosition>(sec.Positions.GetClosedForBar(bar));
            return list.FindAll(pos => pos.EntrySignalName.Contains(signalName) && pos.ExitBar.Date.ToUniversalTime().DayOfYear == DateTime.UtcNow.DayOfYear).Count;
        }

        public double CalcPrice(int positionNuber)
        {
            return positionNuber * EnterPriceStep;
        }
    }
}
