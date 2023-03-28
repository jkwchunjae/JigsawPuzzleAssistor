using System.Runtime.Serialization;

namespace SquareGraphLib
{
    [Serializable]
    internal class CornerIsNotCornerException : Exception
    {
        public CornerIsNotCornerException()
        {
        }

        public CornerIsNotCornerException(string? message) : base(message)
        {
        }

        public CornerIsNotCornerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CornerIsNotCornerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}