using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageSpriteAnimation : MonoBehaviour
{
    [SerializeField] bool playOnStart = true;
    [SerializeField] bool isLoop = true;
    [SerializeField] List<Sprite> sprites;
    [SerializeField] int frameRate = 30;
    [SerializeField][Range(0.1f, 5f)] float speed = 1f;
    [SerializeField][Range(0.0f, 30.0f)] float timeOut = 1f;
    [SerializeField][HideInInspector] Image image;

    CancellationTokenSource cts;

    void Reset()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        if (playOnStart)
            Play();
    }

    public void Play()
    {
        cts = new();
        _ = AnimateAsync(cts.Token).AttachExternalCancellation(destroyCancellationToken);
    }
    public void Stop()
    {
        cts.Cancel();
    }

    async UniTask AnimateAsync(CancellationToken token)
    {
        int i = 0;
        while (gameObject != null & !token.IsCancellationRequested)
        {
            image.sprite = sprites[i];
            if (++i == sprites.Count)
            {
                if (!isLoop) return;
                await UniTask.Delay((int)(timeOut * 1000));
                i = 0;
            }
            await UniTask.Delay((int)(1000f / frameRate / speed));
        }
    }
}
