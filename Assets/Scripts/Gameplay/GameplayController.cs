using Cysharp.Threading.Tasks;
using DiceMiner.Camera;
using DiceMiner.Gameplay.UI;
using DiceMiner.UI;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private GameplayUIRoot  gameplayUIRoot;
        [SerializeField] private FieldController fieldController;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private LevelCameraAnchor levelCameraAnchor;
        [SerializeField] private DiceDropController diceDropController;

        private void Start()
        {
            diceDropController.EnableInteraction(false);
        }

        public async UniTask PrepareSave(SavedGame savedGame)
        {
            
        }

        public async UniTask PrepareNextLevel()
        {
            
        }
        
        public async UniTask StartNextLevel()
        {
            await CrossFadeController.StartCrossFade();
            ClearOldGameplay();
            StartGameplay();
            await CrossFadeController.EndCrossFade();
            diceDropController.EnableInteraction(true);
        }

        private void ClearOldGameplay()
        {
            
        }

        private void StartGameplay()
        {
            fieldController.PrepareMap();

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