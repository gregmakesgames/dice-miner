using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DiceMiner.Gameplay.Map;
using DiceMiner.Meta;
using UnityEngine;

namespace DiceMiner.Gameplay.Actions
{
    public class PlainDiceAction : Action
    {
        [SerializeField] private Dice thisDice;
        
        public async override UniTask Act()
        {
            var buffAmount = Mathf.FloorToInt(Game.Instance.UpgradeManager.GetUpgradeValue(UpgradeIds.DICE_BUFF_OTHER_DICE) * thisDice.Value);
            var tasks = new List<UniTask>();
            foreach (var elem in NearEntities())
            {
                if (elem is Tile tile)
                {
                    var task = tile.TakeDamage(thisDice.Value);
                    tasks.Add(task);
                }

                if (elem is Dice dice && buffAmount != 0)
                {
                    var task = dice.ModifyValue(buffAmount);
                    tasks.Add(task);
                }
            }
            
            await UniTask.WhenAll(tasks);
            Destroy(thisDice.gameObject);
            FieldController.Instance.RemoveEntity(thisDice);
        }

        private IEnumerable<FieldEntity> NearEntities()
        {
            var upgrade = Game.Instance.UpgradeManager.GetUpgradeValue(UpgradeIds.DICE_DISTANCE);
            int distance = Mathf.FloorToInt(1 + upgrade);

            return FieldController.Instance.FieldEntities.Where(x => (x.Position - thisDice.Position).magnitude <= distance);
        }
    }
}