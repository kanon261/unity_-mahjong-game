using System.Collections.Generic;
using Mahjong.Core;
using Mahjong.Core.Analysis;
using UnityEngine;

namespace Mahjong.Tests
{
    /// <summary>
    /// デバッグ用：手牌解析のテスト
    /// </summary>
    public class AnalysisDebugger : MonoBehaviour
    {
        [ContextMenu("Test Shanten Calculation")]
        public void TestShantenCalculation()
        {
            Debug.Log("=== シャンテン数計算テスト ===");

            // テスト1: 聴牌
            var hand1 = MakeHand(
                TileId.Man1, TileId.Man2, TileId.Man3,
                TileId.Pin4, TileId.Pin5, TileId.Pin6,
                TileId.Sou7, TileId.Sou8, TileId.Sou9,
                TileId.East, TileId.East,
                TileId.South, TileId.South
            );
            int shanten1 = HandAnalyzer.CalculateShanten(hand1);
            Debug.Log($"聴牌テスト: {shanten1}シャンテン（期待値: 0）");

            // テスト2: 一向聴
            var hand2 = MakeHand(
                TileId.Man1, TileId.Man2, TileId.Man3,
                TileId.Pin4, TileId.Pin5, TileId.Pin6,
                TileId.Sou7, TileId.Sou8,
                TileId.East, TileId.East,
                TileId.South, TileId.South, TileId.West
            );
            int shanten2 = HandAnalyzer.CalculateShanten(hand2);
            Debug.Log($"一向聴テスト: {shanten2}シャンテン（期待値: 1）");
        }

        private List<Tile> MakeHand(params TileId[] ids)
        {
            var hand = new List<Tile>();
            foreach (var id in ids)
            {
                hand.Add(new Tile(id, 0));
            }
            return hand;
        }
    }
}
