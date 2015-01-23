// Здесь прописываем все формулы для расчета каждого параметра в таблице для проведение оптимизации с личными параметрами, 
// Эта таблица автоматом передает все расчеты в VdvScoreCard
// Уважаемые участники мастер-группы 2014 проекта "Финансовая Лаборатория"
// Примите в подарок эту карту... :-)

using System;
using WealthLab;
using WealthLab.Indicators;
using System.Collections.Generic;
using System.Globalization;

namespace VDV.ScoreCard
{
    class vdvScoreCardPE
    {
        #region Свойства

        #region Универсальные показатели (PortfolioSimulation + RowProfit)
        
        public double NetProfit = 0; // +++ Базовая итоговая прибыль
        public double NetProfitPct = 0; //Прибыль в %
        public double Trades = 0; //+++  Количество сделок 
        public double WinningTradesPct = 0; //+++ Процент прибыльных сделок
        public double MaxLossPunkt = 0; //+++ Самый большой убыток в одной сделке за всю историю
        public double MaxDrawDawnPunkt = 0; //+++ Самая большая просадка по серии сделок
        public double CountNewHighEqv = 0; //++количество выходов эквити на новый максимум за весь период торговли,
        public double MaxConsecutiveWin = 0; //+++ максимальное подряд количесвто выигрышей
        public double MaxConsecutiveLoss = 0; //+++ максимальное подряд количество проигрышей
        public double TotalComission = 0; //+++ Сумма комиссии по всем сделкам
        public double RecoveryFactor = 0; //+++ Онтношение NetProfit к наибольшей просадке за историю
        
        public double AvgMonth = 0; //Среднее значение
        public double DevMonth = 0; //Отклонение
        public double SharpeMonth = 0;

        public double AvgDay = 0; //Среднее значение
        public double DevDay = 0; //Отклонение
        public double SharpeDay = 0;

        public double AvgYear = 0; //Среднее значение
        public double DevYear = 0; //Отклонение
        public double SharpeYear = 0;

        public double AvgWeek = 0; //Среднее значение
        public double DevWeek = 0; //Отклонение
        public double SharpeWeek = 0;

        public double AvgQuarter = 0; //Среднее значение
        public double DevQuarter = 0; //Отклонение
        public double SharpeQuarter = 0;

        public double SharpeAvg = 0; //среднегеометрический шарп за день, неделю, месяц, квартал, год
        public double TradesInYear = 0;
        public double GraalMetr = 0; //среднегеометрический шарп за день, неделю, месяц, квартал, год
        public double RecoveryAndSharp = 0; // отношение рекавери и шарпа,


        #endregion


        #region Только для PortfolioSimulation

        public double TWR = 1; // +++отношение конечного капитала к начальному в разах
        public double APR = 0; //+++ (! RowProfit) Среднегодовая доходность (APR) 
        public double AverageProfitPct = 0; //+++ (! RowProfit) Размер средней сделки в %
        public double MaxLossPct = 0; // +++ Наибольший процент убытка за одну сделку
        public double MaxDrawDawnPct = 0; //+++ Максимальная просадка в процентах от текущего максимума эквити
        public double MAXkoff = 0; //+++ Отношение APR к maxDrawDawnPct
        public double Exposure = 0; //+++ Сколько % денег задействованны в сделках по отношению к свободным деньгам на счете
        public double RAR = 0; // +++Какова было бы значение APR если Экспозиция (Exposure) равнялась 100%
        
        public double pctProfitableDay = 0; //сколько % дней было прибыльными
        public double pctProfitableWeek = 0; //сколько % недель было прибыльными
        public double pctProfitableMonth = 0; //сколько % месяцев было прибыльными
        public double pctProfitableQuarter = 0; //сколько % дней было прибыльными
        public double pctProfitableYear = 0; //сколько % дней было прибыльными
        public double pctProfitableAvg = 0; //сколько % месяцев было прибыльными


        public double SpDay = 0; //сколько % дней было прибыльными
        public double SpWeek = 0; //сколько % дней было прибыльными
        public double SpMonth = 0; //сколько % дней было прибыльными
        public double SpQuarter = 0; //сколько % дней было прибыльными
        public double SpYear = 0; //сколько % дней было прибыльными
        public double SpAvg = 0; //сколько % месяцев было прибыльными
        
        public double WealthLabScore = 0; //+++ Нашли показатель WLScore

     //   public double SortinoMonth = 0;

        //    public double SharpeQuart = 0;
        //     public double SharpeYearly = 0;
        //     public double SharpeAvg = 0; //среднее значение шарпа (месяц, квартал, год)
        //  public double RecoveryAndSharp = 0; // Умножаем коэффициент шарпа на рекавери фактор

        #endregion

        #endregion

        #region Конструктор

