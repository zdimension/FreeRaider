using System;
using System.Linq;
using FreeRaider.Loader;
using NLua;

namespace FreeRaider.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var state = new Lua();
            state.DoString(@"
    function ScriptFunc (val1, val2)
        if val1 > val2 then
            return val1 + 1
        else
            return val2 - 1
        end
    end
    ");
            var scriptFunc = state["ScriptFunc"] as LuaFunction;
            var res = (scriptFunc.Call(3, 5).First());
            Console.WriteLine(res + " " + res.GetType().Name);

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