using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeRaider.Loader;

namespace FreeRaider
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.Random = new Random();

            var lvl = Level.CreateLoader(args[0]);
            
            Console.ReadLine();
        }
    }
}
