using System;

namespace Simple22Ide.Scripts
{
    internal class UnableToCompileException : Exception
    {
        public UnableToCompileException(string message) : base(message) { }
    }
}
