using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Energy8.FirebaseManager
{
    public class FirebasePackageParser
    {
        private readonly HttpClient httpClient;
        
        public FirebasePackageParser(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        
        public async Task<string> FetchHtmlFromFirebaseArchive()
        {
            return await httpClient.GetStringAsync("https://developers.google.com/unity/archive");
        }
        
        public Dictionary<string, FirebasePackage> GetFirebasePackages()
        {
            var packages = new Dictionary<string, FirebasePackage>();
            
            // Register all Firebase packages
            AddFirebasePackage(packages, "com.google.firebase.app", "Firebase App (Core)");
            AddFirebasePackage(packages, "com.google.firebase.auth", "Firebase Authentication");
            AddFirebasePackage(packages, "com.google.firebase.database", "Firebase Realtime Database");
            AddFirebasePackage(packages, "com.google.firebase.firestore", "Cloud Firestore");
            AddFirebasePackage(packages, "com.google.firebase.storage", "Cloud Storage for Firebase");
            AddFirebasePackage(packages, "com.google.firebase.functions", "Cloud Functions for Firebase");
            AddFirebasePackage(packages, "com.google.firebase.messaging", "Firebase Cloud Messaging");
            AddFirebasePackage(packages, "com.google.firebase.analytics", "Google Analytics for Firebase");
            AddFirebasePackage(packages, "com.google.firebase.crashlytics", "Firebase Crashlytics");
            AddFirebasePackage(packages, "com.google.firebase.remote-config", "Firebase Remote Config");
            AddFirebasePackage(packages, "com.google.firebase.app-check", "Firebase App Check");
            AddFirebasePackage(packages, "com.google.external-dependency-manager", "External Dependency Manager for Unity");
            
            return packages;
        }
        
        private void AddFirebasePackage(Dictionary<string, FirebasePackage> packages, string packageId, string displayName)
        {
            packages[packageId] = new FirebasePackage
            {
                PackageId = packageId,
                DisplayName = displayName
            };
        }
        
        public Dictionary<string, List<VersionInfo>> ParseVersions(string html, Dictionary<string, FirebasePackage> packages)
        {
            var packageVersions = new Dictionary<string, List<VersionInfo>>();
            
            foreach (var packageId in packages.Keys.ToList())
            {
                // Find package sections and extract version info
                string pattern = $@"<code[^>]*>{Regex.Escape(packageId)}</code>.*?<table.*?>.*?<\/table>";
                Match packageMatch = Regex.Match(html, pattern, RegexOptions.Singleline);
                
                if (packageMatch.Success)
                {
                    string packageHtml = packageMatch.Value;
                    
                    // Extract versions and download URLs
                    pattern = @"<tr>.*?<td>([\d\.]+)</td>.*?<a href=""(https://dl.google.com/games/registry/unity/[^""]+\.tgz)"">";
                    MatchCollection versionMatches = Regex.Matches(packageHtml, pattern, RegexOptions.Singleline);
                    
                    List<VersionInfo> versions = new List<VersionInfo>();
                    foreach (Match versionMatch in versionMatches)
                    {
                        string version = versionMatch.Groups[1].Value.Trim();
                        string tgzUrl = versionMatch.Groups[2].Value.Trim();
                        
                        VersionInfo versionInfo = new VersionInfo
                        {
                            Version = version,
                            TgzUrl = tgzUrl
                        };
                        
                        // Try to extract dependencies for this version
                        string versionHtml = versionMatch.Value;
                        
                        // Look for dependencies
                        pattern = @"<a href=""(https://dl.google.com/games/registry/unity/([^""]+)/(.[^""]+)-(\d+\.\d+\.\d+)\.tgz)"">(.[^<]+)</a>";
                        MatchCollection depMatches = Regex.Matches(versionHtml, pattern, RegexOptions.Singleline);
                        
                        foreach (Match depMatch in depMatches)
                        {
                            string depUrl = depMatch.Groups[1].Value.Trim();
                            string depPackageId = depMatch.Groups[2].Value.Trim();
                            string depVersion = depMatch.Groups[4].Value.Trim();
                            
                            // Don't add self as dependency
                            if (depPackageId != packageId)
                            {
                                versionInfo.Dependencies.Add(new DependencyInfo
                                {
                                    PackageId = depPackageId,
                                    Version = depVersion,
                                    TgzUrl = depUrl
                                });
                            }
                        }
                        
                        versions.Add(versionInfo);
                    }
                    
                    if (versions.Count > 0)
                    {
                        packageVersions[packageId] = versions;
                    }
                }
                else
                {
                    Debug.Log($"Could not find package section for {packageId}");
                }
            }
            
            return packageVersions;
        }
    }
}