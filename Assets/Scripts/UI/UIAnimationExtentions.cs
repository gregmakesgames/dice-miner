using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace DiceMiner.UI
{
    public static class UIAnimationExtentions
    {
          private class TweenerKiller : IDisposable
        {
            private List<Tweener> _tweens;
            private List<Sequence> _sequences;

            public TweenerKiller(List<Tweener> tweens)
            {
                _tweens = tweens;
            }

            public TweenerKiller(List<Sequence> sequences)
            {
                _sequences = sequences;
            }

            public void Dispose()
            {
                if (_tweens != null)
                {
                    foreach (var elem in _tweens)
                    {
                        elem.Kill();
                    }
                }

                if (_sequences != null)
                {
                    foreach (var elem in _sequences)
                    {
                        elem.Kill();
                    }
                }
            }
        }

        public static IDisposable AnimateAppearance(this Transform transform, float startValue, float timeForScale,
            float timeForAlpha, Ease easing, Action onComplite)
        {
            List<Tweener> tweens = new List<Tweener>();

            if (transform != null)
            {
                transform.localScale = new Vector3(0f, 0f, 0f);
                var scale = transform.DOScale(Vector3.one, timeForScale).SetEase(easing);
                tweens.Add(scale);
                var canvasGroup = transform.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = startValue;
                    var alpha = canvasGroup.DOFade(1, timeForAlpha)
                        .OnComplete(() => onComplite?.Invoke());
                    tweens.Add(alpha);
                }
            }

            return new TweenerKiller(tweens);
        }
        public static IDisposable AnimateAString(this Transform transform, float startValue,
            float timeForAlpha, Ease easing, Action onComplete,float moveTo,float timeFoMove,float timeForDisapp)
        {
            List<Tweener> tweens = new List<Tweener>();

            transform.AnimateAppearance(startValue, 0f, timeForAlpha, easing, () =>
            {
               var move = transform.DOMoveY(transform.position.y + moveTo, timeFoMove).OnComplete(() =>
                {
                    transform.AnimateDisappearance(timeForDisapp, () => { onComplete?.Invoke(); });
                });
                tweens.Add(move);
            });

            return new TweenerKiller(tweens);
        }

        public static IDisposable AnimateDisappearance(this Transform transform, float timeForAlpha, Action onComplite)
        {
            List<Tweener> tweens = new List<Tweener>();
            if (transform != null)
            {
                var canvasGroup = transform.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    var alpha = canvasGroup.DOFade(0, timeForAlpha)
                        .OnComplete(() => onComplite?.Invoke());
                    tweens.Add(alpha);
                }
            }

            return new TweenerKiller(tweens);
        }

        public static IDisposable AppearanceFlashingElement(this Transform transform, float duration, float delay,
            Action onAppear)
        {
            List<Tweener> tweens = new List<Tweener>();
            if (transform != null)
            {
                var canvasGroup = transform.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0;
                    var alpha = canvasGroup.DOFade(1, duration).SetDelay(delay).OnComplete(() =>
                    {
                        onAppear?.Invoke();
                        if (transform != null)
                        {
                            AnimateFlashingElement(transform, duration + 2, tweens);
                        }
                    });
                    tweens.Add(alpha);
                }
            }

            return new TweenerKiller(tweens);
        }

        private static void AnimateFlashingElement(this Transform transform, float duration, List<Tweener> tweens)
        {
            if (transform != null)
            {
                var alpha = transform.GetComponent<CanvasGroup>().DOFade(0, duration)
                    .OnComplete(() => transform.GetComponent<CanvasGroup>().DOFade(1, duration)).SetLoops(-1);
                tweens.Add(alpha);
            }
        }

        public static IDisposable AnimateScale(this Transform transform, float scale, float time, Ease easing)
        {
            List<Sequence> sequences = new List<Sequence>();
            if (transform != null)
            {
                var sequence = DOTween.Sequence().Append(
                        transform.DOScale(new Vector3(transform.localScale.x / scale,
                                transform.localScale.y / scale), time)
                            .SetEase(easing))
                    .Append(transform.DOScale(new Vector3(transform.localScale.x * scale,
                        transform.localScale.y * scale), time).SetEase(easing))
                    .Append(transform.DOScale(transform.localScale, time).SetEase(easing));
                sequences.Add(sequence);
            }

            return new TweenerKiller(sequences);
        }


        public static IDisposable AnimateLoopScale(this Transform transform, float scale, float time, Ease easing)
        {
            List<Sequence> sequences = new List<Sequence>();
            if (transform != null)
            {
                var sequence = DOTween.Sequence().Append(
                        transform.DOScale(new Vector3(transform.localScale.x / scale,
                                    transform.localScale.y / scale),
                                time)
                            .SetEase(easing))
                    .Append(
                        transform.DOScale(Vector3.one,
                                time)
                            .SetEase(easing))
                    .Append(transform.DOScale(new Vector3(
                            transform.localScale.x * scale,
                            transform.localScale.y * scale), time)
                        .SetEase(easing))
                    .Append(
                        transform.DOScale(Vector3.one,
                                time)
                            .SetEase(easing))
                    .SetLoops(-1);
                sequences.Add(sequence);
            }

            return new TweenerKiller(sequences);
        }

        public static IDisposable AnimateBubble(this Transform transform, float moved, float time, Ease easing)
        {
            List<Sequence> sequences = new List<Sequence>();
            if (transform != null)
            {
                var tweenSequence = DOTween.Sequence().Append(
                        transform.DORotate(
                            new Vector3(0, 0,
                                transform.position.z + moved), time))
                    .Append(
                        transform.DORotate(
                            new Vector3(0, 0,
                                transform.position.z - moved), time))
                    .Append(
                        transform.DORotate(new Vector3(0, 0, 0), time))
                    .SetEase(easing).SetLoops(-1);
                sequences.Add(tweenSequence);
            }

            return new TweenerKiller(sequences);
        }
    }
}