using System.Collections;
using Mahjong.Engine;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private TurnManager _turn;

    private void Start()
    {
        _turn = new TurnManager();
        _turn.StartNewHand();

        // 0.5秒ごとに1手ずつ進める
        StartCoroutine(AutoPlay());
    }

    private IEnumerator AutoPlay()
    {
        while (_turn.StepOnce())
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("=== 局終了（山が尽きた）===");
    }
}
