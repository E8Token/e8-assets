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
        [SerializeField] bool enableLogs = true;

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
                Log("Playing idle animation on start");
                PlayIdleAnimation();
            }
        }

        void OnDestroy()
        {
            StopAllAnimations();
        }

        public void PlayIdleAnimation()
        {
            Debug.Log(animationClips[0].type);
            var idleClip = animationClips.FirstOrDefault(clip => clip.type == AnimationType.Idle);
            if (idleClip != null)
            {
                Log($"Playing idle animation: {idleClip.name}");
                PlayAnimation(idleClip.name);
            }
            else
            {
                LogWarning("No idle animation found");
            }
        }

        public void PlayAnimation(string clipName)
        {
            Log($"Request to play animation: {clipName}");
            var clip = animationClips.FirstOrDefault(c => c.name == clipName);
            if (clip == null)
            {
                LogWarning($"Animation clip '{clipName}' not found!");
                return;
            }

            // Check if we're already playing this animation
            if (currentClipName == clipName)
            {
                Log($"Already playing animation '{clipName}', ignoring request");
                return;
            }

            // If we already have an animation running, queue this one
            if (currentClipName != null && cts != null && !cts.IsCancellationRequested)
            {
                Log($"Queueing animation '{clipName}' after current animation '{currentClipName}'");
                queuedClipName = clipName;
                return;
            }

            // Actually start the animation from scratch
            StartNewAnimation(clipName);
        }

        private void StartNewAnimation(string clipName)
        {
            // Stop any existing animations
            StopAllAnimations();

            var clip = animationClips.FirstOrDefault(c => c.name == clipName);
            if (clip == null) return; // Already checked in PlayAnimation, but double-check

            // Set up the new animation
            currentClipName = clipName;
            queuedClipName = null;

            Log($"Starting animation: {clipName} (Type: {clip.type})");
            cts = new CancellationTokenSource();
            _ = AnimateClipAsync(clip, cts.Token).AttachExternalCancellation(this.GetCancellationTokenOnDestroy());

            if (clip.type == AnimationType.Idle)
            {
                Log("Starting random animation check for idle animation");
                _ = CheckRandomAnimationsAsync(cts.Token);
            }
        }

        public void StopAllAnimations()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                Log($"Stopping animation: {currentClipName}");
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

            Log($"Starting animation playback: {clip.name} with {totalFrames} frames at speed {currentSpeed}");

            while (!token.IsCancellationRequested)
            {
                // Check if we need to transition to a queued animation
                if (queuedClipName != null && !transitionRequested)
                {
                    // Store the queued name to avoid it changing during transition
                    nextAnimation = queuedClipName;
                    transitionRequested = true;

                    // If we can play in reverse and we've played less than half of the animation
                    if (clip.canPlayInReverse && frameIndex < totalFrames / 2 && frameIndex > 0)
                    {
                        isReverse = true;
                        currentSpeed = maxTransitionSpeed;
                        Log($"Reversing animation {clip.name} at frame {frameIndex}/{totalFrames} with speed {currentSpeed}");
                    }
                    else
                    {
                        // Speed up to finish quickly
                        currentSpeed = maxTransitionSpeed;
                        Log($"Speeding up animation {clip.name} to {currentSpeed} to transition faster");

                        // If this is an idle animation, prepare to transition at end of frame
                        if (isIdleAnimation && frameIndex >= totalFrames - 1)
                        {
                            Log($"Idle animation {clip.name} at end of cycle, will transition to {nextAnimation}");
                            break; // Exit the loop to handle the transition outside
                        }
                    }
                }

                // Set the current sprite
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
                        Log($"Reverse animation {clip.name} complete, will transition to {nextAnimation}");
                        break; // Exit the loop to handle the transition outside
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
                                Log($"Animation {clip.name} complete, will transition to queued animation {nextAnimation}");
                            }
                            else
                            {
                                // Otherwise, return to idle
                                nextAnimation = animationClips.FirstOrDefault(c => c.type == AnimationType.Idle)?.name;
                                if (nextAnimation != null)
                                {
                                    Log($"Animation {clip.name} complete, returning to idle animation {nextAnimation}");
                                }
                                else
                                {
                                    LogWarning($"Animation {clip.name} complete, but no idle animation found to return to");
                                    return;
                                }
                            }
                            break; // Exit loop to handle transition
                        }

                        // Only idle animations loop
                        if (transitionRequested)
                        {
                            Log($"Idle animation {clip.name} complete, will transition to {nextAnimation} instead of looping");
                            break; // Exit the loop to handle the transition outside
                        }

                        Log($"Idle animation {clip.name} reached end, looping after timeout {clip.timeOut}s");
                        await UniTask.Delay((int)(clip.timeOut * 1000), cancellationToken: token);
                        frameIndex = 0;
                    }
                }

                // Wait for next frame
                await UniTask.Delay((int)(1000f / clip.frameRate / currentSpeed), cancellationToken: token);
            }

            // Handle transition to next animation outside the animation loop
            if (!token.IsCancellationRequested && nextAnimation != null)
            {
                Log($"Animation {clip.name} finished, transitioning to {nextAnimation}");
                // Use StartNewAnimation instead of PlayAnimation to avoid recursion issues
                StartNewAnimation(nextAnimation);
            }
            else if (token.IsCancellationRequested)
            {
                Log($"Animation {clip.name} cancelled");
            }
            else if (!isIdleAnimation)
            {
                // As a fallback for non-idle animations without a next animation set
                Log($"Non-idle animation {clip.name} finished with no transition target, returning to idle");
                PlayIdleAnimation();
            }
        }

        private async UniTask CheckRandomAnimationsAsync(CancellationToken token)
        {
            Log("Starting random animation check cycle");
            while (!token.IsCancellationRequested)
            {
                // Only check if we're currently playing idle animation and no queued animation
                if (currentClipName != null && queuedClipName == null)
                {
                    Log(currentClipName);
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
                                Log($"Random animation triggered: {randomClip.name} (probability: {randomClip.randomProbability}, roll: {randomValue})");
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