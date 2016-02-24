using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeRaider.Loader;

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
        public Fader();

        /// <summary>
        /// Shows and updates fader
        /// </summary>
        public void Show();

        /// <summary>
        /// Resets and starts fader
        /// </summary>
        /// <param name="fadeDir"></param>
        public void Engage(FaderDir fadeDir);

        /// <summary>
        /// Immediately cuts fader
        /// </summary>
        public void Cut();

        /// <summary>
        /// Gets current state of the fader
        /// </summary>
        /// <returns></returns>
        public FaderStatus IsFading();

        public void SetScaleMode(FaderScale mode = FaderScale.Zoom);

        public void SetColor(byte r, byte g, byte b, FaderCorner corner = FaderCorner.None);

        public void SetBlendingMode(BlendingMode mode = BlendingMode.Opaque);

        public void SetAlpha(byte alpha = 255);

        public void SetSpeed(ushort fadeSpeed, ushort fadeSpeedSecondary = 200);

        public void SetDelay(uint delayMs);

        public bool SetTexture(string texturePath);


        private void setAspect();

        private bool dropTexture();

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

    public class GUIItemNotifier
    {
        public GUIItemNotifier();

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
        public static void InitFontManager();

        public static void Init();

        public static void Destroy();

        public static void InitBars();

        public static void InitFaders();

        public static void InitNotifier();

        public static void InitTempLines();

        public static void FillCrosshairBuffer();

        public static void AddLine(TextLine line);

        public static void DeleteLine(TextLine line);

        public static void MoveLive(TextLine line);

        public static void RenderStringLine(TextLine line);

        public static void RenderStrings();

        /// <summary>
        /// Draws text using a <see cref="FontType.Secondary"/>.
        /// </summary>
        public static TextLine OutTextXY(float x, float y, string fmt);

        /// <summary>
        /// Helper method to setup OpenGL state for console drawing.
        /// </summary>
        public static void SwitchGLMode(char isGui);



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
        public static void Render();

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
            uint texture = 0);

        /**
         *  Fader functions.
         */
        public static bool FadeStart(FaderType fader, FaderDir fadeDirection);

        public static bool FadeStop(FaderType fader);

        public static bool FadeAssignPic(FaderType fader, string picName);

        public static FaderStatus FadeCheck(FaderType fader);

        public static void FadeSetup(
            FaderType fader,
            uint alpha, uint r, uint g, uint b,
            BlendingMode blendingMode,
            ushort fadeInSpeed,
            ushort fadeOutSpeed);

        /**
         * Item notifier functions.
         */
        public static void NotifierStart(int item);

        public static void NotifierStop();

        /**
         * General GUI drawing routines.
         */
        public static void DrawCrosshair();

        public static void DrawFaders();

        public static void DrawBars();

        public static void DrawLoadScreen(int value);

        public static void DrawInventory();

        public static void DrawNotifier();

        /**
         * General GUI update routines.
         */
        public static void Update();

        public static void Resize();

        /**
         * Inventory rendering / manipulation functions
         */
        public static void Item_Frame(SSBoneFrame bf, float time);

        public static void RenderItem(SSBoneFrame bf, float size, Transform mvMatrix);
    }

    public partial class Global
    {
        public static FontManager FontManager;
    }
}
