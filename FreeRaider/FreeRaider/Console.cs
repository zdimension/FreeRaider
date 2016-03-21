using System;
using System.Collections.Generic;
using System.Linq;
using FreeRaider.Loader;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

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
        private FontTexture font;

        private float[] backgroundColor = new float[4];

        private int historyPos;

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
        private bool showCursor;

        /// <summary>
        /// Ready-to-use flag
        /// </summary>
        private bool inited;

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

        private static ConsoleInfo _instance;

        public static ConsoleInfo Instance => _instance ?? (_instance = new ConsoleInfo());

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
            LineHeight = (short)((1 + spacing) * font.FontSize);
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
                y += LineHeight;
                font.GLFontColor = Global.FontManager.GetFontStyle(line.StyleID).RealColor;
                GLF.RenderStr(font, x, y, line.Text);
                n++;
                if (n >= visibleLines)
                    break;
            }
            font.GLFontColor = Global.FontManager.GetFontStyle(FontStyle.ConsoleInfo).RealColor;
            GLF.RenderStr(font, x, CursorY + LineHeight, editingLine);
        }

        public void DrawBackground()
        {
            // draw console background to see the text
            Gui.DrawRect(0, CursorY + LineHeight - 8, Global.ScreenInfo.W, Global.ScreenInfo.H, backgroundColor,
                backgroundColor, backgroundColor, backgroundColor, BlendingMode.Screen);

            // draw finalise line
            var white = new[] {1, 1, 1, 0.7f};
            Gui.DrawRect(0, CursorY + LineHeight - 8, Global.ScreenInfo.W, 2, white, white, white, white, BlendingMode.Screen);
        }

        public void DrawCursor()
        {
            if(BlinkPeriod != 0)
            {
                blinkTime += Global.EngineFrameTime;
                if(blinkTime > BlinkPeriod)
                {
                    blinkTime = 0;
                    showCursor = !showCursor;
                }
            }

            if(showCursor)
            {
                var white = new[] { 1, 1, 1, 0.7f };
                Gui.DrawRect(cursorX, CursorY + LineHeight * 0.9f, 1, LineHeight * 0.8f, white, white, white, white, BlendingMode.Screen);
            }
        }

        public void Filter(string text)
        {
            foreach (var c in text)
            {
                Edit(c);
            }
        }

        public void Edit(int key, int mod = -1)
        {
            if (key == '`' || key == '\\' || !inited)
            {
                return;
            }

            if (key == '\n')
            {
                AddLog(editingLine);
                AddLine("> " + editingLine, FontStyle.ConsoleInfo);
                Engine.ExecCmd(editingLine);
                editingLine = "";
                cursorPos = 0;
                cursorX = 8 + 1;
                return;
            }

            blinkTime = 0;
            showCursor = true;

            var oldLength = Helper.UTF8StrLen(editingLine);
            var k = (Key) key;
            switch (k)
            {
                case Key.Up:
                case Key.Down:
                    if (historyLines.Count == 0) break;
                    Audio.Send((uint)Global.EngineLua.GetGlobalSound((int)TR_AUDIO_SOUND_GLOBALID.MenuPage));
                    if (k == Key.Up && historyPos < historyLines.Count)
                        historyPos++;
                    else if (k == Key.Down && historyPos > 0)
                        historyPos--;
                    editingLine = historyPos > 0 ? historyLines[historyPos - 1] : "";
                    cursorPos = (short) Helper.UTF8StrLen(editingLine);
                    break;
                case Key.Left:
                    if (cursorPos > 0)
                        cursorPos--;
                    break;
                case Key.Right:
                    if (cursorPos < oldLength)
                        cursorPos++;
                    break;
                case Key.Home:
                    cursorPos = 0;
                    break;
                case Key.End:
                    cursorPos = (short) oldLength;
                    break;
                case Key.BackSpace:
                    if (cursorPos > 0)
                    {
                        editingLine = editingLine.Remove(cursorPos - 1, 1);
                        cursorPos--;
                    }
                    break;
                case Key.Delete:
                    if (cursorPos < oldLength)
                    {
                        editingLine = editingLine.Remove(cursorPos, 1);
                    }
                    break;
                case Key.Tab:
                    var needle = editingLine.Substring(0, cursorPos);
                    // find auto-completion terms, case-insensitive
                    var found = CompletionItems.Where(x => x.StartsWithLowercase(needle)).ToList();
                    if (found.Count == 0)
                    {
                        // no completion, do nothing
                    }
                    else if (found.Count == 1)
                    {
                        // if we have only one term found, use it!
                        var completion = found[0];
                        editingLine = editingLine.Remove(0, completion.Length);
                        editingLine = editingLine.Insert(0, completion);
                        cursorPos = (short) completion.Length;
                    }
                    else
                    {
                        // else we must find the common completion string
                        for (var i = 0; i < found.Count; i++)
                        {
                            // cut off the needle part
                            found[i] = found[i].Remove(0, needle.Length);
                        }
                        // now find a common start
                        var common = found[0];
                        for (var i = 1; common.Length != 0 && i < found.Count; i++)
                        {
                            // cut off from the end that's not common with current
                            for (var j = 0; j < Math.Min(common.Length, found[i].Length); j++)
                            {
                                if (char.ToLower(common[j]) != char.ToLower(found[i][j]))
                                {
                                    common = common.Remove(j);
                                    break;
                                }
                            }
                        }
                        if (common.Length == 0)
                        {
                            // nothing common, print possible completions
                            AddLine("Possible completions:", FontStyle.ConsoleInfo);
                            foreach (var term in found)
                            {
                                AddLine("* " + needle + term, FontStyle.ConsoleInfo);
                            }
                        }
                        else
                        {
                            editingLine = editingLine.Insert(cursorPos, common);
                            cursorPos += (short) common.Length;
                        }
                    }
                    break;
                default:
                    if (k == Key.V && mod > 0 && mod.HasFlagSig((int) KeyModifiers.Control))
                    {
                        var clipboard = Helper.GetClipboardText();
                        if(!string.IsNullOrWhiteSpace(clipboard))
                        {
                            var textLength = Helper.UTF8StrLen(clipboard);
                            if(oldLength < LineSize - textLength)
                            {
                                editingLine = editingLine.Insert(cursorPos, clipboard);
                                cursorPos += (short)textLength;
                            }
                        }
                    }
                    else if(mod < 0 && oldLength < LineSize - 1 && k >= Key.Space)
                    {
                        editingLine = editingLine.Insert(cursorPos, ((char) key).ToString());
                        cursorPos++;
                    }
                    break;
            }

            CalcCursorPosition();
        }

        public void CalcCursorPosition()
        {
            if(font != null)
            {
                cursorX = (short)(8 + 1 + GLF.GetStringLen(font, editingLine, cursorPos));
            }
        }

        public void AddLog(string text)
        {
            if(inited && !string.IsNullOrWhiteSpace(text))
            {
                historyLines.Insert(0, text);
                if(historyLines.Count > BufferSize)
                    historyLines.Resize(BufferSize);
                historyPos = 0;
            }
        }

        public void AddLine(string text, FontStyle style)
        {
            if(inited && !string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("CON: " + text);
                lines.Insert(0, new Line(text, style));
                historyPos = 0;
            }
        }

        public void AddText(string text, FontStyle style)
        {
            foreach (var ln in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                AddLine(ln, style);
            }
        }

        public void Printf(string fmt, params object[] args)
        {
            AddLine(Helper.Format(fmt, args), FontStyle.ConsoleNotify);
        }

        public void Warning(int warnStringIndex, params object[] args)
        {
            var fmt = Global.EngineLua.GetSysNotify(warnStringIndex);
            AddLine(Helper.Format(fmt, args), FontStyle.ConsoleWarning);
        }

        public void Notify(int notifyStringIndex, params object[] args)
        {
            var fmt = Global.EngineLua.GetSysNotify(notifyStringIndex);
            AddLine(Helper.Format(fmt, args), FontStyle.ConsoleNotify);
        }

        public void Clean()
        {
            lines.Clear();
        }

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

    public partial class Extensions
    {
        public static bool StartsWithLowercase(this string haystack, string needle)
        {
            return haystack.StartsWith(needle, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
