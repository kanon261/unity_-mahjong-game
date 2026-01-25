using System.Collections.Generic;

namespace Mahjong.Core
{
    public class Wall
    {
        private readonly Queue<Tile> _tiles = new Queue<Tile>();

        private Wall() { }

        // シャッフル済みの山を返す
        public static Wall CreateShuffled()
        {
            var wall = new Wall();
            var allTiles = new List<Tile>();

            // 34種類の牌を各4枚ずつ生成（136枚）
            for (int tileId = 0; tileId < 34; tileId++)
            {
                for (int index = 0; index < 4; index++)
                {
                    allTiles.Add(new Tile((TileId)tileId, index));
                }
            }

            // シャッフル
            var rng = new System.Random();
            int n = allTiles.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var temp = allTiles[k];
                allTiles[k] = allTiles[n];
                allTiles[n] = temp;
            }

            // キューに追加
            foreach (var tile in allTiles)
            {
                wall._tiles.Enqueue(tile);
            }

            return wall;
        }

        public int Count => _tiles.Count;

        public Tile Draw()
        {
            return _tiles.Count > 0 ? _tiles.Dequeue() : null;
        }
    }
}
