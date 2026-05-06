using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public abstract class Action : MonoBehaviour
    {
        public int Priority { get; set; }
        public abstract UniTask Act();
    }
}