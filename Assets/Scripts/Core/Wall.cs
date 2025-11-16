using System.Collections.Generic;

namespace Mahjong.Core
{
    public class Wall
    {
        private readonly Queue<Tile> _tiles = new Queue<Tile>();

        private Wall() { }

        // シャッフル済みの山を返す（今はダミー）
        public static Wall CreateShuffled()
        {
            var wall = new Wall();

            // とりあえず 136 枚のダミー牌を積む
            for (int i = 0; i < 136; i++)
            {
                wall._tiles.Enqueue(new Tile(TileId.Dummy, i));
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
