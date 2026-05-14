using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DiceMiner.UI.FlyLabels
{
    public class FlyStringElement: MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private float timeForAlpha;
        [SerializeField] private Ease easing;
        [SerializeField] private float moveTo;
        [SerializeField] private float timeToMove;
        [SerializeField] private float timeForDisapp;
        public Color TintColor
        {
            get => text.color;
            set => text.color = value;
        }
       
        public void SetText(string str)
        {
            if(text != null)
                text.text = str;
        }
        public float GetTimeAlphaApp()
        {
            return timeForAlpha;
        }
        public Ease GetEasing()
        {
            return easing;
        }
        public float GetMoveTo()
        {
            return moveTo;
        }
        public float TimeToMove()
        {
            return timeToMove;
        }
        public float GetTimeAlphaDisapp()
        {
            return timeForDisapp;
        }
    }
}