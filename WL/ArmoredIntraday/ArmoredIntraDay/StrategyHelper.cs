using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ArmorediIntraday
{
    class MyStrategyHelper : StrategyHelper
    {
        public override string Author
        {
            get { return "ASHab"; }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2014, 01, 01, 12, 12,12); }
        }

        public override string Description
        {
            get { return "Armored"; }
        }

        public override Guid ID
        {
            get { return new Guid("66CD23A6-0242-46DA-8A6B-DE940C75CFF3"); }
        }

        public override DateTime LastModifiedDate
        {
            get { return DateTime.Now; }
        }

        public override string Name
        {
            get { return "Armored"; }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ArmoredIntraday); }
        }
    }
}
