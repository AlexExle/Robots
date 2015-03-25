using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;
using ArmorediIntraday.Binders.EnterBindings;
using ArmoredIntraDay.Binders.EnterBindings;



namespace ArmorediIntraday.Binders
{

    public enum EnterSignalType
    {
        None = 0,
        Up = 1,
        Down = 2     
    }

    /// <summary>
    /// Абстрактный класс стратегий на вход, ниче полезного кроме пораждающей функции.
    /// </summary>
    public abstract class AEnterStrategy
    {

        public static int firstValidValue = 0;

        public WealthScript StrategyInstance;

        public AEnterStrategy(WealthScript strategyInstance)
        {
            StrategyInstance = strategyInstance;
        }

        public abstract EnterSignalType GenerateSignal(int bar, out double price);       

        public static AEnterStrategy CreateInstance(ArmorediIntraday.ArmoredIntraday.EntryType enterType, WealthScript wlInstance)
        {
            switch (enterType)
            {
                case ArmoredIntraday.EntryType.Impulse:
                    return new Trio(wlInstance);                  
                case ArmoredIntraday.EntryType.TrendImpulse:
                    return new TrandDoubleEmaCrossover(wlInstance);                  
                case ArmoredIntraday.EntryType.TrendPullback:
                    return new TrendPullback(wlInstance);                  
                case ArmoredIntraday.EntryType.TrendPullbackImpulse:
                    return new TrendPullbackImpulse(wlInstance);     
                case ArmoredIntraday.EntryType.StaticImpulse:
                    return new StaticInterval(wlInstance as ArmoredIntraday);
                case ArmoredIntraday.EntryType.ExpImpulse:
                        return new ExpInterval(wlInstance as ArmoredIntraday);
                default:
                    throw new NotImplementedException(enterType.ToString() + " not implemented");
            }
        }
    }
}
