using System.Collections.Generic;

namespace Mahjong.Core
{
    public class Wall
    {
        private readonly Queue<Tile> _tiles = new Queue<Tile>();
        private static readonly System.Random _rng = new System.Random();

        private Wall() { }

        public static Wall CreateShuffled()
        {
            var wall = new Wall();
            var list = new List<Tile>();

            // 34 種類 × 4 枚 = 136 枚
            var allIds = (TileId[])System.Enum.GetValues(typeof(TileId));
            foreach (var id in allIds)
            {
                for (int i = 0; i < 4; i++)
                {
                    list.Add(new Tile(id, i));
                }
            }

            // フィッシャー–イェーツでシャッフル
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            foreach (var t in list)
            {
                wall._tiles.Enqueue(t);
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
