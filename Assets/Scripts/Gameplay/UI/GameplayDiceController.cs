using System.Collections.Generic;
using DiceMiner.Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;

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

            var pending = new List<(DiceGameplayData dice, RectTransform anchor)>();
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

                pending.Add((dice, anchor));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(diceAnchorContainer);
            Canvas.ForceUpdateCanvases();

            for (var i = 0; i < pending.Count; i++)
            {
                var (dice, anchor) = pending[i];
                var holder = Instantiate(diceHolderPrefab, diceHoldersContainer);
                holder.Init(dice, anchor, this, i);
                _diceInstances.Add((anchor, holder));
            }
        }

        public bool TryCommitDropFromHolder(DiceHolder holder, Vector2Int cell)
        {
            if (holder == null)
                return false;

            var field = FieldController.Instance;
            if (field == null)
                return false;

            if (field.HasEntityAt(cell))
                return false;

            var dice = holder.GetComponentInChildren<Dice>(true);
            if (dice == null)
                return false;

            var diceRect = dice.transform as RectTransform;

            var index = _diceInstances.FindIndex(x => x.holder == holder);

            var (anchor, h) = _diceInstances[index];
            _diceInstances.RemoveAt(index);

            if (anchor != null)
                Destroy(anchor.gameObject);

            var value = dice.Value;

            diceRect.SetParent(field.EntitiesParent, false);
            MapHelper.ApplyGridCellLayout(diceRect, cell.x, cell.y);
            dice.Init(cell, value);
            field.AddEntity(dice);

            Destroy(h.gameObject);
            return true;
        }
    }
}
