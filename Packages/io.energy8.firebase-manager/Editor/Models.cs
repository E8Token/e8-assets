using System.Collections.Generic;

namespace Energy8.FirebaseManager
{
    public class FirebasePackage
    {
        public string PackageId { get; set; }
        public string DisplayName { get; set; }
    }

    public class VersionInfo
    {
        public string Version { get; set; }
        public string TgzUrl { get; set; }
        public List<DependencyInfo> Dependencies { get; set; } = new List<DependencyInfo>();
    }

    public class DependencyInfo
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string TgzUrl { get; set; }
    }
}