using UnityEngine;

namespace DiceMiner.Gameplay.UI
{
    public class GameplayUIRoot : MonoBehaviour
    {

        [SerializeField] private GameplayMainWindow _mainMenu;
        public GameplayMainWindow MainMenu => _mainMenu;
    }
}