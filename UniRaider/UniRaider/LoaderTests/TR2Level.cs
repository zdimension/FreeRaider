using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniRaider.LoaderTests
{
    public partial class Loader
    {
        public static dynamic TR2Format = new ExpandoObject();

        private static void InitTR2Format()
        {
            //TR1Format = new dynamic();
        }
    }
}
