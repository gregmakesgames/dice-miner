using GameData;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public static class DiceFactory
    {
        public static Dice CreateDice(DiceTypeData diceType)
        {
            var prefab = diceType.Prefab;
            if (prefab == null)
            {
                Debug.LogError($"DiceType '{diceType.Id}' has no prefab or Resources.Load failed.");
                return null;
            }

            var instance = MonoBehaviour.Instantiate(prefab);
            var dice = instance.GetComponentInChildren<Dice>(true);
            if (dice == null)
            {
                Debug.LogError($"Dice prefab for type '{diceType.Id}' is missing a {nameof(Dice)} component.");
                MonoBehaviour.Destroy(instance);
                return null;
            }

            return dice;
        }
    }
}
