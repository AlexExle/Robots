/*Пробойная канальная система, собранная по техникам Дмитрия Власова
Сглаживание цен: SMA с периодом 5
Верхний канал строим по сглаженным High, нижний - по сглаженным Low
Периоды для верхнего и нижнего каналов равны
Сигналы на вход:
в лонг - закрытие выше верхней линии канала  
в шорт - закрытие ниже нижней линии канала
Входим лимиткой по справедливой цене (High+Low+2*Close)/4
Выходим из позиции:
из лонга - при закрытии ниже нижнего канала люстры на след. баре по цене Close бара
из шорта - при закрытии выше верхнего канала люстры
Трейлинг стоп: нет
Управление капиталом: Max%Risk
Используются TradeHelpers.cs Ммихаила Шпиня
 */

using System;
using System.Collections.Generic;
//using MMG2015.TSLab.Scripts;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;
using TSLab.Script.Realtime;
using MMG2015.TSLab.Scripts;

namespace Amateur7.TSLab.Scripts
{
    public class Amateur7PC01TSLabMpR : IExternalScript
    {
        public OptimProperty AvgPricePeriod = new OptimProperty(5, 2, 8, 1); // Период сглаживания
        public OptimProperty ChanelPeriod = new OptimProperty(95, 10, 500, 5); // Период канала
        public OptimProperty PercentWidthChannal = new OptimProperty(75, 55, 95, 5); //  % ширины канала для дюстры

        // Параметры управления ММ
        public OptimProperty MaxPercentRisk = new OptimProperty(2, 0.5, 10, 0.5);
        public OptimProperty SystemPercent = new OptimProperty(100, 10, 100, 10);
        public OptimProperty VirtualEquityRate = new OptimProperty(1, 0.1, 3, 0.1);

