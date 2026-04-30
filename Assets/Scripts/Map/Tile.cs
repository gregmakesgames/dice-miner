using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform innerContainer;
    [SerializeField] private Text healthLabel;
    [SerializeField] private GameObject healthContainer;

    private Action<Tile> releaseCallback;

    public ConfigEntity Config { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public int HittableLevelMin { get; private set; }
    public bool IsHittable => HittableLevelMin <= 1;
    public bool IsDestroyed => MaxHealth > 0 && Health <= 0;

    public event Action<Tile> Destroyed;

    public void Init(ConfigEntity tileConfig, int health, Action<Tile> onReleaseRequested)
    {
        Config = tileConfig;
        releaseCallback = onReleaseRequested;
        MaxHealth = Mathf.Max(0, health);
        Health = MaxHealth;
        HittableLevelMin = tileConfig != null ? Mathf.Max(1, tileConfig.GetInt("hittableLevelMin")) : 1;

        UpdateHealthUI();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = tileConfig != null ? tileConfig.GetSprite("sprite") : null;
        }
        else
        {
            Debug.LogWarning($"Tile '{name}' is missing a SpriteRenderer.", this);
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

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
        }

        UpdateHealthUI();
    }

    private void HandleDestroyed()
    {
        // Cache and clear before invoking so re-entrant calls don't fire twice.
        Action<Tile> callback = releaseCallback;
        Action<Tile> destroyed = Destroyed;
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
