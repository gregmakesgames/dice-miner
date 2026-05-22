using GrishaGuWorkshop;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class FieldEntity : GameObjectBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        public DataEntity DataEntity { get; protected set; }
        public Vector2Int Position { get; protected set; }
    }
}