namespace Energy8.Firebase.Core.Models
{
    public class FirebaseAppInfo
    {
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public string ApiKey { get; set; }
        public string AppId { get; set; }
        public bool IsInitialized { get; set; }
        public bool IsDataCollectionEnabled { get; set; }
    }
}