        public virtual void Execute(IContext ctx, ISecurity sec)
        {
            int firstValidValue = 10; // Первое значение свечки при которой существуют все индикаторы
            // Указываем максимальное количество контрактов. Защита "От Дурака"
            const int protectingShares = 50;

            // Сглаженные цены верхнего канала
            IList<double> highPrices = ctx.GetData("HighestPrices", new[] { AvgPricePeriod.ToString() },
                                                   () => Series.SMA(sec.HighPrices, AvgPricePeriod));

            // Сглаженные цены нижнего канала
            IList<double> lowPrices = ctx.GetData("LowPrices", new[] { AvgPricePeriod.ToString() },
                                                  () => Series.SMA(sec.LowPrices, AvgPricePeriod));

            firstValidValue = Math.Max(firstValidValue, AvgPricePeriod);

            // Верхний канал
            IList<double> highLevel = ctx.GetData("Highest", new[] { ChanelPeriod.ToString() },
                                                  () => Series.Highest(highPrices, ChanelPeriod));

            highLevel = Series.Shift(highLevel, 1); // сдвигаем на одну свечу вправо

            // Нижний канал
            IList<double> lowLevel = ctx.GetData("Lowest", new[] { ChanelPeriod.ToString() },
                                                 () => Series.Lowest(lowPrices, ChanelPeriod));

            lowLevel = Series.Shift(lowLevel, 1); // сдвигаем на одну свечу вправо

            firstValidValue = Math.Max(firstValidValue, ChanelPeriod);
            int count = Math.Min(highLevel.Count, lowLevel.Count);
          
            // Нижний канал люстры для выхода из длинной позиции  и верхний для выхода из короткой

            IList<double> lowLevelLustra = ctx.GetData("highLevelLustra", new[] { PercentWidthChannal.ToString(), count.ToString() },
                                                delegate()
                                                {
                                                    IList<Double> result = new double[count];
                                                    for (int i = 0; (i < count); i++)
                                                    {
                                                        result[i] = (highLevel[i] - (highLevel[i] - lowLevel[i]) * PercentWidthChannal / 100);

                                                    }
                                                    return result;
                                                });
            IList<double> highLevelLustra = ctx.GetData("highLevelLustra", new[] { PercentWidthChannal.ToString(), count.ToString() },
                                                delegate()
                                                {
                                                    IList<Double> result = new double[count];
                                                    for (int i = 0; (i < count); i++)
                                                    {
                                                        result[i] = (lowLevel[i] + (highLevel[i] - lowLevel[i]) * PercentWidthChannal / 100);
                                                    }
                                                    return result;
                                                });

            bool signalBuy = false, signalShort = false; // Сигналы на вход в длинную и короткую позиции
            double entryPrice = 0;
            double stopPrice = 0; // Цены заявок
            int sharesCount = 1; // кол-во контрактов

            for (int bar = firstValidValue; bar < sec.Bars.Count; bar++)
            {
                #region Сигналы на вход в позицию и выход из нее
                
                signalBuy = sec.ClosePrices[bar] > highLevel[bar];
                signalShort = sec.ClosePrices[bar] < lowLevel[bar];

                // Цена лимитной заявки: Справедливая цена по Д. Власову
                entryPrice = (sec.HighPrices[bar] + sec.LowPrices[bar] + sec.ClosePrices[bar] * 2) / 4;

                #endregion

                #region Сопровождение и выход из позиции

                IPosition position = sec.Positions.GetLastPositionActive(bar); // получить ссылку на последнию позицию

                if (position == null)  // Если нет позиции
                {

                    #region МаниМенеджмент
                    
                    double equity = (sec as ISecurityRt).CurrentBalance(bar); // Сумма счёта в Rt или Equity в режиме тестирования
                    double summForSystem = equity * (SystemPercent / 100d) * VirtualEquityRate;

                    // Сколько денег выделяем ЭТОЙ торговой системе
                    double money = summForSystem * (MaxPercentRisk / 100d); // деньги для расчета кол-ва контрактов

                    #endregion

                    if (signalBuy)
                    {
                        // Пришёл сигнал в длинную позицию
                        stopPrice = lowLevelLustra[bar];

                        // Используем метод расширение из TradeHelper
                        sharesCount = sec.MaxPercentRiskShares(money, entryPrice, stopPrice); // кол-во контрактов

                        // Страховка от глюков депозита
                        sharesCount = Math.Min(sharesCount, protectingShares);

                        //выводим сообщение в лог
                        ctx.Log(
                            String.Format("Баланс: {0}; Выделено системе: {1}% - {2} руб.; Выделено для входа: {3}% - {4} р.",
                                equity,
                                SystemPercent.Value, summForSystem, MaxPercentRisk, money), new Color());
                        ctx.Log(String.Format("Хочу войти в Long: {0} контрактами", sharesCount), new Color());

                        sec.Positions.BuyAtPrice(bar + 1, sharesCount, entryPrice, "Buy");
                    }

                    else if (signalShort)
                    {
                        // Пришёл сигнал в короткую позицию
                        stopPrice = highLevelLustra[bar];

                        // Используем метод расширение из TradeHelper
                        sharesCount = sec.MaxPercentRiskShares(money, entryPrice, stopPrice); // кол-во контрактов

                        // Страховка от глюков депозита
                        sharesCount = Math.Min(sharesCount, protectingShares);

                        //выводим сообщение в лог
                        ctx.Log(
                            String.Format("Баланс: {0}; Выделено системе: {1}% - {2} руб.; Выделено для входа: {3}% - {4} р.",
                                equity,
                                SystemPercent.Value, summForSystem, MaxPercentRisk, money), new Color());
                        ctx.Log(String.Format("Хочу войти в Short: {0} контрактами", sharesCount), new Color());

                        sec.Positions.SellAtPrice(bar + 1, sharesCount, entryPrice, "Short");
                    }
                }
                else    // Если позиция есть
                {
                    if (position.IsLong)
                    {
                        if (sec.ClosePrices[bar] < lowLevelLustra[bar])
                        {
                            position.CloseAtPrice(bar + 1, sec.ClosePrices[bar], "Sell");
                        }
                    }
                    else
                    {
                        if (sec.ClosePrices[bar] > highLevelLustra[bar])
                            position.CloseAtPrice(bar + 1, sec.ClosePrices[bar], "Cover");
                    }
                }

                #endregion
            }

            #region Прорисовка графиков

            // Если оптимизация, то пропускаем отрисовку
            if (ctx.IsOptimization)
                return;

            // Создаём график скриптом
            IPane pricePane = ctx.First;
            Color color = new Color(System.Drawing.Color.Navy.ToArgb());

            // Отрисовка PC
            IGraphList list = pricePane.AddList("Верхний канал - Buy", highLevel, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
            list = pricePane.AddList("Нижний канал - Short", lowLevel, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
            list.Thickness = 1;

            color = new Color(System.Drawing.Color.Red.ToArgb());
            list = pricePane.AddList("Exit Short Level", highLevelLustra, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
            color = new Color(System.Drawing.Color.DarkMagenta.ToArgb());
            list = pricePane.AddList("Exit Long Level", lowLevelLustra, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
            list.Thickness = 1;

            #endregion
        }
    }
}