using Cysharp.Threading.Tasks;
using Energy8.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class AnalyticsContent : AuthContentBase
    {
        [Header("UI (Analytics)")]
        [SerializeField] Button yesBut;
        [SerializeField] Button noBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            yesBut.onClick.AddListener(() => 
                taskCompletionSource.TrySetResult(new AnalyticsContentResult(true) as TResult));
            noBut.onClick.AddListener(() => 
                taskCompletionSource.TrySetResult(new AnalyticsContentResult(false) as TResult));
        }
    }
    public class AnalyticsContentResult : AuthContentResultBase
    {
        public bool IsDetailedAnalyticsAllowed { get; set; }

        public AnalyticsContentResult(bool allowed)
        {
            IsDetailedAnalyticsAllowed = allowed;
        }
    }
}