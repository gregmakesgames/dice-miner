using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DiceMiner.UI.FlyLabels
{
    public static class FlyLabelManager
    {
        private static FlyStringElement _flyStringPrefab = Resources.Load<FlyStringElement>("UI/FlyStringElement");

        public static UniTask LaunchString(string text)
        {
            return LaunchString(text, FlyLabelContainer.Instance.DefaultAnchor);
        }

        public static UniTask LaunchString(string text, RectTransform anchor)
        {
            return LaunchCustomElement(_flyStringPrefab, text, anchor);
        }

        private static FlyStringElement InstantiateFlyLabel(FlyStringElement prefab, string text)
        {
            var flyString = GameObject.Instantiate(prefab, FlyLabelContainer.Instance.DefaultAnchor);

            if (flyString != null)
            {
                flyString.transform.SetParent(FlyLabelContainer.Instance.DefaultAnchor);
                flyString.SetText(text);
            }

            return flyString;
        }

        public static UniTask LaunchCustomElement(FlyStringElement prefab)
        {
            return LaunchCustomElement(prefab, "");
        }

        public static UniTask LaunchCustomElement(FlyStringElement prefab, string text)
        {
            return LaunchCustomElement(prefab, text, FlyLabelContainer.Instance.DefaultAnchor);
        }

        public static UniTask LaunchCustomElement(FlyStringElement prefab, string text, RectTransform anchor)
        {
            var textElement = InstantiateFlyLabel(prefab, text);
            var anchorPosition = anchor.position;
            var localPosition = textElement.transform.parent.GetComponent<RectTransform>().InverseTransformPoint(anchorPosition);
            textElement.GetComponent<RectTransform>().localPosition = localPosition;
            
            bool isCompleted = false;
            textElement.gameObject.transform.AnimateAString(0f, textElement.GetTimeAlphaApp(),
                textElement.GetEasing(),
                () =>
                {
                    isCompleted = true;
                    GameObject.Destroy(textElement.gameObject);
                }, textElement.GetMoveTo(), textElement.TimeToMove(),
                textElement.GetTimeAlphaDisapp());


            return UniTask.WaitUntil(() => isCompleted);
        }
    }
}