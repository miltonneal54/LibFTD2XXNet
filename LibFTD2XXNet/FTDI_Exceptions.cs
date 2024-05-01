using System;
using System.Runtime.Serialization;

namespace FTD2xxNet
{
    /// <summary>
    /// Exceptions thrown by errors within the FTDI class.
    /// </summary>
    public class FT_Exception : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public FT_Exception() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public FT_Exception(string message) : base(message) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public FT_Exception(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected FT_Exception(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
