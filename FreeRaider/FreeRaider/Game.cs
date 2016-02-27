using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
