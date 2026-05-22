using DiceMiner.Gameplay.Actions;
using GrishaGuWorkshop;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class PhasesController
    {
        public enum Phases
        {
            Prepare,
            PlaceDices,
            ActivateQueue,
            End,
        }
        
        public Phases CurrentPhase { get; private set; }
        public int TurnCount { get; private set; } = 0;
        
        private void PreparePhase()
        {
            TurnCount++;
            Game.run.actionQueue.PerformActionsForTrigger(ActionTriggerTag.TriggerType.PreparePhase);

        }

        private void PlaceDicesPhase()
        {
            
        }

        private void ActivateQueuePhase()
        {
            Game.run.actionQueue.PerformActionsForTrigger(ActionTriggerTag.TriggerType.PreparePhase);
        }

        private void EndPhase()
        {
            // Check winning conditions
            
            
        }
    }
}