using DG.Tweening;
using Gameplay;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Dice : FieldEntity
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Init(Vector2Int position)
    {
        Position = position;
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
