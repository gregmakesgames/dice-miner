using System;
using GameData;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public abstract class FieldEntity : MonoBehaviour, IDisposable
    {
        public DataEntity DataEntity { get; protected set; }
        public Vector2Int Position { get; protected set; }

        public abstract void Dispose();
    }
}