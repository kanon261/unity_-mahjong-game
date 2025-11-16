using System.Collections.Generic;
using Mahjong.Core;

namespace Mahjong.Engine
{
    public class Player
    {
        public int Id { get; }
        public List<Tile> Hand { get; } = new List<Tile>();

        public Player(int id)
        {
            Id = id;
        }

        public void Draw(Tile tile)
        {
            if (tile != null)
            {
                Hand.Add(tile);
            }
        }

        // とりあえず一番右の牌を捨てるだけ
        public int ChooseDiscardIndex()
        {
            return Hand.Count - 1;
        }
    }
}
