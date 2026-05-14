using UnityEngine;

namespace DiceMiner.Gameplay.UI
{
    public class GameplayLevelWindow : MonoBehaviour
    {
        [SerializeField] private GameplayDiceController diceController;
        public GameplayDiceController DiceController => diceController;

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}