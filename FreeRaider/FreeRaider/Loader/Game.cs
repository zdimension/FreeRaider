using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeRaider.Loader;

namespace FreeRaider.Loader
{
    public enum Game
    {
        TR1,
        TR1Demo,
        TR1UnfinishedBusiness,
        TR2,
        TR2Demo,
        TR2Gold,
        TR3,
        TR3Gold,
        TR4,
        TR4Demo,
        TR5,
        Unknown
    }

    public enum Engine
    {
        Unknown,
        TR1,
        TR2,
        TR3,
        TR4,
        TR5
    }
}
namespace FreeRaider
{
    public partial class Helper
    {
        public static Loader.Engine GameToEngine(Game game)
        {
            {
                switch (game)
                {
                    case Game.TR1:
                    case Game.TR1Demo:
                    case Game.TR1UnfinishedBusiness:
                        return Loader.Engine.TR1;
                    case Game.TR2:
                    case Game.TR2Demo:
                    case Game.TR2Gold:
                        return Loader.Engine.TR2;
                    case Game.TR3:
                    case Game.TR3Gold:
                        return Loader.Engine.TR3;
                    case Game.TR4:
                    case Game.TR4Demo:
                        return Loader.Engine.TR4;
                    case Game.TR5:
                        return Loader.Engine.TR5;
                    default:
                        return Loader.Engine.Unknown;
                }
            }
        }
    }
}
