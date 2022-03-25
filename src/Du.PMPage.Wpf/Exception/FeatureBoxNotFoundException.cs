using System;
using System.Runtime.Serialization;

namespace Du.PMPage.Wpf
{
    [Serializable]
    public class FeatureBoxNotFoundException : Exception
    {
        public FeatureBoxNotFoundException() { }
        public FeatureBoxNotFoundException(string message) : base(message) { }
        public FeatureBoxNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected FeatureBoxNotFoundException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}