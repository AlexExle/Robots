using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;
using ArmoredIntradaySpace.Binders.EnterBindings;
using ArmoredIntraDay.Binders.EnterBindings;



namespace ArmoredIntradaySpace.Binders
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

        public WealthScript si;

        public ArmoredIntraday ArmoredInstanse
        {
            get { return si as ArmoredIntraday; }
        }

        public AEnterStrategy(WealthScript strategyInstance)
        {
            si = strategyInstance;
        }

        public abstract EnterSignalType GenerateSignal(int bar, out double price);       

        public static AEnterStrategy CreateInstance(ArmoredIntradaySpace.ArmoredIntraday.EntryType enterType, WealthScript wlInstance)
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
                case ArmoredIntraday.EntryType.ArmoredPullback:
                    return new ArmoredPullback(wlInstance as ArmoredIntraday);    
                default:
                    throw new NotImplementedException(enterType.ToString() + " not implemented");
            }
        }
    }
}
