using UnityEngine;
using UnityEngine.InputSystem;

public sealed class DiceDropController : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private Dice dicePrefab;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Camera aimCamera;
    [SerializeField, Min(0.01f)] private float dropDuration = 0.45f;
    [SerializeField, Min(0f)] private float spawnTopMargin = 1f;

    private GameObject ghost;

    private void OnDisable()
    {
        SetGhostVisible(false);
    }

    private void Update()
    {
        if (mapGenerator == null)
        {
            SetGhostVisible(false);
            return;
        }

        if (dicePrefab == null)
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

        if (!mapGenerator.TryWorldXToColumn(pointerWorld.x, out var column))
        {
            SetGhostVisible(false);
            return;
        }

        var landingRow = mapGenerator.GetTopFreeRow(column);
        if (landingRow < 0)
        {
            SetGhostVisible(false);
            return;
        }

        var landing = mapGenerator.GridToWorldPosition(column, landingRow - 1);
        var spawnY = GetSpawnWorldY(cam, landing);
        EnsureGhosts();
        ghost.transform.position = landing;
        SetGhostVisible(true);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DropDice(dicePrefab, landing, spawnY);
        }
    }

    private void DropDice(Dice prefab, Vector3 landing, float spawnY)
    {
        var spawn = new Vector3(landing.x, spawnY, landing.z);
        var parent = mapGenerator.TilesParent;
        var dropped = Instantiate(prefab, spawn, Quaternion.identity, parent);
        dropped.gameObject.name = "DroppedDice";
        dropped.DropTo(landing, dropDuration);
    }

    private void EnsureGhosts()
    {
        if (ghost == null)
        {
            ghost = Instantiate(ghostPrefab, mapGenerator.TilesParent);
        }
    }

    private void SetGhostVisible(bool visible)
    {
        if (ghost != null)
        {
            ghost.SetActive(visible);
        }
    }

    private float GetSpawnWorldY(Camera cam, Vector3 landing)
    {
        var depth = Mathf.Abs(cam.transform.position.z - landing.z);
        var top = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));
        return top.y + spawnTopMargin;
    }

    private bool TryGetPointerWorld(Camera cam, out Vector3 world)
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

        var targetZ = mapGenerator.TilesParent != null ? mapGenerator.TilesParent.position.z : 0f;
        var depth = Mathf.Abs(cam.transform.position.z - targetZ);
        world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        return true;
    }

    private Camera ResolveCamera()
    {
        if (aimCamera != null)
        {
            return aimCamera;
        }

        aimCamera = Camera.main;
        return aimCamera;
    }
}
