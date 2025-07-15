using System;
using UnityEngine;

namespace Energy8.Identity.Configuration.Core
{
    [Serializable]
    public class AuthConfig
    {
        public AuthType authType;
        public TextAsset config;
    }
}