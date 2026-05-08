using Cysharp.Threading.Tasks;
using DiceMiner.Camera;
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
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private LevelCameraAnchor levelCameraAnchor;
        [SerializeField] private DiceDropController diceDropController;

        private SavedGame _savedGame;
        private FieldController _fieldController;
        
        private void Start()
        {
            diceDropController.EnableInteraction(false);
        }

        public async UniTask PrepareSave(SavedGame savedGame)
        {
            _savedGame = savedGame;
            GoToSelectLevelMenu();
        }

        private void GoToSelectLevelMenu()
        {
            gameplayUIRoot.MainMenu.Show();
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
            diceDropController.EnableInteraction(true);
        }

        private void ClearOldGameplay()
        {
            Destroy(_fieldController.gameObject);
            _fieldController = null;
        }

        private void StartGameplay()
        {
            _fieldController = Instantiate(fieldControllerPrefab);
            _fieldController.PrepareMap();

            if (levelCameraAnchor != null)
            {
                levelCameraAnchor.Init();

                if (cameraFollow != null)
                {
                    cameraFollow.SnapTo(levelCameraAnchor.transform);
                }
            }
        }
    }
}