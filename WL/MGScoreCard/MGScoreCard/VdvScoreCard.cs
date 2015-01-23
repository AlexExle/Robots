// Таблица для проведение оптимизации с личными параметрами


using System.Collections.Generic;
using System.Windows.Forms;
using WealthLab;

namespace VDV.ScoreCard
{
    public class VdvScoreCard : StrategyScorecard
    {
        #region Переменные 

        #region формируем столбцы таблицы для режима RowProfit
        private string[] _headersRowProfit = 
         {
            "Graal Metr",
            "Trades",
            //"NetProfitPct",
            "MAX koff", // отношение APR к максимальной просадке,
            "RS",
            "R.F",      //Recovery Factor
            
           // "D-Sharpe Day",
            //"W-Sharpe Week",
            "M-Sharpe Month",
            "Q-Sharpe Quarter",
            //"Y-Sharpe Year",
            "Avg-Sharpe",
            
           // "D-%Profitable", // сколько % прибыльных периодов из 100% ,
            //"W-%Profitable",
            "M-%Profitable",
            "Q-%Profitable",
            //"Y-%Profitable",
            "Avg-%Profitable", 
            
            //"D-SP Day",// шарп скоректированный на % прибыльных периодов ,
            //"W-SP Week",
            //"M-SP Month",
            //"Q-SP Quarter",
            //"Y-SP Year",
            //"Avg-SP",
            
            
            "TWR",              //отношение конечного капитала к начальному в разы
            "CountNewHighEqv", //количество обновлений эквити за весь период торговли,
            "Winning trades %",
            "Avg Profit %",
            //"Сделок в год",
        };


        private string[] _typesRowProfit = { "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N" };
        #endregion

        #region формируем таблицу для режима PortfolioSimulation

        private string[] _headersPortfolioSimulation = 
         {
            "Graal Metr",
            "Trades",
           // "NetProfitPct",
            "APR",//только PortfolioSimulation,
            "Max DrawDawn %",//только PortfolioSimulation,
            "MAX koff",        //Отношение APR% к maxDrawDown%//только PortfolioSimulation,
            "RS", //Recovery and Sharp
            "R.F",            //Recovery Factor

            
           // "D-Sharpe Day",
           // "W-Sharpe Week",
            "M-Sharpe Month",
            "Q-Sharpe Quarter",
            //"Y-Sharpe Year",
            "Avg-Sharpe",
            
            
           // "D-%Profitable",// сколько % прибыльных периодов из 100% 
            //"W-%Profitable",
            "M-%Profitable",
            "Q-%Profitable",
            //"Y-%Profitable",
            "Avg-%Profitable",
            
           // "D-SP Day",// шарп скоректированный на % прибыльных периодов
           // "W-SP Week",
           // "M-SP Month",
           // "Q-SP Quarter",
           // "Y-SP Year",
           // "Avg-SP",

            "TWR",              //отношение конечного капитала к начальному в разах
            "Wealth-Lab Score",//только PortfolioSimulation,
            "Exposure",//только PortfolioSimulation,
            "CountNewHighEqv", //количество обновлений эквити за весь период торговли,
            "Winning trades %",
            "Avg Profit %",
            //"Сделок в год",
           
        };

