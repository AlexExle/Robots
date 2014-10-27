using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;
using WealthLab.Indicators;
using System.Drawing;
using ClassLibrary1.Binders.EnterBindings;



namespace ClassLibrary1.Binders
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

        public abstract EnterSignalType GenerateSignal(int bar);       

        public static AEnterStrategy CreateInstance(ClassLibrary1.CourseBinders64.EntryType enterType, WealthScript wlInstance)
        {
            switch (enterType)
            {
                case CourseBinders64.EntryType.Impulse:
                    return new Trio(wlInstance);                  
                case CourseBinders64.EntryType.TrendImpulse:
                    return new TrandDoubleEmaCrossover(wlInstance);                  
                case CourseBinders64.EntryType.TrendPullback:
                    return new TrendPullback(wlInstance);                  
                case CourseBinders64.EntryType.TrendPullbackImpulse:
                    return new TrendPullbackImpulse(wlInstance);                  
                default:
                    throw new NotImplementedException(enterType.ToString() + " not implemented");
            }
        }
    }
}
