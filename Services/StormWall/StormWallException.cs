using System;
// ReSharper disable UnusedMember.Global

namespace Leaf.xNet.Services.StormWall
{
    /// <inheritdoc />
    /// <summary>
    /// The exception that is thrown if StormWall clearance failed.
    /// </summary>
    [Serializable]
    public class StormWallException : Exception
    {
        /// <inheritdoc />
        public StormWallException() { }

        public StormWallException(string message) : base(message) {}

        public StormWallException(string message, Exception inner) : base(message, inner) {}
    }
}
