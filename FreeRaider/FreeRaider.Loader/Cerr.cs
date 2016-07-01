using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeRaider.Loader
{
    internal class Cerr
    {
        public static void Write(string str)
        {
            Console.Error.WriteLine(str);
        }
    }
}
