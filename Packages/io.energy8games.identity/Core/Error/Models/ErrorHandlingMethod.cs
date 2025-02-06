using System;

namespace Energy8.Identity.Core.Error
{
    public enum ErrorHandlingMethod
    {
        Close,
        TryAgain,
        SignOut
    }
}