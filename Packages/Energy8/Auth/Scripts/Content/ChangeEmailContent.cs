using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class ChangeEmailContent : AuthContentBase
    {
        const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [Header("UI (Login)")]
        [SerializeField] TMP_InputField emailIF;
        [SerializeField] Button nextBut;
        [SerializeField] Button closeBut;


        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);

            emailIF.onValueChanged.AddListener((email) =>
            {
                nextBut.interactable = IsValidEmail(email);
            });

            closeBut.onClick.AddListener(() => taskCompletionSource.TrySetCanceled());
            nextBut.onClick.AddListener(() => taskCompletionSource.TrySetResult(
                new SignInContentResult(SignInMethod.Email, emailIF.text) as TResult));
        }

        static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            return Regex.IsMatch(email, EMAIL_PATTERN);
        }
    }


    public class ChangeEmailContentResult : AuthContentResultBase
    {
        public string Email { get; set; }
        public ChangeEmailContentResult(string email)
        {
            Email = email;
        }
    }
}