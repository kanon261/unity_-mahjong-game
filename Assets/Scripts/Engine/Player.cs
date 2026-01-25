using System.Collections.Generic;
using System.Linq;
using Mahjong.Core;

namespace Mahjong.Engine
{
    public class Player
    {
        public int Id { get; }
        public List<Tile> Hand { get; } = new List<Tile>();
        public List<Tile> Discards { get; } = new List<Tile>(); // 捨て牌（河）

        public Player(int id)
        {
            Id = id;
        }

        /// <summary>
        /// 牌を捨てる
        /// </summary>
        public Tile Discard(int index)
        {
            if (index < 0 || index >= Hand.Count) return null;
            var tile = Hand[index];
            Hand.RemoveAt(index);
            Discards.Add(tile);

            // 打牌後に手牌をソート（13枚の状態でソート）
            SortHand();
            return tile;
        }

        /// <summary>
        /// 牌をツモる（ソートしない、最後に追加）
        /// </summary>
        public void Draw(Tile tile)
        {
            if (tile != null)
            {
                Hand.Add(tile);
                // ツモ牌はソートしない（14枚目として右端に表示）
            }
        }

        /// <summary>
        /// 配牌用（ソートする）
        /// </summary>
        public void DrawInitial(Tile tile)
        {
            if (tile != null)
            {
                Hand.Add(tile);
                SortHand();
            }
        }

        /// <summary>
        /// 手牌をソートする（萬子→筒子→索子→字牌の順）
        /// </summary>
        public void SortHand()
        {
            Hand.Sort((a, b) => ((int)a.Id).CompareTo((int)b.Id));
        }

        // とりあえず一番右の牌を捨てるだけ
        public int ChooseDiscardIndex()
        {
            return Hand.Count - 1;
        }
    }
}