        public vdvScoreCardPE(SystemPerformance Perf, SystemResults Results)
        {
            DataSeries Equity = Results.EquityCurve; //Переменная для доступа к эквити
            DataSeries Cash = Results.CashCurve;     //Переменная для доступа к количеству денег на счете (не задействованных в позициях)
            
            IList<Position> posList = Results.Positions; //Храним здесь все наши позиции

            double numWinTades = 0; //Количество выигрышных сделок
            double numLossTrades = 0; //Количество проигрышных сделок
            double EquityMax = 0; //Максимальное значение Equity на текущий момент
            double currentConsecutiveWin = 0; //для подсчета количества выигрышей подряд
            double currentConsecutiveLoss = 0; //для подсчета количества проигрышей подряд
            double StartingCapital = 0; //переменная для хранения начального капитала
            double EndingCapital = 0; //переменная для хранения величины конечного капитала
            double equitySum = 0; //Показатель, для расчета Exposure - обозначает сумму эквити на каждом баре
            double unrealizedProfitSum = 0; //Показатель, для расчета Exposure - обозначает сумму (эквити - cach) на каждом баре
            double sumProfitPerctnt = 0; //Промежуточный показатель для расчета средней сделки
            DateTime startDateTrade = Equity.Date[0]; //дата начала торгов
            DateTime finishDateTrade = Equity.Date[Equity.Count - 1]; //дата окончания торгов
            TimeSpan tradingPeriod = finishDateTrade - startDateTrade; //продолжительность периода тестирования

            #region Универсальные показатели (PortfolioSimulation + RowProfit)

            NetProfit = Results.NetProfit; //Узнаем величину NetProfit
            NetProfitPct = NetProfit / Equity[0] * 100;
            Trades = Results.Positions.Count; // Узнаем количество сделок
            TotalComission = Results.TotalCommission; //Узнаем сумму комиссии

            if (!Perf.PositionSize.RawProfitMode) //Считаем эти показатели только когда запущены в режиме PortfolioSimulation
            {
                #region Только для PortfolioSimulation
                APR = Results.APR; //Узнаем величину APR
                StartingCapital = Perf.PositionSize.StartingCapital; //узнаем размер стартового капитала
                EndingCapital = StartingCapital + NetProfit; //расчитываем размер итогового капитала
                TWR = EndingCapital / StartingCapital; // +++ отношение конечного капитала к начальному
               
                #endregion
            }


            #region Последовательно просматриваем каждое значение Equity  (i++  -тожесамое,что-  i = i + 1)

            #region переменные для расчета Sharpe за месяц

           
            DataSeries ReturnMonth = Equity - Equity; //Ряд данных, в котором будут хранится значения шарпа для месяца
            int countOfMonth = 0; //счетчик для формирования ряда данных SharpeSer (для коэффициентов шарпа)
            int currentMonth = (int)Equity.Date[0].Month; //Узнаем номер месяца на самом первом баре
            int lastMonthBar = 0; //Переменная, в которой хранится номер бара прошлого месяца
            double resultMonth = 0; //Переменная, в которой будем хранить сумму дохода
            int countOfDayPlus = 0; //счетчик для подсчета положительных месяцев
            int countOfWeekPlus = 0; //счетчик для подсчета положительных месяцев
            int countOfMonthPlus = 0; //счетчик для подсчета положительных месяцев
            int countOfQuarterPlus = 0; //счетчик для подсчета положительных месяцев
            int countOfYearPlus = 0; //счетчик для подсчета положительных месяцев
            
            #endregion

            #region переменные для расчета Sharpe за день


            DataSeries ReturnDay = Equity - Equity; //Ряд данных, в котором будут хранится значения шарпа для дня
            int countOfDay = 0; //счетчик для формирования ряда данных SharpeSerDay (для коэффициентов шарпа) по дню
            int currentDay = (int)Equity.Date[0].Day; //Узнаем номер дня на самом первом баре
            int lastDayBar = 0; //Переменная, в которой хранится номер бара прошлого месяца
            double resultDay = 0; //Переменная, в которой будем хранить сумму дохода

            #endregion

            #region переменные для расчета Sharpe за год


            DataSeries ReturnYear = Equity - Equity; //Ряд данных, в котором будут хранится значения шарпа для месяца
            int countOfYear = 0; //счетчик для формирования ряда данных SharpeSer (для коэффициентов шарпа)
            int currentYear = (int)Equity.Date[0].Year; //Узнаем номер месяца на самом первом баре
            int lastYearBar = 0; //Переменная, в которой хранится номер бара прошлого месяца
            double resultYear = 0; //Переменная, в которой будем хранить сумму дохода

            #endregion

            #region переменные для расчета Sharpe за недели


            DataSeries ReturnWeek = Equity - Equity; //Ряд данных, в котором будут хранится значения шарпа для недели
            int countOfWeek = 0; //счетчик для формирования ряда данных SharpeSer (для коэффициентов шарпа)
            int currentWeek = GetWeekNumber(Equity.Date[0]); //(int)Equity.Date[0].Week; //Узнаем номер недели  на самом первом баре
            int lastWeekBar = 0; //Переменная, в которой хранится номер бара прошлого недели
            double resultWeek = 0; //Переменная, в которой будем хранить сумму дохода

            #endregion

            #region переменные для расчета Sharpe за квартал


            DataSeries ReturnQuarter = Equity - Equity; //Ряд данных, в котором будут хранится значения шарпа для недели
            int countOfQuarter = 0; //счетчик для формирования ряда данных SharpeSer (для коэффициентов шарпа)
            int currentQuarter = GetQuarterNumber(Equity.Date[0]); //(int)Equity.Date[0].Quarter; //Узнаем номер недели  на самом первом баре
            int lastQuarterBar = 0; //Переменная, в которой хранится номер бара прошлого недели
            double resultQuarter = 0; //Переменная, в которой будем хранить сумму дохода

            #endregion

            for (int bar = 0; bar < Equity.Count; bar++) //Последовательно просматриваем каждое значение Equity  (i++  -тожесамое,что-  i = i + 1)
            {

                if (Equity[bar] > EquityMax) //Если Эквити выходит на новый максимум
                {
                    CountNewHighEqv = CountNewHighEqv + 1; //+++ Увеличиваем на единицу количество выходов на новый максимум
                    EquityMax = Equity[bar]; //Запоминаем максимальное значение эквити (новый максимум)
                }

                if (Equity[bar] - EquityMax < MaxDrawDawnPunkt) //Расчитываем текущую просадку в пунктах и сравниваем ее с максимальной просадкой
                {
                    MaxDrawDawnPunkt = Equity[bar] - EquityMax; //+++ Запоминаем максимальную просадку в пунктах
                }

                if (!Perf.PositionSize.RawProfitMode) //если находимся в режиме Portfolio Simulation (не в режиме RowProfit)
                {
                    if (Equity.Count > 0)
                    {

                        if ( (Equity[bar] - EquityMax) / EquityMax * 100 < MaxDrawDawnPct) //Расчитываем текущую просадку в % и сравниваем ее с максимальной просадкой в %
                        {
                            MaxDrawDawnPct = (Equity[bar] - EquityMax) / EquityMax * 100; //+++ Запоминаем максимальную просадку в %
                        }

                        equitySum = equitySum + Equity[bar]; //Находим площадь под  кривой эквити (зеленая)
                        unrealizedProfitSum = unrealizedProfitSum + (Equity[bar] - Cash[bar]); //Находим площадь фигуры, которая сверху ограничена кривой Equity, а снизу кривой Cash (незадействованные в сделке деньги)



                        #region расчет массива для коэффициента шарпа за день

                        if (Equity.Date[bar].Day != currentDay || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров
                        {
                            if (bar == Equity.Count - 1) //добежали до конца баров
                            {

                                resultDay = (Equity[bar - 1] - Equity[lastDayBar]) / Equity[lastDayBar] * 100; //Нашли процент дохода (убытка) за неполный месяц
                                ReturnDay[countOfDay] = resultDay; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                                countOfDay = countOfDay + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultDay > 0)
                                {
                                    countOfDayPlus = countOfDayPlus + 1; //работает счетчик положительных периодов
                                }
                            }
                            else //сменился месяц
                            {
                                currentDay = Equity.Date[bar].Day; //Узнаем номер месяца на текущем баре и запоминаем его
                                resultDay = (Equity[bar-1] - Equity[lastDayBar]) / Equity[lastDayBar] * 100; //Нашли процент дохода (убытка) за месяц
                                lastDayBar = bar-1; //Запоминаем номер бара, на котором произошла смена месяца
                                ReturnDay[countOfDay] = resultDay; //Запоминаем сумму дохода, полученную в текущем месяце
                                countOfDay = countOfDay + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultDay > 0)
                                {
                                    countOfDayPlus = countOfDayPlus + 1; //работает счетчик положительных периодов
                                }

                            }

                        }
                        #endregion

                        #region расчет массива для коэффициента шарпа за неделю

                        if (GetWeekNumber(Equity.Date[bar]) != currentWeek || bar == Equity.Count - 1) //отслеживаем смену недели или когда добежали до конца баров (/*Equity.Date[bar].Week */)
                        {
                            if (bar == Equity.Count - 1) //добежали до конца баров
                            {

                                resultWeek = (Equity[bar - 1] - Equity[lastWeekBar]) / Equity[lastWeekBar] * 100; //Нашли процент дохода (убытка) за неполный месяц
                                ReturnWeek[countOfWeek] = resultWeek; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                                countOfWeek = countOfWeek + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultWeek > 0)
                                {
                                    countOfWeekPlus = countOfWeekPlus + 1; //работает счетчик положительных периодов
                                }
                            }
                            else //сменилась неделя
                            {
                                currentWeek = GetWeekNumber(Equity.Date[bar]); //Equity.Date[bar].Week; //Узнаем номер месяца на текущем баре и запоминаем его
                                resultWeek = (Equity[bar - 1] - Equity[lastWeekBar]) / Equity[lastWeekBar] * 100; //Нашли процент дохода (убытка) за месяц
                                lastWeekBar = bar - 1; //Запоминаем номер бара, на котором произошла смена месяца
                                ReturnWeek[countOfWeek] = resultWeek; //Запоминаем сумму дохода, полученную в текущем месяце
                                countOfWeek = countOfWeek + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultWeek > 0)
                                {
                                    countOfWeekPlus = countOfWeekPlus + 1; //работает счетчик положительных периодов
                                }

                            }

                        }
                        #endregion

                        #region расчет массива для коэффициента шарпа за месяц

                        if (Equity.Date[bar].Month != currentMonth || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров
                        {
                            if (bar == Equity.Count - 1) //добежали до конца баров
                            {

                                resultMonth = (Equity[bar - 1] - Equity[lastMonthBar]) / Equity[lastMonthBar] * 100; //Нашли процент дохода (убытка) за неполный месяц
                                ReturnMonth[countOfMonth] = resultMonth; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                                countOfMonth = countOfMonth + 1; //Увеличиваем номер счетчика для подсчета количества месяцев

                                if (resultMonth > 0)
                                {
                                    countOfMonthPlus = countOfMonthPlus + 1; //работает счетчик положительных периодов
                                }
                            }
                            else //сменился месяц
                            {
                                currentMonth = Equity.Date[bar].Month; //Узнаем номер месяца на текущем баре и запоминаем его
                                resultMonth = (Equity[bar - 1] - Equity[lastMonthBar]) / Equity[lastMonthBar] * 100; //Нашли процент дохода (убытка) за месяц
                                lastMonthBar = bar - 1; //Запоминаем номер бара, на котором произошла смена месяца
                                ReturnMonth[countOfMonth] = resultMonth; //Запоминаем сумму дохода, полученную в текущем месяце
                                countOfMonth = countOfMonth + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultMonth > 0)
                                {
                                    countOfMonthPlus = countOfMonthPlus + 1; //работает счетчик положительных периодов
                                }

                            }

                        }
                        #endregion

                        #region расчет массива для коэффициента шарпа за квартал

                        if (GetQuarterNumber(Equity.Date[bar]) != currentQuarter || bar == Equity.Count - 1) //отслеживаем смену недели или когда добежали до конца баров (/*Equity.Date[bar].Quarter */)
                        {
                            if (bar == Equity.Count - 1) //добежали до конца баров
                            {

                                resultQuarter = (Equity[bar - 1] - Equity[lastQuarterBar]) / Equity[lastQuarterBar] * 100; //Нашли процент дохода (убытка) за неполный месяц
                                ReturnQuarter[countOfQuarter] = resultQuarter; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                                countOfQuarter = countOfQuarter + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultQuarter > 0)
                                {
                                    countOfQuarterPlus = countOfQuarterPlus + 1; //работает счетчик положительных периодов
                                }
                            }
                            else //сменился месяц
                            {
                                currentQuarter = GetQuarterNumber(Equity.Date[bar]); //Equity.Date[bar].Quarter; //Узнаем номер месяца на текущем баре и запоминаем его
                                resultQuarter = (Equity[bar - 1] - Equity[lastQuarterBar]) / Equity[lastQuarterBar] * 100; //Нашли процент дохода (убытка) за месяц
                                lastQuarterBar = bar - 1; //Запоминаем номер бара, на котором произошла смена месяца
                                ReturnQuarter[countOfQuarter] = resultQuarter; //Запоминаем сумму дохода, полученную в текущем месяце
                                countOfQuarter = countOfQuarter + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultQuarter > 0)
                                {
                                    countOfQuarterPlus = countOfQuarterPlus + 1; //работает счетчик положительных периодов
                                }

                                //  FirstProfit = Profit; // ?????? Запоминаем доход за последний полный (уже закончившийся) месяц
                            }

                        }
                        #endregion

