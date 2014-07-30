using System;

namespace Complexion.Portable.Exceptions
{
    public class NotConnectedToPlexException : Exception
    {
        private const string YouAreNotConnectedToPlex = "You are not connected to Plex";

        public NotConnectedToPlexException() : base(YouAreNotConnectedToPlex)
        {
        }

        public NotConnectedToPlexException(Exception innerException)
            : base(YouAreNotConnectedToPlex, innerException)
        {
        }
    }
}