        private string[] _typesPortfolioSimulation = { "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "N" };

        #endregion


        #endregion

        #region Свойства

        public override string FriendlyName
        {
            get { return "MasterGroup-ScoreCard"; }
        }

        #region Режим Raw Profit

        /// <summary>
        /// Список наименования колонок
        /// </summary>
        public override IList<string> ColumnHeadersRawProfit
        {
            get { return _headersRowProfit; }
        }

        /// <summary>
        /// Тип колонок
        /// </summary>
        public override IList<string> ColumnTypesRawProfit
        {
            get { return _typesRowProfit; }
        }

        #endregion

        #region Режим Portfolio Simulation
        /// <summary>
        /// Список наименования колонок
        /// </summary>
        public override IList<string> ColumnHeadersPortfolioSim
        {
            get { return _headersPortfolioSimulation; }
        }

        /// <summary>
        /// Тип колонок
        /// </summary>
        public override IList<string> ColumnTypesPortfolioSim
        {
            get { return _typesPortfolioSimulation; }
        }

        #endregion

        
        #endregion


        #region Рассчитать и записать значения набора показателей

        /// <summary>
        /// Рассчитать и записать значения набора показателей
        /// </summary>
        /// <param name="lvi"></param>
        /// <param name="performance"></param>
        public override void PopulateScorecard(ListViewItem lvi, SystemPerformance performance)
        {
            SystemResults results = performance.Results;
            vdvScoreCardPE pe = new vdvScoreCardPE(performance, results);
            
            lvi.SubItems.Add(pe.GraalMetr.ToString("N0")); // Граал
            lvi.SubItems.Add(pe.Trades.ToString("N0")); // Трейды
            // lvi.SubItems.Add(pe.NetProfitPct.ToString("N2")); // Профит

            if (!performance.PositionSize.RawProfitMode)
            {
                lvi.SubItems.Add(pe.APR.ToString("N2")); // Средне годовой доход
                lvi.SubItems.Add(pe.MaxDrawDawnPct.ToString("N2")); // Максимальная просад в %
              
            }
            
            lvi.SubItems.Add(pe.MAXkoff.ToString("N2")); // отношение APR к максимальной просадке
            lvi.SubItems.Add(pe.RecoveryAndSharp.ToString("N2")); // Рекавери и шарп
            lvi.SubItems.Add(pe.RecoveryFactor.ToString("N2")); // Рекавери

            //lvi.SubItems.Add(pe.SharpeDay.ToString("N2")); // шарп день
            //lvi.SubItems.Add(pe.SharpeWeek.ToString("N2")); // шарп неделя
            lvi.SubItems.Add(pe.SharpeMonth.ToString("N2")); // шарп месяц
            lvi.SubItems.Add(pe.SharpeQuarter.ToString("N2")); // шарп квартал
            //lvi.SubItems.Add(pe.SharpeYear.ToString("N2")); // шарп год
            lvi.SubItems.Add(pe.SharpeAvg.ToString("N2")); // Среднее геометрическое шарп по всем периодам,

            //lvi.SubItems.Add(pe.pctProfitableDay.ToString("N2")); // сколько % прибыльных периодов из 100% по дням
            //lvi.SubItems.Add(pe.pctProfitableWeek.ToString("N2")); // сколько % прибыльных периодов из 100% по неделям
            lvi.SubItems.Add(pe.pctProfitableMonth.ToString("N2")); // сколько % прибыльных периодов из 100% по месяцам
            lvi.SubItems.Add(pe.pctProfitableQuarter.ToString("N2")); // сколько % прибыльных периодов из 100% по кварталам
            //lvi.SubItems.Add(pe.pctProfitableYear.ToString("N2")); // сколько % прибыльных периодов из 100% по годам
            lvi.SubItems.Add(pe.pctProfitableAvg.ToString("N2")); // Среднее геометрическое сколько % прибыльных периодов из 100% по всем периодам

            //lvi.SubItems.Add(pe.SpDay.ToString("N2")); // шарп скоректированный на % прибыльных периодов день
            //lvi.SubItems.Add(pe.SpWeek.ToString("N2")); // шарп скоректированный на % прибыльных периодов неделя
            //lvi.SubItems.Add(pe.SpMonth.ToString("N2")); // шарп скоректированный на % прибыльных периодов месяц
            //lvi.SubItems.Add(pe.SpQuarter.ToString("N2")); // шарп скоректированный на % прибыльных периодов квартал
            //lvi.SubItems.Add(pe.SpYear.ToString("N2")); // шарп скоректированный на % прибыльных периодов год
            //lvi.SubItems.Add(pe.SpAvg.ToString("N2")); // Среднее геометрическое шарп скоректированный на % прибыльных периодов

            lvi.SubItems.Add(pe.TWR.ToString("N1")); // отношение конечного капитала к начальному в разах
           
            if (!performance.PositionSize.RawProfitMode)
            {
                lvi.SubItems.Add(pe.WealthLabScore.ToString("N2")); // Коэффициент WLS
                lvi.SubItems.Add(pe.Exposure.ToString("N2")); // Exposure
            }
            lvi.SubItems.Add(pe.CountNewHighEqv.ToString("N2")); // количество обновлений эквити за весь период торговли,
            lvi.SubItems.Add(pe.WinningTradesPct.ToString("N2")); // % выйгрышных сделок
            lvi.SubItems.Add(pe.AverageProfitPct.ToString("N2")); // средний % доход с одной сделки
            //lvi.SubItems.Add(pe.TradesInYear.ToString("N0")); // колличество сделок в год,
             
        }

        #endregion
    }
}
