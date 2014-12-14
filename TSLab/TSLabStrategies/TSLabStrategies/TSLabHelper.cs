using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Realtime;

namespace MMG2015.TSLab.Scripts
{
    public static class TradeHelper
    {
        /// <summary>
        ///     Текущий баланс
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="bar">Номер бара</param>
        /// <returns></returns>
        public static double CurrentBalance(this ISecurity sec, int bar)
        { 
            ISecurityRt rtSecurity = sec as ISecurityRt; // создаем объект для доступа к информации реальной торговли

            if (rtSecurity != null)
            {
                // находимся мы или нет в режиме реал торговли
                return rtSecurity.EstimatedBalance; // узнаем в ТСЛАБ сумму счета
            }

            return sec.InitDeposit + sec.Positions.CurrentPnL(bar);
        }

        /// <summary>
        ///     Сумарное значение П/У по всем закрытим позициям
        /// </summary>
        /// <param name="positionsList"></param>
        /// <param name="bar">Номер бара</param>
        /// <returns></returns>
        public static double CurrentPnL(this IPositionsList positionsList, int bar)
        {
            // Выбираем закрытие позиции
            IEnumerable<IPosition> positions = positionsList.GetClosedForBar(bar);

            // Суммируем П/У
            return positions.Sum(p => p.Profit());
        }

        /// <summary>
        ///     Получить кол-во контрактов по методоу Max Percent Risk
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="money">Выделенная сумма для сделки</param>
        /// <param name="entryPrice">Цена входа</param>
        /// <param name="stopPrice">Цена стопа</param>
        /// <returns></returns>
        public static int MaxPercentRiskShares(this ISecurity sec, double money, double entryPrice, double stopPrice)
        {
            if (entryPrice > stopPrice)
                return (int)Math.Floor(money / ((entryPrice - stopPrice) * sec.LotSize));

            return (int)Math.Floor(money / ((stopPrice - entryPrice) * sec.LotSize));
        }

        /// <summary>
        ///     Получить кол-во контрактов по методоу Percent Of Equity
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="bar">Номер бара</param>
        /// <param name="money">Выделенная сумма для сделки</param>
        /// <returns></returns>
        public static int PercentOfEquityShares(this ISecurity sec, int bar, double money)
        {
            ISecurityRt rtSecurity = sec as ISecurityRt; // создаем объект для доступа к информации реальной торговли
            if (rtSecurity != null && rtSecurity.FinInfo.BuyDeposit.HasValue)
            {
                return (int)Math.Floor(money / (rtSecurity.FinInfo.BuyDeposit.Value * sec.LotSize));                       
            }
            else
                return (int)Math.Floor(money / (sec.ClosePrices[bar] * sec.LotSize));
        }
    }
}