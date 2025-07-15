using System;

namespace Energy8.Identity.Shared.Core.Exceptions
{
    public class Energy8Exception : Exception
    {
        public string Header { get; set; }
        public bool CanProceed { get; set; }
        public bool CanRetry { get; set; }
        public bool MustSignOut { get; set; }

        public Energy8Exception(
            string header,
            string description,
            bool canProceed = false,
            bool canRetry = false,
            bool mustSignOut = false) : base(description)
        {
            Header = header;
            CanProceed = canProceed;
            CanRetry = canRetry;
            MustSignOut = mustSignOut;
        }
        public Energy8Exception(string description) : base(description)
        { }
    }
}