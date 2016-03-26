using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using static FreeRaider.Constants;

namespace FreeRaider
{
    public class GLUtil
    {
        public static int CheckOpenGLError()
        {
            return CheckOpenGLErrorDetailed(Helper.__FILE__, Helper.__LINE__);
        }

        public static int CheckOpenGLErrorDetailed(string file, int line)
        {
            while(true)
            {
                var glErr = GL.GetError();
                if(glErr == ErrorCode.NoError)
                {
                    return 0;
                }

                Sys.DebugLog(GL_LOG_FILENAME, "OpenGL Error: {0} (0x{0:X}}) in {1}:{2}", glErr, file, line);
                return 1;

                /*switch(glErr)
                {
                    case ErrorCode.InvalidValue:
                        Sys.DebugLog(Constants.LOG_FILENAME, "OpenGL Error: Invalid Value in {0}:{1}", file, line);
                        return 1;

                    case ErrorCode.InvalidEnum:
                        Sys.DebugLog(Constants.LOG_FILENAME, "OpenGL Error: Invalid Enum in {0}:{1}", file, line);
                        return 1;

                    case ErrorCode.InvalidFramebufferOperation:
                        Sys.DebugLog(Constants.LOG_FILENAME, "OpenGL Error: Invalid Operation in {0}:{1}", file, line);
                        return 1;

                    case ErrorCode.StackOverflow:
                        Sys.DebugLog(Constants.LOG_FILENAME, "OpenGL Error: Stack Overflow in {0}:{1}", file, line);
                        return 1;

                    case ErrorCode.StackUnderflow:
                        Sys.DebugLog(Constants.LOG_FILENAME, "OpenGL Error: Stack Underflow in {0}:{1}", file, line);
                        return 1;

                    case ErrorCode.OutOfMemory:
                        Sys.DebugLog(Constants.LOG_FILENAME, "OpenGL Error: Out of Memory in {0}:{1}", file, line);
                        return 1;

                    default:
                        Sys.DebugLog(Constants.LOG_FILENAME, "OpenGL Error: '{2}' = 0x{2:X2} in {0}:{1}", file, line, glErr);
                        return 1;
                }*/
            }
        }

        public static void PrintShaderInfoLog(int obj)
        {
            var isProgram = GL.IsProgram(obj);
            var isShader = GL.IsShader(obj);

            if (!(isProgram ^ isShader))
            {
                Sys.DebugLog(GL_LOG_FILENAME, "Object {0} is neither a shader nor a program", obj);
                return;
            }

            CheckOpenGLError(); // check for OpenGL errors

            var infoLog = isProgram ? GL.GetProgramInfoLog(obj) : GL.GetShaderInfoLog(obj);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Sys.DebugLog(GL_LOG_FILENAME, "GL InfoLog[{0}]:", infoLog.Length);
                Sys.DebugLog(GL_LOG_FILENAME, infoLog);
            }
        }

        public static bool LoadShaderFromBuff(int shaderObj, string source)
        {
            var compileStatus = 0;
            GL.ShaderSource(shaderObj, source);
            Sys.DebugLog(GL_LOG_FILENAME, "Source loaded");
            GL.CompileShader(shaderObj);
            Sys.DebugLog(GL_LOG_FILENAME, "Trying to compile");
            // check for OpenGL errors
            if(CheckOpenGLError() != 0)
            {
                Sys.DebugLog(GL_LOG_FILENAME, "Compilation failed");
                return false;
            }
            GL.GetShader(shaderObj, ShaderParameter.CompileStatus, out compileStatus);
            PrintShaderInfoLog(shaderObj);

            if (compileStatus == 0)
            {
                Sys.DebugLog(GL_LOG_FILENAME, "Compilation failed");
                return false;
            }
            else
            {
                Sys.DebugLog(GL_LOG_FILENAME, "Compilation succeeded");
                return true;
            }
        }

        public static bool LoadShaderFromFile(int shaderObj, string fileName, string additionalDefines)
        {
            var compileStatus = 0;

            var version = "#version 150\n";

            try
            {
                if (!string.IsNullOrWhiteSpace(additionalDefines))
                {
                    var bufs = new[] {version, additionalDefines, File.ReadAllText(fileName)};
                    var lengths = bufs.Select(x => x.Length).ToArray();
                    GL.ShaderSource(shaderObj, 3, bufs, lengths);
                }
                else
                {
                    var bufs = new[] {version, File.ReadAllText(fileName)};
                    var lengths = bufs.Select(x => x.Length).ToArray();
                    GL.ShaderSource(shaderObj, 2, bufs, lengths);
                }
            }
            catch
            {
                return false;
            }

            Sys.DebugLog(GL_LOG_FILENAME, "Source loaded");
            GL.CompileShader(shaderObj);
            Sys.DebugLog(GL_LOG_FILENAME, "Trying to compile");
            GL.GetShader(shaderObj, ShaderParameter.CompileStatus, out compileStatus);
            PrintShaderInfoLog(shaderObj);

            if(compileStatus != 1)
            {
                Sys.DebugLog(GL_LOG_FILENAME, "Compilation failed");
                return false;
            }
            else
            {
                Sys.DebugLog(GL_LOG_FILENAME, "Compilation succeeded");
                return true;
            }
        }
    }
}
