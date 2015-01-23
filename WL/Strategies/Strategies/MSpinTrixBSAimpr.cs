/* Канальня система - усовершенствованная система MSpinTrixVDV (добавлен фильр на вход, вводим когда индикатор
TRIX последовательно растет на bar[-1] b bar[] при входе в лонг и последовательно умешьшается на bar[-1] b bar[] 
при входе в шорт. Это позволяет избежать ожидания окончания лишнего перегиба для выхода
Также добавлено условие выхода по стопу в случае пробоя противоположного канала (если если цены пошли 
резко в другую сторону до образования по пергиба TRIX). 
Это также формализует условие для расчета позиции для RiskStopLevel)
Также добавлено условие для выхода в случае образования гэпа на баре выхода (ранее в этом случае, если приказ 
на выход не выполнялся, то он снимался и в случае продолженияя негативной тенднции увеличивались убытки).
 Вход пробой канала Close[bar] > highLevel[bar - 1]
                    Close[bar] < lowLevel[bar - 1]
 Выход по индикатору TRIX (Когда он после вершины разворачивается TurnUp(bar, trix), TurnDown(bar, trix)
В итоге последних усовершенствований  Max koeff  образовывает более   устойчивое плато 
 */

using System;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;
//Для РТС (10 минут)

namespace MShpin
{

    public class MSpinTrixBSAimpr : WealthScript
    {
        #region Объявление параметров торговой системы

        private readonly StrategyParameter _periodTrix;
        private readonly StrategyParameter _periodPC;

        #endregion

        public MSpinTrixBSAimpr()
        {
            #region Инициализация параметров торговой системы

            _periodTrix = CreateParameter("period Trix", 37, 5, 200, 5);
            _periodPC = CreateParameter("period PC", 200, 5, 500, 10);

            #endregion
        }

        protected override void Execute()
        {
            int firstValidValue = 1; // Первое значение свечки, при котором существуют все индикаторы

            ClearDebug(); // Очистить окно отладки
            PlotStops(); // Отображать уровни, на которых были попытки выхода по S/L
            HideVolume(); // Скрыть объемы 

            #region Представление цен с необходимой разрядностью

            string pricePattern = "0.";

            for (int i = 0; i < Bars.SymbolInfo.Decimals; i++)
                pricePattern += "0";

            #endregion

            #region Индикаторы

            // TRIX
            DataSeries trix = TRIX.Series(Close, _periodTrix.ValueInt);
            DataSeries highLevel = Highest.Series(Close, _periodPC.ValueInt);
            DataSeries lowLevel = Lowest.Series(Close, _periodPC.ValueInt);


            firstValidValue = Math.Max(firstValidValue, highLevel.FirstValidValue + 1);
            firstValidValue = Math.Max(firstValidValue, lowLevel.FirstValidValue + 1);
            firstValidValue = Math.Max(firstValidValue, trix.FirstValidValue);

            #endregion

            #region прорисовка графикв

            ChartPane trixPane = CreatePane(20, false, true);
            PlotSeriesOscillator(trixPane, trix, 0, 0, Color.LightPink, Color.LightBlue, Color.Red, LineStyle.Solid, 1);

            PlotSeries(PricePane, highLevel, Color.Green, LineStyle.Solid, 2);
            PlotSeries(PricePane, lowLevel, Color.Red, LineStyle.Solid, 2);
            #endregion

            #region Переменные для обслуживания позиции

            bool signalBuy = false, signalShort = false; // Сигналы на вход в длинную и короткую позиции 
            bool signalExit = false;

            #endregion

            for (int bar = firstValidValue; bar < Bars.Count - 1; bar++) // Пробегаемся по всем свечкам (кроме последней)
            {
                signalBuy = false;
                signalShort = false;

                #region Сигналы на вход в позицию и выход из нее

                signalBuy = (Close[bar] > highLevel[bar - 1]) && (trix[bar - 1] < trix[bar]);
                signalShort = (Close[bar] < lowLevel[bar - 1]) && (trix[bar - 1] > trix[bar]);



                #endregion

                #region Сопровождение и выход из позиции

                if (!IsLastPositionActive) // Если позиции нет
                {

                    signalExit = false;
                    if (signalBuy) // При получении сигнала на вход в длинную позицию
                    {

                        RiskStopLevel = lowLevel[bar];

                        BuyAtLimit(bar + 1, Close[bar]);

                        if (IsLastPositionActive) // Если вошли в позицию
                            AnnotateBar("Buy", bar, true, Color.White, Color.LimeGreen);
                    }
                    else if (signalShort) // При получении сигнала на вход в короткую позицию
                    {

                        RiskStopLevel = highLevel[bar];
                        ShortAtLimit(bar + 1, Close[bar]);

                        if (IsLastPositionActive) // Если вошли в позицию
                            AnnotateBar("Short", bar, true, Color.White, Color.OrangeRed);
                    }
                }
                else // Если позиция есть
                {

                    if (LastActivePosition.PositionType == PositionType.Long)
                    {
                        //	if (trix[bar] <= trix[bar - 1] && trix[bar-1] >= trix[bar-2]  ) // Для длинной позиции
                        if (Close[bar] < lowLevel[bar - 1])
                        {

                            ExitAtLimit(bar + 1, LastActivePosition, Close[bar]);
                        }

                        else if ((signalExit) && (Close[bar - 1] > High[bar]))
                        {
                            ExitAtLimit(bar + 1, LastActivePosition, Close[bar]);



                        }

                        else if (TurnDown(bar, trix))
                        {
                            ExitAtLimit(bar + 1, LastActivePosition, Close[bar]);
                            signalExit = true;

                        }

                    }
                    else if (LastActivePosition.PositionType == PositionType.Short)
                    {

                        //if (trix[bar] >= trix[bar - 1] && trix[bar-1] <= trix[bar-2]) // Для короткой позиции
                        if (Close[bar] > highLevel[bar - 1])
                        {

                            ExitAtLimit(bar + 1, LastActivePosition, Close[bar]);
                        }
                        else if ((signalExit) && (Close[bar - 1] < Low[bar]))
                        {
                            ExitAtLimit(bar + 1, LastActivePosition, Close[bar]);


                        }
                        else if (TurnUp(bar, trix))
                        {

                            ExitAtLimit(bar + 1, LastActivePosition, Close[bar]);
                            signalExit = true;
                        }

                    }
                }
                #endregion
            }
        }
    }
}