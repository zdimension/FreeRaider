using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int CON_MIN_LOG = 16;
        public const int CON_MAX_LOG = 128;

        public const int CON_MIN_LINES = 64;
        public const int CON_MAX_LINES = 256;

        public const int CON_MIN_LINE_SIZE = 80;
        public const int CON_MAX_LINE_SIZE = 256;

        public const float CON_MIN_LINE_INTERVAL = 0.5f;
        public const float CON_MAX_LINE_INTERVAL = 4.0f;
    }

    public class ConsoleInfo
    {
        private class Line
        {
            public string Text = "";

            public FontStyle
        }
    }
}
