using System;
using System.Runtime.Serialization;

namespace Du.PMPage.Wpf
{
    [Serializable]
    public class CreatePMPageErrorException : Exception
    {
        public CreatePMPageErrorException()
        {
        }

        public CreatePMPageErrorException(string message) : base(message)
        {
        }

        public CreatePMPageErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CreatePMPageErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}