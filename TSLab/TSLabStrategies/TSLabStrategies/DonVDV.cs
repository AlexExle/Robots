// - Единственный параметр - общий на вход и выход для лонга и шорта, все РС построены по экстремумам
// - Вход - при касание тени единого РС   - выставляется лимитку на середину сигнальной свечи (High[bar] + Low[bar]) / 2), 
// - Если не зацепило снимаем и переставляем по следующей свече при условии подтверждения сигнала на вход
// - Выход - по единому РС  выставляется ЛЗ на середину сигнальной свечи (High[bar] + Low[bar]) / 2),
// - Если не зацепило снимаем и переставляем по следующей свече при условии подтверждения сигнала на выход
// - Нет принципа "Ни шагу назад"
// - Размер позиции определяем по единому РС,



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
            const double Equity = 200000;  //тут надо поставить сумму счета
            const double maxPercentRisk = 2.0;  //максимальный риск в одной сделке,
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
                        if (symbol.LowPrices[bar] < lowLevel[bar])
                        {
                            LastActivePosition.CloseAtPrice(bar + 1, (symbol.HighPrices[bar] + symbol.LowPrices[bar]) / 2, "Exit Long");
                        }

                    }
                    else //если позиция короткая
                    {
                        if (symbol.HighPrices[bar] > highLevel[bar])
                        {
                            LastActivePosition.CloseAtPrice(bar + 1, (symbol.HighPrices[bar] + symbol.LowPrices[bar]) / 2, "Exit Long");
                        }

                    }


                }
                else //если позиции нет:
                {

                    if (symbol.HighPrices[bar] > highLevel[bar])
                    {
                        double orderTrailingStop = lowLevel[bar];
                        double orderEntry = (symbol.HighPrices[bar] + symbol.LowPrices[bar]) / 2;
                        int kontraktBuy = (int)System.Math.Floor((Equity * maxPercentRisk / 100) / ((orderEntry - orderTrailingStop))); // просчитываем кол-во контрактов для пакупки (размера позиции)
                        // int kontraktBuy = 1; // если хотим торговать одним контрактом


                        symbol.Positions.BuyAtPrice(bar + 1, kontraktBuy, orderEntry, "Buy");


                    }
                    else if (symbol.LowPrices[bar] < lowLevel[bar])
                    {
                        double orderTrailingStop = highLevel[bar];
                        double orderEntry = (symbol.HighPrices[bar] + symbol.LowPrices[bar]) / 2;
                        // int kontraktShort = 1;
                        int kontraktShort = (int)System.Math.Floor((Equity * maxPercentRisk / 100) / ((orderTrailingStop - orderEntry))); // просчитываем кол-во контрактов для продать (размера позиции)


                        symbol.Positions.SellAtPrice(bar + 1, kontraktShort, orderEntry, "Sell");

                    }

                }
            }
            #endregion

        }

    }
}
