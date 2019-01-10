using System;

namespace Dnn.KeyMaster.Exceptions
{
    public class ForbiddenKeyMasterException : Exception
    {
        public ForbiddenKeyMasterException()
            : base("Forbidden access, unable to complete request. Check access token permissions and try again.") { }
    }
}
