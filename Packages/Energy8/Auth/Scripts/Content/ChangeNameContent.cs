using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Energy8.Models;
using Energy8.Models.User;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Auth.Content
{
    public class ChangeNameContent : AuthContentBase
    {
        [Header("UI (ChangeName)")]
        [SerializeField] TMP_InputField nameIF;
        [SerializeField] Button nextBut;


        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);

            nameIF.onValueChanged.AddListener((name) =>
                nextBut.interactable = name.Length > 3);

            nextBut.onClick.AddListener(() => taskCompletionSource.TrySetResult(
                new ChangeNameContentResult(nameIF.text) as TResult));
        }
    }


    public class ChangeNameContentResult : AuthContentResultBase
    {
        public string Name { get; set; }
        public ChangeNameContentResult(string name)
        {
            Name = name;
        }
    }
}