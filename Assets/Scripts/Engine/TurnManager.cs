using Mahjong.Core;
using Mahjong.Core.Analysis;
using UnityEngine;

namespace Mahjong.Engine
{
    public sealed class TurnManager
    {
        private readonly Player[] _players = new Player[4];
        private Wall _wall;
        private int _turnIndex; // 0..3

        public int CurrentPlayerIndex => _turnIndex;

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

            // 配牌：4人に13枚ずつ（ソートあり）
            for (int r = 0; r < 13; r++)
            {
                for (int p = 0; p < 4; p++)
                {
                    _players[p].DrawInitial(_wall.Draw());
                }
            }

            // 東家だけ14枚目ツモ（これはツモなのでソートしない）
            _players[0].Draw(_wall.Draw());

            Debug.Log($"=== 新しい局を開始 === 残り山牌: {_wall.Count}枚, 東家手牌: {_players[0].Hand.Count}枚");
        }

        /// <summary>
        /// 山牌の残り枚数
        /// </summary>
        public int RemainingTiles => _wall?.Count ?? 0;

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

            var discard = p.Discard(discardIdx);

            Debug.Log($"[P{_turnIndex}] 打牌: {discard}");

            // プレイヤー0のシャンテン数を表示
            if (_turnIndex == 0)
            {
                int shanten = HandAnalyzer.CalculateShanten(p.Hand);
                string shantenStr = shanten == 0 ? "聴牌" :
                                   shanten == -1 ? "アガリ" :
                                   $"{shanten}向聴";
                Debug.Log($"[P0] シャンテン数: {shantenStr}");
            }

            // 次のプレイヤーへ
            _turnIndex = (_turnIndex + 1) % 4;

            // 山が残っていれば true
            return _wall.Count > 0;
        }

        public Player GetPlayer(int index) => _players[index];

        public bool IsGameOver()
        {
            return _wall == null || _wall.Count == 0;
        }

        /// <summary>
        /// プレイヤーが手動で打牌する
        /// </summary>
        public void PlayerDiscard(int tileIndex)
        {
            if (_turnIndex != 0)
            {
                Debug.LogWarning("プレイヤー以外のターンです");
                return;
            }

            var p = _players[0];
            if (tileIndex < 0 || tileIndex >= p.Hand.Count)
            {
                Debug.LogWarning($"無効な牌インデックス: {tileIndex}");
                return;
            }

            var discard = p.Discard(tileIndex);

            Debug.Log($"[P0] 打牌: {discard}");

            // シャンテン数を表示
            int shanten = HandAnalyzer.CalculateShanten(p.Hand);
            string shantenStr = shanten == 0 ? "聴牌" :
                               shanten == -1 ? "アガリ" :
                               $"{shanten}向聴";
            Debug.Log($"[P0] シャンテン数: {shantenStr}");

            // 次のプレイヤーへ
            _turnIndex = (_turnIndex + 1) % 4;
            // 注意: 次のプレイヤーのツモはStepOnce()またはDrawForCurrentPlayer()で行う
        }

        /// <summary>
        /// 現在のプレイヤー（プレイヤー0）がツモを引く
        /// </summary>
        public Tile DrawForCurrentPlayer()
        {
            if (_wall == null || _wall.Count == 0) return null;

            var drawn = _wall.Draw();
            if (drawn != null)
            {
                _players[_turnIndex].Draw(drawn);
                Debug.Log($"[P{_turnIndex}] 自摸: {drawn}");
            }
            return drawn;
        }
    }
}
