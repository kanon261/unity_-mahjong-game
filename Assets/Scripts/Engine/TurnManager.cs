using Mahjong.Core;
using UnityEngine;

namespace Mahjong.Engine
{
    public sealed class TurnManager
    {
        private readonly Player[] _players = new Player[4];
        private Wall _wall;
        private int _turnIndex; // 0..3

        // 直前の捨て牌
        public Tile LastDiscard { get; private set; }

        public TurnManager()
        {
            for (int i = 0; i < 4; i++)
            {
                _players[i] = new Player(i);
            }
        }

        public void StartNewHand()
        {
            _wall = Wall.CreateShuffled();
            _turnIndex = 0;
            LastDiscard = null;

            // 配牌：4人に13枚ずつ
            for (int r = 0; r < 13; r++)
            {
                for (int p = 0; p < 4; p++)
                {
                    _players[p].Draw(_wall.Draw());
                }
            }

            // 東家だけ14枚目ツモ
            _players[0].Draw(_wall.Draw());

            Debug.Log("=== 新しい局を開始 ===");
        }

        public bool StepOnce()
        {
            if (_wall == null) return false;

            var p = _players[_turnIndex];

            // ツモ
            var drawn = _wall.Draw();
            if (drawn != null)
            {
                p.Draw(drawn);
                Debug.Log($"[P{_turnIndex}] 自摸: {drawn}");
            }

            // 手牌0枚なら捨てられない
            if (p.Hand.Count == 0)
            {
                Debug.LogWarning($"[P{_turnIndex}] 扱える手牌がありません");
                return false;
            }

            // 捨てる位置を決める（今は一番右）
            int discardIdx = p.ChooseDiscardIndex();
            if (discardIdx < 0 || discardIdx >= p.Hand.Count)
            {
                discardIdx = p.Hand.Count - 1;
            }

            var discard = p.Hand[discardIdx];
            p.Hand.RemoveAt(discardIdx);

            // 直前の捨て牌を記録
            LastDiscard = discard;

            Debug.Log($"[P{_turnIndex}] 打牌: {discard}");

            // 次のプレイヤーへ
            _turnIndex = (_turnIndex + 1) % 4;

            // 山が残っていれば true
            return _wall.Count > 0;
        }

        public Player GetPlayer(int index) => _players[index];
    }
}
