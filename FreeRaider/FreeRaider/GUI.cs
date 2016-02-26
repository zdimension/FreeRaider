using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FreeRaider.Loader;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int MaxTempLines = 256;

        // Screen metering resolution specifies user-friendly relative dimensions of screen,
        // which are not dependent on screen resolution. They're primarily used to parse
        // bar and string dimensions.

        public const float ScreenMeteringResolution = 1000.0f;

        public const int MaxFonts = 8; // 8 fonts is PLENTY.

        public const int MinFontSize = 1;
        public const int MaxFontSize = 72;

        public const float FontFadeSpeed = 1.0f; // Global fading style speed.
        public const float FontFadeMin = 0.3f; // Minimum fade multiplier.

        public const float FontShadowTransparency = 0.7f;
        public const float FontShadowVerticalShift = -0.9f;
        public const float FontShadowHorizontalShift = 0.7f;

        // Default line size is generally used for static in-game strings. Strings
        // that are created dynamically may have variable string sizes.

        public const int LineDefaultSize = 128;

        // Offscreen divider specifies how far item notifier will be placed from
        // the final slide position. Usually it's enough to be 1/8 of the screen
        // width, but if you want to increase or decrease notifier size, you must
        // change this value properly.

        public const float GUI_NOTIFIER_OFFSCREEN_DIVIDER = 8.0f;

        // Notifier show time is a time notifier stays on screen (excluding slide
        // effect). Maybe it's better to move it to script later.

        public const float GUI_NOTIFIER_SHOWTIME = 2.0f;
    }

    // Anchoring is needed to link specific GUI element to specific screen position,
    // independent of screen resolution and aspect ratio. Vertical and horizontal
    // anchorings are seperated, so you can link element at any place - top, bottom,
    // center, left or right.
    public enum VerticalAnchor
    {
        Top,
        Bottom,
        Center
    }

    public enum HorizontalAnchor
    {
        Left,
        Right,
        Center
    }

    // Horizontal alignment is simple side alignment, like in original TRs.
    // It means that X coordinate will be either used for left, right or
    // center orientation.
    public enum LineAlignment
    {
        Left,
        Right,
        Center
    }

    public enum FaderDir
    {
        In, // Normal fade-in.
        Out, // Normal fade-out.
        Timed // Timed fade: in -> stay -> out.
    }

    // Scale type specifies how textures with various aspect ratios will be
    // handled. If scale type is set to ZOOM, texture will be zoomed up to
    // current screen's aspect ratio. If type is LETTERBOX, empty spaces
    // will be filled with bars of fader's color. If type is STRETCH, image
    // will be simply stretched across whole screen.
    // ZOOM type is the best shot for loading screens, while LETTERBOX is
    // needed for pictures with crucial info that shouldn't be cut by zoom,
    // and STRETCH type is usable for full-screen effects, like vignette.

    public enum FaderScale
    {
        Zoom,
        LetterBox,
        Stretch
    }

    public enum FaderStatus
    {
        Invalid,
        Idle,
        Fading,
        Complete
    }

    public enum FaderCorner
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }


    // FreeRaider has three types of fonts - primary, secondary and console
    // font. This should be enough for most of the cases. However, user
    // can generate and use additional font types via script, but engine
    // behaviour with extra font types is undefined.

    public enum FontType
    {
        Primary,
        Secondary,
        Console
    }

    // This is predefined enumeration of font styles, which can be extended
    // with user-defined script functions.
    // TODO: add system message console style
    public enum FontStyle
    {
        ConsoleInfo,
        ConsoleWarning,
        ConsoleEvent,
        ConsoleNotify,
        MenuTitle,
        MenuHeading1,
        MenuHeading2,
        MenuItemActive,
        MenuItemInactive,
        MenuContent,
        StatsTitle,
        StatsContent,
        Notifier,
        SavegameList,
        Generic,
        Sentinel
    }

    // Font struct contains additional field for font type which is
    // used to dynamically create or delete fonts.
    public class Font
    {
        public FontType Index;

        public ushort Size;

        public FontTexture GLFont;
    }

    // Font style is different to font itself - whereas engine can have
    // only three fonts, there could be unlimited amount of font styles.
    // Font style management is done via font manager.
    public class FontStyleData
    {
        /// <summary>
        /// Unique index which is used to identify style
        /// </summary>
        public FontStyle Index;

        public float[] Color = new float[4];

        public float[] RealColor = new float[4];

        public float[] RectColor = new float[4];

        public float RectBorder;

        public bool Shadowed;

        public bool Rect;

        /// <summary>
        /// TR4-like looped fading font effect
        /// </summary>
        public bool Fading;

        /// <summary>
        /// Used to bypass certain GUI lines easily
        /// </summary>
        public bool Hidden;
    }

    public class FontManager
    {
        public FontManager();

        ~FontManager();

        public bool AddFont(FontType index, uint size, string path);

        public bool RemoveFont(FontType index);

        public FontTexture GetFont(FontType index);

        public bool AddFontStyle(
            FontStyle index,
            float[] color,
            bool shadow, bool fading,
            bool rect, float rectBorder,
            float[] rectColor,
            bool hide);

        public bool RemoveFontStyle(FontStyle index);

        public FontStyleData GetFontStyle(FontStyle index);

        public uint FontCount => (uint) fonts.Count;

        public uint FontStyleCount => (uint) styles.Count;

        /// <summary>
        /// Do fading routine here, etc. Put into GUI.Update, maybe
        /// </summary>
        public void Update();

        /// <summary>
        /// Resize fonts on window resize event
        /// </summary>
        public void Resize();

        private Font getFontAddress(FontType index);

        /// <summary>
        /// Multiplier used with font RGB values to animate fade
        /// </summary>
        private float fadeValue;

        private bool fadeDirection;

        private List<FontStyleData> styles;

        private List<Font> fonts;

        /// <summary>
        /// GLF font library unit
        /// </summary>
        private FTLibrary fontLibrary;
    }

    public class TextLine
    {
        public string Text;

        public FontType FontID;

        public FontStyle StyleID;

        public float X;

        public HorizontalAnchor Xanchor;

        public float AbsXoffset;

        public float Y;

        public VerticalAnchor Yanchor;

        public float AbsYoffset;

        public float[] Rect = new float[4];

        public bool Show;

        public TextLine Next;

        public TextLine Previous;
    }

    public class Rect
    {
        public float[] Rect = new float[4];

        public float[] AbsRect = new float[4];

        public float X;

        public float AbsX;

        public float Y;

        public float AbsY;

        public sbyte Align;

        public uint Texture;

        public float[] Color = new float[16];

        public uint BlendingMode;

        public short LineCount;

        public TextLine Lines;

        public sbyte State;

        public sbyte Show;

        public float CurrentAlpha;

        public sbyte Focused;

        public sbyte FocusIndex;

        public sbyte Selectable;

        public sbyte SelectionIndex;

        public string LuaClickFunction;
    }

    // Fader is a simple full-screen rectangle, which always sits above the scene,
    // and, when activated, either shows or hides gradually - hence, creating illusion
    // of fade in and fade out effects.
    // TR1-3 had only one type of fader - black one, which was activated on level
    // transitions. Since TR4, additional colored fader was introduced to emulate
    // various full-screen effects (flashes, flares, and so on).
    // With OpenTomb, we extend fader functionality to support not only simple dip to
    // color effect, but also various advanced parameters - texture, delay and variable
    // fade-in and fade-out speeds.

    // Immutable fader enumeration.
    // These faders always exist in engine, and rarely you will need more than these.

    public enum FaderType
    {
        Effect, // Effect fader (flashes, etc.)
        Sun, // Sun fader (engages on looking at the sun)
        Vignette, // Just for fun - death fader.
        LoadScreen, // Loading screen
        Black, // Classic black fader
        Sentinel
    }

    /// <summary>
    /// Main fader class
    /// </summary>
    public class Fader
    {
        /// <summary>
        /// Fader constructor
        /// </summary>
        public Fader()
        {
            SetColor(0, 0, 0);
            SetBlendingMode(BlendingMode.Opaque);
            SetAlpha(255);
            SetSpeed(500);
            SetDelay(0);

            active = false;
            complete = true; // All faders must be initialized as complete to receive proper start-up callbacks.
            direction = FaderDir.In;

            texture = 0;
        }

        /// <summary>
        /// Shows and updates fader
        /// </summary>
        public void Show()
        {
            if (!active)
            {
                complete = true;
                return; // If fader is not active, don't render it.
            }

            if (direction == FaderDir.In)
            {
                if (currentAlpha > 0.0f) // If alpha is more than zero, continue to fade.
                {
                    currentAlpha -= Global.EngineFrameTime * speed;
                }
                else
                {
                    complete = true; // We've reached zero alpha, complete and disable fader.
                    active = false;
                    currentAlpha = 0.0f;
                    dropTexture();
                }
            }
            else if (direction == FaderDir.Out)
            {
                if(currentAlpha < maxAlpha) // If alpha is less than maximum, continue to fade.
                {
                    currentAlpha += Global.EngineFrameTime * speed;
                }
                else
                {
                    // We've reached maximum alpha, so complete fader but leave it active.
                    // This is needed for engine to receive proper callback in case some events are
                    // delayed to the next frame - e.g., level loading.

                    complete = true;
                    currentAlpha = maxAlpha;
                }
            }
            else
            {
                if(currentTime <= maxTime)
                {
                    if(currentAlpha == maxAlpha)
                    {
                        currentTime += Global.EngineFrameTime;
                    }
                    else if(currentAlpha < maxAlpha)
                    {
                        currentAlpha += Global.EngineFrameTime * speed;
                    }
                    else
                    {
                        currentAlpha = maxAlpha;
                    }
                }
                else
                {
                    if(currentAlpha > 0.0f)
                    {
                        currentAlpha -= Global.EngineFrameTime * speedSecondary;
                    }
                    else
                    {
                        complete = true; // We've reached zero alpha, complete and disable fader.
                        active = false;
                        currentAlpha = 0.0f;
                        currentTime = 0.0f;
                        dropTexture();
                    }
                }
            }

            // Apply current alpha value to all vertices.
            topLeftColor[3] = topRightColor[3] = bottomLeftColor[3] = bottomRightColor[3] = currentAlpha;

            // Draw the rectangle.
            // We draw it from the very top left corner to the end of the screen.

            if (texture != 0)
            {
                // Texture is always modulated with alpha!
                var texColor = new[] {currentAlpha, currentAlpha, currentAlpha, currentAlpha};

                if(textureScaleMode == FaderScale.LetterBox)
                {
                    if(textureWide)
                    {
                        // Draw lower letterbox.
                        Gui.DrawRect(
                            0.0f,
                            0.0f,
                            Global.ScreenInfo.W,
                            (Global.ScreenInfo.H - Global.ScreenInfo.W * textureAspectRatio) / 2,
                            bottomLeftColor, bottomRightColor, bottomLeftColor, bottomRightColor,
                            blendingMode);

                        // Draw texture.
                        Gui.DrawRect(
                            0.0f,
                            (Global.ScreenInfo.H - Global.ScreenInfo.W * textureAspectRatio) / 2,
                            Global.ScreenInfo.W,
                            Global.ScreenInfo.W * textureAspectRatio,
                            texColor, texColor, texColor, texColor,
                            blendingMode,
                            texture);

                        // Draw upper letterbox.
                        Gui.DrawRect(
                            0.0f,
                            Global.ScreenInfo.H - (Global.ScreenInfo.H - Global.ScreenInfo.W * textureAspectRatio) / 2,
                            Global.ScreenInfo.W,
                            (Global.ScreenInfo.H - Global.ScreenInfo.W * textureAspectRatio) / 2,
                            topLeftColor, topRightColor, topLeftColor, topRightColor,
                            blendingMode);
                    }
                    else
                    {
                        // Draw left pillarbox.
                        Gui.DrawRect(
                            0.0f,
                            0.0f,
                            (Global.ScreenInfo.W - Global.ScreenInfo.H / textureAspectRatio) / 2,
                            Global.ScreenInfo.H,
                            topLeftColor, topLeftColor, bottomLeftColor, bottomLeftColor,
                            blendingMode);

                        // Draw texture.
                        Gui.DrawRect(
                            (Global.ScreenInfo.W - Global.ScreenInfo.H / textureAspectRatio) / 2,
                            0.0f,
                            Global.ScreenInfo.H / textureAspectRatio,
                            Global.ScreenInfo.H,
                            texColor, texColor, texColor, texColor,
                            blendingMode,
                            texture);

                        // Draw right letterbox.
                        Gui.DrawRect(
                            Global.ScreenInfo.W - (Global.ScreenInfo.W - Global.ScreenInfo.H / textureAspectRatio) / 2,
                            0.0f,
                            (Global.ScreenInfo.W - Global.ScreenInfo.H / textureAspectRatio) / 2,
                            Global.ScreenInfo.H,
                            topRightColor, topRightColor, bottomRightColor, bottomRightColor,
                            blendingMode);
                    }
                }
                else if(textureScaleMode == FaderScale.Zoom)
                {
                    if(textureWide)
                    {
                        Gui.DrawRect(
                            -(((Global.ScreenInfo.H / textureAspectRatio) - Global.ScreenInfo.W) / 2),
                            0.0f,
                            Global.ScreenInfo.H / textureAspectRatio,
                            Global.ScreenInfo.H,
                            texColor, texColor, texColor, texColor,
                            blendingMode,
                            texture);
                    }
                    else
                    {
                        Gui.DrawRect(
                        0.0f,
                        -(((Global.ScreenInfo.W / textureAspectRatio) - Global.ScreenInfo.H) / 2),
                        Global.ScreenInfo.W,
                        Global.ScreenInfo.W / textureAspectRatio,
                        texColor, texColor, texColor, texColor,
                        blendingMode,
                        texture);
                    }
                }
                else
                {
                    Gui.DrawRect(
                        0.0f,
                        0.0f,
                        Global.ScreenInfo.W,
                        Global.ScreenInfo.H,
                        texColor, texColor, texColor, texColor,
                        blendingMode,
                        texture);
                }
            }
            else
            {
                Gui.DrawRect(
                    0.0f, 
                    0.0f, 
                    Global.ScreenInfo.W, 
                    Global.ScreenInfo.H,
                    topLeftColor, topRightColor, bottomLeftColor, bottomRightColor,
                    blendingMode);
            }
        }

        /// <summary>
        /// Resets and starts fader
        /// </summary>
        /// <param name="fadeDir"></param>
        public void Engage(FaderDir fadeDir)
        {
            direction = fadeDir;
            active = true;
            complete = false;
            currentTime = 0.0f;

            if(direction == FaderDir.In)
            {
                currentAlpha = maxAlpha; // Fade in: set alpha to maximum
            }
            else
            {
                currentAlpha = 0.0f; // Fade out or timed: set alpha to zero
            }
        }

        /// <summary>
        /// Immediately cuts fader
        /// </summary>
        public void Cut()
        {
            active = false;
            complete = false;
            currentAlpha = 0.0f;
            currentTime = 0.0f;

            dropTexture();
        }

        /// <summary>
        /// Gets current state of the fader
        /// </summary>
        /// <returns></returns>
        public FaderStatus IsFading()
        {
            if(complete)
            {
                return FaderStatus.Complete;
            }
            else if(active)
            {
                return FaderStatus.Fading;
            }
            else
            {
                return FaderStatus.Idle;
            }
        }

        public void SetScaleMode(FaderScale mode = FaderScale.Zoom)
        {
            textureScaleMode = mode;
        }

        public void SetColor(byte r, byte g, byte b, FaderCorner corner = FaderCorner.None)
        {
            // Each corner of the fader could be colored independently, thus allowing
            // to create gradient faders. It is nifty yet not so useful feature, so
            // it is completely optional - if you won't specify corner, color will be
            // set for the whole fader.

            var clr = new[] { r / 255.0f, g / 255.0f, b / 255.0f };
            switch (corner)
            {
                case FaderCorner.TopLeft:
                    topLeftColor = clr;
                    break;
                case FaderCorner.TopRight:
                    topRightColor = clr;
                    break;
                case FaderCorner.BottomLeft:
                    bottomLeftColor = clr;
                    break;
                case FaderCorner.BottomRight:
                    bottomRightColor = clr;
                    break;
                default:
                    topLeftColor = topRightColor = bottomLeftColor = bottomLeftColor = clr;
                    break;
            }
        }

        public void SetBlendingMode(BlendingMode mode = BlendingMode.Opaque)
        {
            blendingMode = mode;
        }

        public void SetAlpha(byte alpha = 255)
        {
            maxAlpha = alpha / 255.0f;
        }

        public void SetSpeed(ushort fadeSpeed, ushort fadeSpeedSecondary = 200)
        {
            speed = 1000.0f / fadeSpeed;
            speedSecondary = 1000.0f / fadeSpeedSecondary;
        }

        public void SetDelay(uint delayMs)
        {
            maxTime = delayMs / 1000.0f;
        }

        public bool SetTexture(string texturePath)
        {
            Bitmap bmp = null;
            try
            {
                bmp = (Bitmap) Image.FromFile(texturePath);
                var textureFormat = PixelFormat.Bgra;
                var colorDepth = PixelInternalFormat.Rgba;
                
                // Drop previously assigned texture, if exists
                dropTexture();

                // Have OpenGL generate a texture object handle for us
                texture = (uint)GL.GenTexture();

                // Bind the texture object
                GL.BindTexture(TextureTarget.Texture2D, texture);

                // Set the texture's stretching properties
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                // Edit the texture object's image data using the information SDL_Surface gives us
                var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
            }
            catch (Exception ex)
            {
                bmp = null;

                ConsoleInfo.Instance.Warning(Strings.SYSWARN_IMG_NOT_LOADED_SDL, texturePath, ex.Message);
            }

            if (bmp == null)
            {
                if (GL.IsTexture(texture))
                {
                    GL.DeleteTexture(texture);
                }
                texture = 0;

                return false;
            }
            else
            {
                // Set additional parameters
                textureWidth = (ushort) bmp.Width;
                textureHeight = (ushort) bmp.Height;

                setAspect();

                ConsoleInfo.Instance.Notify(Strings.SYSNOTE_LOADED_FADER, texturePath);
                bmp.Dispose();
                return true;
            }
        }


        private void setAspect()
        {
            if(texture != 0)
            {
                if((float)textureWidth / Global.ScreenInfo.W >= (float)textureHeight / Global.ScreenInfo.H)
                {
                    textureWide = true;
                    textureAspectRatio = (float) textureHeight / textureWidth;
                }
                else
                {
                    textureWide = false;
                    textureAspectRatio = (float)textureWidth / textureHeight;
                }
            }
        }

        private bool dropTexture()
        {
            if(texture != 0)
            {
                if(GL.IsTexture(texture))
                {
                    GL.DeleteTexture(texture);
                }
                texture = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        // All colors are defined separately, for
        // further possibility of advanced full
        // screen effects with gradients.

        private float[] topLeftColor = new float[4];

        private float[] topRightColor = new float[4];

        private float[] bottomLeftColor = new float[4];

        private float[] bottomRightColor = new float[4];

        /// <summary>
        /// Fader's blending mode
        /// </summary>
        private BlendingMode blendingMode;

        /// <summary>
        /// Current alpha value
        /// </summary>
        private float currentAlpha;

        /// <summary>
        /// Maximum reachable alpha value
        /// </summary>
        private float maxAlpha;

        /// <summary>
        /// Fade speed
        /// </summary>
        private float speed;

        /// <summary>
        /// Secondary speed - used with TIMED type
        /// </summary>
        private float speedSecondary;

        /// <summary>
        /// Texture (optional)
        /// </summary>
        private uint texture;

        private ushort textureWidth;

        private ushort textureHeight;

        /// <summary>
        /// Set if texture width is greater than height
        /// </summary>
        private bool textureWide;

        /// <summary>
        /// Pre-calculated aspect ratio
        /// </summary>
        private float textureAspectRatio;

        /// <summary>
        /// Fader texture's scale mode
        /// </summary>
        private FaderScale textureScaleMode;

        /// <summary>
        /// Specifies if fader is active or not
        /// </summary>
        private bool active;

        /// <summary>
        /// Specifies if fading is complete or not
        /// </summary>
        private bool complete;

        /// <summary>
        /// Specifies fade direction
        /// </summary>
        private FaderDir direction;

        /// <summary>
        /// Current fader time
        /// </summary>
        private float currentTime;

        /// <summary>
        /// Maximum delay time
        /// </summary>
        private float maxTime;
    }

    // Immutable bars enumeration.
    // These are the bars that are always exist in GUI.
    // Scripted bars could be created and drawn separately later.

    public enum BarType
    {
        Health, // TR 1-5
        Air, // TR 1-5, alternate state - gas (TR5)
        Stamina, // TR 3-5
        Warmth, // TR 3 only
        Loading,
        Sentinel
    }

    // Bar color types.
    // Each bar part basically has two colours - main and fade.

    public enum BarColorType
    {
        BaseMain,
        BaseFade,
        AltMain,
        AltFade,
        BackMain,
        BackFade,
        BorderMain,
        BorderFade
    }

    // Main bar class
    public class ProgressBar
    {
        /// <summary>
        /// Bar constructor
        /// </summary>
        public ProgressBar();

        /// <summary>
        /// Main show bar procedure
        /// </summary>
        public void Show(float value);

        public void Resize();

        public void SetColor(BarColorType colType, byte r, byte g, byte b, byte alpha);

        public void SetSize(float width, float height, float borderSize);

        public void SetPosition(HorizontalAnchor anchorX, float offsetX, VerticalAnchor anchorY, float offsetY);

        public void SetValues(float maxValue, float warnValue);

        public void SetBlink(int interval);

        public void SetExtrude(bool enabled, byte depth);

        public void SetAutoshow(bool enabled, int delay, bool fade, int fadeDelay);

        /// <summary>
        /// Forced flag is set when bar is strictly drawn
        /// </summary>
        public bool Forced;

        /// <summary>
        /// Is it visible or not
        /// </summary>
        public bool Visible;

        /// <summary>
        /// Alternate state, in which bar changes color to AltColor
        /// </summary>
        public bool Alternate;

        /// <summary>
        /// Invert decrease direction flag
        /// </summary>
        public bool Invert;

        /// <summary>
        /// Change bar style to vertical
        /// </summary>
        public bool Vertical;


        /// <summary>
        /// Recalculate size
        /// </summary>
        private void recalculateSize();

        /// <summary>
        /// Recalculate position
        /// </summary>
        private void recalculatePosition();

        /// <summary>
        /// Horizontal position
        /// </summary>
        private float x;

        /// <summary>
        /// Vertical position
        /// </summary>
        private float y;

        /// <summary>
        /// Real width
        /// </summary>
        private float width;

        /// <summary>
        /// Real height
        /// </summary>
        private float height;

        /// <summary>
        /// Real border size (horizontal)
        /// </summary>
        private float borderWidth;

        /// <summary>
        /// Real border size (vertical)
        /// </summary>
        private float borderHeight;

        /// <summary>
        /// Horizontal anchoring: left, right or center
        /// </summary>
        private HorizontalAnchor anchorX;

        /// <summary>
        /// Vertical anchoring: top, bottom or center
        /// </summary>
        private VerticalAnchor anchorY;

        /// <summary>
        /// Absolute (resolution-independent) X offset
        /// </summary>
        private float absXoffset;

        /// <summary>
        /// Absolute (resolution-independent) Y offset
        /// </summary>
        private float absYoffset;

        /// <summary>
        /// Absolute width
        /// </summary>
        private float absWidth;

        /// <summary>
        /// Absolute height
        /// </summary>
        private float absHeight;

        /// <summary>
        /// Absolute border size (horizontal)
        /// </summary>
        private float absBorderSize;

        /// <summary>
        /// Color at the min. of bar
        /// </summary>
        private float[] baseMainColor = new float[5];

        /// <summary>
        /// Color at the max. of bar
        /// </summary>
        private float[] baseFadeColor = new float[5];

        /// <summary>
        /// Alternate main color
        /// </summary>
        private float[] altMainColor = new float[5];

        /// <summary>
        /// Alternate fade color
        /// </summary>
        private float[] altFadeColor = new float[5];

        /// <summary>
        /// Background main color
        /// </summary>
        private float[] backMainColor = new float[5];

        /// <summary>
        /// Background fade color
        /// </summary>
        private float[] backFadeColor = new float[5];

        /// <summary>
        /// Border main color
        /// </summary>
        private float[] borderMainColor = new float[5];

        /// <summary>
        /// Border fade color
        /// </summary>
        private float[] borderFadeColor = new float[5];

        /// <summary>
        /// Extrude effect
        /// </summary>
        private bool extrude;

        /// <summary>
        /// Extrude effect depth
        /// </summary>
        private float[] extrudeDepth = new float[5];

        /// <summary>
        /// Maximum possible value
        /// </summary>
        private float maxValue;

        /// <summary>
        /// Warning value, at which bar begins to blink
        /// </summary>
        private float warnValue;

        /// <summary>
        /// Last value back-up for autoshow on change event
        /// </summary>
        private float lastValue;

        /// <summary>
        /// Warning state (blink) flag
        /// </summary>
        private bool blink;

        /// <summary>
        /// Blink interval (speed)
        /// </summary>
        private float blinkInterval;

        /// <summary>
        /// Blink counter
        /// </summary>
        private float blinkCount;

        /// <summary>
        /// Autoshow on change flag
        /// </summary>
        private bool autoShow;

        /// <summary>
        /// How long bar will stay on-screen in Autoshow mode
        /// </summary>
        private float autoShowDelay;

        /// <summary>
        /// Autoshow counter
        /// </summary>
        private float autoShowCount;

        /// <summary>
        /// Fade flag
        /// </summary>
        private bool autoShowFade;

        /// <summary>
        /// Fade length
        /// </summary>
        private float autoShowFadeDelay;

        /// <summary>
        /// Fade progress counter
        /// </summary>
        private float autoShowFadeCount;

        /// <summary>
        /// Range unit used to set base bar size
        /// </summary>
        private float rangeUnit;

        /// <summary>
        /// Base bar size
        /// </summary>
        private float baseSize;

        /// <summary>
        /// Max. / actual value ratio
        /// </summary>
        private float baseRatio;
    }

    public class GuiItemNotifier
    {
        public GuiItemNotifier();

        public void Start(int item, float time);

        public void Reset();

        public void Animate();

        public void Draw();

        public void SetPos(float x, float y);

        public void SetRot(float x, float y);

        public void SetSize(float size);

        public void SetRotateTime(float time);


        private bool active;

        private int item;

        private float absPosY;

        private float absPosX;

        private float posY;

        private float startPosX;

        private float endPosX;

        private float currPosX;

        private float rotX;

        private float rotY;

        private float currRotX;

        private float currRotY;

        private float size;

        private float showTime;

        private float currTime;

        private float rotateTime;
    }

    public class Gui
    {
        public static void InitFontManager()
        {
            Global.FontManager = new FontManager();
        }

        public static void Init()
        {
            InitBars();
            InitFaders();
            InitNotifier();
            InitTempLines();

            Global.CrosshairBuffer = (uint)GL.GenBuffer();
            FillCrosshairBuffer();

            //Global.MainInventoryMenu = new GuiInventoryMenu();
            Global.MainInventoryManager = new InventoryManager();
        }

        public static void Destroy()
        {
            for (var i = 0; i < Constants.MaxTempLines; i++)
            {
                Global.GuiTempLines[i].Show = false;
                Global.GuiTempLines[i].Text = "";
            }

            foreach (var fader in Global.FaderType)
            {
                fader.Value.Cut();
            }

            Global.TempLinesUsed = Constants.MaxTempLines;

            /*
            Global.MainInventoryMenu = null;
            */

            Global.MainInventoryManager = null;
            

            Global.FontManager = null;
            
        }

        public static void InitBars()
        {
            Global.Bar = new Dictionary<BarType, ProgressBar>();
            {
                var pb = new ProgressBar
                {
                    Visible = false,
                    Alternate = false,
                    Invert = false,
                    Vertical = false
                };

                pb.SetSize(250, 15, 3);
                pb.SetPosition(HorizontalAnchor.Left, 30, VerticalAnchor.Top, 30);
                pb.SetColor(BarColorType.BaseMain, 255, 50, 50, 200);
                pb.SetColor(BarColorType.BaseFade, 100, 255, 50, 200);
                pb.SetColor(BarColorType.AltMain, 255, 180, 0, 255);
                pb.SetColor(BarColorType.AltFade, 255, 255, 0, 255);
                pb.SetColor(BarColorType.BackMain, 0, 0, 0, 160);
                pb.SetColor(BarColorType.BackFade, 60, 60, 60, 130);
                pb.SetColor(BarColorType.BorderMain, 200, 200, 200, 50);
                pb.SetColor(BarColorType.BorderFade, 80, 80, 80, 100);
                pb.SetValues(Constants.LARA_PARAM_HEALTH_MAX, Constants.LARA_PARAM_HEALTH_MAX / 3);
                pb.SetBlink(300);
                pb.SetExtrude(true, 100);
                pb.SetAutoshow(true, 2000, true, 400);

                Global.Bar.Add(BarType.Health, pb);
            }
            {
                var pb = new ProgressBar
                {
                    Visible = false,
                    Alternate = false,
                    Invert = true,
                    Vertical = false
                };

                pb.SetSize(250, 15, 3);
                pb.SetPosition(HorizontalAnchor.Right, 30, VerticalAnchor.Top, 30);
                pb.SetColor(BarColorType.BaseMain, 0, 50, 255, 200);
                pb.SetColor(BarColorType.BaseFade, 190, 190, 255, 200);
                pb.SetColor(BarColorType.BackMain, 0, 0, 0, 160);
                pb.SetColor(BarColorType.BackFade, 60, 60, 60, 130);
                pb.SetColor(BarColorType.BorderMain, 200, 200, 200, 50);
                pb.SetColor(BarColorType.BorderFade, 80, 80, 80, 100);
                pb.SetValues(Constants.LARA_PARAM_HEALTH_MAX, Constants.LARA_PARAM_HEALTH_MAX / 3);
                pb.SetBlink(300);
                pb.SetExtrude(true, 100);
                pb.SetAutoshow(true, 2000, true, 400);

                Global.Bar.Add(BarType.Air, pb);
            }
            {
                var pb = new ProgressBar
                {
                    Visible = false,
                    Alternate = false,
                    Invert = false,
                    Vertical = false
                };

                pb.SetSize(250, 15, 3);
                pb.SetPosition(HorizontalAnchor.Left, 30, VerticalAnchor.Top, 55);
                pb.SetColor(BarColorType.BaseMain, 255, 100, 50, 200);
                pb.SetColor(BarColorType.BaseFade, 255, 200, 0, 200);
                pb.SetColor(BarColorType.BackMain, 0, 0, 0, 160);
                pb.SetColor(BarColorType.BackFade, 60, 60, 60, 130);
                pb.SetColor(BarColorType.BorderMain, 110, 110, 110, 100);
                pb.SetColor(BarColorType.BorderFade, 60, 60, 60, 180);
                pb.SetValues(Constants.LARA_PARAM_STAMINA_MAX, 0);
                pb.SetExtrude(true, 100);
                pb.SetAutoshow(true, 500, true, 300);

                Global.Bar.Add(BarType.Stamina, pb);
            }
            {
                var pb = new ProgressBar
                {
                    Visible = false,
                    Alternate = false,
                    Invert = true,
                    Vertical = false
                };

                pb.SetSize(250, 15, 3);
                pb.SetPosition(HorizontalAnchor.Right, 30, VerticalAnchor.Top, 55);
                pb.SetColor(BarColorType.BaseMain, 255, 0, 255, 255);
                pb.SetColor(BarColorType.BaseFade, 190, 120, 255, 255);
                pb.SetColor(BarColorType.BackMain, 0, 0, 0, 160);
                pb.SetColor(BarColorType.BackFade, 60, 60, 60, 130);
                pb.SetColor(BarColorType.BorderMain, 200, 200, 200, 50);
                pb.SetColor(BarColorType.BorderFade, 80, 80, 80, 100);
                pb.SetValues(Constants.LARA_PARAM_WARMTH_MAX, Constants.LARA_PARAM_WARMTH_MAX / 3);
                pb.SetBlink(200);
                pb.SetExtrude(true, 60);
                pb.SetAutoshow(true, 500, true, 300);

                Global.Bar.Add(BarType.Warmth, pb);
            }
            {
                var pb = new ProgressBar
                {
                    Visible = true,
                    Alternate = false,
                    Invert = false,
                    Vertical = false
                };

                pb.SetSize(800, 25, 3);
                pb.SetPosition(HorizontalAnchor.Center, 0, VerticalAnchor.Bottom, 40);
                pb.SetColor(BarColorType.BaseMain, 255, 225, 127, 230);
                pb.SetColor(BarColorType.BaseFade, 255, 187, 136, 230);
                pb.SetColor(BarColorType.BackMain, 30, 30, 30, 100);
                pb.SetColor(BarColorType.BackFade, 60, 60, 60, 100);
                pb.SetColor(BarColorType.BorderMain, 200, 200, 200, 80);
                pb.SetColor(BarColorType.BorderFade, 80, 80, 80, 80);
                pb.SetValues(1000, 0);
                pb.SetExtrude(true, 70);
                pb.SetAutoshow(false, 500, false, 300);

                Global.Bar.Add(BarType.Loading, pb);
            }
        }

        public static void InitFaders()
        {
            Global.FaderType = new Dictionary<FaderType, Fader>();
            {
                var fd = new Fader();
                fd.SetAlpha(255);
                fd.SetColor(0, 0, 0);
                fd.SetBlendingMode(BlendingMode.Opaque);
                fd.SetSpeed(500);
                fd.SetScaleMode(FaderScale.Zoom);

                Global.FaderType.Add(FaderType.LoadScreen, fd);
            }
            {
                var fd = new Fader();
                fd.SetAlpha(255);
                fd.SetColor(255, 180, 0);
                fd.SetBlendingMode(BlendingMode.Multiply);
                fd.SetSpeed(10, 800);

                Global.FaderType.Add(FaderType.Effect, fd);
            }
            {
                var fd = new Fader();
                fd.SetAlpha(255);
                fd.SetColor(0, 0, 0);
                fd.SetBlendingMode(BlendingMode.Opaque);
                fd.SetSpeed(500);
                fd.SetScaleMode(FaderScale.Zoom);

                Global.FaderType.Add(FaderType.Black, fd);
            }
        }

        public static void InitNotifier()
        {
            Global.Notifier.SetPos(850.0f, 850.0f);
            Global.Notifier.SetRot(180.0f, 270.0f);
            Global.Notifier.SetSize(128.0f);
            Global.Notifier.SetRotateTime(2500.0f);
        }

        public static void InitTempLines()
        {
            for (var i = 0; i < Constants.MaxTempLines; i++)
            {
                Global.GuiTempLines[i] = new TextLine
                {
                    Text = "",
                    Show = false,

                    Next = null,
                    Previous = null,

                    FontID = FontType.Secondary,
                    StyleID = FontStyle.Generic
                };
            }
        }

        public struct GuiBufferEntryS
        {
            public float[] Position;

            public byte[] Color;
        }

        public static void FillCrosshairBuffer()
        {
            var crosshairBuf = new[]
            {
                new GuiBufferEntryS
                {
                    Position = new[] {Global.ScreenInfo.W / 2.0f - 5, Global.ScreenInfo.H / 2.0f},
                    Color = new byte[] {255, 0, 0, 255}
                },
                new GuiBufferEntryS
                {
                    Position = new[] {Global.ScreenInfo.W / 2.0f + 5, Global.ScreenInfo.H / 2.0f},
                    Color = new byte[] {255, 0, 0, 255}
                },
                new GuiBufferEntryS
                {
                    Position = new[] {Global.ScreenInfo.W / 2.0f, Global.ScreenInfo.H / 2.0f - 5},
                    Color = new byte[] {255, 0, 0, 255}
                },
                new GuiBufferEntryS
                {
                    Position = new[] {Global.ScreenInfo.W / 2.0f, Global.ScreenInfo.H / 2.0f + 5},
                    Color = new byte[] {255, 0, 0, 255}
                }
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, Global.CrosshairBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) Marshal.SizeOf(crosshairBuf), crosshairBuf,
                BufferUsageHint.StaticDraw);

            var attribs = new[]
            {
                new VertexArrayAttribute((int) GuiShaderDescription.VertexAttribs.Position, 2,
                    VertexAttribPointerType.Float, false, Global.CrosshairBuffer,
                    Marshal.SizeOf(typeof (GuiBufferEntryS)),
                    (int) Marshal.OffsetOf(typeof (GuiBufferEntryS), "Position")),
                new VertexArrayAttribute((int) GuiShaderDescription.VertexAttribs.Color, 4,
                    VertexAttribPointerType.UnsignedByte, true, Global.CrosshairBuffer,
                    Marshal.SizeOf(typeof (GuiBufferEntryS)),
                    (int) Marshal.OffsetOf(typeof (GuiBufferEntryS), "Color"))
            };
            Global.CrosshairArray = new VertexArray(0, attribs);
        }

        public static void AddLine(TextLine line)
        {
            if(Global.GuiBaseLines == null)
            {
                Global.GuiBaseLines = line;
                line.Next = null;
                line.Previous = null;
                return;
            }

            line.Previous = null;
            line.Next = Global.GuiBaseLines;
            Global.GuiBaseLines.Previous = line;
            Global.GuiBaseLines = line;
        }

        public static void DeleteLine(TextLine line)
        {
            if(line == Global.GuiBaseLines)
            {
                Global.GuiBaseLines = line.Next;
                if(Global.GuiBaseLines != null)
                {
                    Global.GuiBaseLines.Previous = null;
                }
                return;
            }

            line.Previous.Next = line.Next;
            if(line.Next != null)
            {
                line.Next.Previous = line.Previous;
            }
        }

        public static void MoveLine(TextLine line)
        {
            line.AbsXoffset = line.X * Global.ScreenInfo.ScaleFactor;
            line.AbsYoffset = line.Y * Global.ScreenInfo.ScaleFactor;
        }

        public static void RenderStringLine(TextLine line)
        {
            float realX = 0, realY = 0;

            if(Global.FontManager == null)
            {
                return;
            }

            var glFont = Global.FontManager.GetFont(line.FontID);
            var style = Global.FontManager.GetFontStyle(line.StyleID);

            if(glFont == null || style == null || !line.Show || style.Hidden)
            {
                return;
            }

            GLF.GetStringBB(glFont, line.Text, -1, ref line.Rect[0], ref line.Rect[1], ref line.Rect[2], ref line.Rect[3]);

            switch(line.Xanchor)
            {
                case HorizontalAnchor.Left:
                    realX = line.AbsXoffset; // Used with center and right alignments.
                    break;
                    case HorizontalAnchor.Right:
                    realX = Global.ScreenInfo.W - (line.Rect[2] - line.Rect[0]) - line.AbsXoffset;
                    break;
                    case HorizontalAnchor.Center:
                    realX = Global.ScreenInfo.W / 2.0f - (line.Rect[2] - line.Rect[0]) / 2.0f + line.AbsXoffset; // Absolute center
                    break;
            }

            switch (line.Yanchor)
            {
                case VerticalAnchor.Bottom:
                    realY = line.AbsYoffset; // Used with center and right alignments.
                    break;
                case VerticalAnchor.Top:
                    realY = Global.ScreenInfo.H - (line.Rect[3] - line.Rect[1]) - line.AbsYoffset;
                    break;
                case VerticalAnchor.Center:
                    realY = Global.ScreenInfo.H / 2.0f + (line.Rect[3] - line.Rect[1]) - line.AbsYoffset; // Absolute center
                    break;
            }

            if(style.Shadowed)
            {
                glFont.GLFontColor = new[] {0, 0, 0, style.Color[3] * Constants.FontShadowTransparency}; // derive alpha from base color
                GLF.RenderStr(glFont, realX + Constants.FontShadowHorizontalShift,
                    realY + Constants.FontShadowVerticalShift, line.Text);
            }

            glFont.GLFontColor = style.RealColor;
            GLF.RenderStr(glFont, realX, realY, line.Text);
        }

        public static void RenderStrings()
        {
            if(Global.FontManager != null)
            {
                var l = Global.GuiBaseLines;

                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                TextShaderDescription shader = Global.Renderer.ShaderManager.GetTextShader();
                GL.UseProgram(shader.Program);
                var screenSize = new float[] {Global.ScreenInfo.W, Global.ScreenInfo.H};
                GL.Uniform2(shader.ScreenSize, 1, screenSize);
                GL.Uniform1(shader.Sampler, 0);

                while(l != null)
                {
                    RenderStringLine(l);
                    l = l.Next;
                }

                for(var i = 0; i < Global.TempLinesUsed; i++)
                {
                    var cl = Global.GuiTempLines[i];
                    if(cl.Show)
                    {
                        RenderStringLine(cl);
                        cl.Show = false;
                    }
                }

                Global.TempLinesUsed = 0;
            }
        }

        /// <summary>
        /// Draws text using a <see cref="FontType.Secondary"/>.
        /// </summary>
        public static TextLine OutTextXY(float x, float y, string fmt, params object[] args)
        {
            if(Global.FontManager != null && Global.TempLinesUsed < Constants.MaxTempLines - 1)
            {

                var l = Global.GuiTempLines[Global.TempLinesUsed];
                l.FontID = FontType.Secondary;
                l.StyleID = FontStyle.Generic;
                l.Text = string.Format(fmt, args);
                l.Next = null;
                l.Previous = null;
                Global.TempLinesUsed++;
                l.X = x;
                l.Y = y;
                l.Xanchor = HorizontalAnchor.Left;
                l.Yanchor = VerticalAnchor.Bottom;
                MoveLine(l);
                l.Show = true;
                return l;
            }

            return null;
        }

        /// <summary>
        /// Helper method to setup OpenGL state for console drawing.
        /// </summary>
        public static void SwitchGLMode(bool isGui)
        {
            if(isGui)
            {
                var farDist = 4096.0f;
                var nearDist = -1.0f;

                Global.GuiProjectionMatrix = new Matrix4
                {
                    [0, 0] = 2.0f / Global.ScreenInfo.W,
                    [1, 1] = 2.0f / Global.ScreenInfo.H,
                    [2, 2] = -2.0f / (farDist - nearDist),
                    [3, 0] = -1.0f,
                    [3, 1] = -1.0f,
                    [3, 2] = -(farDist + nearDist) / (farDist - nearDist)
                };
            }
            else
            {
                Global.GuiProjectionMatrix = Global.EngineCamera.glProjMat;
            }
        }



        /**
         * Draws wireframe of this frustum.
         *
         * Expected state:
         *  - Vertex array is enabled, color, tex coord, normal disabled
         *  - No vertex buffer object is bound
         *  - Texturing is disabled
         *  - Alpha test is disabled
         *  - Blending is enabled
         *  - Lighting is disabled
         * Ignored state:
         *  - Currently bound texture.
         *  - Currently bound element buffer.
         *  - Depth test enabled (disables it, then restores)
         *  - Vertex pointer (changes it)
         *  - Matrices (changes them, restores)
         *  - Line width (changes it, then restores)
         *  - Current color (changes it)
         * Changed state:
         *  - Current position will be arbitrary.
         *  - Vertex pointer will be arbitray.
         *  - Current color will be arbitray (set by console)
         *  - Blend mode will be SRC_ALPHA, ONE_MINUS_SRC_ALPHA (set by console)
         */
        public static void Render()
        {
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthMask(false);

            GL.Disable(EnableCap.DepthTest);
            if(Global.ScreenInfo.ShowDebugInfo) DrawCrosshair();
            DrawBars();
            DrawFaders();
            RenderStrings();
            ConsoleInfo.Instance.Draw();

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
        }

        /**
         *  Draw simple rectangle.
         *  Only state it changes is the blend mode, according to blendMode value.
         */

        public static void DrawRect(
            float x, float y,
            float width, float height,
            float[] colorUpperLeft, float[] colorUpperRight,
            float[] colorLowerLeft, float[] colorLowerRight,
            BlendingMode blendMode,
            uint texture = 0)
        {
            switch (blendMode)
            {
                case BlendingMode.Hide:
                    return;
                case BlendingMode.Multiply:
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                    break;
                case BlendingMode.SimpleShade:
                    GL.BlendFunc(BlendingFactorSrc.OneMinusSrcColor, BlendingFactorDest.OneMinusSrcAlpha);
                    break;
                case BlendingMode.Screen:
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    break;
                default:
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    break;
            }

            if (Global.RectanglePositionBuffer == 0)
            {
                Global.RectanglePositionBuffer = (uint) GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, Global.RectanglePositionBuffer);
                var rectCoords = new[] {0, 0, 1, 0, 1, 1, 0, 1};
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) Marshal.SizeOf(rectCoords), rectCoords,
                    BufferUsageHint.StaticDraw);

                Global.RectangleColorBuffer = (uint) GL.GenBuffer();
                var attribs = new[]
                {
                    new VertexArrayAttribute((int) GuiShaderDescription.VertexAttribs.Position, 2,
                        VertexAttribPointerType.Float, false, Global.RectanglePositionBuffer,
                        Marshal.SizeOf(new float[2]), 0),
                    new VertexArrayAttribute((int) GuiShaderDescription.VertexAttribs.Color, 4,
                        VertexAttribPointerType.Float, false, Global.RectangleColorBuffer, Marshal.SizeOf(new float[4]),
                        0),
                };
                Global.RectangleArray = new VertexArray(0, attribs);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, Global.RectangleColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (Marshal.SizeOf(new float[4]) * 4), IntPtr.Zero,
                BufferUsageHint.StreamDraw);
            unsafe
            {
                var rectColors = (float*) GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
                colorLowerLeft = Helper.GetArrayFromPointer(rectColors + 0, 4);
                colorLowerRight = Helper.GetArrayFromPointer(rectColors + 4, 4);
                colorUpperLeft = Helper.GetArrayFromPointer(rectColors + 8, 4);
                colorUpperRight = Helper.GetArrayFromPointer(rectColors + 12, 4);
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            }

            var offset = new[] {x / (Global.ScreenInfo.W * 0.5f) - 1.0f, y / (Global.ScreenInfo.Y * 0.5f) - 1.0f};
            var factor = new[] {width / Global.ScreenInfo.W * 2.0f, height / Global.ScreenInfo.H * 2.0f};

            GuiShaderDescription shader = Global.Renderer.ShaderManager.GetGuiShader(texture != 0);
            GL.UseProgram(shader.Program);
            GL.Uniform1(shader.Sampler, 0);
            if(texture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texture);
            }
            GL.Uniform2(shader.Offset, 1, offset);
            GL.Uniform2(shader.Factor, 1, factor);

            Global.RectangleArray.Bind();

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        /**
         *  Fader functions.
         */
        public static bool FadeStart(FaderType fader, FaderDir fadeDirection)
        {
            // If fader exists, and is not active, we engage it.

            if(fader < FaderType.Sentinel && Global.FaderType[fader].IsFading() != FaderStatus.Fading)
            {
                Global.FaderType[fader].Engage(fadeDirection);
                return true;
            }
            return false;
        }

        public static bool FadeStop(FaderType fader)
        {
            if (fader < FaderType.Sentinel && Global.FaderType[fader].IsFading() != FaderStatus.Idle)
            {
                Global.FaderType[fader].Cut();
                return true;
            }
            return false;
        }

        public static bool FadeAssignPic(FaderType fader, string picName)
        {
            if (fader >= FaderType.Effect && fader < FaderType.Sentinel)
            {
                /*var buf = picName.Left(Constants.MAX_ENGINE_PATH);

                if(!Engine.FileFound(buf, false))
                {
                    var extLen = 0;

                    for (; extLen + 1 < picName.Length; extLen++)
                    {
                        if(buf[picName.Length - extLen - 1] == '.')
                        {
                            break;
                        }
                    }

                    if(extLen + 1 == picName.Length)
                    {
                        return false;
                    }
                }*/

                if (!Engine.FileFound(picName, false))
                {
                    return false;
                }

                return Global.FaderType[fader].SetTexture(picName);
            }

            return false;
        }

        public static FaderStatus FadeCheck(FaderType fader)
        {
            if(fader >= FaderType.Effect && fader < FaderType.Sentinel)
            {
                return Global.FaderType[fader].IsFading();
            }
            else
            {
                return FaderStatus.Invalid;
            }
        }

        public static void FadeSetup(
            FaderType fader,
            byte alpha, byte r, byte g, byte b,
            BlendingMode blendingMode,
            ushort fadeInSpeed,
            ushort fadeOutSpeed)
        {
            if (fader >= FaderType.Sentinel) return;

            Global.FaderType[fader].SetAlpha(alpha);
            Global.FaderType[fader].SetColor(r, g, b);
            Global.FaderType[fader].SetBlendingMode(blendingMode);
            Global.FaderType[fader].SetSpeed(fadeInSpeed, fadeOutSpeed);
        }

        /**
         * Item notifier functions.
         */
        public static void NotifierStart(int item)
        {
            Notifier.Start(item, Constants.GUI_NOTIFIER_SHOWTIME);
        }

        public static void NotifierStop()
        {
            Notifier.Reset();
        }

        /**
         * General GUI drawing routines.
         */

        public static void DrawCrosshair()
        {
            GuiShaderDescription shader = Global.Renderer.ShaderManager.GetGuiShader(false);

            GL.UseProgram(shader.Program);
            var factor = new[] {2.0f / Global.ScreenInfo.W, 2.0f / Global.ScreenInfo.H};
            GL.Uniform2(shader.Factor, 1, factor);
            var offset = new[] {-1.0f, -1.0f};
            GL.Uniform2(shader.Offset, 1, offset);

            Global.CrosshairArray.Bind();

            GL.DrawArrays(PrimitiveType.Lines, 0, 4);
        }

        public static void DrawFaders()
        {
            foreach (var i in Global.FaderType)
            {
                i.Value.Show();
            }
        }

        public static void DrawBars()
        {
            if(Global.EngineWorld.Character != null)
            {
                if (Global.EngineWorld.Character.WeaponCurrentState > WeaponState.HideToReady)
                    Global.Bar[BarType.Health].Forced = true;

                if (Global.EngineWorld.Character.GetParam(Constants.PARAM_POISON) > 0.0f)
                    Global.Bar[BarType.Health].Alternate = true;

                Global.Bar[BarType.Air].Show(Global.EngineWorld.Character.GetParam(Constants.PARAM_AIR));
                Global.Bar[BarType.Stamina].Show(Global.EngineWorld.Character.GetParam(Constants.PARAM_STAMINA));
                Global.Bar[BarType.Health].Show(Global.EngineWorld.Character.GetParam(Constants.PARAM_HEALTH));
                Global.Bar[BarType.Warmth].Show(Global.EngineWorld.Character.GetParam(Constants.PARAM_WARMTH));
            }
        }

        public static void DrawLoadScreen(int value)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            SwitchGLMode(true);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthMask(false);

            GL.PixelStore(PixelStoreParameter.UnpackLsbFirst, 0);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            Global.FaderType[FaderType.LoadScreen].Show();
            Global.Bar[BarType.Loading].Show(value);

            GL.DepthMask(true);

            SwitchGLMode(false);

            // SDL_GL_SwapWindow(sdl_window);
        }

        public static void DrawInventory()
        {
            Global.MainInventoryManager.Frame(Global.EngineFrameTime);
            if(Global.MainInventoryManager.CurrentState == InventoryState.Disabled)
            {
                return;
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.PushAttrib(AttribMask.EnableBit | AttribMask.PixelModeBit | AttribMask.ColorBufferBit);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.AlphaTest);
            GL.DepthMask(false);

            // background

            var upperColor = new[] {0, 0, 0, 0.45f};
            var lowerColor = new[] {0, 0, 0, 0.75f};

            DrawRect(0, 0, Global.ScreenInfo.W, Global.ScreenInfo.H, upperColor, upperColor, lowerColor, lowerColor,
                BlendingMode.Opaque);

            GL.DepthMask(true);
            GL.PopAttrib();

            SwitchGLMode(false);
            Global.MainInventoryManager.Render();
            SwitchGLMode(true);
        }

        public static void DrawNotifier()
        {
            Notifier.Draw();
            Notifier.Animate();
        }

        /**
         * General GUI update routines.
         */
        public static void Update()
        {
            if(Global.FontManager != null)
            {
                Global.FontManager.Update();
            }
        }

        public static void Resize()
        {
            var l = Global.GuiBaseLines;

            while(l != null)
            {
                MoveLine(l);

                l = l.Next;
            }

            for (var i = 0; i < Global.TempLinesUsed; i++)
            {
                MoveLine(Global.GuiTempLines[i]);
            }

            foreach (var i in Global.Bar)
            {
                i.Value.Resize();
            }

            if(Global.FontManager != null)
            {
                Global.FontManager.Resize();
            }

            // let's update console too
            ConsoleInfo.Instance.SetLineInterval(ConsoleInfo.Instance.Spacing);
            FillCrosshairBuffer();
        }

        /**
         * Inventory rendering / manipulation functions
         */

        /// <summary>
        /// That function updates item animation and rebuilds skeletal matrices
        /// </summary>
        /// <param name="bf">Extended bone frame of the item</param>
        public static void Item_Frame(SSBoneFrame bf, float time)
        {
            short frame, anim;
            long t;
            float dt;
            StateChange stc;

            bf.Animations.Lerp = 0;
            stc = StaticFuncs.Anim_FindStateChangeByID(bf.Animations.Model.Animations[bf.Animations.CurrentAnimation],
                (uint) bf.Animations.NextState);
            Entity.GetNextFrame(bf, time, stc, out frame, out anim, 0x00);
            if(anim != bf.Animations.CurrentAnimation)
            {
                bf.Animations.LastAnimation = bf.Animations.CurrentAnimation;
                stc =
                    StaticFuncs.Anim_FindStateChangeByID(bf.Animations.Model.Animations[bf.Animations.CurrentAnimation],
                        (uint) bf.Animations.NextState);
            }
            else if(frame != bf.Animations.CurrentFrame)
            {
                if(bf.Animations.CurrentFrame == 0)
                {
                    bf.Animations.LastAnimation = bf.Animations.CurrentAnimation;
                }
                bf.Animations.CurrentFrame = frame;
            }

            bf.Animations.FrameTime += time;

            t = (long)(bf.Animations.FrameTime / bf.Animations.Period);
            dt = bf.Animations.FrameTime - t * bf.Animations.Period;
            bf.Animations.FrameTime = frame * bf.Animations.Period + dt;
            bf.Animations.Lerp = dt / bf.Animations.Period;
            Entity.GetNextFrame(bf, bf.Animations.Period, stc, out bf.Animations.NextFrame, out bf.Animations.NextAnimation, 0x00);
            Entity.UpdateCurrentBoneFrame(bf, null);
        }

        public static void RenderItem(SSBoneFrame bf, float size, Transform mvMatrix)
        {
            LitShaderDescription shader = Global.Renderer.ShaderManager.GetEntityShader(0, false);
            GL.UseProgram(shader.Program);
            GL.Uniform1(shader.NumberOfLights, 0);
            GL.Uniform4(shader.LightAmbient, 1.0f, 1.0f, 1.0f, 1.0f);

            if(size != 0.0f)
            {
                var bb = bf.BBMax - bf.BBMin;
                if(bb.X >= bb.Y)
                {
                    size /= Math.Max(bb.X, bb.Z);
                }
                else
                {
                    size /= Math.Max(bb.Y, bb.Z);
                }
                size *= 0.8f;

                var scaledMatrix = new Transform();
                scaledMatrix.SetIdentity();
                if(size < 1.0f)
                {
                    VMath.Mat4_Scale(scaledMatrix, size, size, size);
                }
                var scaledMvMatrix = mvMatrix * scaledMatrix;
                var mvpMatrix = Global.GuiProjectionMatrix.MultiplyByTransform(scaledMvMatrix);

                // Render with scaled model view projection matrix
                // Use original modelview matrix, as that is used for normals whose size shouldn't change.
                Global.Renderer.RenderSkeletalModel(shader, bf, (Matrix4) mvMatrix, mvpMatrix);
            }
            else
            {
                var mvpMatrix = Global.GuiProjectionMatrix.MultiplyByTransform(mvMatrix);
                Global.Renderer.RenderSkeletalModel(shader, bf, (Matrix4)mvMatrix, mvpMatrix);
            }
        }
    }

    public partial class Global
    {
        public static FontManager FontManager;

        public static TextLine GuiBaseLines = null;

        public static TextLine[] GuiTempLines = new TextLine[Constants.MaxTempLines];

        public static ushort TempLinesUsed = 0;

        public static GuiItemNotifier Notifier;

        public static Dictionary<BarType, ProgressBar> Bar;

        public static Dictionary<FaderType, Fader> FaderType;

        public static uint CrosshairBuffer;

        public static VertexArray CrosshairArray;

        public static Matrix4 GuiProjectionMatrix = new Matrix4();

        public static uint RectanglePositionBuffer = 0;

        public static uint RectangleColorBuffer = 0;

        public static VertexArray RectangleArray = null;
    }
}
