using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace DiceMiner.Gameplay.Actions
{
    public class ActionQueue
    {
        public async void PerformActionsForTrigger(ActionTriggerTag.TriggerType trigger, object triggerSource = null)
        {
            var allEntitiesToTrigger = Game.run.field.Entities.SelectMany(e =>
            {
                var triggerTag = e.DataEntity.GetAllTags<ActionTriggerTag>();
                return triggerTag.Where(at => at.trigger == trigger && at.TriggerSourceFits(triggerSource))
                    .Select(at => (e, at));
            });

            foreach (var (e, at) in allEntitiesToTrigger)
            {
                if (at.HasTag<ActionTag>())
                {
                    await UniTask.WhenAll(
                        at.GetAllTags<ActionTag>()
                            .Select(x => ProcessActionTag(x, e)));
                }
            }
        }

        private readonly Dictionary<string, ActionTagProcessor> _actionTagProcessors = new ();

        private UniTask ProcessActionTag(ActionTag actionTag, FieldEntity fieldEntity)
        {
            var fullname = actionTag.GetType().FullName;
            if (fullname == null)
            {
                return UniTask.CompletedTask;
            }
            
            if (!_actionTagProcessors.ContainsKey(fullname))
            {
                var processorType = actionTag.GetType().Assembly.GetTypes()
                    .First(x => x.FullName == fullname + "Processor");
                ActionTagProcessor processor = Activator.CreateInstance(processorType) as ActionTagProcessor;
                _actionTagProcessors.Add(fullname, processor);
            }

            return _actionTagProcessors[fullname].Process(fieldEntity, actionTag);
        }
    }
}