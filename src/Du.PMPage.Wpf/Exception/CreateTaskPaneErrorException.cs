using System;
using System.Runtime.Serialization;

namespace Du.PMPage.Wpf
{
    [Serializable]
    public class CreateTaskPaneErrorException : Exception
    {
        public CreateTaskPaneErrorException() { }
        public CreateTaskPaneErrorException(string message) : base(message) { }
        public CreateTaskPaneErrorException(string message, Exception inner) : base(message, inner) { }
        protected CreateTaskPaneErrorException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}