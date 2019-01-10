using System;

namespace Dnn.KeyMaster.Exceptions
{
    public class KeyMasterException : Exception
    {
        public KeyMasterException(string message) : base(message) { }
        public KeyMasterException(string message, Exception inner) : base(message, inner) { }
    }
}
