using System;

namespace DiceMiner.Gameplay
{
    public class Run : IDisposable
    {
        public PhasesController phases;

        public ActionQueue actionQueue;

        public Run()
        {
            phases = new PhasesController();
            actionQueue = new ActionQueue();
        }

        public void Dispose()
        {
            
        }
    }
}