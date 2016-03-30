using System;
using System.Collections.Generic;
using System.Linq;
using AT.MIN;
using FreeRaider.Loader;
using NLua;

namespace FreeRaider.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var state = new Lua();
            state.DoString(@"abc = nil");
           state.NewTable("abc");
           state.NewTable("abc.ab");
           state.NewTable("abc.3");
           state.NewTable("abc.'g'");
            var g = ((LuaTable) state["abc"]);
            var d = g.Keys.Cast<dynamic>().ToDictionary(x => x, x => g.Values.Cast<dynamic>().ToList()[g.Keys.Cast<dynamic>().ToList().IndexOf(x)]);

            Console.WriteLine(string.Join(", ", d.Select(x => x.Key + " : " + x.Value)));*/

            long t1 = 0;
            long t2 = 0;

            var f1 = "{0} {1:0.00}";
            var f2 = "%s %.2f";

            var s1 = "aBcDeF123";
            var fl1 = 123.456789f;

            DateTime n;
            TimeSpan d1, d2;


            for (var k = 0; k < 10; k++)
            {
                n = DateTime.Now;

                for (var i = 0; i < 10000; i++)
                {
                    var g = string.Format(f1, s1, fl1);
                }

                d1 = DateTime.Now - n;

                t1 += d1.Ticks;

                n = DateTime.Now;

                for (var i = 0; i < 10000; i++)
                {
                    var g = Tools.sprintf(f2, s1, fl1);
                }

                d2 = DateTime.Now - n;

                t2 += d2.Ticks;
            }

            Console.WriteLine(new TimeSpan(t1 / 10));
            Console.WriteLine(new TimeSpan(t2 / 10));

            // multiple tests

            /*var tr1_1 = Level.CreateLoader("tr1\\LEVEL2.PHD");
            var tr1_2 = Level.CreateLoader("tr1\\LEVEL4.PHD");
            var tr1_3 = Level.CreateLoader("tr1\\LEVEL7A.PHD");           

            var tr2_1 = Level.CreateLoader("tr2\\ASSAULT.TR2");
            var tr2_2 = Level.CreateLoader("tr2\\deck.TR2");
            var tr2_3 = Level.CreateLoader("tr2\\MONASTRY.TR2");

            var tr3_1 = Level.CreateLoader("tr3\\HOUSE.TR2");
            var tr3_2 = Level.CreateLoader("tr3\\QUADCHAS.TR2");
            var tr3_3 = Level.CreateLoader("tr3\\TEMPLE.TR2");

            var tr4_1 = Level.CreateLoader("tr4\\citnew.tr4");
            var tr4_2 = Level.CreateLoader("tr4\\lake.tr4");
            var tr4_3 = Level.CreateLoader("tr4\\train.tr4");
            Console.Clear();
            Console.SetBufferSize(Console.BufferWidth, 10000);
            var tr5_1 = Level.CreateLoader("tr5\\Andrea3.trc");
            var tr5_2 = Level.CreateLoader("tr5\\joby4.trc");
            var tr5_3 = Level.CreateLoader("tr5\\rich3.trc");*/


            Console.ReadLine();
        }
    }
}