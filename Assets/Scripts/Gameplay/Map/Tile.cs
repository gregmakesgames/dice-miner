using GameData;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Map
{
    public class Tile : FieldEntity
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform innerContainer;
        [SerializeField] private Text healthLabel;
        [SerializeField] private GameObject healthContainer;

        public TileTypeData Config { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int HittableLevelMin { get; private set; }
        public bool IsHittable => HittableLevelMin <= 1;
        public bool IsDestroyed => MaxHealth > 0 && Health <= 0;

        public void Init(TileTypeData tileConfig, Vector2Int position, int health)
        {
            Position = position;
            Config = tileConfig;
            MaxHealth = Mathf.Max(0, health);
            Health = MaxHealth;
            HittableLevelMin = tileConfig != null ? Mathf.Max(1, tileConfig.HittableLevelMin) : 1;

            UpdateHealthUI();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = tileConfig != null ? tileConfig.Sprite : null;
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
            VisualMessageBroker.TryVisualize(this, VisualMessageType.TileDestroyed);

            TilePool.Release(this);
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
}
