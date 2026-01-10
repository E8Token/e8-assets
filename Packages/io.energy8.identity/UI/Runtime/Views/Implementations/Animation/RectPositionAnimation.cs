using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Energy8.Identity.UI.Runtime.Views.Animation
{
    public class RectPositionAnimation : ViewAnimationBase
    {
        private readonly RectTransform target;
        private readonly Vector2 from;
        private readonly Vector2 to;
        private readonly bool fullWidth;

        /// <summary>
        /// Создает анимацию перемещения RectTransform
        /// </summary>
        /// <param name="target">Целевой RectTransform</param>
        /// <param name="from">Начальная позиция</param>
        /// <param name="to">Конечная позиция</param>
        /// <param name="duration">Продолжительность анимации</param>
        /// <param name="fullWidth">Если true, ширина будет рассчитана на основе window.Width^2 / window.Height</param>
        /// <param name="curve">Кривая анимации</param>
        public RectPositionAnimation(
            RectTransform target, 
            Vector2 from, 
            Vector2 to, 
            float duration, 
            bool fullWidth = false,
            AnimationCurve curve = null) : base(duration, curve)
        {
            this.target = target;
            this.from = from;
            this.to = to;
            this.fullWidth = fullWidth;
        }

        public override async UniTask Play(CancellationToken ct)
        {
            IsPlaying = true;
            float elapsed = 0;

            // Если включен режим полной ширины, обновляем ширину объекта
            if (fullWidth)
            {
                UpdateWidth();
            }

            // Запоминаем текущий размер экрана для отслеживания изменений
            Vector2 lastScreenSize = new Vector2(Screen.width, Screen.height);

            while (elapsed < duration && !ct.IsCancellationRequested && IsPlaying)
            {
                // Проверяем, изменился ли размер экрана
                Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
                if (fullWidth && currentScreenSize != lastScreenSize)
                {
                    UpdateWidth();
                    lastScreenSize = currentScreenSize;
                }

                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                Vector2 current = Vector2.Lerp(from, to, EvaluateProgress(progress));
                
                target.anchoredPosition = current;
                await UniTask.Yield();
            }

            if (IsPlaying && !ct.IsCancellationRequested)
            {
                target.anchoredPosition = to;
            }

            IsPlaying = false;
        }

        /// <summary>
        /// Обновляет ширину объекта по формуле window.Width^2 / window.Height
        /// </summary>
        private void UpdateWidth()
        {
            if (target != null)
            {
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                float desiredWidth = (screenWidth * screenWidth) / screenHeight;

                Vector2 sizeDelta = target.sizeDelta;
                sizeDelta.x = desiredWidth;
                target.sizeDelta = sizeDelta;
            }
        }
    }
}
