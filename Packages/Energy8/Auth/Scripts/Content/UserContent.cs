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
        [SerializeField] Button changeNameBut;
        [SerializeField] Button signOutBut;
        [SerializeField] Button deleteAccountBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            titleText.text = (string)args[0];
            changeNameBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new UserContentResult(UserWindowAction.ChangeName) as TResult));
            signOutBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new UserContentResult(UserWindowAction.SignOut) as TResult));
            deleteAccountBut.onClick.AddListener(() =>
                taskCompletionSource.TrySetResult(new UserContentResult(UserWindowAction.DeleteAccount) as TResult));
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
        ChangeName,
        SignOut,
        DeleteAccount
    }
}