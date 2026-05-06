using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DiceMiner.UI
{
    public class CrossFadeController : MonoBehaviour
    {
        private const string PrefabResourcePath = "UI/CrossFadeController";

        public static CrossFadeController Instance { get; private set; }
        
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.35f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetAlpha(0f);
        }

        public static async UniTask StartCrossFade()
        {
            if (!EnsureInstance())
            {
                return;
            }

            await Instance.FadeTo(1f);
        }
        
        public static async UniTask EndCrossFade()
        {
            if (!EnsureInstance())
            {
                return;
            }
            
            await Instance.FadeTo(0f);
        }

        private static bool EnsureInstance()
        {
            if (Instance != null)
            {
                return true;
            }
            
            var prefab = Resources.Load<GameObject>(PrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"CrossFadeController prefab not found at Resources/{PrefabResourcePath}.");
                return false;
            }
            
            var instanceObject = Instantiate(prefab);
            Instance = instanceObject.GetComponent<CrossFadeController>();
            if (Instance == null)
            {
                Debug.LogError("CrossFadeController component is missing on the prefab.");
                Destroy(instanceObject);
                return false;
            }

            return true;
        }

        private async UniTask FadeTo(float targetAlpha)
        {
            if (fadeImage == null)
            {
                Debug.LogError("CrossFadeController fadeImage is not assigned.");
                return;
            }

            var color = fadeImage.color;
            var startAlpha = color.a;
            var elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsedTime / fadeDuration);
                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                fadeImage.color = color;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            color.a = targetAlpha;
            fadeImage.color = color;
        }

        private void SetAlpha(float alpha)
        {
            if (fadeImage == null)
            {
                return;
            }

            var color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}