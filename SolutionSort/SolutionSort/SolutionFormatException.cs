namespace RJCP.VsSolutionSort
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// There is a format error in the solution file that it can't be parsed.
    /// </summary>
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
    }
}
