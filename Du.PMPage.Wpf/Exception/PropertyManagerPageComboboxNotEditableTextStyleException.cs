using System;
using System.Runtime.Serialization;

namespace Du.PMPage.Wpf
{
    [Serializable]
    public class PropertyManagerPageComboboxNotEditableTextStyleException : Exception
    {
        public PropertyManagerPageComboboxNotEditableTextStyleException() { }
        public PropertyManagerPageComboboxNotEditableTextStyleException(string message) : base(message) { }
        public PropertyManagerPageComboboxNotEditableTextStyleException(string message, Exception inner) : base(message, inner) { }
        protected PropertyManagerPageComboboxNotEditableTextStyleException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}