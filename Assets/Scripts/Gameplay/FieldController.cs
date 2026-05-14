using System.Collections.Generic;
using System.Linq;
using DiceMiner.Gameplay.Map;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class FieldController : MonoBehaviour
    {
        public static FieldController Instance { get; private set; }
        
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private RectTransform entitiesParent;
        
        public RectTransform EntitiesParent => entitiesParent;
        
        private List<FieldEntity> _fieldEntities = new List<FieldEntity>();
        public IReadOnlyList<FieldEntity> FieldEntities => _fieldEntities;

        private void Start()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PrepareMap()
        {
            _fieldEntities = new List<FieldEntity>();
            var tiles = mapGenerator.Generate(EntitiesParent);
            _fieldEntities.AddRange(tiles);
        }

        public void AddEntity(FieldEntity fieldEntity)
        {
            _fieldEntities.Add(fieldEntity);
        }

        public void RemoveEntity(FieldEntity fieldEntity)
        {
            _fieldEntities.Remove(fieldEntity);
        }
        
        public int GetTopRow(int column)
        {
            if (FieldEntities.Any(x => x.Position.x == column))
            {
                return FieldEntities.Where(x => x.Position.x == column).Min(x => x.Position.y);
            }
            else
            {
                return 0;
            }
        }
    }
}