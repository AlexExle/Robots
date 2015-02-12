
using System;
using System.Collections.Generic;
using TSLab.Script; // для работы с ТС в TSL
using TSLab.Script.Handlers; // для работы с индикаторими и обработчиками
using TSLab.Script.Helpers; // помошники
using TSLab.Script.Optimization; // для оптимизации
using MMG2015.TSLab.Scripts;


namespace MasterGroop2014.TSLab.Strategies
{
    public class DoubleDonchian : IExternalScript
    {
        public IPosition LastActivePosition = null;
        //задаем параметры для оптимизации
        public OptimProperty _longPeriod = new OptimProperty(130, 10, 800, 10);
        public OptimProperty _shortPeriod = new OptimProperty(30, 10, 500, 10);
        public OptimProperty _equityPercent = new OptimProperty(30, 5, 50, 5);

        public virtual void Execute(IContext ctx, ISecurity symbol) // IContext ctx - источник данных,ISecurity symbol - фин инструмент и инф. о нем
        {
            int longPeriod = _longPeriod; //Определяем период канала
            int shortPeriod = _shortPeriod; //Определяем период канала
            int firstValidValue = 0; // Первое значение свечки при которой существуют все индикаторы

            #region Индикаторы

            #region Верхний канал

            IList<double> highLongLevel = ctx.GetData
            (
                "Верхняя граница ширкого канала", //вводим название нового индикатора
                new[] { longPeriod.ToString() }, //работа с буфером данных
                delegate { return Series.Highest(symbol.HighPrices, longPeriod); } // именно здесь расчитывается индикатор
            );

            IList<double> highShortLevel = ctx.GetData
            (
                "Верхняя граница узкого канала", //вводим название нового индикатора
                new[] { shortPeriod.ToString() }, //работа с буфером данных
                delegate { return Series.Highest(symbol.HighPrices, shortPeriod); } // именно здесь расчитывается индикатор
            );

            highLongLevel = Series.Shift(highLongLevel, 1); //сдвигаем на одну свечу вправо
            highShortLevel = Series.Shift(highShortLevel, 1);
          
            #endregion

            #region Нижний канал

            IList<double> lowLongLevel = ctx.GetData
              (
                  "Нижняя граница канала", //вводим название нового индикатора
                  new[] { longPeriod.ToString() }, //работа с буфером данных
                  delegate { return Series.Lowest(symbol.LowPrices, longPeriod); } // именно здесь расчитывается индикатор
              );

            IList<double> lowShortLevel = ctx.GetData
              (
                  "Нижняя граница канала", //вводим название нового индикатора
                  new[] { shortPeriod.ToString() }, //работа с буфером данных
                  delegate { return Series.Lowest(symbol.LowPrices, shortPeriod); } // именно здесь расчитывается индикатор
              );
            
            lowLongLevel = Series.Shift(lowLongLevel, 1); //сдвигаем на одну свечу вправо
            lowShortLevel = Series.Shift(lowShortLevel, 1); //сдвигаем на одну свечу вправо         

            firstValidValue = Math.Max(firstValidValue, longPeriod);

            #endregion

            #region прорисовка графиков

            // Берем основную панель (Pane)
            IPane pricePane = ctx.First;

            // Отрисовка PC
            pricePane.AddList("Верхняя граница широкого канала", highLongLevel, ListStyles.LINE, 0x0000a0, LineStyles.DASH, PaneSides.RIGHT);
            pricePane.AddList("Нижняя граница широкого канала", lowLongLevel, ListStyles.LINE, 0xa00000, LineStyles.DASH, PaneSides.RIGHT);

            pricePane.AddList("Верхняя граница узкого канала", highShortLevel, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
            pricePane.AddList("Нижняя граница узкого канала", lowShortLevel, ListStyles.LINE, 0xa00000, LineStyles.DOT, PaneSides.RIGHT);


            #endregion

            #endregion

            #region Главный торговый цикл

            for (int bar = firstValidValue; bar < symbol.Bars.Count; bar++)
            {

                LastActivePosition = symbol.Positions.GetLastPositionActive(bar);// получить ссылку на последнию позицию

                if (LastActivePosition != null)//if (IsLastPositionActive) //если позиция есть:
                {
                    if (LastActivePosition.IsLong) //если позиция длинная
                    {
                        LastActivePosition.CloseAtStop(bar + 1, lowShortLevel[bar], "Exit Long");
                    }
                    else //если позиция короткая
                    {
                        LastActivePosition.CloseAtStop(bar + 1, highShortLevel[bar], "Exit Short");
                    }
                }
                else //если позиции нет:
                {
                    int shares = Math.Max(1, symbol.PercentOfEquityShares(bar, symbol.CurrentBalance(bar) * _equityPercent.Value / 100));                 

                    symbol.Positions.BuyIfGreater(bar + 1, shares, highLongLevel[bar], "Buy");

                    symbol.Positions.SellIfLess(bar + 1, shares, lowLongLevel[bar], "Sell");
                }
            }
            #endregion

        }

    }
}
