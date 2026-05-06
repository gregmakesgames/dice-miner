using System.Collections.Generic;
using GameData;
using UnityEngine;

namespace DiceMiner.Gameplay.Map
{
    public sealed class VerticalMapGeneratorAlhorythm : IMapGeneratorAlhorythm
    {
        private const string FillerFamily = "filler";
        private const string OutlineFamily = "outline";

        // Difficulty 1 produces a 5x100 vertical strip; each extra level
        // widens the shaft slightly and lengthens it considerably.
        private const int BaseWidth = 7;
        private const int BaseHeight = 100;
        private const int WidthStep = 2;
        private const int HeightStep = 50;

        private const int ExitRowOffset = 1;          // distance from bottom outline

        private readonly int difficulty;

        private TileTypeData[,] tiles;
        private int gridWidth;
        private int gridHeight;
        private bool ready;

        public int Difficulty => difficulty;

        public VerticalMapGeneratorAlhorythm(int difficulty)
        {
            this.difficulty = Mathf.Max(1, difficulty);
        }

        public static int GetWidthForDifficulty(int difficulty)
        {
            return BaseWidth + (Mathf.Max(1, difficulty) - 1) * WidthStep;
        }

        public static int GetHeightForDifficulty(int difficulty)
        {
            return BaseHeight + (Mathf.Max(1, difficulty) - 1) * HeightStep;
        }

        public void Initialize(
            IReadOnlyList<TileTypeData> tileConfigs,
            int width,
            int height,
            System.Random random)
        {
            ready = false;
            tiles = null;

            // Vertical generator owns its dimensions; the requested width/height
            // are treated as a hint and clamped to what difficulty produces.
            gridWidth = GetWidthForDifficulty(difficulty);
            gridHeight = GetHeightForDifficulty(difficulty);

            if (tileConfigs == null || tileConfigs.Count == 0 || gridWidth <= 0 || gridHeight <= 0)
            {
                return;
            }

            var rng = random ?? new System.Random();

            var fillers = new List<TileTypeData>();
            var outlines = new List<TileTypeData>();

            for (var i = 0; i < tileConfigs.Count; i++)
            {
                var entry = tileConfigs[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.Family == FillerFamily)
                {
                    fillers.Add(entry);
                }
                else if (entry.Family == OutlineFamily)
                {
                    outlines.Add(entry);
                }
            }

            if (fillers.Count == 0 || outlines.Count == 0)
            {
                Debug.LogWarning(
                    "VerticalDungeonGenerator: missing tile configs. " +
                    $"Found fillers={fillers.Count}, outlines={outlines.Count}. " +
                    "Need at least one TileType per family ('filler', 'outline').");
                return;
            }

            BuildLayout(rng, fillers, outlines);
            ready = true;
        }

        public TileTypeData PickTile(int x, int y)
        {
            if (!ready || tiles == null || x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
            {
                return null;
            }

            return tiles[x, y];
        }

        private void BuildLayout(System.Random rng, List<TileTypeData> fillers, List<TileTypeData> outlines)
        {
            tiles = new TileTypeData[gridWidth, gridHeight];

            // Pick a single outline tile so the perimeter looks consistent,
            // but vary the filler per cell to give the shaft visual variety.
            var outlineTile = outlines[rng.Next(0, outlines.Count)];

            for (var y = 0; y < gridHeight; y++)
            {
                for (var x = 0; x < gridWidth; x++)
                {
                    var isPerimeter = x == 0 || y == gridHeight - 1 || x == gridWidth - 1;
                    tiles[x, y] = isPerimeter
                        ? outlineTile
                        : fillers[rng.Next(0, fillers.Count)];
                }
            }
        }
    }
}
