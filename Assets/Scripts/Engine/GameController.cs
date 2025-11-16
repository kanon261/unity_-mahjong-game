using System.Collections;
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

    private void Start()
    {
        _turn = new TurnManager();
        _turn.StartNewHand();

        // 開始時点の表示
        UpdateHandView();
        UpdateDiscardView();

        // 自動再生開始
        StartCoroutine(AutoPlay());
    }

    private IEnumerator AutoPlay()
    {
        while (_turn.StepOnce())
        {
            // 1手ごとに更新
            UpdateHandView();
            UpdateDiscardView();

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("=== 局終了（山が尽きた）===");
    }

    private void UpdateHandView()
    {
        var p0 = _turn.GetPlayer(0);

        var text = string.Join("  ", p0.Hand.Select(t => t.ToString()));

        if (handText != null)
        {
            handText.text = text;
        }
    }

    private void UpdateDiscardView()
    {
        var d = _turn.LastDiscard;
        if (discardText != null)
        {
            discardText.text = d != null ? $"捨て牌: {d}" : "捨て牌: なし";
        }
    }
}
