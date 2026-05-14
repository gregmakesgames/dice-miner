using UnityEngine;

namespace DiceMiner.Gameplay
{
    public static class MapHelper
    {
        public const float TILE_SIZE = 100.0f;

        public static void ApplyGridCellLayout(RectTransform rect, int x, int y)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(TILE_SIZE, TILE_SIZE);
            rect.anchoredPosition = (Vector2)GridToLocalPosition(x, y);
        }

        public static bool TryWorldPointToGridCell(Vector3 worldPoint, out Vector2Int cell)
        {
            cell = default;
            var field = FieldController.Instance;
            if (field == null || field.EntitiesParent == null)
                return false;

            var local = field.EntitiesParent.InverseTransformPoint(worldPoint);
            var x = Mathf.RoundToInt(local.x / TILE_SIZE - 0.5f);
            var y = Mathf.RoundToInt(0.5f - local.y / TILE_SIZE);
            cell = new Vector2Int(x, y);
            return true;
        }

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

        public static Vector2 GridToAnchorPosition(int x, int y)
        {
            var centeredX = (x + 0.5f) * TILE_SIZE;
            var centeredY = -(y + 0.5f) * TILE_SIZE;
            return new Vector2(centeredX, centeredY);
        }
    }
}