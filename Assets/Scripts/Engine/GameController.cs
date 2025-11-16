using System.Collections;
using System.Linq;
using Mahjong.Engine;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private TurnManager _turn;

    public TextMeshProUGUI handText;

    private void Start()
    {
        _turn = new TurnManager();
        _turn.StartNewHand();

        // 局開始時点の手牌を表示
        UpdateHandView();

        // 自動再生
        StartCoroutine(AutoPlay());
    }

    private IEnumerator AutoPlay()
    {
        while (_turn.StepOnce())
        {
            UpdateHandView();

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("=== 局終了（山が尽きた）===");
    }
    
    private void UpdateHandView()
    {
        var p0 = _turn.GetPlayer(0);

        // Tile.ToString() の結果をスペース区切りで並べる
        var text = string.Join("  ", p0.Hand.Select(t => t.ToString()));

        if (handText != null)
        {
            handText.text = text;
        }
    }
}