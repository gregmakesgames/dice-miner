using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FieldEntity : MonoBehaviour
{
    private const string PlacementUnderneath = "underneath";

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform innerContainer;

    private Action<FieldEntity> releaseCallback;

    public ConfigEntity Config { get; private set; }
    public string Placement { get; private set; }
    public bool IsUnderneath => string.Equals(Placement, PlacementUnderneath, StringComparison.Ordinal);

    public void Init(ConfigEntity entityConfig, Action<FieldEntity> onReleaseRequested)
    {
        Config = entityConfig;
        releaseCallback = onReleaseRequested;
        Placement = entityConfig != null ? entityConfig.GetString("placement") : string.Empty;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = entityConfig != null ? entityConfig.GetSprite("sprite") : null;
            // 'underneath' entities sit behind the tile so they only show once the tile is gone.
            spriteRenderer.sortingOrder = IsUnderneath ? -1 : 0;
        }
        else
        {
            Debug.LogWarning($"FieldEntity '{name}' is missing a SpriteRenderer.", this);
        }
    }

    public void Clear()
    {
        Config = null;
        releaseCallback = null;
        Placement = string.Empty;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.sortingOrder = 0;
        }
    }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}
