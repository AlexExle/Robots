//Пробойная канальная ТС собранная по техникам Дмитрия Власова http://chechet.org/67
//Сглаживание цен: Нет
//Цены для канала: Close
//Индикатор канала: Price Chanel
//Сигнал на вход: Пересечение цены закрытия и канала
//Цена лимитной заявки: Цена закрытия
//Периоды каналов на Лог и Шорт: Равны
//Выход: изменение направления индикатора Trix
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
	public class MSpinTrixVdvPoE : IExternalScript
	{
		public OptimProperty TrixPeriod = new OptimProperty(37, 5, 200, 5); // Период Trix
		public OptimProperty ChanelPeriod = new OptimProperty(85, 10, 300, 1); // Период канала

		// Параметры управления ММ
		public OptimProperty EquityPercent = new OptimProperty(300, 10, 1000, 10);

		public virtual void Execute(IContext ctx, ISecurity sec)
		{
			int firstValidValue = 0; // Первое значение свечки при которой существуют все индикаторы

			// Указываем максимальное количество контрактов. Защита глюков портфеля
			const int protectingShares = 100;
			
			// Создаем кубик Trix
			TRIX trixHandler = new TRIX{Period = TrixPeriod };
			
			//Вычисляем Trix 
			IList<double> trix = trixHandler.Execute(sec.ClosePrices);

			firstValidValue = Math.Max(firstValidValue, TrixPeriod);

			// Верхний канал
			IList<double> highLevel = ctx.GetData("Highest", new[] {ChanelPeriod.ToString()},
												  () => Series.Highest(sec.ClosePrices, ChanelPeriod));

			highLevel = Series.Shift(highLevel, 1); // сдвигаем на одну свечу вправо

			// Нижний канал
			IList<double> lowLevel = ctx.GetData("Lowest", new[] {ChanelPeriod.ToString()},
												 () => Series.Lowest(sec.ClosePrices, ChanelPeriod));

			lowLevel = Series.Shift(lowLevel, 1); // сдвигаем на одну свечу вправо

			firstValidValue = Math.Max(firstValidValue, ChanelPeriod);

			bool signalBuy = false, signalShort = false; // Сигналы на вход в длинную и короткую позиции
			double entryPrice = 0;
			int sharesCount = 1; // кол-во контрактов

			for (int bar = firstValidValue; bar < sec.Bars.Count; bar++)
			{
				signalBuy = sec.ClosePrices[bar] > highLevel[bar];
				signalShort = sec.ClosePrices[bar] < lowLevel[bar];

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
                    sharesCount = Math.Max(1, sec.PercentOfEquityShares(bar, equity * EquityPercent.Value / 100)); // кол-во контрактов

                    //Страховка от глюков депозита
                    sharesCount = Math.Min(sharesCount, protectingShares);

					#endregion

					if (signalBuy)
					{
						//выводим сообщение в лог
						/*ctx.Log(
							String.Format("Баланс: {0}; Выделено системе: {1}% - {2} руб.; Выделено для входа: {3}% - {4} р.", equity,
										  SystemPercent.Value, summForSystem, EquityPercent, money), new Color());
						ctx.Log(String.Format("Хочу войти в Long: {0} контрактами", sharesCount), new Color());*/

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
				else // Если позиция есть
				{
					if (position.IsLong)// Для длинной позиции
					{
						if (trix[bar] <= trix[bar - 1] && trix[bar-1] >= trix[bar-2])
							position.CloseAtPrice(bar + 1, entryPrice, "Exit Long");
					}
					else// Для короткой позиции
					{
						if (trix[bar] >= trix[bar - 1] && trix[bar-1] <= trix[bar-2])
							position.CloseAtPrice(bar + 1, entryPrice, "Exit Short");
					}
				}

				#endregion
			}

			// Если оптимизация, то пропускаем отрисовку
			if (ctx.IsOptimization)
				return;

			// Создаём график скриптом
            IPane pane = ctx.First;

			// Отрисовка PC
            Color color = new Color(System.Drawing.Color.Blue.ToArgb());
			IGraphList  list = pane.AddList(string.Format("Highest({0})",ChanelPeriod), highLevel, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
			list.Thickness = 1;

			color = new Color(System.Drawing.Color.Red.ToArgb());
			list = pane.AddList(string.Format("Lowest({0})",ChanelPeriod), lowLevel, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
			list.Thickness = 1;

			//график Trix
			IPane trixPane = ctx.CreatePane("Trix", 15, false);
			trixPane.UpdatePrecision(PaneSides.RIGHT, 4);
			color = new Color(System.Drawing.Color.DodgerBlue.ToArgb());
			list = trixPane.AddList(string.Format("Trix({0})", TrixPeriod), trix, ListStyles.LINE, color, LineStyles.SOLID, PaneSides.RIGHT);
			list.Thickness = 2;
	
		}
	}
}