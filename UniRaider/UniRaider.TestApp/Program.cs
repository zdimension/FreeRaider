using System;
using UniRaider.Loader;

namespace UniRaider.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            bool testtombpc = false;
            if(testtombpc)
            {
                var lvl1 = TOMBPCParser.ParseFile(args[0]);
            }
            else
            {
                var lvl2 = TombLevelParser.ParseFile(args[0]);
            }
            
            Console.ReadLine();
        }
    }
}