                        #region расчет массива для коэффициента шарпа за год

                        if (Equity.Date[bar].Year != currentYear || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров
                        {
                            if (bar == Equity.Count - 1) //добежали до конца баров
                            {

                                resultYear = (Equity[bar-1] - Equity[lastYearBar]) / Equity[lastYearBar] * 100; //Нашли процент дохода (убытка) за неполный месяц
                                ReturnYear[countOfYear] = resultYear; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                                countOfYear = countOfYear + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultYear > 0)
                                {
                                    countOfYearPlus = countOfYearPlus + 1; //работает счетчик положительных периодов
                                }
                            }
                            else //сменился месяц
                            {
                                currentYear = Equity.Date[bar].Year; //Узнаем номер месяца на текущем баре и запоминаем его
                                resultYear = (Equity[bar-1] - Equity[lastYearBar]) / Equity[lastYearBar] * 100; //Нашли процент дохода (убытка) за месяц
                                lastYearBar = bar -1; //Запоминаем номер бара, на котором произошла смена месяца
                                ReturnYear[countOfYear] = resultYear; //Запоминаем сумму дохода, полученную в текущем месяце
                                countOfYear = countOfYear + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                                if (resultYear > 0)
                                {
                                    countOfYearPlus = countOfYearPlus + 1; //работает счетчик положительных периодов
                                }

                            }

                        }
                        #endregion

                    }
                }
                else //находимся в режиме RowProfitMode
                {
                    #region расчет массива для коэффициента шарпа за день

                    if (Equity.Date[bar].Day != currentDay || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров
                    {
                        if (bar == Equity.Count - 1) //добежали до конца баров
                        {

                            resultDay = Equity[bar - 1] - Equity[lastDayBar]; //Нашли процент дохода (убытка) за неполный месяц
                            ReturnDay[countOfDay] = resultDay; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                            countOfDay = countOfDay + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultDay > 0)
                            {
                                countOfDayPlus = countOfDayPlus + 1; //работает счетчик положительных периодов
                            }
                        }
                        else //сменился месяц
                        {
                            currentDay = Equity.Date[bar].Day; //Узнаем номер месяца на текущем баре и запоминаем его
                            resultDay = Equity[bar - 1] - Equity[lastDayBar]; //Нашли абсолютную величину дохода в месяц (а не процент дохода (убытка) за месяц как в портфельной торговле)
                            lastDayBar = bar - 1; //Запоминаем номер бара, на котором произошла смена месяца
                            ReturnDay[countOfDay] = resultDay; //Запоминаем сумму дохода (или убытка), полученную в текущем месяце
                            countOfDay = countOfDay + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultDay > 0)
                            {
                                countOfDayPlus = countOfDayPlus + 1; //работает счетчик положительных периодов
                            }

                        }

                    }
                    #endregion

                    #region расчет массива для коэффициента шарпа за неделя

                    if (GetWeekNumber(Equity.Date[bar]) != currentWeek || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров /*Equity.Date[bar].Week */
                    {
                        if (bar == Equity.Count - 1) //добежали до конца баров
                        {

                            resultWeek = Equity[bar - 1] - Equity[lastWeekBar]; //Нашли процент дохода (убытка) за неполный неделя
                            ReturnWeek[countOfWeek] = resultWeek; //Запоминаем сумму дохода, полученную в текущем неполном неделя
                            countOfWeek = countOfWeek + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultWeek > 0)
                            {
                                countOfWeekPlus = countOfWeekPlus + 1; //работает счетчик положительных периодов
                            }
                        }
                        else //сменился неделя
                        {
                            currentWeek = GetWeekNumber(Equity.Date[bar]); //Equity.Date[bar].Week; //Узнаем номер месяца на текущем баре и запоминаем его
                            resultWeek = Equity[bar - 1] - Equity[lastWeekBar]; //Нашли абсолютную величину дохода в неделя (а не процент дохода (убытка) за месяц как в портфельной торговле)
                            lastWeekBar = bar - 1; //Запоминаем номер бара, на котором произошла смена неделя
                            ReturnWeek[countOfWeek] = resultWeek; //Запоминаем сумму дохода (или убытка), полученную в текущем месяце
                            countOfWeek = countOfWeek + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultWeek > 0)
                            {
                                countOfWeekPlus = countOfWeekPlus + 1; //работает счетчик положительных периодов
                            }

                        }

                    }
                    #endregion

                    #region расчет массива для коэффициента шарпа за месяц

                    if (Equity.Date[bar].Month != currentMonth || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров
                    {
                        if (bar == Equity.Count - 1) //добежали до конца баров
                        {

                            resultMonth = Equity[bar-1] - Equity[lastMonthBar]; //Нашли процент дохода (убытка) за неполный месяц
                            ReturnMonth[countOfMonth] = resultMonth; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                            countOfMonth = countOfMonth + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultMonth > 0)
                            {
                                countOfMonthPlus = countOfMonthPlus + 1; //работает счетчик положительных периодов
                            }
                        }
                        else //сменился месяц
                        {
                            currentMonth = Equity.Date[bar].Month; //Узнаем номер месяца на текущем баре и запоминаем его
                            resultMonth = Equity[bar-1] - Equity[lastMonthBar]; //Нашли абсолютную величину дохода в месяц (а не процент дохода (убытка) за месяц как в портфельной торговле)
                            lastMonthBar = bar-1; //Запоминаем номер бара, на котором произошла смена месяца
                            ReturnMonth[countOfMonth] = resultMonth; //Запоминаем сумму дохода (или убытка), полученную в текущем месяце
                            countOfMonth = countOfMonth + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultMonth > 0)
                            {
                                countOfMonthPlus = countOfMonthPlus + 1; //работает счетчик положительных периодов
                            }

                        }

                    }
                    #endregion

                    #region расчет массива для коэффициента шарпа за квартал

                    if (GetQuarterNumber(Equity.Date[bar]) != currentQuarter || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров /*Equity.Date[bar].Quarter */
                    {
                        if (bar == Equity.Count - 1) //добежали до конца баров
                        {

                            resultQuarter = Equity[bar - 1] - Equity[lastQuarterBar]; //Нашли процент дохода (убытка) за неполный неделя
                            ReturnQuarter[countOfQuarter] = resultQuarter; //Запоминаем сумму дохода, полученную в текущем неполном неделя
                            countOfQuarter = countOfQuarter + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultQuarter > 0)
                            {
                                countOfQuarterPlus = countOfQuarterPlus + 1; //работает счетчик положительных периодов
                            }
                        }
                        else //сменился неделя
                        {
                            currentQuarter = GetQuarterNumber(Equity.Date[bar]); //Equity.Date[bar].Quarter; //Узнаем номер месяца на текущем баре и запоминаем его
                            resultQuarter = Equity[bar - 1] - Equity[lastQuarterBar]; //Нашли абсолютную величину дохода в неделя (а не процент дохода (убытка) за месяц как в портфельной торговле)
                            lastQuarterBar = bar - 1; //Запоминаем номер бара, на котором произошла смена неделя
                            ReturnQuarter[countOfQuarter] = resultQuarter; //Запоминаем сумму дохода (или убытка), полученную в текущем месяце
                            countOfQuarter = countOfQuarter + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultQuarter > 0)
                            {
                                countOfQuarterPlus = countOfQuarterPlus + 1; //работает счетчик положительных периодов
                            }

                        }

                    }
                    #endregion

                    #region расчет массива для коэффициента шарпа за год

                    if (Equity.Date[bar].Year != currentYear || bar == Equity.Count - 1) //отслеживаем смену месяца или когда добежали до конца баров
                    {
                        if (bar == Equity.Count - 1) //добежали до конца баров
                        {

                            resultYear = Equity[bar-1] - Equity[lastYearBar]; //Нашли процент дохода (убытка) за неполный месяц
                            ReturnYear[countOfYear] = resultYear; //Запоминаем сумму дохода, полученную в текущем неполном месяце
                            countOfYear = countOfYear + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultYear > 0)
                            {
                                countOfYearPlus = countOfYearPlus + 1; //работает счетчик положительных периодов
                            }
                        }
                        else //сменился месяц
                        {
                            currentYear = Equity.Date[bar].Year; //Узнаем номер месяца на текущем баре и запоминаем его
                            resultYear = Equity[bar-1] - Equity[lastYearBar]; //Нашли абсолютную величину дохода в месяц (а не процент дохода (убытка) за месяц как в портфельной торговле)
                            lastYearBar = bar-1; //Запоминаем номер бара, на котором произошла смена месяца
                            ReturnYear[countOfYear] = resultYear; //Запоминаем сумму дохода (или убытка), полученную в текущем месяце
                            countOfYear = countOfYear + 1; //Увеличиваем номер счетчика для подсчета количества месяцев
                            if (resultYear > 0)
                            {
                                countOfYearPlus = countOfYearPlus + 1; //работает счетчик положительных периодов
                            }

                        }

                    }
                    #endregion
                }

            }
            #endregion


            #region Последовательно пробегаемся по всему списку сделок (позициям)
            foreach (Position p in posList) //Последовательно пробегаемся по всему списку сделок (позициям)
            {
                sumProfitPerctnt = sumProfitPerctnt + p.NetProfitPercent; //находим сумму процентов на сделку

                if (p.NetProfit <= 0) //Если сделка убыточна-p.NetProfit - результат по текущей сделки
                {
                    currentConsecutiveWin = 0; //сбрасываем в ноль счетчик прибыльных сделок подряд
                    
                    currentConsecutiveLoss = currentConsecutiveLoss + 1; //Увеличиваем на единицу счетчик убыточных сделок...
                    if (currentConsecutiveLoss > MaxConsecutiveLoss) //если количество убыточных сделок подряд больше чем ранее
                    {
                        MaxConsecutiveLoss = currentConsecutiveLoss; //+++Запоминаем максимальное значение проигрышей подряд
                    }

                    numLossTrades = numLossTrades + 1; //увеличиваем количество убыточных сделок

                    if (p.NetProfit < MaxLossPunkt) //Если убыток текущей сделки является наихудшим
                    {
                        MaxLossPunkt = p.NetProfit; //+++запоминаем значение самого большого убытка
                    }
                }
                else //Если сделка прибыльна
                {
                    currentConsecutiveWin = currentConsecutiveWin + 1; //Увеличиваем на единицу счетчик прибыльных сделок...
                    if (currentConsecutiveWin > MaxConsecutiveWin) //если количество выигрышных сделок подряд больше чем ранее
                    {
                        MaxConsecutiveWin = currentConsecutiveWin; //+++Запоминаем максимальное значение выигрышей подряд
                    }
                    currentConsecutiveLoss = 0; //сбрасываем в ноль счетчик убыточных сделок подряд
                    numWinTades = numWinTades + 1; //увеличиваем количество выигрышных сделок
                }

                if (!Perf.PositionSize.RawProfitMode) //если не находимся в режиме RowProfit
                {
                    if ((p.NetProfit / Equity[p.EntryBar - 1]) * 100 < MaxDrawDawnPct)
                    {
                        MaxDrawDawnPct = (p.NetProfit / Equity[p.EntryBar - 1]) * 100; //+++ нашли максимальный убыток в одной сделке в %
                    }
                }

            } 
            #endregion



            AverageProfitPct = sumProfitPerctnt / Trades; //Считаем размер средней сделки в %/
            WinningTradesPct = numWinTades / Trades * 100; //+++Подсчитываем % прибыльных сделок
            RecoveryFactor = NetProfit / Math.Abs(MaxDrawDawnPunkt); //+++ Расчитываем RecoveryFactor

            if (!Perf.PositionSize.RawProfitMode) //при портфельном тестировании
            {
                MAXkoff = APR / Math.Abs(MaxDrawDawnPct); //Рассчитываем отношение APR к максимальной просадке в %
            }
            else
            {
                MAXkoff = (NetProfit / ( (double) tradingPeriod.Days / 365d ) )/ Math.Abs(MaxDrawDawnPunkt);
            }
           
           
            #region считаем коэфф. Шарпа по сформированному массиву за месяц

            if (countOfMonth > 2) //Если есть доходность минимум по 2-м месяцам:
            {
                AvgMonth = SMA.Value(countOfMonth - 1, ReturnMonth, countOfMonth); //Находим среднемесячный доход 
                DevMonth = StdDev.Value(countOfMonth - 1, ReturnMonth, countOfMonth, StdDevCalculation.Population); //Находим среднегодовое отклонение

                if (DevMonth != 0)
                {
                    SharpeMonth = (AvgMonth * Math.Sqrt(12) - Perf.CashReturnRate) / DevMonth; //Из среднегодовой доходности вычитаем доходность от неинвестированного кеша и делим на стандартное отклонение
                }
            }
            #endregion

            #region считаем коэфф. Шарпа по сформированному массиву за день

            if (countOfDay > 2) //Если есть доходность минимум по 2-м месяцам:
            {
                AvgDay = SMA.Value(countOfDay - 1, ReturnDay, countOfDay); //Находим среднемесячный доход 
                DevDay = StdDev.Value(countOfDay - 1, ReturnDay, countOfDay, StdDevCalculation.Population); //Находим среднегодовое отклонение

                if (DevDay != 0)
                {
                    SharpeDay = (AvgDay * Math.Sqrt(365) - Perf.CashReturnRate) / DevDay; //Из среднегодовой доходности вычитаем доходность от неинвестированного кеша и делим на стандартное отклонение
                }
            }
            #endregion

            #region считаем коэфф. Шарпа по сформированному массиву за год

            if (countOfYear > 2) //Если есть доходность минимум по 2-м месяцам:
            {
                AvgYear = SMA.Value(countOfYear - 1, ReturnYear, countOfYear); //Находим среднемесячный доход 
                DevYear = StdDev.Value(countOfYear - 1, ReturnYear, countOfYear, StdDevCalculation.Population); //Находим среднегодовое отклонение

                if (DevYear != 0)
                {
                    SharpeYear = (AvgYear * Math.Sqrt(1) - Perf.CashReturnRate) / DevYear; //Из среднегодовой доходности вычитаем доходность от неинвестированного кеша и делим на стандартное отклонение
                }
            }
            #endregion

            #region считаем коэфф. Шарпа по сформированному массиву за неделю

            if (countOfWeek > 2) //Если есть доходность минимум по 2-м месяцам:
            {
                AvgWeek = SMA.Value(countOfWeek - 1, ReturnWeek, countOfWeek); //Находим среднемесячный доход 
                DevWeek = StdDev.Value(countOfWeek - 1, ReturnWeek, countOfWeek, StdDevCalculation.Population); //Находим среднегодовое отклонение

                if (DevWeek != 0)
                {
                    SharpeWeek = (AvgWeek * Math.Sqrt(52.1) - Perf.CashReturnRate) / DevWeek; //Из среднегодовой доходности вычитаем доходность от неинвестированного кеша и делим на стандартное отклонение
                }
            }
            #endregion

            #region считаем коэфф. Шарпа по сформированному массиву за квартал

            if (countOfQuarter > 2) //Если есть доходность минимум по 2-м месяцам:
            {
                AvgQuarter = SMA.Value(countOfQuarter - 1, ReturnQuarter, countOfQuarter); //Находим среднемесячный доход 
                DevQuarter = StdDev.Value(countOfQuarter - 1, ReturnQuarter, countOfQuarter, StdDevCalculation.Population); //Находим среднегодовое отклонение

                if (DevQuarter != 0)
                {
                    SharpeQuarter = (AvgQuarter * Math.Sqrt(4) - Perf.CashReturnRate) / DevQuarter; //Из среднегодовой доходности вычитаем доходность от неинвестированного кеша и делим на стандартное отклонение
                }
            }
            #endregion


            #region Определяем % положительных периодов
            
            if (countOfDay > 0)
            pctProfitableDay = (double)countOfDayPlus / (double)countOfDay * 100;

            if (countOfWeek > 0)
            pctProfitableWeek = (double)countOfWeekPlus / (double)countOfWeek * 100;

            if (countOfMonth > 0)
            pctProfitableMonth = (double)countOfMonthPlus / (double)countOfMonth * 100;

            if (countOfQuarter > 0)
            pctProfitableQuarter = (double)countOfQuarterPlus / (double)countOfQuarter * 100;

            if (countOfYear > 0)
            pctProfitableYear = (double)countOfYearPlus / (double)countOfYear * 100;

            #endregion


            #region  В зависимости от длины тестируемого периода будем определять этот показатель

            if (tradingPeriod.Days > 365 * 3) //если период торговли больше чем 3 года
            {
                SharpeAvg = Math.Pow((SharpeDay * SharpeWeek * SharpeMonth * SharpeQuarter * SharpeYear), (1.0 / 5.0)); //находим среднегеометрическое от всех шарпов
                pctProfitableAvg = Math.Pow((pctProfitableDay * pctProfitableWeek * pctProfitableMonth * pctProfitableQuarter * pctProfitableYear), (1.0 / 5.0)); //находим среднегеометрическое от всех шарпов
            }
            else if (tradingPeriod.Days > (31 * 3) * 3) //если период торгов больше 3 кварталов
            {
                SharpeAvg = Math.Pow((SharpeDay * SharpeWeek * SharpeMonth * SharpeQuarter), (1.0 / 4.0)); //находим среднегеометрическое от всех шарпов (кроме годового)
                pctProfitableAvg = Math.Pow((pctProfitableDay * pctProfitableWeek * pctProfitableMonth * pctProfitableQuarter ), (1.0 / 4.0)); //находим среднегеометрическое от всех шарпов
            }
            else if (tradingPeriod.Days > 31 * 3) //если период торгов больше 3 месяцев
            {
                SharpeAvg = Math.Pow((SharpeDay * SharpeWeek * SharpeMonth), (1.0 / 3.0)); //находим среднегеометрическое от всех шарпов (кроме годового)
                pctProfitableAvg = Math.Pow((pctProfitableDay * pctProfitableWeek * pctProfitableMonth), (1.0 / 3.0)); //находим среднегеометрическое от всех шарпов
            }
            else if (tradingPeriod.Days > 7 * 3) //если период торгов больше 3 недель
            {
                SharpeAvg = Math.Pow((SharpeDay * SharpeWeek), (1.0 / 2.0)); //находим среднегеометрическое от всех шарпов (кроме годового)
                pctProfitableAvg = Math.Pow((pctProfitableDay * pctProfitableWeek), (1.0 / 2.0)); //находим среднегеометрическое от всех шарпов
            }
            else if (tradingPeriod.Days > 3) //если период торгов больше 3 дней
            {
                SharpeAvg = SharpeDay; //находим среднегеометрическое от всех шарпов (кроме годового) 
                pctProfitableAvg = pctProfitableDay; //находим среднегеометрическое от всех шарпов
            }
            else
            {
                SharpeAvg = 0;
                pctProfitableAvg = 0; //находим среднегеометрическое от всех шарпов
            }

            #endregion

            #region Считаем показатель Sharpe скорректированный на % прибыльных периодов

            SpDay = SharpeDay * pctProfitableDay / 100d;
            SpWeek = SharpeWeek * pctProfitableWeek / 100d;
            SpQuarter = SharpeQuarter * pctProfitableQuarter / 100d;
            SpMonth = SharpeMonth * pctProfitableMonth / 100d;
            SpYear = SharpeYear * pctProfitableYear / 100d;
            SpAvg = SharpeAvg * pctProfitableAvg / 100d;

            #endregion


            #region считаем грааальность системы

            if (!Perf.PositionSize.RawProfitMode) // Портфель - Считаем эти показатели только когда запущены в режиме PortfolioSimulation
            {
                TradesInYear = Trades / (tradingPeriod.Days / 365.0); //находим сколько сделок совершается за год

                double RecoveryFactorLimit = RecoveryFactor / 7.0; //Если рековери меньше 7-ми показатели системы будут ухудшаться
                if (RecoveryFactorLimit > 3d) RecoveryFactorLimit = 3d; //Не может улучшать показатели системы больше чем в 3 раза

                double SharpeAvgDWMQYLimit = SharpeAvg / 2.2; //Если шарп меньше 1,8 - показатели системы будут ухудшаться
                if (SharpeAvgDWMQYLimit > 2d) SharpeAvgDWMQYLimit = 2d; //Не может улучшать показатели системы больше чем в 2 раза

                double AverageProfitPctLimit = AverageProfitPct / 0.1; //Если  меньше 0,1 - показатели системы будут ухудшаться
                if (AverageProfitPctLimit > 1d) AverageProfitPctLimit = 1d; //Не может улучшать показатели системы больше чем в 1 раза

                double TradesInYearLimit = TradesInYear / 150d; //Если  меньше 150 - показатели системы будут ухудшаться
                if (TradesInYearLimit > 1d) TradesInYearLimit = 1d; //Не может улучшать показатели системы больше чем в 1 раза

                double MAXkoffLimit = MAXkoff / 3.0; //Если  меньше 3 - показатели системы будут ухудшаться
                if (MAXkoffLimit > 3d) MAXkoffLimit = 3d; //Не может улучшать показатели системы больше чем в 3 раза

                GraalMetr = RecoveryFactorLimit * SharpeAvgDWMQYLimit * AverageProfitPctLimit * TradesInYearLimit *MAXkoffLimit; //нашли произведения
                GraalMetr = Math.Pow(GraalMetr, (1d / 5d)); //взяли корень 5й степени
                GraalMetr = GraalMetr * 100d; //Перевели в проценты

                RecoveryAndSharp = ((RecoveryFactor / 10d) * (SharpeMonth / 2.5d)) * 100d; //Если показатель >100 - хорошая система. Если <100 - не очень.
                    
                 }
            else // в режиме RawProfitMode
            {

                double TradesInYear = Trades / (tradingPeriod.Days / 365.0); //находим сколько сделок совершается за год

                double RecoveryFactorLimit = RecoveryFactor / 7.0; //Если рековери меньше 7-ми показатели системы будут ухудшаться
                if (RecoveryFactorLimit > 3.0) RecoveryFactorLimit = 3.0; //Не может улучшать показатели системы больше чем в 3 раза

                double SharpeAvgDWMQYLimit = SharpeAvg / 1.8; //Если шарп меньше 1,8 - показатели системы будут ухудшаться
                if (SharpeAvgDWMQYLimit > 2.0) SharpeAvgDWMQYLimit = 2.0; //Не может улучшать показатели системы больше чем в 2 раза

                double AverageProfitPctLimit = AverageProfitPct / 0.1; //Если  меньше 0,1 - показатели системы будут ухудшаться
                if (AverageProfitPctLimit > 1.0) AverageProfitPctLimit = 1.0; //Не может улучшать показатели системы больше чем в 1 раза

                double TradesInYearLimit = TradesInYear / 70.0; //Если  меньше 70 - показатели системы будут ухудшаться
                if (TradesInYearLimit > 1.0) TradesInYearLimit = 1.0; //Не может улучшать показатели системы больше чем в 1 раза

                GraalMetr = 100.0 * ( Math.Pow((RecoveryFactorLimit * SharpeAvgDWMQYLimit * AverageProfitPctLimit * TradesInYearLimit ), (1.0 / 4.0)) ); //находим среднегеометрическое от всех шарпов
                RecoveryAndSharp = ((RecoveryFactor / 10d) * (SharpeMonth / 2.5d)) * 100d; //Если показатель >100 - хорошая система. Если <100 - не очень.

            }

            #endregion

            if (!Perf.PositionSize.RawProfitMode) //Считаем эти показатели только когда запущены в режиме PortfolioSimulation
            {

                Exposure = unrealizedProfitSum / equitySum * 100; //+++делим красную фигуру на зеленую (% денег, задействованных в сделке по отношению к свободным деньгам на счете)

                //Считаем Exposure, RAR и  WealthLabScore
                if (Exposure > 0)
                {
                    RAR = APR / Exposure * 100; //+++ Расчитываем показатель RAR - сколько была бы среднегодовая доходность если экспозиция равна 100%

                    if (RAR < 0)
                    {
                        WealthLabScore = RAR * (1 + Math.Abs(MaxDrawDawnPct / 100)); //+++ Посчитали WLScore
                    }
                    else
                    {
                        WealthLabScore = RAR * (1 + MaxDrawDawnPct / 100); //*** Посчитали WLScore
                    }
                   
                }

                       
            }

            #endregion

        }
        
        #endregion

        //создаем собственный метод для того, чтобы узнать какая сейчас неделя (номер с начала года)
        private int GetWeekNumber(DateTime currentDateTime)
        {
            CultureInfo culturInfo = CultureInfo.CurrentCulture; //узнаем и запоминаем, какая страновая культура используется на компьютере
            return culturInfo.Calendar.GetWeekOfYear(currentDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday); 
        }

        //создаем собственный метод для того, чтобы узнать какой сейчас квартал (номер с начала года)
        private int GetQuarterNumber(DateTime currentDateTime)
        {
            int numberOfMonth = currentDateTime.Month; //узнаем номер месяца

            if (numberOfMonth <= 3) //если с января по март
                return 1;  //первый квартал
            else if (numberOfMonth <= 6) //если с апреля по июнь
                return 2; // второй квартал
            else if (numberOfMonth <= 9) //если с июля по сентябрь 
                return 3; //третий квартал
            else // если с октября по декабрь
                return 4; //четвертый квартал
        }
    }
}
