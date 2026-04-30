using System;
using GameData;
using UnityEngine;

[DisallowMultipleComponent]
public class FieldEntity : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform innerContainer;

    private Action<FieldEntity> releaseCallback;

    public FieldEntityData Config { get; private set; }
    public bool IsUnderneath => Config != null && Config.IsUnderneath;

    public void Init(FieldEntityData entityConfig, Action<FieldEntity> onReleaseRequested)
    {
        Config = entityConfig;
        releaseCallback = onReleaseRequested;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = entityConfig != null ? entityConfig.Sprite : null;
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
