using Gameplay.Map;
using UnityEngine;

namespace Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private FieldController fieldController;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private LevelCameraAnchor levelCameraAnchor;
        [SerializeField] private PlayerController playerControllerPrefab;
        [SerializeField] private DiceDropController diceDropController;

        private void Start()
        {
            StartGameplay();
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