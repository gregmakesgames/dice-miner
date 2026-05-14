using System.Collections.Generic;
using DiceMiner.Gameplay.Data;
using UnityEngine;

namespace DiceMiner.Gameplay.UI
{
    public class GameplayDiceController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject diceAnchorPrefab;
        [SerializeField] private DiceHolder diceHolderPrefab;

        [Header("Containers")]
        [SerializeField] private RectTransform diceAnchorContainer;
        [SerializeField] private RectTransform diceHoldersContainer;

        private readonly List<(RectTransform anchor, DiceHolder holder)> _diceInstances = new();

        public void ClearDices()
        {
            foreach (var (anchor, holder) in _diceInstances)
            {
                if (holder != null)
                    Destroy(holder.gameObject);
                if (anchor != null)
                    Destroy(anchor.gameObject);
            }

            _diceInstances.Clear();
        }

        public void CreateDices(List<DiceGameplayData> dices)
        {
            ClearDices();

            if (dices == null || dices.Count == 0)
                return;

            if (diceAnchorPrefab == null || diceHolderPrefab == null ||
                diceAnchorContainer == null || diceHoldersContainer == null)
            {
                Debug.LogError("GameplayDiceController: prefab or container reference is missing.");
                return;
            }

            foreach (var dice in dices)
            {
                var anchorGo = Instantiate(diceAnchorPrefab, diceAnchorContainer);
                var anchor = anchorGo.transform as RectTransform;
                if (anchor == null)
                {
                    Debug.LogError("GameplayDiceController: diceAnchorPrefab must have a RectTransform root.");
                    Destroy(anchorGo);
                    continue;
                }

                var holder = Instantiate(diceHolderPrefab, diceHoldersContainer);
                holder.Init(dice, anchor);
                _diceInstances.Add((anchor, holder));
            }
        }
    }
}
