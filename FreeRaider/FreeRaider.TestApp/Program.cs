using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AT.MIN;
using FreeRaider.Loader;
using NLua;
using OpenTK;
using OpenTK.Graphics.ES20;

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

        public static float sum(int b, params float[] args)
        {
            return args.Sum() - b;
        }

        public static void testtest()
        {
            Console.WriteLine("lolol");
        }

        public static void abc(string cmd)
        {
            cmd = cmd.ToLower().Trim();
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                if (!cmd.Contains('(') && !cmd.Contains(')'))
                {
                    var tok = cmd.SplitOnce(' ');
                    if (new[] {
            "help", "goto", "save", "load", "exit", "cls", "spacing",
            "showing_lines", "r_wireframe", "r_points", "r_coll", "r_normals", "r_portals", "r_frustums", "r_room_boxes",
            "r_boxes", "r_axis", "r_allmodels", "r_dummy_statics", "r_skip_room", "room_info"
        }.Contains(tok[0]))
                    {
                        if (tok.Length == 1)
                        {
                            cmd = tok[0] + "()";
                        }
                        else
                        {
                            cmd = tok[0] + "(" + tok[1].Trim() + ")";
                        }
                    }
                }

            }

            Console.WriteLine(cmd);
        }

        static void Main(string[] args)
        {
            /*abc("lol");
            abc("help");
            abc("  save  \"test.sav\" ");
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
            state.DoString("ggg = {\"lol\", 5.5, 8}");
            Console.WriteLine(string.Join("; ", (state["ggg"] as LuaTable).Values.Cast<object>()));
            state["hhhg"] = new Dictionary<string, int>
            {
                { "ABC", 1},
                {"def", 5 },
                {"ghI", 8 }
            };
            state.DoString("print(hhhg[\"def\"])");
            state.RegisterFunction("sumlol", typeof (Program).GetMethod("sum"));
            state.DoString("print(sumlol(80, 2.8, 591, -258))");
            Console.WriteLine(String.Join("; ", state.Globals));
            Console.WriteLine();
            var result = new List<string>();
            var L = state.GetLuaState();
            LuaLib.LuaNetPushGlobalTable(L);
            LuaLib.LuaPushNil(L);
            while(LuaLib.LuaNext(L, -2) != 0)
            {
                result.Add(LuaLib.LuaToString(L, -2));
                LuaLib.LuaPop(L, 1);
            }
            LuaLib.LuaPop(L, 1);
            Console.WriteLine(String.Join("; ", result));

            state.DoString("CVAR_LUA_TABLE_NAME = {}");
            Console.WriteLine(state["CVAR_LUA_TABLE_NAME"]);

            state.RegisterFunction("wrlol",Console.Out, 
                typeof (TextWriter).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .First(
                        x =>
                            x.Name == "WriteLine" && x.GetParameters().Length == 1 &&
                            x.GetParameters()[0].ParameterType == typeof (string)));
            state.DoString("wrlol(\"abcd\")");

            state.DoString("abclol = {abc=5, def=8, ghi=9}");

           Console.WriteLine(((dynamic)state["abclol"])["def"]);*/


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

            /*var lvl = Level.CreateLoader(args[0]);
            for (int i = 0; i < lvl.Rooms.Length; i++)
            {
                var r = lvl.Rooms[i];
                Console.WriteLine("R #{0}: {{{1};{2};{3}}}\tD{4}\tW{5}\tH{6}\t{7}\t{8}\t{9}\t{10}", i, Math.Ceiling(r.Offset.X / 1024),
                    Math.Ceiling((r.Y_Bottom + 32768) / 1024), Math.Ceiling(-r.Offset.Z / 1024), r.Num_Z_Sectors - 2, r.Num_X_Sectors - 2,
                    Math.Ceiling((r.Y_Top - r.Y_Bottom) / 1024),
                    r.Flags.HasFlagUns(RoomFlags.FilledWithWater) ? "Water" : "",
                    r.Flags.HasFlagUns(RoomFlags.None) ? "Sky" : "",
                    r.Flags.HasFlagUns(RoomFlags.Outdoor) ? "Outdoor" : "",
                    r.Flags.HasFlagUns(RoomFlags.Quicksand) ? "Quicksand" : "");
            }*/


            Console.ReadLine();
        }

        public static void test4(ConsoleColor hgg)
        {
            Console.WriteLine(hgg);
        }
    }
}