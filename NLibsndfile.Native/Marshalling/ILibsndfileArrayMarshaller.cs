namespace NLibsndfile.Native
{
    /// <summary>
    /// Interface to provide support for marshalling arrays.
    /// </summary>
    internal interface ILibsndfileArrayMarshaller
    {
        T[] ToArray<T>(UnmanagedMemoryHandle memory) where T : struct;
    }
}