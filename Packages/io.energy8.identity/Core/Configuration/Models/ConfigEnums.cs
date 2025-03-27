namespace Energy8.Identity.Core.Configuration.Models
{
    public enum IPType
    {
        LocalPC,
        LocalNetwork,
        Debug,
        DebugTLS,
        Production,
        ProductionTLS
    }

    public enum AuthType
    {
        Local,
        Debug,
        Production
    }
}