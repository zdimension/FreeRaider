using System;
using System.Runtime.Serialization;

namespace NLibsndfile.Native
{
    [Serializable]
    public class LibsndfileException : Exception
    {
        public LibsndfileException() { }
        public LibsndfileException(string message) : base(message) { }
        public LibsndfileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public LibsndfileException(string message, Exception innerException) : base(message, innerException) { }
    }
}