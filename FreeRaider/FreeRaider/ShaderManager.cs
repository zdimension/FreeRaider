using OpenTK.Graphics.OpenGL;
using static FreeRaider.Constants;

namespace FreeRaider
{
    public partial class Constants
    {
        /// <summary>
        /// Highest number of lights that will show up in the entity shader.
        /// </summary>
        public const int MAX_NUM_LIGHTS = 8;
    }

    /// <summary>
    /// Class containing all shaders used by FreeRaider. The shader objects
    /// are owned by this manager and must not be deleted by anyone.
    /// </summary>
    public class ShaderManager
    {
        private UnlitTintedShaderDescription[][] roomShaders = Helper.RepeatValue(2,
            () => new UnlitTintedShaderDescription[2]);

        private UnlitTintedShaderDescription staticMeshShader;

        private UnlitShaderDescription stencil;

        private UnlitShaderDescription debugLine;

        private LitShaderDescription[][] entityShader = Helper.RepeatValue(MAX_NUM_LIGHTS + 1,
            () => new LitShaderDescription[2]);
        private GuiShaderDescription gui;

        private GuiShaderDescription guiTextured;

        private TextShaderDescription text;

        private SpriteShaderDescription sprites;

        public ShaderManager()
        {
            var staticMeshVsh = new ShaderStage(ShaderType.VertexShader, "shaders/static_mesh.vsh");
            var staticMeshFsh = new ShaderStage(ShaderType.FragmentShader, "shaders/static_mesh.fsh");
            // Color mult prog
            staticMeshShader = new UnlitTintedShaderDescription(staticMeshVsh, staticMeshFsh);

            // Room prog
            var roomFragmentShader = new ShaderStage(ShaderType.FragmentShader, "shaders/room.fsh");
            for (var isWater = 0; isWater < 2; isWater++)
            {
                for (var isFlicker = 0; isFlicker < 2; isFlicker++)
                {
                    var stream =
                        "#define IS_WATER " + isWater + "\n" +
                        "#define IS_FLICKER " + isFlicker + "\n";

                    var roomVsh = new ShaderStage(ShaderType.VertexShader, "shaders/room.vsh", stream);
                    roomShaders[isWater][isFlicker] = new UnlitTintedShaderDescription(roomVsh, roomFragmentShader);
                }
            }

            // Entity prog
            var entityVertexShader = new ShaderStage(ShaderType.VertexShader, "shaders/entity.vsh");
            var entitySkinVertexShader = new ShaderStage(ShaderType.VertexShader, "shaders/entity_skin.vsh");
            for (var i = 0; i < MAX_NUM_LIGHTS; i++)
            {
                var stream = "#define NUMBER_OF_LIGHTS " + i + "\n";

                var fragment = new ShaderStage(ShaderType.FragmentShader, "shaders/entity.fsh", stream);
                entityShader[i][0] = new LitShaderDescription(entityVertexShader, fragment);
                entityShader[i][1] = new LitShaderDescription(entitySkinVertexShader, fragment);
            }

            // GUI prog
            var guiVertexShader = new ShaderStage(ShaderType.VertexShader, "shaders/gui.vsh");
            var guiFsh = new ShaderStage(ShaderType.FragmentShader, "shaders/gui.fsh");
            gui = new GuiShaderDescription(guiVertexShader, guiFsh);

            var guiTexFsh = new ShaderStage(ShaderType.FragmentShader, "shaders/gui_tex.fsh");
            guiTextured = new GuiShaderDescription(guiVertexShader, guiTexFsh);

            var textVsh = new ShaderStage(ShaderType.VertexShader, "shaders/text.vsh");
            var textFsh = new ShaderStage(ShaderType.FragmentShader, "shaders/text.fsh");
            text = new TextShaderDescription(textVsh, textFsh);

            var spriteVsh = new ShaderStage(ShaderType.VertexShader, "shaders/sprite.vsh");
            var spriteFsh = new ShaderStage(ShaderType.FragmentShader, "shaders/sprite.fsh");
            sprites = new SpriteShaderDescription(spriteVsh, spriteFsh);

            var stencilVsh = new ShaderStage(ShaderType.VertexShader, "shaders/stencil.vsh");
            var stencilFsh = new ShaderStage(ShaderType.FragmentShader, "shaders/stencil.fsh");
            stencil = new LitShaderDescription(stencilVsh, stencilFsh);

            var debugVsh = new ShaderStage(ShaderType.VertexShader, "shaders/debuglines.vsh");
            var debugFsh = new ShaderStage(ShaderType.FragmentShader, "shaders/debuglines.fsh");
            debugLine = new UnlitTintedShaderDescription(debugVsh, debugFsh);
        }

        public LitShaderDescription GetEntityShader(int numberOfLights, bool skin)
        {
            Assert.That(numberOfLights <= MAX_NUM_LIGHTS);

            return entityShader[numberOfLights][skin ? 1 : 0];
        }

        public UnlitTintedShaderDescription GetStaticMeshShader()
        {
            return staticMeshShader;
        }

        public UnlitShaderDescription GetStencilShader()
        {
            return stencil;
        }

        public UnlitShaderDescription GetDebugLineShader()
        {
            return debugLine;
        }

        public UnlitTintedShaderDescription GetRoomShader(bool isFlickering, bool isWater)
        {
            return roomShaders[isWater ? 1 : 0][isFlickering ? 1 : 0];
        }

        public GuiShaderDescription GetGuiShader(bool includingShader)
        {
            return includingShader ? guiTextured : gui;
        }

        public TextShaderDescription GetTextShader()
        {
            return text;
        }

        public SpriteShaderDescription GetSpriteShader()
        {
            return sprites;
        }
    }
}
