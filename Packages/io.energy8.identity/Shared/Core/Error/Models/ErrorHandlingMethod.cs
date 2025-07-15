using System;

namespace Energy8.Identity.Shared.Core.Error
{
    public enum ErrorHandlingMethod
    {
        Close,
        TryAgain,
        SignOut
    }
}