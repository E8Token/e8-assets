using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

namespace Energy8.Auth.Content
{
    public class DeleteAccountContent : AuthContentBase
    {
        [Header("Config (DeleteAccount)")]
        [SerializeField] int waitingTime = 10;

        [Header("UI (DeleteAccount)")]
        [SerializeField] Button nextBut;
        [SerializeField] TMP_Text nextButText;
        [SerializeField] LocalizeStringEvent nextButLocalizedString;
        [SerializeField] Button cancelBut;

        private protected override void Initialize<TResult>(UniTaskCompletionSource<TResult> taskCompletionSource, params object[] args)
        {
            base.Initialize(taskCompletionSource, args);
            UniTask.Create(async () =>
            {
                nextBut.interactable = false;
                for (int i = waitingTime; i > 0 & !destroyCancellationToken.IsCancellationRequested; i--)
                {
                    nextButText.text = i.ToString();
                    await UniTask.Delay(1000);
                }
                nextButLocalizedString.RefreshString();
                nextBut.interactable = true;
            });
            nextBut.onClick.AddListener(() => taskCompletionSource.TrySetResult(new UpdateContentResult() as TResult));
            cancelBut.onClick.AddListener(() => taskCompletionSource.TrySetCanceled());
        }
    }
    public class DeleteAccountContentResult : AuthContentResultBase
    { }
}