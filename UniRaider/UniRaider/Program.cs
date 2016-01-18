using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRaider.Loader;

namespace UniRaider
{
    class Program
    {
        static void Main(string[] args)
        {
            var lvl = Level.CreateLoader(args[0]);

            Console.ReadLine();
        }
    }
}
