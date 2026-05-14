using Cysharp.Threading.Tasks;
using DiceMiner.Gameplay.UI;
using DiceMiner.UI;
using GameData;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private GameplayUIRoot  gameplayUIRoot;
        [SerializeField] private FieldController fieldControllerPrefab;

        private SavedGame _savedGame;
        private FieldController _fieldController;
        
        public async UniTask PrepareSave(SavedGame savedGame)
        {
            _savedGame = savedGame;
            GoToSelectLevelMenu();
        }

        private void GoToSelectLevelMenu()
        {
            //gameplayUIRoot.MainMenu.Show();
            StartNextLevel();
        }

        private void GoToLevel(LevelData level)
        {
            
        }

        private async UniTask PrepareNextLevel()
        {
            
        }
        
        private async UniTask StartNextLevel()
        {
            await CrossFadeController.StartCrossFade();
            ClearOldGameplay();
            StartGameplay();
            await CrossFadeController.EndCrossFade();
        }

        private void ClearOldGameplay()
        {
            if (_fieldController != null)
            {
                Destroy(_fieldController.gameObject);
                _fieldController = null;    
            }
        }

        private void StartGameplay()
        {
            _fieldController = Instantiate(fieldControllerPrefab, transform);
            _fieldController.PrepareMap();
        }
    }
}