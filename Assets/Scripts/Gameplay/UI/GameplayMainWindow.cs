using UnityEngine;

namespace DiceMiner.Gameplay.UI
{
    public class GameplayMainWindow : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}