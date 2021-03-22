using System;
using System.Runtime.Serialization;
using Risk.Shared;

namespace Risk.Shared
{
    [Serializable]
    public class InvalidGameStateException : Exception
    {
        public GameState GameState { get; private set; }

        public InvalidGameStateException()
        {
        }

        public InvalidGameStateException(string message) : base(message)
        {
        }

        public InvalidGameStateException(string message, GameState gameState) : base(message)
        {
            GameState = gameState;
        }

        public InvalidGameStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidGameStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}