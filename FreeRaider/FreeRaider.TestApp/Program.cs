using System;
using System.Collections.Generic;
using System.Linq;
using AT.MIN;
using FreeRaider.Loader;
using NLua;
using OpenTK;

namespace FreeRaider.TestApp
{
    class Program
    {
        public static void test(string a, int b, float c = 0.5f)
        {
            Console.WriteLine(a + " " + b * c);
        }

        public static void test2(int x, int? y = null, int? z = null)
        {
            var g = x * x;
            if (y != null) g += (int)(y * y);
            if (z != null) g += (int)(z * z);
            Console.WriteLine(g);
        }

        public static void test3(bool a)
        {
            Console.WriteLine(a);
        }

        public enum GGG
        {
            A,
            B,
            C,
            D,
            E,
            F
        }

        static void Main(string[] args)
        {
            /*var fmt = "Can't create style. Possibly max. styles? (%d / %d)";
            Console.WriteLine(Tools.sprintf(fmt, 5, GGG.F));*/
            var state = new Lua();
            var n = DateTime.Now;
            state.RegisterFunction("test", typeof (Program).GetMethod("test"));
            state.RegisterFunction("test2", typeof (Program).GetMethod("test2"));
            state.RegisterFunction("test3", typeof (Program).GetMethod("test3"));
            var n1 = DateTime.Now - n;
            n = DateTime.Now;
            state.DoString("test(\"lol\", 5)");
            state.DoString("test(\"lol\", 5, 2)");
            state.DoString("test2(6)");
            state.DoString("test2(3, 4)");
            state.DoString("test2(3, 4, 5)");
            state.DoString("test3(false)");
            var n2 = DateTime.Now - n;
            Console.WriteLine(n1);
            Console.WriteLine(n2);
            state.DoString(@" function maximum (a)
      local mi = 1          -- maximum index
      local m = a[mi]       -- maximum value
      for i,val in ipairs(a) do
        if val > m then
          mi = i
          m = val
        end
      end
      return m, mi
    end");
            var h = state.DoString("return maximum({8,10,23,12,5})");
            Console.WriteLine(string.Join(", ", h));
            state["lolabc"] = Tuple.Create(1.3f, "abc", false);
            var ttt = Tuple.Create(1.3f, "abc", false);
            state["lol2"] = new object[] {ttt.Item1, ttt.Item2, ttt.Item3};
            state.DoString(@" function maxi2 (a)
  return lol2[1]
end");
            Console.WriteLine(string.Join(", ", state.DoString("return maxi2(13)")));

            state.RegisterFunction("test4", typeof (Program).GetMethod("test4"));
            state.DoString("test4(5)");
            /*state.DoString(
"function print_r ( t ) \n " +
"    local print_r_cache={}\n " +
"    local function sub_print_r(t,indent)\n " +
"        if (print_r_cache[tostring(t)]) then\n " +
"            print(indent..\" * \"..tostring(t))\n " +
"        else\n " +
"            print_r_cache[tostring(t)] = true\n " +
"            if (type(t) == \"table\") then\n " +
"                for pos, val in pairs(t) do\n " +
"                    if (type(val) == \"table\") then\n " +
"                          print(indent..\"[\"..pos..\"] => \"..tostring(t)..\" {\")\n " +
"                        sub_print_r(val, indent..string.rep(\" \", string.len(pos) + 8))\n " +
"                        print(indent..string.rep(\" \", string.len(pos) + 6)..\"}\")\n " +
"                    elseif(type(val) == \"string\") then\n " +
"                         print(indent..\"[\"..pos..'] => \"'..val..'\"')\n " +
"                    else\n " +
"                        print(indent..\"[\"..pos..\"] => \"..tostring(val))\n " +
"                    end\n " +
"                end\n " +
"            else\n " +
"                print(indent..tostring(t))\n " +
"            end\n " +
"        end\n " +
"    end\n " +
"    if (type(t) == \"table\") then\n " +
"          print(tostring(t)..\" {\")\n " +
"        sub_print_r(t, \"  \")\n " +
"        print(\"}\")\n " +
"    else\n " +
"        sub_print_r(t, \"  \")\n " +
"    end\n " +
"    print()\n " +
"end");
            state.DoString("print_r(lol1)");
            state.DoString(
@"print(lol1[1])
");*/

            /*long t1 = 0;
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
            Console.WriteLine(new TimeSpan(t2 / 10));*/

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

        public static void test4(ConsoleColor hgg)
        {
            Console.WriteLine(hgg);
        }
    }
}