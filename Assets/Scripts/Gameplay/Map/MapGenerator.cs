using System.Collections.Generic;
using DiceMiner.Gameplay;
using GameData;
using UnityEngine;

namespace Gameplay.Map
{
    public sealed class MapGenerator : MonoBehaviour
    {
        [Header("Random")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int seed = 12345;

        private IMapGeneratorAlhorythm map;
        private System.Random random;
        private Tile pooledTilePrefab;
        private int generatedWidth;
        private int generatedHeight;

        public List<Tile> Generate(Transform tileParent, int difficulty = 1)
        {
            var generatedWidth = VerticalMapGeneratorAlhorythm.GetWidthForDifficulty(difficulty);
            var generatedHeight = VerticalMapGeneratorAlhorythm.GetHeightForDifficulty(difficulty);

            var resultingTiles = new List<Tile>();

            random = useRandomSeed ? new System.Random() : new System.Random(seed);

            map = new VerticalMapGeneratorAlhorythm(difficulty);

            map.Initialize(GameDataRegistry.GetAll<TileTypeData>(), generatedWidth, generatedHeight, random);

            for (int y = 0; y < generatedHeight; y++)
            {
                for (int x = 0; x < generatedWidth; x++)
                {
                    Vector3 localPos = MapHelper.GridToLocalPosition(x, y);

                    var tileData = map.PickTile(x, y);
                    if (tileData != null)
                    {
                        Tile tile = TilePool.Get();
                        tile.name = $"Tile_{x}_{y}_{tileData.Id}";
                        tile.transform.parent = tileParent;
                        tile.transform.localPosition = localPos;
                        tile.Init(tileData, new Vector2Int(x, y), RollHealth(tileData));
                        resultingTiles.Add(tile);
                    }
                }
            }

            return resultingTiles;
        }

        private int RollHealth(TileTypeData config)
        {
            int min = Mathf.Max(0, config.HealthMin);
            int max = Mathf.Max(min, config.HealthMax);
            if (min == max)
            {
                return min;
            }

            return random.Next(min, max + 1);
        }
    }
}