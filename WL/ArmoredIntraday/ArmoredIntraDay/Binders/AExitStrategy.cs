﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;
using ArmoredIntradaySpace.Binders.ExitBindings;
using ArmoredIntraDay.Binders.ExitBindings;


namespace ArmoredIntradaySpace.Binders
{
    /// <summary>
    /// Загатовка под стратегии на выход. Немного костылей и порождающая функция.
    /// </summary>
    public abstract class AExitStrategy
    {
        public static readonly double ProfitRisk = 3;

        public static readonly int PullbackPeriod = 50;

        public WealthScript si;

        /// <summary>
        /// Не знаю... может для более серьезных случаев, в случае различных вариантов isTrend следует делать своих наследников.. но пока так.
        /// </summary>
        public bool isTrend;

        /// <summary>
        /// По хорошему, конечно, надо в каждом конкретном наследнике AExitStrategy объявлять свой Data Series по которому будем генерить выход
        /// например в Time и StopAndReverse ATR вообще не используется.
        /// но будем считать это небольшим костылем :) не было времени такие мелочи вычищать.
        /// </summary>
        public DataSeries atr;

        public ArmoredIntraday ArmoredInstanse
        {
            get { return si as ArmoredIntraday; }
        }

        public AExitStrategy()
        { }
        public AExitStrategy(WealthScript strategyInstance, bool isTrend)
        {
            si = strategyInstance;
            this.isTrend = isTrend;
            atr = ATR.Series(si.Bars, PullbackPeriod);
            ChartPane atrPane = si.CreatePane(20, false, true);
            si.PlotSeries(atrPane, atr, Color.Red, LineStyle.Solid, 1);
            //Костыль номер 2. как-то не хорошо что стратегия на выход лезет в кишки стратегии на вход. видимо место этой переменной в абстракции более высокого уровня. Например в самом классе CourseBinders64
            AEnterStrategy.firstValidValue = Math.Max(AEnterStrategy.firstValidValue, atr.FirstValidValue); // Первое значение, на котором существует ATR          
        }

        abstract public void InitExitConditions(Position position, int bar);
        abstract public void RecalculateExitConditions(Position position, int bar);
        abstract public bool TryExit(Position position, int bar, EnterSignalType lastEnterSignal);


        public static AExitStrategy CreateInstance(ArmoredIntradaySpace.ArmoredIntraday.ExitType exitType, WealthScript wlInstance, bool isTrend)
        {
            switch (exitType)
            {
               /* case ArmoredIntraday.ExitType.TrailingStop:
                    return new TrailingStop(wlInstance, isTrend);                  
                case ArmoredIntraday.ExitType.StopAndReverse:
                    return new StopAndReverse(wlInstance, isTrend);                  
                case ArmoredIntraday.ExitType.StopAndProfit:
                    return new StopAndProfit(wlInstance, isTrend);                    
                case ArmoredIntraday.ExitType.Time:
                    return new TimeExit(wlInstance, isTrend);  */
                case ArmoredIntraday.ExitType.AtrProfit :
                    return new OnlyProfit(wlInstance, isTrend);
                case ArmoredIntraday.ExitType.StaticProfit :
                    return new StaticProfit(wlInstance as ArmoredIntraday, isTrend);
                case ArmoredIntraday.ExitType.FractalProfit:
                    return new FractalExit(wlInstance, isTrend);
                default:
                    throw new NotImplementedException(exitType.ToString() + " exit strategy not implemented");       
            }
        }
    }
}
