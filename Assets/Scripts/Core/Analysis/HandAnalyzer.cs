using System.Collections.Generic;
using Mahjong.Core;

namespace Mahjong.Core.Analysis
{
    /// <summary>
    /// 手牌の解析（シャンテン数、受け入れ枚数）
    /// </summary>
    public static class HandAnalyzer
    {
        /// <summary>
        /// シャンテン数を計算（通常手のみ、簡易版）
        /// </summary>
        public static int CalculateShanten(List<Tile> hand)
        {
            var counts = ConvertToCounts(hand);
            return CalculateRegularShanten(counts);
        }

        /// <summary>
        /// 通常手（4面子1雀頭）のシャンテン数
        /// </summary>
        private static int CalculateRegularShanten(int[] counts)
        {
            int minShanten = 8;

            // 雀頭なしの場合（テンパイ以前の状態を評価）
            int noHeadShanten = CalculateShantenWithoutHead(counts, 0, 0, 0) + 1;
            minShanten = System.Math.Min(minShanten, noHeadShanten);

            // 雀頭候補を試す
            for (int headIndex = 0; headIndex < 34; headIndex++)
            {
                if (counts[headIndex] >= 2)
                {
                    counts[headIndex] -= 2;
                    int shanten = CalculateShantenWithoutHead(counts, 0, 0, 0);
                    counts[headIndex] += 2;

                    if (shanten < minShanten)
                    {
                        minShanten = shanten;
                    }
                }
            }

            return minShanten;
        }

        /// <summary>
        /// 雀頭確定後のシャンテン数計算
        /// </summary>
        private static int CalculateShantenWithoutHead(int[] counts, int index, int mentsu, int tatsu)
        {
            if (mentsu == 4) return -1;
            if (index >= 34) return 8 - mentsu * 2 - tatsu;
            if (counts[index] == 0) return CalculateShantenWithoutHead(counts, index + 1, mentsu, tatsu);

            int minShanten = 8;

            // 刻子を試す
            if (counts[index] >= 3)
            {
                counts[index] -= 3;
                int s = CalculateShantenWithoutHead(counts, index, mentsu + 1, tatsu);
                minShanten = System.Math.Min(minShanten, s);
                counts[index] += 3;
            }

            // 対子を試す
            if (counts[index] >= 2 && mentsu + tatsu < 4)
            {
                counts[index] -= 2;
                int s = CalculateShantenWithoutHead(counts, index, mentsu, tatsu + 1);
                minShanten = System.Math.Min(minShanten, s);
                counts[index] += 2;
            }

            // 順子を試す（数牌のみ）
            if (index < 27)
            {
                int num = index % 9;
                if (num <= 6 && counts[index] >= 1 && counts[index + 1] >= 1 && counts[index + 2] >= 1)
                {
                    counts[index]--;
                    counts[index + 1]--;
                    counts[index + 2]--;
                    int s = CalculateShantenWithoutHead(counts, index, mentsu + 1, tatsu);
                    minShanten = System.Math.Min(minShanten, s);
                    counts[index]++;
                    counts[index + 1]++;
                    counts[index + 2]++;
                }

                // 塔子を試す（両面・嵌張）
                if (mentsu + tatsu < 4)
                {
                    if (num <= 7 && counts[index] >= 1 && counts[index + 1] >= 1)
                    {
                        counts[index]--;
                        counts[index + 1]--;
                        int s = CalculateShantenWithoutHead(counts, index, mentsu, tatsu + 1);
                        minShanten = System.Math.Min(minShanten, s);
                        counts[index]++;
                        counts[index + 1]++;
                    }

                    if (num <= 6 && counts[index] >= 1 && counts[index + 2] >= 1)
                    {
                        counts[index]--;
                        counts[index + 2]--;
                        int s = CalculateShantenWithoutHead(counts, index, mentsu, tatsu + 1);
                        minShanten = System.Math.Min(minShanten, s);
                        counts[index]++;
                        counts[index + 2]++;
                    }
                }
            }

            // 何も取らずに次へ
            int skipShanten = CalculateShantenWithoutHead(counts, index + 1, mentsu, tatsu);
            minShanten = System.Math.Min(minShanten, skipShanten);

            return minShanten;
        }

        /// <summary>
        /// 手牌を牌種ごとのカウント配列に変換
        /// </summary>
        private static int[] ConvertToCounts(List<Tile> hand)
        {
            int[] counts = new int[34];
            foreach (var tile in hand)
            {
                int index = (int)tile.Id;
                counts[index]++;
            }
            return counts;
        }

        /// <summary>
        /// 指定した牌を切った場合の受け入れ枚数を計算
        /// </summary>
        public static int CalculateAcceptance(List<Tile> hand, int discardIndex)
        {
            if (discardIndex < 0 || discardIndex >= hand.Count) return 0;

            // 牌を切った後の手牌を作成
            var tempHand = new List<Tile>(hand);
            tempHand.RemoveAt(discardIndex);

            var counts = ConvertToCounts(tempHand);
            int currentShanten = CalculateRegularShanten(counts);

            // シャンテン数が下がる牌の枚数をカウント
            int acceptance = 0;
            for (int tileId = 0; tileId < 34; tileId++)
            {
                // 既に4枚使われている牌は受け入れにならない
                if (counts[tileId] >= 4) continue;

                // この牌を引いた場合のシャンテン数を計算
                counts[tileId]++;
                int newShanten = CalculateRegularShanten(counts);
                counts[tileId]--;

                // シャンテン数が下がれば受け入れ
                if (newShanten < currentShanten)
                {
                    // 残り枚数（4 - 手牌での使用枚数）
                    int remaining = 4 - CountTilesInHand(tempHand, (TileId)tileId);
                    acceptance += remaining;
                }
            }

            return acceptance;
        }

        /// <summary>
        /// 手牌内の特定TileIdの枚数をカウント
        /// </summary>
        private static int CountTilesInHand(List<Tile> hand, TileId tileId)
        {
            int count = 0;
            foreach (var tile in hand)
            {
                if (tile.Id == tileId) count++;
            }
            return count;
        }

        /// <summary>
        /// ベストな打牌のインデックスを取得（受け入れ枚数が最大になる牌）
        /// </summary>
        public static int GetBestDiscardIndex(List<Tile> hand)
        {
            if (hand.Count != 14) return -1; // 14枚でないと判定不可

            int bestIndex = -1;
            int bestAcceptance = -1;
            int bestShanten = int.MaxValue;

            for (int i = 0; i < hand.Count; i++)
            {
                // この牌を切った場合のシャンテン数を計算
                var tempHand = new List<Tile>(hand);
                tempHand.RemoveAt(i);
                int shanten = CalculateShanten(tempHand);

                // シャンテン数が低い方を優先、同じなら受け入れ枚数が多い方
                int acceptance = CalculateAcceptance(hand, i);

                if (shanten < bestShanten ||
                    (shanten == bestShanten && acceptance > bestAcceptance))
                {
                    bestShanten = shanten;
                    bestAcceptance = acceptance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// 各牌を切った場合の評価を取得（デバッグ/学習用）
        /// </summary>
        public static List<(int index, int shanten, int acceptance)> EvaluateAllDiscards(List<Tile> hand)
        {
            var results = new List<(int, int, int)>();

            for (int i = 0; i < hand.Count; i++)
            {
                var tempHand = new List<Tile>(hand);
                tempHand.RemoveAt(i);
                int shanten = CalculateShanten(tempHand);
                int acceptance = CalculateAcceptance(hand, i);
                results.Add((i, shanten, acceptance));
            }

            return results;
        }
    }
}
