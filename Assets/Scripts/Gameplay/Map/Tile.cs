using Cysharp.Threading.Tasks;
using GrishaGuWorkshop;
using UnityEngine;
using UnityEngine.UI;

namespace DiceMiner.Gameplay.Map
{
    public class Tile : FieldEntity
    {
        [SerializeField] private Image tileImage;
        [SerializeField] private Transform innerContainer;
        [SerializeField] private Text healthLabel;
        [SerializeField] private GameObject healthContainer;

        public DataEntity Config { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int HittableLevelMin { get; private set; }
        public bool IsHittable => HittableLevelMin <= 1;
        public bool IsDestroyed => MaxHealth > 0 && Health <= 0;

        public void Init(DataEntity tileConfig, Vector2Int position, int health)
        {
            Position = position;
            Config = tileConfig;
            MaxHealth = Mathf.Max(0, health);
            Health = MaxHealth;
            HittableLevelMin = 1;

            UpdateHealthUI();

            if (tileImage != null)
            {
                // TODO: move this logics to FieldEntity
                //tileImage.sprite = tileConfig != null ? tileConfig.Sprite : null;
            }
        }

        public async UniTask TakeDamage(int amount)
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

            if (tileImage != null)
            {
                tileImage.sprite = null;
            }

            UpdateHealthUI();
        }

        private void HandleDestroyed()
        {
            Dispose();
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

        public override void Dispose()
        {
            TilePool.Release(this);
        }
    }
}
