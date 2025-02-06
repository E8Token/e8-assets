namespace Energy8.Identity.Views.Models
{

    public class ErrorViewParams : ViewParams
    {
        public string Header { get; set; }
        public string Description { get; set; }
        public bool CanProceed { get; set; }
        public bool CanRetry { get; set; }
        public bool MustSignOut { get; set; }

        public ErrorViewParams(
        string header,
        string description,
        bool canProceed = false,
        bool canRetry = false,
        bool mustSignOut = false)
        {
            Header = header;
            Description = description;
            CanProceed = canProceed;
            CanRetry = canRetry;
            MustSignOut = mustSignOut;
        }
    }
}
