using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int CON_MIN_LOG = 16;
        public const int CON_MAX_LOG = 128;

        public const int CON_MIN_LINES = 64;
        public const int CON_MAX_LINES = 256;

        public const ushort CON_MIN_LINE_SIZE = 80;
        public const ushort CON_MAX_LINE_SIZE = 256;

        public const float CON_MIN_LINE_INTERVAL = 0.5f;
        public const float CON_MAX_LINE_INTERVAL = 4.0f;
    }

    public class ConsoleInfo
    {
        private class Line
        {
            public string Text = "";

            public FontStyle StyleID = FontStyle.Generic;

            public Line()
            {
            }

            public Line(string t, FontStyle s = FontStyle.Generic)
            {
                Text = t;
                StyleID = s;
            }
        }

        /// <summary>
        /// Texture font renderer
        /// </summary>
        private FontTexture font = null;

        private float[] backgroundColor = new float[4];

        private int historyPos = 0;

        private List<string> historyLines;
        
        private List<Line> lines;

        private int visibleLines = 40;

        private float spacing = Constants.CON_MIN_LINE_INTERVAL;

        /// <summary>
        /// Current cursor position, in symbols
        /// </summary>
        private short cursorPos;

        /// <summary>
        /// Current cursor in pixels
        /// </summary>
        private short cursorX;

        /// <summary>
        /// Current cursor draw time
        /// </summary>
        private float blinkTime;

        /// <summary>
        /// Cursor visibility flag
        /// </summary>
        private sbyte showCursor;

        /// <summary>
        /// Ready-to-use flag
        /// </summary>
        private bool inited = false;

        private string editingLine;

        

        private ConsoleInfo()
        {
        }


        public void Init()
        {
            // log size check
            if(historyLines.Count > Constants.CON_MAX_LOG)
                historyLines.Resize(Constants.CON_MAX_LOG);

            // spacing check
            spacing = spacing.Clamp(Constants.CON_MIN_LINE_INTERVAL, Constants.CON_MAX_LINE_INTERVAL);

            // linesize check
            LineSize = LineSize.Clamp(Constants.CON_MIN_LINE_SIZE, Constants.CON_MAX_LINE_SIZE);

            inited = true;
        }

        public static ConsoleInfo Instance => new ConsoleInfo();

        ~ConsoleInfo();

        public void InitFonts()
        {
            font = Global.FontManager.GetFont(FontType.Console);
            SetLineInterval(spacing);
        }

        public void InitGlobals()
        {
            backgroundColor = new[] {1.0f, 0.9f, 0.7f, 0.4f};

            spacing = Constants.CON_MIN_LINE_INTERVAL;
            LineSize = Constants.CON_MIN_LINE_SIZE;

            BlinkPeriod = 0.5f;
        }

        public void SetLineInterval(float interval)
        {
            if (!inited || Global.FontManager == null ||
                !interval.IsBetween(Constants.CON_MIN_LINE_INTERVAL, Constants.CON_MAX_LINE_INTERVAL))
                return; // nothing to do

            inited = false;
            spacing = interval;
            // font.FontSize has absolute size (after scaling)
            LineHeight = (1 + spacing) * font.FontSize;
            cursorX = 8 + 1;
            CursorY = Math.Min((short)(Global.ScreenInfo.H - LineHeight * visibleLines), (short)8);
            inited = true;
        }

        public void Draw()
        {
            if (Global.FontManager == null || !inited || !IsVisible)
                return;

            DrawBackground();
            DrawCursor();

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            TextShaderDescription shader = Global.Renderer.ShaderManager.GetTextShader();
            GL.UseProgram(shader.Program);
            GL.Uniform1(shader.Sampler, 0);
            var screenSize = new float[] {Global.ScreenInfo.W, Global.ScreenInfo.H};
            GL.Uniform2(shader.ScreenSize, 1, screenSize);

            var x = 8;
            var y = CursorY + LineHeight;
            var n = 0;
            foreach (var line in lines)
            {
                var col = Global.FontManager.GetFontStyle(line.StyleID).RealColor;
                y += LineHeight;
                font.GLFontColour = col;
                
            }
        }

        public void DrawBackground();

        public void DrawCursor();

        public void Filter(string text);

        public void Edit(int key, int mod = -1);

        public void CalcCursorPosition();

        public void AddLog(string text);

        public void AddLine(string text, FontStyle style);

        public void AddText(string text, FontStyle style);

        public void Printf(string fmt);

        public void Warning(int warnStringIndex);

        public void Notify(int notifyStringIndex);

        public void Clean();

        /// <summary>
        /// Visibility flag
        /// </summary>
        public bool IsVisible { get; set; }

        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        /// <summary>
        /// Line spacing
        /// </summary>
        public float Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (value.IsBetween(Constants.CON_MIN_LINE_INTERVAL, Constants.CON_MAX_LINE_INTERVAL)) spacing = value;
            }
        }

        public float BlinkPeriod { get; set; }

        /// <summary>
        /// Console line size
        /// </summary>
        public ushort LineSize { get; set; } = Constants.CON_MAX_LINE_SIZE;

        public int VisibleLines
        {
            get
            {
                return visibleLines;
            }
            set
            {
                visibleLines = value;
                CursorY = (short) (visibleLines * LineHeight);
            }
        }

        public int BufferSize { get; set; }

        public int HistorySize { get; set; }

        /// <summary>
        /// Height, including spacing
        /// </summary>
        public short LineHeight { get; private set; }

        public short CursorY { get; set; }

        public void SetBackgroundColor(float r, float g, float b, float a)
        {
            backgroundColor = new[] {r, g, b, a};
        }

        public List<string> CompletionItems { get; set; }
    }
}
