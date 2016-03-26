using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using static FreeRaider.Global;

namespace FreeRaider
{
    public partial class Global
    {
        public static uint FontBufferVBO;

        public static VertexArray FontBufferVAO;

        public static int CurrentSize;
    }

    public class FontBuffer
    {
        private static void ensureAvailable()
        {
            if(FontBufferVAO != null && FontBufferVBO != 0)
            {
                return;
            }

            FontBufferVBO = Helper.GenBufferU();

            FontBufferVAO = new VertexArray(0, new []
            {
                new VertexArrayAttribute((int)TextShaderDescription.VertexAttribs.Position, 2, VertexAttribPointerType.Float, false, FontBufferVBO, Marshal.SizeOf(new float[8]), 0), 
                new VertexArrayAttribute((int)TextShaderDescription.VertexAttribs.TexCoord, 2, VertexAttribPointerType.Float, false, FontBufferVBO, Marshal.SizeOf(new float[8]), Marshal.SizeOf(new float[2])), 
                new VertexArrayAttribute((int)TextShaderDescription.VertexAttribs.Color, 4, VertexAttribPointerType.Float, false, FontBufferVBO, Marshal.SizeOf(new float[8]), Marshal.SizeOf(new float[4]))
            });
        }

        /// <summary>
        /// Sets the minimum size of the font buffer that is needed, in bytes, and
        /// returns a write-only pointer to the new data.
        /// 
        /// At least the given amount of bytes will be available after. The initial
        /// contents will be undefined.
        /// 
        /// If the buffer's internal structures don't exist yet, they are created.
        /// 
        /// Nothing may be read from the pointer, not even data that was previously
        /// written! The writes (may, depending on driver) go more ore less
        /// directly to the GPU, and the reverse path may not be set up, so reading
        /// will result in undefined behavior.
        /// </summary>
        public static unsafe float* ResizeAndMap(int bytes)
        {
            ensureAvailable();

            GL.BindBuffer(BufferTarget.ArrayBuffer, FontBufferVBO);
            CurrentSize = Math.Max(CurrentSize, bytes);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)CurrentSize, IntPtr.Zero, BufferUsageHint.StreamDraw);
            return (float*) GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
        }

        /// <summary>
        /// Stops mapping. After this is called, the pointer created by MapForWriting
        /// may no longer be used. Must be called before any other operation on the
        /// buffer, including draws from it, or before some other VBO gets mapped.
        /// </summary>
        public static void Unmap()
        {
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
        }

        /// <summary>
        /// Sets the buffer as the current draw buffer. Must not be called between
        /// ResizeAndMap and Unmap. Creates the internal structures if necessary.
        /// </summary>
        public static void Bind()
        {
            ensureAvailable();

            FontBufferVAO.Bind();
        }
    }
}
