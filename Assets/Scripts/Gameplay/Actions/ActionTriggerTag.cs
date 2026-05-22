using System;
using GrishaGuWorkshop;

namespace DiceMiner.Gameplay.Actions
{
    [Serializable]
    public class ActionTriggerTag : DataEntityTag
    {
        public enum TriggerType
        {
            PreparePhase,
            MainPhase,
            
        }

        public TriggerType trigger;

        public virtual bool TriggerSourceFits(object triggerSource = null)
        {
            return true;
        }
    }
}