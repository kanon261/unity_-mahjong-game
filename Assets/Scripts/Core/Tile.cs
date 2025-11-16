namespace Mahjong.Core
{
    // ひとまずダミー（本当の牌は後で実装）
    public enum TileId
    {
        Dummy = 0
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
