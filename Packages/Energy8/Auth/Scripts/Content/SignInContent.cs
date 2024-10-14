using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Energy8.Models;
using Energy8.Models.User;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class SignInContent : AuthContentBase
    {
        const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        [Header("UI (Login)")]
        [SerializeField] TMP_InputField emailIF;
        [SerializeField] Button nextBut;
        [SerializeField] Button appleBut;
        [SerializeField] Button googleBut;


        private protected override void Initialize<TResult>(UniTaskCompletionSource<TryResult<TResult>> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);

            emailIF.onValueChanged.AddListener((email) =>
            {
                nextBut.interactable = IsValidEmail(email);
            });

            appleBut.onClick.AddListener(() => taskCompletionSource.TrySetResult(
                TryResult<TResult>.CreateSuccessful(new SignInContentResult(SignInMethod.Apple, string.Empty) as TResult)));
            googleBut.onClick.AddListener(() => taskCompletionSource.TrySetResult(
                TryResult<TResult>.CreateSuccessful(new SignInContentResult(SignInMethod.Google, string.Empty) as TResult)));
            nextBut.onClick.AddListener(() => taskCompletionSource.TrySetResult(
                TryResult<TResult>.CreateSuccessful(new SignInContentResult(SignInMethod.Email, emailIF.text) as TResult)));
        }

        static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            return Regex.IsMatch(email, EMAIL_PATTERN);
        }
    }


    public class SignInContentResult : AuthContentResultBase
    {
        public SignInMethod SignInMethod { get; set; }
        public string Email {get; set;}
        public SignInContentResult(SignInMethod signInMethod, string email)
        {
            SignInMethod = signInMethod;
            Email = email;
        }
    }
}