using System;

namespace DirectSQL
{
    public class DatabaseException : Exception
    {
        internal DatabaseException(String message, Exception exception) : base( message, exception) {}
    }
}
