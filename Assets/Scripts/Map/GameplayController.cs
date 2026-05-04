using System;
using System.Linq;
using UnityEngine;

namespace Map
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private LevelCameraAnchor levelCameraAnchor;
        [SerializeField] private PlayerController playerControllerPrefab;

        private void Awake()
        {
            StartGameplay();
        }

        private void StartGameplay()
        {
            mapGenerator.Generate();

            if (levelCameraAnchor != null)
            {
                levelCameraAnchor.SetTiles(mapGenerator.Tiles);

                if (cameraFollow != null)
                {
                    cameraFollow.SnapTo(levelCameraAnchor.transform);
                }
            }
        }
    }
}