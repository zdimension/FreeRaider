using System;
using System.Runtime.InteropServices;

namespace NLibsndfile.Native
{
    /// <summary>
    /// Internal container class that holds a pointer to an allocated chunk of unmanaged memory.
    /// </summary>
    /// <remarks>
    /// Use of this class enables allocation/deallocation of unmanaged memory within the scope of a 'using' statement.
    /// </remarks>
    internal class UnmanagedMemoryHandle : IDisposable
    {
        private bool m_IsDisposed;

        /// <summary>
        /// Pointer to native memory location we have allocated.
        /// </summary>
        internal IntPtr Handle { get; private set; }

        /// <summary>
        /// Size of chunk of unmanaged memory which we allocated.
        /// </summary>
        /// <remarks>
        /// This is not known when this <see cref="UnmanagedMemoryHandle"/> on top of an existing handle.
        /// </remarks>
        internal int Size { get; private set; }

        /// <summary>
        /// Initializes a new instances of <see cref="UnmanagedMemoryHandle"/> on top of an empty pointer.
        /// </summary>
        /// <remarks>
        /// The default parameterless c'tor is here so we can mock the object.
        /// Defining an interface wouldn't work because we can have implicit operators on the interface.
        /// </remarks>
        internal UnmanagedMemoryHandle()
            : this(IntPtr.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UnmanagedMemoryHandle"/> on top of the given pointer.
        /// </summary>
        /// <param name="handle">IntPtr to unmanaged memory location.</param>
        internal UnmanagedMemoryHandle(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UnmanagedMemoryHandle"/>.
        /// </summary>
        /// <param name="size">Size of unmanaged memory in bytes to allocate.</param>
        internal UnmanagedMemoryHandle(int size)
        {
            Size = size;
            Handle = Marshal.AllocHGlobal(size);
        }

        /// <summary>
        /// Implicitly convert our <see cref="UnmanagedMemoryHandle"/> to an IntPtr.
        /// </summary>
        /// <param name="memory">Reference to an UnmanagedMemoryHandle object.</param>
        /// <returns>IntPtr handle which points to allocated unmanaged memory.</returns>
        public static implicit operator IntPtr(UnmanagedMemoryHandle memory)
        {
            return memory.Handle;
        }

        /// <summary>
        /// Implicitly convert an IntPtr to a <see cref="UnmanagedMemoryHandle"/>.
        /// </summary>
        /// <param name="handle">Reference to an IntPtr object.</param>
        /// <returns><see cref="UnmanagedMemoryHandle"/> which wraps the given IntPtr handle.</returns>
        public static implicit operator UnmanagedMemoryHandle(IntPtr handle)
        {
            return new UnmanagedMemoryHandle(handle);
        }

        /// <summary>
        /// Disposes of the previously allocated unmanaged memory.
        /// </summary>
        /// <param name="disposing">Determines whether this was called by the public Dispose method or finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_IsDisposed)
                return;

            if(disposing)
                Marshal.FreeHGlobal(Handle);

            m_IsDisposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows an <see cref="T:System.Object"/> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Object"/> is reclaimed by garbage collection.
        /// </summary>
        ~UnmanagedMemoryHandle()
        {
            Dispose(false);
        }
    }
}