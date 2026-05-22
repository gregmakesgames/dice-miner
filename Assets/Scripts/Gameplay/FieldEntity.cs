using System;
using GrishaGuWorkshop;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public abstract class FieldEntity : GameObjectBehaviour, IDisposable
    {
        public DataEntity DataEntity { get; protected set; }
        public Vector2Int Position { get; protected set; }

        public abstract void Dispose();
    }
}