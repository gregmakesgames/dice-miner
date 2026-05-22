using Cysharp.Threading.Tasks;
using GrishaGuWorkshop;

namespace DiceMiner.Gameplay.Actions
{
    public abstract class ActionTag : DataEntityTag
    {
        
    }

    public abstract class ActionTagProcessor
    {
        public abstract UniTask Process(FieldEntity actor, ActionTag actorTag);
    }
}