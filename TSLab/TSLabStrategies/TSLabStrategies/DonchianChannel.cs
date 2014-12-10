
using System;
using System.Collections.Generic;
using TSLab.Script; // для работы с ТС в TSL
using TSLab.Script.Handlers; // для работы с индикаторими и обработчиками
using TSLab.Script.Helpers; // помошники
using TSLab.Script.Optimization; // для оптимизации



namespace MasterGroop2014.TSLab.Strategies
{
    public class tslPC_1 : IExternalScript
    {
        public IPosition LastActivePosition = null;

        public OptimProperty _period = new OptimProperty(100, 10, 800, 10);//задаем параметры для оптимизации

        public virtual void Execute(IContext ctx, ISecurity symbol) // IContext ctx - источник данных,ISecurity symbol - фин инструмент и инф. о нем
        {
            int period = _period; //Определяем период канала
            int firstValidValue = 0; // Первое значение свечки при которой существуют все индикаторы

            #region Индикаторы

            #region Верхний канал

            IList<double> highLevel = ctx.GetData
                (
                    "Верхняя граница канала", //вводим название нового индикатора
                    new[] { period.ToString() }, //работа с буфером данных
                    delegate { return Series.Highest(symbol.HighPrices, period); } // именно здесь расчитывается индикатор
                );

            highLevel = Series.Shift(highLevel, 1); //сдвигаем на одну свечу вправо

            firstValidValue = Math.Max(firstValidValue, period);

            #endregion

            #region Нижний канал

            IList<double> lowLevel = ctx.GetData
              (
                  "Нижняя граница канала", //вводим название нового индикатора
                  new[] { period.ToString() }, //работа с буфером данных
                  delegate { return Series.Lowest(symbol.LowPrices, period); } // именно здесь расчитывается индикатор
              );

            lowLevel = Series.Shift(lowLevel, 1); //сдвигаем на одну свечу вправо

            firstValidValue = Math.Max(firstValidValue, period);

            #endregion

            #region прорисовка графиков

            // Берем основную панель (Pane)
            IPane pricePane = ctx.First;

            //Задаем цвета


            // Отрисовка PC
            pricePane.AddList("Верхняя граница канала", highLevel, ListStyles.LINE, 0x0000a0, LineStyles.DOT, PaneSides.RIGHT);
            pricePane.AddList("Нижняя граница канала", lowLevel, ListStyles.LINE, 0xa00000, LineStyles.DOT, PaneSides.RIGHT);

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
                        LastActivePosition.CloseAtStop(bar + 1, lowLevel[bar], "Exit Long");
                    }
                    else //если позиция короткая
                    {
                        LastActivePosition.CloseAtStop(bar + 1, highLevel[bar], "Exit Long");
                    }


                }
                else //если позиции нет:
                {
                    int kontraktCount = 1;

                    symbol.Positions.BuyIfGreater(bar + 1, kontraktCount, highLevel[bar], "Buy");

                    symbol.Positions.SellIfLess(bar + 1, kontraktCount, lowLevel[bar], "Sell");
                }
            }
            #endregion

        }

    }
}
