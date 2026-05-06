
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public static class MapHelper
    {
        public const float TILE_SIZE = 1.0f;
        
        public static int GetColumnByWorldX(float worldX)
        {
            var localX = FieldController.Instance.EntitiesParent.InverseTransformPoint(new Vector3(worldX, 0f, 0f)).x;
            var normalized = localX / TILE_SIZE - 0.5f;
            var rawColumn = Mathf.RoundToInt(normalized);
            return rawColumn;
        }

        public static Vector3 GridToWorldPosition(int x, int y)
        {
            var local = GridToLocalPosition(x, y);
            return FieldController.Instance.EntitiesParent.TransformPoint(local);
        }

        public static Vector3 GridToLocalPosition(int x, int y)
        {
            float centeredX = (x + 0.5f) * TILE_SIZE;
            float centeredY = (-y + 0.5f) * TILE_SIZE;
            return new Vector3(centeredX, centeredY, 0f);
        }
    }
}