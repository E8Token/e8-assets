using System;
using UnityEngine;

namespace Energy8.Identity.Core.Configuration.Models
{
    [Serializable]
    public class AuthConfig
    {
        public AuthType authType;
        public TextAsset config;
    }
}