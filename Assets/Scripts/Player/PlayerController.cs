using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerController : MonoBehaviour
{
    private const string DefaultActionMap = "Player";
    private const string DefaultMoveAction = "Move";
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = DefaultActionMap;
    [SerializeField] private string moveActionName = DefaultMoveAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera aimCamera;

    private Rigidbody2D body;
    private InputAction moveAction;
    private InputActionMap playerMap;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (inputActions == null)
        {
            Debug.LogWarning($"PlayerController '{name}' has no InputActionAsset assigned.", this);
            return;
        }

        playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
        if (playerMap == null)
        {
            Debug.LogWarning(
                $"PlayerController '{name}' could not find action map '{actionMapName}'.", this);
            return;
        }

        moveAction = playerMap.FindAction(moveActionName, throwIfNotFound: false);
        if (moveAction == null)
        {
            Debug.LogWarning(
                $"PlayerController '{name}' could not find action '{moveActionName}' in map '{actionMapName}'.",
                this);
        }
    }

    private void OnEnable()
    {
        playerMap?.Enable();
    }

    private void OnDisable()
    {
        playerMap?.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        if (input.sqrMagnitude > 1f)
        {
            input = input.normalized;
        }

        body.linearVelocity = input * moveSpeed;

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, input.magnitude);
        }
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Camera cam = ResolveCamera();
        if (cam == null)
        {
            return;
        }

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
            return;
        }

        // For an orthographic 2D camera the depth picked here is irrelevant
        // for the X comparison; we use the camera-to-z distance to be safe.
        float depth = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));

        float dx = world.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
        {
            spriteRenderer.flipX = dx < 0f;
        }
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

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }
}
