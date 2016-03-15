using FreeRaider.Loader;

namespace FreeRaider.Loader
{
    public enum TRGame
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
        public static Loader.Engine GameToEngine(TRGame game)
        {
            {
                switch (game)
                {
                    case TRGame.TR1:
                    case TRGame.TR1Demo:
                    case TRGame.TR1UnfinishedBusiness:
                        return Loader.Engine.TR1;
                    case TRGame.TR2:
                    case TRGame.TR2Demo:
                    case TRGame.TR2Gold:
                        return Loader.Engine.TR2;
                    case TRGame.TR3:
                    case TRGame.TR3Gold:
                        return Loader.Engine.TR3;
                    case TRGame.TR4:
                    case TRGame.TR4Demo:
                        return Loader.Engine.TR4;
                    case TRGame.TR5:
                        return Loader.Engine.TR5;
                    default:
                        return Loader.Engine.Unknown;
                }
            }
        }
    }
}
