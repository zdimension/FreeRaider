using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static Loader.Engine GameToEngine(Loader.Game game)
        {
            {
                switch (game)
                {
                    case Loader.Game.TR1:
                    case Loader.Game.TR1Demo:
                    case Loader.Game.TR1UnfinishedBusiness:
                        return Loader.Engine.TR1;
                    case Loader.Game.TR2:
                    case Loader.Game.TR2Demo:
                    case Loader.Game.TR2Gold:
                        return Loader.Engine.TR2;
                    case Loader.Game.TR3:
                    case Loader.Game.TR3Gold:
                        return Loader.Engine.TR3;
                    case Loader.Game.TR4:
                    case Loader.Game.TR4Demo:
                        return Loader.Engine.TR4;
                    case Loader.Game.TR5:
                        return Loader.Engine.TR5;
                    default:
                        return Loader.Engine.Unknown;
                }
            }
        }
    }
}
