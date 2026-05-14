using UnityEngine;

namespace DiceMiner.Gameplay.UI
{
    public class GameplayUIRoot : MonoBehaviour
    {

        [SerializeField]
        private GameplayMainWindow _mainMenu;
        public GameplayMainWindow MainMenu => _mainMenu;

        [SerializeField]
        private GameplayLevelWindow _levelWindow;
        public GameplayLevelWindow LevelWindow => _levelWindow;
    }
}