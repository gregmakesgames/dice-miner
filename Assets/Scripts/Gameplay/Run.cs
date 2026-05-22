using System;
using DiceMiner.Gameplay.Actions;

namespace DiceMiner.Gameplay
{
    public class Run : IDisposable
    {
        public FieldController field;
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