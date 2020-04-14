using System;
using System.Runtime.Serialization;

namespace CalendarBot.Messengers.Exceptions
{
    [Serializable]
    public class NullInputException : Exception
    {
        public NullInputException()
        {
        }

        public NullInputException(string message) : base(message)
        {
        }

        public NullInputException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NullInputException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
