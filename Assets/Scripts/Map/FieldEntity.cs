using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FieldEntity : MonoBehaviour
{
    private const string PlacementUnderneath = "underneath";
    private const string PlacementReplace = "replace";

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform innerContainer;
    [SerializeField] private Text healthLabel;
    [SerializeField] private GameObject healthContainer;

    private Action<FieldEntity> releaseCallback;

    public ConfigEntity Config { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public int HittableLevelMin { get; private set; }
    public string Placement { get; private set; }
    public bool IsHittable => HittableLevelMin <= 1;
    public bool IsDestroyed => MaxHealth > 0 && Health <= 0;
    public bool IsUnderneath => string.Equals(Placement, PlacementUnderneath, StringComparison.Ordinal);

    public event Action<FieldEntity> Destroyed;

    public void Init(ConfigEntity entityConfig, int health, Action<FieldEntity> onReleaseRequested)
    {
        Config = entityConfig;
        releaseCallback = onReleaseRequested;
        MaxHealth = Mathf.Max(0, health);
        Health = MaxHealth;
        HittableLevelMin = entityConfig != null ? Mathf.Max(1, entityConfig.GetInt("hittableLevelMin")) : 1;
        Placement = entityConfig != null ? entityConfig.GetString("placement") : string.Empty;

        UpdateHealthUI();

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

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || MaxHealth <= 0 || Health <= 0 || !IsHittable)
        {
            return;
        }

        Health = Mathf.Max(0, Health - amount);
        UpdateHealthUI();

        if (Health <= 0)
        {
            HandleDestroyed();
        }
    }

    public void Clear()
    {
        Config = null;
        releaseCallback = null;
        Destroyed = null;
        Health = 0;
        MaxHealth = 0;
        HittableLevelMin = 1;
        Placement = string.Empty;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.sortingOrder = 0;
        }

        UpdateHealthUI();
    }

    private void HandleDestroyed()
    {
        // Cache and clear before invoking so re-entrant calls don't fire twice.
        Action<FieldEntity> callback = releaseCallback;
        Action<FieldEntity> destroyed = Destroyed;
        releaseCallback = null;

        destroyed?.Invoke(this);
        callback?.Invoke(this);
    }

    private void UpdateHealthUI()
    {
        bool showHealth = MaxHealth > 0 && IsHittable;

        if (healthContainer != null)
        {
            healthContainer.SetActive(showHealth);
        }

        if (healthLabel != null)
        {
            healthLabel.text = showHealth ? Health.ToString() : string.Empty;
        }
    }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}
