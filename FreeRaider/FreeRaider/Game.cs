using System.Collections.Generic;
using FreeRaider.Script;
using OpenTK;

namespace FreeRaider
{
    public partial class Constants
    {
        /// <summary>
        /// Original (canonical) TR frame rate.
        /// Needed for animation speed calculations.
        /// </summary>
        public const float TR_FRAME_RATE = 30.0f;

        /// <summary>
        /// This is the global game logic refresh interval.
        /// All game logic should be refreshed at this rate, including enemy AI, values processing and audio update.
        /// </summary>
        public const float GAME_LOGIC_REFRESH_INTERVAL = 1.0f / 60.0f;
    }

    public partial class Global
    {
        public static Vector3 CamAngles;
    }

    public class Game
    {
        public static void InitGlobals();

        public static void RegisterLUAFunction(ScriptEngine state);

        public static int Load(string name);

        public static int Save(string name);

        public static float Tick(float gameLogicTime);

        public static void Frame(float time);

        public static void Prepare();

        public static void LevelTransition(ushort levelIndex);

        public static void ApplyControls(Entity ent);

        public static void UpdateAllEntities(Dictionary<uint, Entity> entities);

        public static void LoopEntities(Dictionary<uint, Entity> entities);

        public static void UpdateAI();

        public static void UpdateCharacters();
    }
}
