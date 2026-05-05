using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay
{
    public abstract class Action : MonoBehaviour
    {
        public int Priority { get; set; }
        public abstract UniTask Act();
    }
}