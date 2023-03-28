using System.Runtime.Serialization;

namespace SquareGraphLib
{
    [Serializable]
    internal class NotFoundFirstNodeException : Exception
    {
        public NotFoundFirstNodeException()
        {
        }

        public NotFoundFirstNodeException(string? message) : base(message)
        {
        }

        public NotFoundFirstNodeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NotFoundFirstNodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}