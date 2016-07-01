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
        TR1,
        TR2,
        TR3,
        TR4,
        TR5,
        Unknown
    }

    public partial class Helper
    {
        public static Engine GameToEngine(TRGame game, out bool isDemoOrUb)
        {
            isDemoOrUb = false;
            switch (game)
            {
                case TRGame.TR1:
                    return Engine.TR1;
                case TRGame.TR1Demo:
                case TRGame.TR1UnfinishedBusiness:
                    isDemoOrUb = true;
                    return Engine.TR1;
                case TRGame.TR2:
                case TRGame.TR2Gold:
                    return Engine.TR2;
                case TRGame.TR2Demo:
                    isDemoOrUb = true;
                    return Engine.TR2;
                case TRGame.TR3:
                case TRGame.TR3Gold:
                    return Engine.TR3;
                case TRGame.TR4:
                case TRGame.TR4Demo:
                    return Engine.TR4;
                case TRGame.TR5:
                    return Engine.TR5;
                default:
                    return Engine.Unknown;
            }
        }

        public static Engine GameToEngine(TRGame game)
        {
            bool demoOrUb;

            return GameToEngine(game, out demoOrUb);
        }
    }
}
