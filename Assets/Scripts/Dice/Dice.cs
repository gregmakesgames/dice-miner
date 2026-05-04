using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Dice : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Color defaultColor = Color.white;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
    }

    public void DropTo(Vector3 worldTarget, float duration)
    {
        transform.DOKill();
        transform.DOMove(worldTarget, Mathf.Max(0.01f, duration)).SetEase(Ease.InQuad);
    }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}
