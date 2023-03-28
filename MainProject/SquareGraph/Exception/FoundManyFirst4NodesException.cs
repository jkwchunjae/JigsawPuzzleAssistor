using System.Runtime.Serialization;

namespace SquareGraphLib
{
    [Serializable]
    internal class FoundManyFirst4NodesException : Exception
    {
        private int[][,] results;

        public FoundManyFirst4NodesException()
        {
        }

        public FoundManyFirst4NodesException(int[][,] results)
        {
            this.results = results;
        }

        public FoundManyFirst4NodesException(string? message) : base(message)
        {
        }

        public FoundManyFirst4NodesException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected FoundManyFirst4NodesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}