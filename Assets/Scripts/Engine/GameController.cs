using System.Linq;
using Mahjong.Engine;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // 手牌表示用
    public TextMeshProUGUI handText;

    // 捨て牌表示用
    public TextMeshProUGUI discardText;

    private TurnManager _turn;
    private bool _finished;   // 局が終わったかどうか

    private void Start()
    {
        _turn = new TurnManager();
        _turn.StartNewHand();

        UpdateHandView();
        UpdateDiscardView();
    }

    // ★ ボタンから呼ぶ用
    public void OnStepButtonClicked()
    {
        if (_finished) return;  // もう終わってたら何もしない

        bool hasNext = _turn.StepOnce();

        UpdateHandView();
        UpdateDiscardView();

        if (!hasNext)
        {
            _finished = true;
            Debug.Log("=== 局終了（山が尽きた）===");
        }
    }

    private void UpdateHandView()
    {
        var p0 = _turn.GetPlayer(0);
        var text = string.Join("  ", p0.Hand.Select(t => t.ToString()));

        if (handText != null)
            handText.text = text;
    }

    private void UpdateDiscardView()
    {
        var d = _turn.LastDiscard;
        if (discardText != null)
            discardText.text = d != null ? $"捨て牌: {d}" : "捨て牌: なし";
    }
}