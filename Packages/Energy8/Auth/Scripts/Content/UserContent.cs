using Cysharp.Threading.Tasks;
using Energy8.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class UserContent : AuthContentBase
    {
        [Header("UI (User)")]
        [SerializeField] TMP_Text titleText;
        [SerializeField] Button openSettingsBut;
        [SerializeField] Button signOutBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            titleText.text = (string)args[0];
            openSettingsBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new UserContentResult(UserWindowAction.OpenSettings) as TResult));
            signOutBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new UserContentResult(UserWindowAction.SignOut) as TResult));
        }
    }
    public class UserContentResult : AuthContentResultBase
    {
        public UserWindowAction ResultType;
        public UserContentResult(UserWindowAction resultType)
        {
            ResultType = resultType;
        }
    }
    public enum UserWindowAction
    {
        OpenSettings,
        SignOut
    }
}