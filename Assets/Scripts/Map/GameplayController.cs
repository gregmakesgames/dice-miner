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

            var playerSpawn = mapGenerator.FieldEntities.First(x => x.Config.Id == MapEntitiesIds.PLAYER_SPAWN);
            
            var player = Instantiate(playerControllerPrefab, playerSpawn.transform.position, playerSpawn.transform.rotation);

            cameraFollow.Target = player.transform;
            cameraFollow.JumpToTarget();

        }
    }
}