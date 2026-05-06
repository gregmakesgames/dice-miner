using System;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public abstract class FieldEntity : MonoBehaviour, IDisposable
    {
        public Vector2Int Position { get; protected set; }

        public abstract void Dispose();
    }
}