//Пробойная канальная ТС собранная по техникам Дмитрия Власова http://chechet.org/67
//Сглаживание цен: SMA по ценам закрытия
//Цены для канала: Сглаженая цена
//Индикатор канала: Price Chanel
//Сигнал на вход: Пересечение сглаженой ценой каналов
//Цена лимитной заявки: Цена закрытия
//Периоды каналов на Лог и Шорт: Равны
//Выход по Трейлингстопу равному срединему уровню между каналами
//Управление капиталом : Percent of Equity 
//Author of the code - Michael Shpin michael.shpin@gmail.com


using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;

namespace MMG2015.TSLab.Scripts
{
	public class MsStrategy1TsLabPoE : IExternalScript
	{
		public OptimProperty AvgPricePeriod = new OptimProperty(4, 2, 8, 1); // Период сглаживания
		public OptimProperty ChanelPeriod = new OptimProperty(85, 10, 300, 1); // Период канала

		// Параметры управления ММ
		public OptimProperty EquityPercent = new OptimProperty(300, 10, 1000, 10);

		public virtual void Execute(IContext ctx, ISecurity sec)
		{
			int firstValidValue = 0; // Первое значение свечки при которой существуют все индикаторы

			// Указываем максимальное количество контрактов. Защита глюков портфеля
			const int protectingShares = 100;

			// Сглаженные цены 
			IList<double> avgPrices = ctx.GetData("AvgPrices", new[] {AvgPricePeriod.ToString()},
												  () => Series.SMA(sec.ClosePrices, AvgPricePeriod));

			firstValidValue = Math.Max(firstValidValue, AvgPricePeriod);

			// Верхний канал
			IList<double> highLevel = ctx.GetData("Highest", new[] {ChanelPeriod.ToString()},
												  () => Series.Highest(avgPrices, ChanelPeriod));

			highLevel = Series.Shift(highLevel, 1); // сдвигаем на одну свечу вправо

			// Нижний канал
			IList<double> lowLevel = ctx.GetData("Lowest", new[] {ChanelPeriod.ToString()},
												 () => Series.Lowest(avgPrices, ChanelPeriod));

			lowLevel = Series.Shift(lowLevel, 1); // сдвигаем на одну свечу вправо

			firstValidValue = Math.Max(firstValidValue, ChanelPeriod);

			bool signalBuy = false, signalShort = false; // Сигналы на вход в длинную и короткую позиции
			double entryPrice = 0;
			double stopPrice = 0; // Цены заявок
			int sharesCount = 1; // кол-во контрактов

			for (int bar = firstValidValue; bar < sec.Bars.Count; bar++)
			{
				//signalBuy = avgPrices[bar] > highLevel[bar] && avgPrices[bar - 1] < highLevel[bar - 1];
				//signalShort = avgPrices[bar] < lowLevel[bar] && avgPrices[bar - 1] > lowLevel[bar - 1];
				signalBuy = avgPrices[bar] > highLevel[bar];
				signalShort = avgPrices[bar] < lowLevel[bar];

				//Цена лимитной заявки: Закрытие бара
				entryPrice = sec.ClosePrices[bar];

				#region Сопровождение и выход из позиции

				IPosition position = sec.Positions.GetLastPositionActive(bar); // получить ссылку на последнию позицию
				// Если нет позиции
				if (position == null)
				{
					#region МаниМенеджмент

					double equity = sec.CurrentBalance(bar); //Сумма счёта в Rt или Equity в режиме тестирования                  

					//Используем метод расширение из TradeHelper
					sharesCount = Math.Max(1,  sec.PercentOfEquityShares(bar, equity * EquityPercent.Value / 100)); // кол-во контрактов

					//Страховка от глюков депозита
					sharesCount = Math.Min(sharesCount, protectingShares);
					//sharesCount = 1;

					#endregion

					if (signalBuy)
					{
						//выводим сообщение в лог
						//ctx.Log(
						//	String.Format("Баланс: {0}; Выделено системе: {1}% ", equity,
                        //                 EquityPercent.Value), new Color());
						//ctx.Log(String.Format("Хочу войти в Long: {0} контрактами", sharesCount), new Color());

						sec.Positions.BuyAtPrice(bar + 1, sharesCount, entryPrice, "Enter Long");
					}
					else if (signalShort)
					{
						//выводим сообщение в лог
						/*ctx.Log(
							String.Format("Баланс: {0}; Выделено системе: {1}% - {2} руб.; Выделено для входа: {3}% - {4} р.", equity,
										  SystemPercent.Value, summForSystem, EquityPercent, money), new Color());
						ctx.Log(String.Format("Хочу войти в Short: {0} контрактами", sharesCount), new Color());*/

						sec.Positions.SellAtPrice(bar + 1, sharesCount, entryPrice, "Enter Short");
					}
				}
				else
				{
					// Если позиция есть
					if (position.IsLong)
					{
						// Цена Трейлинг-стопа
						stopPrice = Math.Max(stopPrice, (highLevel[bar] + lowLevel[bar])/2d);
						position.CloseAtStop(bar + 1, stopPrice, "Exit Long");
					}
					else
					{
						// Цена Трейлинг-стопа
						stopPrice = Math.Min(stopPrice, (highLevel[bar] + lowLevel[bar])/2d);
						position.CloseAtStop(bar + 1, stopPrice, "Exit Short");
					}
				}

				#endregion
			}

			// Если оптимизация, то пропускаем отрисовку
			if (ctx.IsOptimization)
				return;

			// Создаём график скриптом
			IPane pane = ctx.CreatePane(sec.ToString(), 100, false);
			Color color = new Color(System.Drawing.Color.Navy.ToArgb());
			pane.AddList(sec.ToString(), sec, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);

			// Отрисовка средней цены
			color = new Color(System.Drawing.Color.DeepPink.ToArgb());
			IGraphList list = pane.AddList("AvgPrice", avgPrices, ListStyles.LINE, color, LineStyles.DASH_DOT, PaneSides.RIGHT);
			list.Thickness = 1;

			// Отрисовка PC
			color = new Color(System.Drawing.Color.Blue.ToArgb());
			list = pane.AddList("Highest", highLevel, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
			list.Thickness = 1;

			color = new Color(System.Drawing.Color.Red.ToArgb());
			list = pane.AddList("Lowest", lowLevel, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
			list.Thickness = 1;
		}
	}
}