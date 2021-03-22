using System;
using System.Runtime.Serialization;

namespace Risk.Game
{
    [Serializable]
    internal class TerritoryNotFoundException : Exception
    {
        public TerritoryNotFoundException()
        {
        }

        public TerritoryNotFoundException(string message) : base(message)
        {
        }

        public TerritoryNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TerritoryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}