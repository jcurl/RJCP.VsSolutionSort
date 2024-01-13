namespace RJCP.VsSolutionSort
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// There is a format error in the solution file that it can't be parsed.
    /// </summary>
    [Serializable]
    public class SolutionFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFormatException"/> class.
        /// </summary>
        public SolutionFormatException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFormatException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SolutionFormatException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFormatException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected SolutionFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
