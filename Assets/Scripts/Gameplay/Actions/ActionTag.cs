using Cysharp.Threading.Tasks;
using GrishaGuWorkshop;

namespace DiceMiner.Gameplay.Actions
{
    public class ActionTag : DataEntityTag
    {
        
    }

    public abstract class ActionTagProcessor
    {
        public abstract UniTask Process();
    }
}