using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace UniRaider
{
    public class GLUtil
    {
        public static int CheckOpenGLError()
        {
            return CheckOpenGLErrorDetailed(Helper.__FILE__, Helper.__LINE__);
        }

        public static int CheckOpenGLErrorDetailed(string file, int line)
        {
            for (;;)
            {
                var glErr = GL.GetError();
                if(glErr == ErrorCode.NoError)
                {
                    return 0;
                }

                // TODO: Log all the stuff
                /*
                switch(glErr)
                {
                    case GL_INVALID_VALUE:
                        Sys_DebugLog(GL_LOG_FILENAME, "glError: GL_INVALID_VALUE in %s:%d", file, line);
                        return 1;

                    case GL_INVALID_ENUM:
                        Sys_DebugLog(GL_LOG_FILENAME, "glError: GL_INVALID_ENUM in %s:%d", file, line);
                        return 1;

                    case GL_INVALID_OPERATION:
                        Sys_DebugLog(GL_LOG_FILENAME, "glError: GL_INVALID_OPERATION in %s:%d", file, line);
                        return 1;

                    case GL_STACK_OVERFLOW:
                        Sys_DebugLog(GL_LOG_FILENAME, "glError: GL_STACK_OVERFLOW in %s:%d", file, line);
                        return 1;

                    case GL_STACK_UNDERFLOW:
                        Sys_DebugLog(GL_LOG_FILENAME, "glError: GL_STACK_UNDERFLOW in %s:%d", file, line);
                        return 1;

                    case GL_OUT_OF_MEMORY:
                        Sys_DebugLog(GL_LOG_FILENAME, "glError: GL_OUT_OF_MEMORY in %s:%d", file, line);
                        return 1;

                        /* GL_CONTEXT_FLAG_ROBUST_ACCESS_BIT_ARB
                           GL_LOSE_CONTEXT_ON_RESET_ARB
                           GL_GUILTY_CONTEXT_RESET_ARB
                           GL_INNOCENT_CONTEXT_RESET_ARB
                           GL_UNKNOWN_CONTEXT_RESET_ARB
                           GL_RESET_NOTIFICATION_STRATEGY_ARB
                           GL_NO_RESET_NOTIFICATION_ARB* /

                        default:
                        Sys_DebugLog(GL_LOG_FILENAME, "glError: uncnown error = 0x%X in %s:%d", file, line, glErr);
                        return 1;
                };
                */

                return 1;
            }
        }

        public static void PrintShaderInfoLog(int obj);

        public static bool LoadShaderFromBuff(int shaderObj, string source)
        {
            var compileStatus = 0;
            GL.ShaderSource(shaderObj, source);
            // TODO: Log all the stuff
            GL.CompileShader(shaderObj);
            // check for OpenGL errors
            if(CheckOpenGLError() != 0)
            {
                return false;
            }
            GL.GetShader(shaderObj, ShaderParameter.CompileStatus, out compileStatus);
            PrintShaderInfoLog(shaderObj);

            return compileStatus != 0;
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

            // TODO: Log all the stuff
            GL.CompileShader(shaderObj);
            // check for OpenGL errors
            GL.GetShader(shaderObj, ShaderParameter.CompileStatus, out compileStatus);
            PrintShaderInfoLog(shaderObj);

            return compileStatus != 0;
        }
    }
}
