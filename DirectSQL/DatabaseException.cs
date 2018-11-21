using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSQL
{
    public class DatabaseException : Exception
    {
        internal DatabaseException(String message, Exception exception) : base( message, exception) {}
    }
}
