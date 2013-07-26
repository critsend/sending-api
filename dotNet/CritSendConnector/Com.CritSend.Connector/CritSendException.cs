using System;
using System.Collections.Generic;
using System.Text;

namespace Com.CritSend.Connector
{
    /// <summary>
    /// Exception for errors during CritSend transactions.
    /// </summary>
    [global::System.Serializable]
    public class CritSendException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CritSendException"/>.
        /// </summary>
        public CritSendException() { }

        /// <summary>
        /// Initializes a new instance of <see cref="CritSendException"/>.
        /// </summary>
        public CritSendException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of <see cref="CritSendException"/>.
        /// </summary>
        public CritSendException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of <see cref="CritSendException"/>.
        /// </summary>
        protected CritSendException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
