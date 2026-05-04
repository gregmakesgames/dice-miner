using System;
using System.Linq;
using UnityEngine;

namespace Map
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private PlayerController playerControllerPrefab;

        private void Awake()
        {
            StartGameplay();
        }

        private void StartGameplay()
        {
            mapGenerator.Generate();
            
            // cameraFollow.Target = player.transform;
            // cameraFollow.JumpToTarget();

        }
    }
}