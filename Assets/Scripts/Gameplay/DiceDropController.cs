using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceMiner.Gameplay
{
    public sealed class DiceDropController : MonoBehaviour
    {
        [SerializeField] private Dice dicePrefab;
        [SerializeField] private GameObject ghostPrefab;
        [SerializeField] private UnityEngine.Camera aimCamera;
        [SerializeField, Min(0.01f)] private float dropDuration = 0.45f;
        [SerializeField, Min(0f)] private float spawnTopMargin = 1f;

        private GameObject ghost;

        private bool _interactionEnabled = true;

        private void OnDisable()
        {
            SetGhostVisible(false);
        }

        private void Update()
        {
            if (!_interactionEnabled)
            {
                SetGhostVisible(false);
                return;
            }
            
            var cam = ResolveCamera();
            if (cam == null)
            {
                SetGhostVisible(false);
                return;
            }

            if (!TryGetPointerWorld(cam, out var pointerWorld))
            {
                SetGhostVisible(false);
                return;
            }

            var column = MapHelper.GetColumnByWorldX(pointerWorld.x);
            var landingRow = FieldController.Instance.GetTopRow(column);

            var landing = MapHelper.GridToWorldPosition(column, landingRow - 1);
            var spawnY = GetSpawnWorldY(cam, landing);
            EnsureGhosts();
            ghost.transform.position = landing;
            SetGhostVisible(true);

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                DropDice(dicePrefab, new Vector2Int(column, landingRow - 1), spawnY);
            }
        }

        private void DropDice(Dice prefab, Vector2Int position, float spawnY)
        {
            var landingPosition = MapHelper.GridToWorldPosition(position.x, position.y);
            var spawn = new Vector3(landingPosition.x, spawnY, landingPosition.z);
            var parent = FieldController.Instance.EntitiesParent;
            var dice = Instantiate(prefab, spawn, Quaternion.identity, parent);
            dice.gameObject.name = "Dice";
            dice.Init(position, Random.Range(1, 7));
            dice.DropTo(landingPosition, dropDuration);
            FieldController.Instance.AddEntity(dice);
        }

        private void EnsureGhosts()
        {
            if (ghost == null)
            {
                ghost = Instantiate(ghostPrefab, FieldController.Instance.EntitiesParent);
            }
        }

        private void SetGhostVisible(bool visible)
        {
            if (ghost != null)
            {
                ghost.SetActive(visible);
            }
        }

        private float GetSpawnWorldY(UnityEngine.Camera cam, Vector3 landing)
        {
            var depth = Mathf.Abs(cam.transform.position.z - landing.z);
            var top = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));
            return top.y + spawnTopMargin;
        }

        private bool TryGetPointerWorld(UnityEngine.Camera cam, out Vector3 world)
        {
            Vector2 screen;
            if (Mouse.current != null)
            {
                screen = Mouse.current.position.ReadValue();
            }
            else if (Pointer.current != null)
            {
                screen = Pointer.current.position.ReadValue();
            }
            else
            {
                world = default;
                return false;
            }

            var targetZ = FieldController.Instance.EntitiesParent != null ? FieldController.Instance.EntitiesParent.position.z : 0f;
            var depth = Mathf.Abs(cam.transform.position.z - targetZ);
            world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            return true;
        }

        private UnityEngine.Camera ResolveCamera()
        {
            if (aimCamera != null)
            {
                return aimCamera;
            }

            aimCamera = UnityEngine.Camera.main;
            return aimCamera;
        }

        public void EnableInteraction(bool enabled)
        {
            _interactionEnabled = enabled;
        }
    }
}
