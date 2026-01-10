using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Identity.UI.Core.Compoents
{
    public enum AnimationType
    {
        Idle,
        Random,
        Manual
    }

    [System.Serializable]
    public class SpriteAnimationClip
    {
        public string name;
        public AnimationType type;
        public List<Sprite> sprites = new();
        [Range(0f, 1f)] public float randomProbability = 0.1f;
        public bool canPlayInReverse = false;
        public int frameRate = 30;
        [Range(0.1f, 5f)] public float speed = 1f;
        [Range(0.0f, 30.0f)] public float timeOut = 1f;
    }

    [RequireComponent(typeof(Image))]
    public class ImageSpriteAnimation : MonoBehaviour
    {
        [SerializeField] bool playOnStart = true;
        [SerializeField] List<SpriteAnimationClip> animationClips = new List<SpriteAnimationClip>();
        [SerializeField][Range(1f, 10f)] float maxTransitionSpeed = 3f;
        [SerializeField][HideInInspector] Image image;
        [SerializeField] bool enableLogs = false;

        private CancellationTokenSource cts;
        private string currentClipName;
        private string queuedClipName;
        private readonly float randomCheckInterval = 1f;

        void Reset()
        {
            image = GetComponent<Image>();
        }

        void OnValidate()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        void Start()
        {
            if (playOnStart)
            {
                PlayIdleAnimation();
            }
        }

        void OnDestroy()
        {
            StopAllAnimations();
        }

        public void PlayIdleAnimation()
        {
            var idleClip = animationClips.FirstOrDefault(clip => clip.type == AnimationType.Idle);
            if (idleClip != null)
            {
                PlayAnimation(idleClip.name);
            }
            else
            {
                LogWarning("No idle animation found");
            }
        }

        public void PlayAnimation(string clipName)
        {
            var clip = animationClips.FirstOrDefault(c => c.name == clipName);
            if (clip == null)
            {
                LogWarning($"Animation clip '{clipName}' not found!");
                return;
            }

            // Check if we're already playing this animation
            if (currentClipName == clipName)
            {
                return;
            }

            // If we already have an animation running, queue this one
            if (currentClipName != null && cts != null && !cts.IsCancellationRequested)
            {
                queuedClipName = clipName;
                return;
            }

            // Actually start animation from scratch
            StartNewAnimation(clipName);
        }

        private void StartNewAnimation(string clipName)
        {
            // Stop any existing animations
            StopAllAnimations();

            var clip = animationClips.FirstOrDefault(c => c.name == clipName);
            if (clip == null) return; // Already checked in PlayAnimation, but double-check

            // Set up new animation
            currentClipName = clipName;
            queuedClipName = null;

            cts = new CancellationTokenSource();
            _ = AnimateClipAsync(clip, cts.Token).AttachExternalCancellation(this.GetCancellationTokenOnDestroy());

            if (clip.type == AnimationType.Idle)
            {
                _ = CheckRandomAnimationsAsync(cts.Token);
            }
        }

        public void StopAllAnimations()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
            currentClipName = null;
            queuedClipName = null;
        }

        private async UniTask AnimateClipAsync(SpriteAnimationClip clip, CancellationToken token)
        {
            if (clip == null || clip.sprites == null || clip.sprites.Count == 0)
            {
                LogWarning("Attempted to play invalid clip");
                return;
            }

            int frameIndex = 0;
            int totalFrames = clip.sprites.Count;
            float currentSpeed = clip.speed;
            bool isReverse = false;
            bool transitionRequested = false;
            string nextAnimation = null;
            bool isIdleAnimation = clip.type == AnimationType.Idle;

            while (!token.IsCancellationRequested)
            {
                // Check if we need to transition to a queued animation
                if (queuedClipName != null && !transitionRequested)
                {
                    // Store queued name to avoid it changing during transition
                    nextAnimation = queuedClipName;
                    transitionRequested = true;

                    // If we can play in reverse and we've played less than half of animation
                    if (clip.canPlayInReverse && frameIndex < totalFrames / 2 && frameIndex > 0)
                    {
                        isReverse = true;
                        currentSpeed = maxTransitionSpeed;
                    }
                    else
                    {
                        // Speed up to finish quickly
                        currentSpeed = maxTransitionSpeed;

                        // If this is an idle animation, prepare to transition at end of frame
                        if (isIdleAnimation && frameIndex >= totalFrames - 1)
                        {
                            break; // Exit loop to handle transition outside
                        }
                    }
                }

                // Set's current sprite
                if (image != null)
                {
                    image.sprite = clip.sprites[frameIndex];
                }

                // Update frame index based on direction
                if (isReverse)
                {
                    frameIndex--;
                    if (frameIndex < 0)
                    {
                        break; // Exit loop to handle's transition outside
                    }
                }
                else
                {
                    frameIndex++;
                    if (frameIndex >= totalFrames)
                    {
                        // For Random and Manual animations, they always play once and return to idle
                        if (!isIdleAnimation)
                        {
                            // If there's a manually queued animation, play that first
                            if (transitionRequested)
                            {
                                // Will transition to queued animation
                            }
                            else
                            {
                                // Otherwise, return to idle
                                nextAnimation = animationClips.FirstOrDefault(c => c.type == AnimationType.Idle)?.name;
                                if (nextAnimation != null)
                                {
                                    // Will return to idle
                                }
                                else
                                {
                                    LogWarning($"Animation {clip.name} complete, but no idle animation found to return to");
                                    return;
                                }
                            }
                            break; // Exit's loop to handle transition
                        }

                        // Only idle animations loop
                        if (transitionRequested)
                        {
                            break; // Exit loop to handle transition outside
                        }

                        await UniTask.Delay((int)(clip.timeOut * 1000), cancellationToken: token);
                        frameIndex = 0;
                    }
                }

                // Wait for next frame
                await UniTask.Delay((int)(1000f / clip.frameRate / currentSpeed), cancellationToken: token);
            }

            // Handle transition to next animation outside's animation loop
            if (!token.IsCancellationRequested && nextAnimation != null)
            {
                // Use StartNewAnimation instead of PlayAnimation to avoid recursion issues
                StartNewAnimation(nextAnimation);
            }
            else if (!isIdleAnimation)
            {
                // As a fallback for non-idle animations without a next animation set
                PlayIdleAnimation();
            }
        }

        private async UniTask CheckRandomAnimationsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Only check if we're currently playing idle animation and no queued animation
                if (currentClipName != null && queuedClipName == null)
                {
                    var currentClip = animationClips.FirstOrDefault(c => c.name == currentClipName);
                    if (currentClip != null && currentClip.type == AnimationType.Idle)
                    {
                        // Get all random animations
                        var randomClips = animationClips.Where(c => c.type == AnimationType.Random).ToList();
                        foreach (var randomClip in randomClips)
                        {
                            float randomValue = Random.value;
                            if (randomValue < randomClip.randomProbability)
                            {
                                PlayAnimation(randomClip.name);
                                break;
                            }
                        }
                    }
                }

                await UniTask.Delay((int)(randomCheckInterval * 1000), cancellationToken: token);
            }
        }

        private void Log(string message)
        {
            if (enableLogs)
                Debug.Log($"[{gameObject.name}] {message}");
        }

        private void LogWarning(string message)
        {
            if (enableLogs)
                Debug.LogWarning($"[{gameObject.name}] {message}");
        }
    }
}
