namespace Mahjong.Core
{
    public enum TileId
    {
        // 萬子 0-8
        Man1, Man2, Man3, Man4, Man5, Man6, Man7, Man8, Man9,
        // 筒子 9-17
        Pin1, Pin2, Pin3, Pin4, Pin5, Pin6, Pin7, Pin8, Pin9,
        // 索子 18-26
        Sou1, Sou2, Sou3, Sou4, Sou5, Sou6, Sou7, Sou8, Sou9,
        // 風牌 27-30
        East, South, West, North,
        // 三元牌 31-33
        White, Green, Red
    }

    public class Tile
    {
        public TileId Id { get; }
        public int Index { get; }

        public Tile(TileId id, int index)
        {
            Id = id;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Id}:{Index}";
        }
    }
}
