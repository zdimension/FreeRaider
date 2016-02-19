using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace UniRaider
{
    /// <summary>
    /// A shader stage is a simple wrapper to load an OpenGL shader
    /// object more easily.
    /// Note about ownership: The destructor calls glDeleteShader,
    /// which marks the shader object for deletion. OpenGL only
    /// deletes it when the last program (shader description) using
    /// it is deleted.
    /// </summary>
    public class ShaderStage
    {
        public int Shader;

        public ShaderStage(ShaderType type, string filename, string additionalDefines = null)
        {
            Shader = GL.CreateShader(type);
            if (!GLUtil.LoadShaderFromFile(Shader, filename, additionalDefines))
                abort();
        }

        ~ShaderStage()
        {
            GL.DeleteShader(Shader);
        }
    }

    /// <summary>
    /// A shader description consists of a program, code to load the
    /// program, and the indices of the various uniform values. Each
    /// shader or set of related shaders will have its own subclass
    /// of shader_description. We assume (for now) that every shader
    /// has a texture.
    /// </summary>
    public class ShaderDescription
    {
        public int Program;

        public int Sampler;

        public ShaderDescription(ShaderStage vertex, ShaderStage fragment)
        {
            Program = GL.CreateProgram();
            GL.AttachShader(Program, vertex.Shader);
            GL.AttachShader(Program, fragment.Shader);
            GL.LinkProgram(Program);
            int isLinked;
            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out isLinked);
            // TODO: Assert isLinked == true

            GLUtil.CheckOpenGLError();
            GLUtil.PrintShaderInfoLog(Program);

            Sampler = GL.GetUniformLocation(Program, "color_map");
        }

        ~ShaderDescription()
        {
            GL.DeleteProgram(Program);
        }
    }

    /// <summary>
    /// A shader description specifically for use in GUI situations.
    /// </summary>
    public class GuiShaderDescription : ShaderDescription
    {
        public int Offset;

        public int Factor;

        public enum VertexAttribs
        {
            Position = 0,
            Color
        }

        public GuiShaderDescription(ShaderStage vertex, ShaderStage fragment) : base(vertex, fragment)
        {
            Offset = GL.GetUniformLocation(Program, "offset");
            Factor = GL.GetUniformLocation(Program, "factor");
            GL.BindAttribLocation(Program, (int)VertexAttribs.Position, "position");
            GL.BindAttribLocation(Program, (int)VertexAttribs.Color, "color");
            GL.LinkProgram(Program);
            int isLinked;
            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out isLinked);
            // TODO: Assert isLinked == true

            GLUtil.CheckOpenGLError();
            GLUtil.PrintShaderInfoLog(Program);
        }
    }

    public class SpriteShaderDescription : ShaderDescription
    {
        public int ModelView;

        public int Projection;

        public enum VertexAttribs
        {
            Position = 0,
            CornerOffset,
            TexCoord
        }

        public SpriteShaderDescription(ShaderStage vertex, ShaderStage fragment) : base(vertex, fragment)
        {
            ModelView = GL.GetUniformLocation(Program, "modelView");
            Projection = GL.GetUniformLocation(Program, "projection");
            GL.BindAttribLocation(Program, (int)VertexAttribs.Position, "position");
            GL.BindAttribLocation(Program, (int)VertexAttribs.CornerOffset, "cornerOffset");
            GL.BindAttribLocation(Program, (int)VertexAttribs.TexCoord, "texCoord");
            GL.LinkProgram(Program);
            int isLinked;
            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out isLinked);
            // TODO: Assert isLinked == true

            GLUtil.CheckOpenGLError();
            GLUtil.PrintShaderInfoLog(Program);
        }
    }

    /// <summary>
    /// A shader description for text
    /// </summary>
    public class TextShaderDescription : ShaderDescription
    {
        public int ScreenSize;

        public enum VertexAttribs
        {
            Position = 0,
            Color,
            TexCoord
        }

        public TextShaderDescription(ShaderStage vertex, ShaderStage fragment) : base(vertex, fragment)
        {
            GL.BindAttribLocation(Program, (int)VertexAttribs.Position, "position");
            GL.BindAttribLocation(Program, (int)VertexAttribs.Color, "color");
            GL.BindAttribLocation(Program, (int)VertexAttribs.TexCoord, "texCoord");
            GL.LinkProgram(Program);
            int isLinked;
            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out isLinked);
            // TODO: Assert isLinked == true

            GLUtil.CheckOpenGLError();
            GLUtil.PrintShaderInfoLog(Program);

            ScreenSize = GL.GetUniformLocation(Program, "screenSize");
        }
    }

    /// <summary>
    /// A shader description type that contains transform information.
    /// This comes in the form of a model view projection matrix.
    /// </summary>
    public class UnlitShaderDescription : ShaderDescription
    {
        public int ModelViewProjection;

        public enum VertexAttribs
        {
            Position = 0,
            Color,
            TexCoord,
            Normal,
            MatrixIndex
        }

        public UnlitShaderDescription(ShaderStage vertex, ShaderStage fragment) : base(vertex, fragment)
        {
            GL.BindAttribLocation(Program, (int)VertexAttribs.Position, "position");
            GL.BindAttribLocation(Program, (int)VertexAttribs.Color, "color");
            GL.BindAttribLocation(Program, (int)VertexAttribs.TexCoord, "texCoord");
            GL.BindAttribLocation(Program, (int)VertexAttribs.Normal, "normal");
            GL.BindAttribLocation(Program, (int)VertexAttribs.MatrixIndex, "matrixIndex");
            GL.LinkProgram(Program);
            int isLinked;
            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out isLinked);
            // TODO: Assert isLinked == true

            GLUtil.CheckOpenGLError();
            GLUtil.PrintShaderInfoLog(Program);

            ModelViewProjection = GL.GetUniformLocation(Program, "modelViewProjection");
        }
    }

    /// <summary>
    /// A shader description type that is suitable for lit objects. Also
    /// contains a model view matrix and information about the current
    /// light situation
    /// </summary>
    public class LitShaderDescription : UnlitShaderDescription
    {
        public int ModelView;

        public int Projection;

        public int NumberOfLights;

        public int LightPosition;

        public int LightColor;

        public int LightInnerRadius;

        public int LightOuterRadius;

        public int LightAmbient;

        public LitShaderDescription(ShaderStage vertex, ShaderStage fragment) : base(vertex, fragment)
        {
            ModelView = GL.GetUniformLocation(Program, "modelView");
            Projection = GL.GetUniformLocation(Program, "projection");
            NumberOfLights = GL.GetUniformLocation(Program, "number_of_lights");
            LightPosition = GL.GetUniformLocation(Program, "light_position");
            LightColor = GL.GetUniformLocation(Program, "light_color");
            LightInnerRadius = GL.GetUniformLocation(Program, "light_innerRadius");
            LightOuterRadius = GL.GetUniformLocation(Program, "light_outerRadius");
            LightAmbient = GL.GetUniformLocation(Program, "light_ambient");
        }
    }

    public class UnlitTintedShaderDescription : UnlitShaderDescription
    {
        public int CurrentTick;

        public int TintMult;

        public UnlitTintedShaderDescription(ShaderStage vertex, ShaderStage fragment) : base(vertex, fragment)
        {
            CurrentTick = GL.GetUniformLocation(Program, "fCurrentTick");
            TintMult = GL.GetUniformLocation(Program, "tintMult");
        }
    }
}
