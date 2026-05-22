using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GrishaGuWorkshop;
using UnityEngine;

namespace DiceMiner.Gameplay.Actions
{
    [Serializable]
    public class SimpleDamageActionTag : ActionTag
    {
        public int damage;
    }
    
    public class SimpleDamageActionTagProcessor : ActionTagProcessor{
        public override UniTask Process(FieldEntity actor, ActionTag actorTag)
        {
            var tag = actorTag as SimpleDamageActionTag;
            
            if (actorTag.GetTag<SfxTag>() != null)
            {
                Game.audio.PlaySound2D(actorTag.GetTag<SfxTag>());
            }

            int distance = 1;
            
            foreach (var entity in Game.run.field.Entities)
            {
                if (entity == actor) continue;
                var xDiff = Mathf.Abs(actor.Position.x - entity.Position.x);
                var yDiff = Mathf.Abs(actor.Position.y - entity.Position.y);
                if (xDiff == 0 && yDiff <= distance ||
                    yDiff == 0 && xDiff <= distance)
                {
                    if(entity.GetComponent<Health>()?.TryMakeDamage(tag.damage) ?? false)
                    {
                        // Game.ui.flyLabels.
                        // Game.vfx.SpawnParticle
                    }
                }
            }
            
            return UniTask.CompletedTask;
        }
    }
}