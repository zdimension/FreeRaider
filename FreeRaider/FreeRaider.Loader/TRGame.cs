using System.Linq;
using System.Linq.Expressions;
using FreeRaider.Loader;

namespace FreeRaider.Loader
{
    public enum LevelPlatform
    {
        PC = 0,
        PSX = 1,
        DC = 2,
        OpenTomb = 3
    }

    // DO NOT FUCKING EDIT THIS WITHOUT EDITING LFormat.SetDemo
    public enum TRGame
    {
        Unknown = -1,
        TR1 = 0,
        TR1Demo = 1,
        TR2,
        TR2Demo,
        //TR2Gold,
        TR3,
        TR3_VICT,
        //TR3Gold,
        TR4,
        TR4Demo,
        TR5,
        
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

    public struct LFormat
    {
        public TRGame Game;
        public Engine Engine => Helper.GameToEngine(Game);
        public LevelPlatform Platform;

        public LFormat(TRGame game, LevelPlatform fmt = LevelPlatform.PC)
        {
            Game = game;
            Platform = fmt;
        }

        public LFormat(Engine eng, LevelPlatform fmt = LevelPlatform.PC)
        {
            Game = Helper.EngineToGame(eng);
            Platform = fmt;
        }

        public bool IsDemoOrVict => new[] {TRGame.TR1Demo, TRGame.TR2Demo, TRGame.TR3_VICT, TRGame.TR4Demo }.Contains(Game);

        public static readonly LFormat Unknown = default(LFormat);

        public static bool operator >=(LFormat f, Engine e)
        {
            return f.Engine != Engine.Unknown && f.Engine >= e;
        }

        public static bool operator <=(LFormat f, Engine e)
        {
            return f.Engine != Engine.Unknown && f.Engine <= e;
        }

        public static bool operator >(LFormat f, Engine e)
        {
            return f.Engine != Engine.Unknown && f.Engine > e;
        }

        public static bool operator <(LFormat f, Engine e)
        {
            return f.Engine != Engine.Unknown && f.Engine < e;
        }

        public static bool operator ==(LFormat f, Engine e)
        {
            return f.Engine == e;
        }

        public static bool operator !=(LFormat f, Engine e)
        {
            return f.Engine != e;
        }

        public static implicit operator LFormat(Engine e)
        {
            return new LFormat(e);
        }

        public static implicit operator LFormat(TRGame g)
        {
            return new LFormat(g);
        }

        public LFormat SetDemo(bool demo)
        {
            if (Game == TRGame.Unknown || Game == TRGame.TR5) return Game;
            return (TRGame)((int)Game & ~1 | (demo ? 1 : 0)); // Normal = even, Demo = odd
        }
    }

    internal partial class Helper
    {
        public static TRGame EngineToGame(Engine g)
        {
            switch (g)
            {
                case Engine.TR1:
                    return TRGame.TR1;
                case Engine.TR2:
                    return TRGame.TR2;
                case Engine.TR3:
                    return TRGame.TR3;
                case Engine.TR4:
                    return TRGame.TR4;
                case Engine.TR5:
                    return TRGame.TR5;
            }
            return TRGame.Unknown;
        }

        public static Engine GameToEngine(TRGame game, out bool isDemoOrUb)
        {
            isDemoOrUb = false;
            switch (game)
            {
                case TRGame.TR1:
                    return Engine.TR1;
                case TRGame.TR1Demo:               
                    isDemoOrUb = true;
                    return Engine.TR1;
                case TRGame.TR2:
                    //case TRGame.TR2Gold:
                    return Engine.TR2;
                case TRGame.TR2Demo:
                    isDemoOrUb = true;
                    return Engine.TR2;
                case TRGame.TR3:
                    //case TRGame.TR3Gold:
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
