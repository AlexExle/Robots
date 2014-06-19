﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthLab;

namespace ClassLibrary1
{
    class MyStrategyHelper : StrategyHelper
    {
        public override string Author
        {
            get { return "ASHab"; }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2014, 03, 26, 12, 12,12); }
        }

        public override string Description
        {
            get { return "FINAM1"; }
        }

        public override Guid ID
        {
            get { return new Guid("5F224023-4AE5-4248-8730-95C8E66FD27D"); }
        }

        public override DateTime LastModifiedDate
        {
            get { return DateTime.Now; }
        }

        public override string Name
        {
            get { return "FINAM1"; }
        }

        public override Type WealthScriptType
        {
            get { return typeof(MyStrategyScript); }
        }
    }
}