using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DiceMiner.Gameplay
{
    public sealed class Dice : FieldEntity
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform innerContainer;
        [SerializeField] private Transform manyNumberContainer;
        [SerializeField] private Text manyNumberLabel;

        [Header("Images")]
        [SerializeField] private Sprite sprite1;
        [SerializeField] private Sprite sprite2;
        [SerializeField] private Sprite sprite3;
        [SerializeField] private Sprite sprite4;
        [SerializeField] private Sprite sprite5;
        [SerializeField] private Sprite sprite6;
        [SerializeField] private Sprite spriteMany;
    
        public int Value { get; private set; }
    
        public void Init(Vector2Int position, int value)
        {
            Position = position;
            Value = value;
            UpdateDiceImage();
        }
    
        public void DropTo(Vector3 worldTarget, float duration)
        {
            transform.DOKill();
        
            transform.DOMove(worldTarget, Mathf.Max(0.01f, duration)).SetEase(Ease.InQuad);
        }

        public async UniTask ModifyValue(int delta)
        {
            Value += delta;
            UpdateDiceImage(true);
        }

        private void UpdateDiceImage(bool animated = false)
        {
            switch (Value)
            {
                case 1:
                    spriteRenderer.sprite = sprite1; 
                    manyNumberContainer.gameObject.SetActive(false);
                    break;
                case 2:
                    spriteRenderer.sprite = sprite2; 
                    manyNumberContainer.gameObject.SetActive(false);
                    break;
                case 3:
                    spriteRenderer.sprite = sprite3; 
                    manyNumberContainer.gameObject.SetActive(false);
                    break;
                case 4:
                    spriteRenderer.sprite = sprite4; 
                    manyNumberContainer.gameObject.SetActive(false);
                    break;
                case 5:
                    spriteRenderer.sprite = sprite5; 
                    manyNumberContainer.gameObject.SetActive(false);
                    break;
                case 6:
                    spriteRenderer.sprite = sprite6; 
                    manyNumberContainer.gameObject.SetActive(false);
                    break;
                default:
                    spriteRenderer.sprite = spriteMany;
                    manyNumberLabel.text = $"{Value}";
                    manyNumberContainer.gameObject.SetActive(true);
                    break;
            }

            innerContainer.DOPunchScale(new Vector3(2, 2, 2), 0.3f, 0);
        }    
    
        private void Reset()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public override void Dispose()
        {
            Destroy(gameObject);
        }
    }
}
