using System;
using System.Runtime.Serialization;

namespace Du.PMPage.Wpf
{
    /// <summary>
    /// The exception that is thrown when creating a SolidWorks Task Pane fails.
    /// </summary>
    /// <remarks>
    /// This exception wraps errors that occur during the native SolidWorks API call to create
    /// a Task Pane view. The inner exception (when available) contains the underlying
    /// COM or API error details.
    /// </remarks>
    [Serializable]
    public class CreateTaskPaneErrorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTaskPaneErrorException"/> class.
        /// </summary>
        public CreateTaskPaneErrorException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTaskPaneErrorException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CreateTaskPaneErrorException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTaskPaneErrorException"/> class
        /// with a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception, or <see langword="null"/>
        /// if no inner exception is specified.
        /// </param>
        public CreateTaskPaneErrorException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTaskPaneErrorException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about the
        /// exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the
        /// source or destination.
        /// </param>
        protected CreateTaskPaneErrorException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}
