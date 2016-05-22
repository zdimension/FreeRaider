using System;
using System.Runtime.InteropServices;

namespace NLibsndfile.Native
{
    /// <summary>
    /// Provides methods for converting <see cref="UnmanagedMemoryHandle"/> to managed arrays.
    /// </summary>
    internal class LibsndfileArrayMarshaller : ILibsndfileArrayMarshaller
    {
        /// <summary>
        /// Returns a <typeparamref name="T"/> array marshalled from the given <paramref name="memory"/>
        /// <see cref="UnmanagedMemoryHandle"/> handle.
        /// </summary>
        /// <typeparam name="T">Type of managed array you wish to convert to.</typeparam>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> memory that contains array to marshal.</param>
        /// <returns>Marshalled array.</returns>
        public T[] ToArray<T>(UnmanagedMemoryHandle memory)
            where T : struct 
        {
            Type type = typeof(T);

            if (type == typeof(byte))
                return ToByteArray(memory) as T[];
            if (type == typeof(short))
                return ToShortArray(memory) as T[];
            if (type == typeof(int))
                return ToIntArray(memory) as T[];
            if (type == typeof(float))
                return ToFloatArray(memory) as T[];
            if (type == typeof(double))
                return ToDoubleArray(memory) as T[];
            if (type == typeof(long))
                return ToLongArray(memory) as T[];

            throw new NotSupportedException(string.Format("No marshalling support for array of type {0}.", type));
        }

        /// <summary>
        /// Marshal a <see cref="UnmanagedMemoryHandle"/> to managed <see cref="System.Byte"/> array.
        /// </summary>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> containing pointer to native array.</param>
        /// <returns>Managed <see cref="System.Byte"/> array.</returns>
        private static byte[] ToByteArray(UnmanagedMemoryHandle memory)
        {
            int length = CalculateArrayLength<byte>(memory);
            var array = new byte[length];
            Marshal.Copy(memory, array, 0, length);
            return array;
        }

        /// <summary>
        /// Marshal <see cref="UnmanagedMemoryHandle"/> to managed <see cref="System.Int16"/> array.
        /// </summary>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> containing pointer to native array.</param>
        /// <returns>Managed <see cref="System.Int16"/> array.</returns>
        private static short[] ToShortArray(UnmanagedMemoryHandle memory)
        {
            int length = CalculateArrayLength<short>(memory);
            var array = new short[length];
            Marshal.Copy(memory, array, 0, length);
            return array;
        }

        /// <summary>
        /// Marshal <see cref="UnmanagedMemoryHandle"/> to managed <see cref="System.Int32"/> array.
        /// </summary>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> containing pointer to native array.</param>
        /// <returns>Managed <see cref="System.Int32"/> array.</returns>
        private static int[] ToIntArray(UnmanagedMemoryHandle memory)
        {
            int length = CalculateArrayLength<int>(memory);
            var array = new int[length];
            Marshal.Copy(memory, array, 0, length);
            return array;
        }

        /// <summary>
        /// Marshal <see cref="UnmanagedMemoryHandle"/> to managed <see cref="System.Single"/> array.
        /// </summary>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> containing pointer to native array.</param>
        /// <returns>Managed <see cref="System.Single"/> array.</returns>
        private static float[] ToFloatArray(UnmanagedMemoryHandle memory)
        {
            int length = CalculateArrayLength<float>(memory);
            var array = new float[length];
            Marshal.Copy(memory, array, 0, length);
            return array;
        }

        /// <summary>
        /// Marshal <see cref="UnmanagedMemoryHandle"/> to managed <see cref="System.Double"/> array.
        /// </summary>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> containing pointer to native array.</param>
        /// <returns>Managed <see cref="System.Double"/> array.</returns>
        private static double[] ToDoubleArray(UnmanagedMemoryHandle memory)
        {
            int length = CalculateArrayLength<double>(memory);
            var array = new double[length];
            Marshal.Copy(memory, array, 0, length);
            return array;
        }

        /// <summary>
        /// Marshal <see cref="UnmanagedMemoryHandle"/> to managed <see cref="System.Int64"/> array.
        /// </summary>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> containing pointer to native array.</param>
        /// <returns>Managed <see cref="System.Int64"/> array.</returns>
        private static long[] ToLongArray(UnmanagedMemoryHandle memory)
        {
            int length = CalculateArrayLength<long>(memory);
            var array = new long[length];
            Marshal.Copy(memory, array, 0, length);
            return array;
        }

        /// <summary>
        /// Determine length required for managed array based on <typeparamref name="T"/> and <paramref name="memory"/> size.
        /// </summary>
        /// <typeparam name="T">Underlying array type.</typeparam>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> to location of native array.</param>
        /// <returns>Length of marshalled array.</returns>
        private static int CalculateArrayLength<T>(UnmanagedMemoryHandle memory)
        {
            return memory.Size / Marshal.SizeOf(typeof(T));
        }
    }
}