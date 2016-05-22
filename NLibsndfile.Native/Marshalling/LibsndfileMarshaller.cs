using System;
using System.Runtime.InteropServices;

namespace NLibsndfile.Native
{
    /// <summary>
    /// Class to provide easy to use helper functions for marshalling command methods.
    /// </summary>
    internal class LibsndfileMarshaller : ILibsndfileMarshaller
    {
        private readonly ILibsndfileArrayMarshaller m_ArrayMarshaller;

        /// <summary>
        /// Initializes a new <see cref="LibsndfileMarshaller"/> with the 
        /// default <see cref="ILibsndfileArrayMarshaller"/> implementation.
        /// </summary>
        internal LibsndfileMarshaller()
            : this(new LibsndfileArrayMarshaller())
        {
        }

        /// <summary>
        /// Initializes a new <see cref="LibsndfileMarshaller"/> with 
        /// the <paramref name="arrayMarshaller"/> implementation.
        /// </summary>
        /// <param name="arrayMarshaller"><see cref="ILibsndfileArrayMarshaller"/> implementation to use.</param>
        /// <remarks>
        /// This constructor should only be used for mocking.
        /// </remarks>
        internal LibsndfileMarshaller(ILibsndfileArrayMarshaller arrayMarshaller)
        {
            if (arrayMarshaller == null)
                throw new ArgumentNullException("arrayMarshaller");

            m_ArrayMarshaller = arrayMarshaller;
        }

        /// <summary>
        /// Create a new <see cref="UnmanagedMemoryHandle"/> allocated for <paramref name="size"/> bytes.
        /// </summary>
        /// <param name="size">Number of bytes of unmanaged memory requested.</param>
        /// <returns><see cref="UnmanagedMemoryHandle"/> with a chunk of memory allocated.</returns>
        public UnmanagedMemoryHandle Allocate(int size)
        {
            return new UnmanagedMemoryHandle(size);
        }

        /// <summary>
        /// Create a new <see cref="UnmanagedMemoryHandle"/> allocated for the size of a single <typeparamref name="T"/> structure.
        /// </summary>
        /// <returns><see cref="UnmanagedMemoryHandle"/> with a chunk of memory allocated.</returns>
        public UnmanagedMemoryHandle Allocate<T>()
            where T : struct 
        {
            return new UnmanagedMemoryHandle(Marshal.SizeOf(typeof(T)));
        }

        /// <summary>
        /// Create a new <see cref="UnmanagedMemoryHandle"/> allocated for the size of
        /// a single <typeparamref name="T"/> structure and populate the memory with <paramref name="obj"/>.
        /// </summary>
        /// <typeparam name="T">Type of structure to calculate size of.</typeparam>
        /// <param name="obj"><typeparamref name="T"/> object to populate newly allocated memory with.</param>
        /// <returns><see cref="UnmanagedMemoryHandle"/> with a chunk of memory allocated and filled.</returns>
        public UnmanagedMemoryHandle Allocate<T>(T obj) 
            where T : struct
        {
            var memory = Allocate<T>();
            Marshal.StructureToPtr(obj, memory, true);
            return memory;
        }

        /// <summary>
        /// Creates a new <see cref="UnmanagedMemoryHandle"/> allocated for the size of
        ///  a single <typeparamref name="T"/> structure with the <paramref name="length"/> multiplier.
        /// </summary>
        /// <typeparam name="T">Type of structure to calculate size of.</typeparam>
        /// <param name="length">Size multiplier</param>
        /// <returns><see cref="UnmanagedMemoryHandle"/> with a chunk of memory allocated.</returns>
        public UnmanagedMemoryHandle AllocateArray<T>(int length)
            where T : struct 
        {
            return new UnmanagedMemoryHandle(Marshal.SizeOf(typeof(T)) * length);
        }

        /// <summary>
        /// Creates a new <see cref="UnmanagedMemoryHandle"/> from previously allocated unmanaged memory.
        /// </summary>
        /// <param name="handle">Existing handle to unmanaged memory location</param>
        /// <returns><see cref="UnmanagedMemoryHandle"/> wrapping previously allocated unmanaged memory.</returns>
        public UnmanagedMemoryHandle Attach(IntPtr handle)
        {
            return new UnmanagedMemoryHandle(handle);
        }

        /// <summary>
        /// Explicitly disposes of the <paramref name="memory"/> object and deallocates its unmanaged memory.
        /// </summary>
        /// <param name="memory"><see cref="UnmanagedMemoryHandle"/> to deallocate.</param>
        public void Deallocate(UnmanagedMemoryHandle memory)
        {
            if (memory == null)
                return;

            memory.Dispose();
        }

        /// <summary>
        /// Marshal a <see cref="UnmanagedMemoryHandle"/> object to an ANSI string.
        /// </summary>
        /// <param name="memory">Reference to <see cref="UnmanagedMemoryHandle"/>.</param>
        /// <returns>ANSI string conversion from unmanaged memory.</returns>
        public string MemoryHandleToString(UnmanagedMemoryHandle memory)
        {
            return Marshal.PtrToStringAnsi(memory.Handle);
        }

        /// <summary>
        /// Marshal an <see cref="UnmanagedMemoryHandle"/> object to a <typeparamref name="T"/> structure.
        /// </summary>
        /// <typeparam name="T">Type of structure to marshal from unmanaged memory.</typeparam>
        /// <param name="memory">Reference to <see cref="UnmanagedMemoryHandle"/>.</param>
        /// <returns>Marshalled structure stored in managed memory.</returns>
        public T MemoryHandleTo<T>(UnmanagedMemoryHandle memory)
            where T : struct 
        {
            return (T)Marshal.PtrToStructure(memory, typeof(T));
        }

        /// <summary>
        /// Marshal a <see cref="UnmanagedMemoryHandle"/> object to a <typeparamref name="T"/> array.
        /// </summary>
        /// <typeparam name="T">Type of array to marshal from unmanaged memory.</typeparam>
        /// <param name="memory">Reference to <see cref="UnmanagedMemoryHandle"/>.</param>
        /// <returns>Copy of marshalled array now in managed memory.</returns>
        public T[] MemoryHandleToArray<T>(UnmanagedMemoryHandle memory) 
            where T : struct
        {
            return m_ArrayMarshaller.ToArray<T>(memory);
        }
    }
}