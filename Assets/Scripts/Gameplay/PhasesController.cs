using DiceMiner.Gameplay.Actions;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class PhasesController : MonoBehaviour
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
            Game.run.actionQueue.PerformActionsOfTag<PreparePhaseTag>();

        }

        private void PlaceDicesPhase()
        {
            
        }

        private void ActivateQueuePhase()
        {
            Game.run.actionQueue.PerformActionsOfTag<MainPhaseTag>();
        }

        private void EndPhase()
        {
            // Check winning conditions
            
            
        }

        private void Update()
        {
            
        }
    }
}