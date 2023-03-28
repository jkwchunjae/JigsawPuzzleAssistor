using System.Runtime.Serialization;

namespace SquareGraphLib
{
    [Serializable]
    internal class DoNotMeetConditionException : Exception
    {
        public DoNotMeetConditionException()
        {
        }

        public DoNotMeetConditionException(string? message) : base(message)
        {
        }

        public DoNotMeetConditionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DoNotMeetConditionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}