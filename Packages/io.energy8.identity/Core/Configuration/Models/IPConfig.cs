using System;
using UnityEngine;

namespace Energy8.Identity.Core.Configuration.Models
{
    [Serializable]
    public class IPConfig
    {
        public IPType ipType;
        public string ipAddress;
    }
}