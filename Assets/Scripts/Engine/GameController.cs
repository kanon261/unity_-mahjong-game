using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mahjong.Core.Analysis;
using Mahjong.Engine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI handText;      // 手牌表示用（デバッグ）
    public TextMeshProUGUI shantenText;   // シャンテン数表示用
    public TextMeshProUGUI turnText;      // ターン表示用
    public TextMeshProUGUI remainingText; // 残り山牌表示用
    public Transform tileButtonContainer; // 牌ボタンの親オブジェクト
    public GameObject tileButtonPrefab;   // 牌ボタンのプレハブ
    public Button tsumoButton;            // ツモボタン
    public GameObject winPanel;           // アガリ表示パネル
    public TextMeshProUGUI winText;       // アガリ詳細テキスト

    [Header("捨て牌表示")]
    public Transform discardContainer;    // 自分の捨て牌コンテナ
    public GameObject discardTilePrefab;  // 捨て牌用プレハブ（クリック不可）

    [Header("ツモ牌分離")]
    public float tsumoTileGap = 15f;      // ツモ牌と手牌の間隔

    [Header("Tile Sprites")]
    public Sprite[] tileSprites; // 牌画像（順番は自由）

    [Header("背景")]
    public Camera mainCamera;      // メインカメラ
    public Image backgroundPanel;  // 背景パネル（オプション）

    private TurnManager _turn;
    private bool _waitingForPlayerInput = false;
    private bool _canTsumo = false;  // ツモアガリ可能フラグ
    private List<GameObject> _tileButtons = new List<GameObject>();
    private List<GameObject> _discardTiles = new List<GameObject>(); // 捨て牌オブジェクト
    private Dictionary<Mahjong.Core.TileId, Sprite> _tileSpriteMap;

    private void Start()
    {
        // 背景色を雀魂風に設定
        SetMahjongSoulBackground();

        // Sprite名からTileIdへのマッピングを作成
        BuildSpriteMap();

        // ツモボタンのイベント設定
        if (tsumoButton != null)
        {
            tsumoButton.onClick.AddListener(OnTsumoClicked);
            tsumoButton.gameObject.SetActive(false);
        }

        // アガリパネルを非表示
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        _turn = new TurnManager();
        _turn.StartNewHand();

        // CheckTurn内でUpdateUIが呼ばれるので、ここでは呼ばない
        CheckTurn();
    }

    private void CheckTurn()
    {
        if (_turn.IsGameOver())
        {
            Debug.Log("=== 局終了（山が尽きた）===");
            ShowDrawGame();
            return;
        }

        if (_turn.CurrentPlayerIndex == 0)
        {
            // プレイヤー0のターン - 入力待ち
            _waitingForPlayerInput = true;

            // プレイヤー0がまだツモしていない場合（13枚）はツモを引く
            var player0 = _turn.GetPlayer(0);
            if (player0.Hand.Count == 13)
            {
                _turn.DrawForCurrentPlayer();
            }

            Debug.Log("=== プレイヤーのターン：打牌を選択してください ===");

            // ツモアガリ判定（14枚の状態でシャンテン数が-1ならアガリ）
            if (player0.Hand.Count == 14)
            {
                int shanten = HandAnalyzer.CalculateShanten(player0.Hand);
                _canTsumo = (shanten == -1);
                if (_canTsumo)
                {
                    Debug.Log("=== ツモアガリ可能！ ===");
                }
            }
            else
            {
                _canTsumo = false;
            }

            UpdateUI(); // ボタンの状態を更新
        }
        else
        {
            // CPU のターン - 自動で進める
            _waitingForPlayerInput = false;
            _canTsumo = false;
            StartCoroutine(CPUTurn());
        }
    }

    private IEnumerator CPUTurn()
    {
        yield return new WaitForSeconds(0.5f);
        _turn.StepOnce();
        UpdateUI();
        CheckTurn();
    }

    private void UpdateUI()
    {
        var player0 = _turn.GetPlayer(0);

        // 手牌を表示（デバッグ用テキスト）
        if (handText != null)
        {
            var handStr = string.Join(" ", player0.Hand.Select(t => FormatTile(t.Id)));
            handText.text = $"手牌: {handStr}";
        }

        // 手牌ボタンを更新
        UpdateTileButtons();

        // 捨て牌を更新
        UpdateDiscards();

        // シャンテン数を非表示（不要なので）
        if (shantenText != null)
        {
            shantenText.gameObject.SetActive(false);
        }

        // ターン表示
        if (turnText != null)
        {
            if (_turn.CurrentPlayerIndex == 0)
            {
                turnText.text = "あなたのターン";
            }
            else
            {
                turnText.text = $"CPU {_turn.CurrentPlayerIndex} のターン";
            }
        }

        // ツモボタンの表示制御
        if (tsumoButton != null)
        {
            tsumoButton.gameObject.SetActive(_canTsumo);
        }

        // 残り山牌を表示
        if (remainingText != null)
        {
            remainingText.text = $"残り: {_turn.RemainingTiles}枚";
        }
    }

    private void UpdateTileButtons()
    {
        // 既存のボタンをすべて削除
        foreach (var btn in _tileButtons)
        {
            Destroy(btn);
        }
        _tileButtons.Clear();

        if (tileButtonContainer == null || tileButtonPrefab == null)
        {
            Debug.LogWarning($"Container or Prefab is null! Container: {tileButtonContainer}, Prefab: {tileButtonPrefab}");
            return;
        }

        var player0 = _turn.GetPlayer(0);
        int handCount = player0.Hand.Count;
        bool hasTsumoTile = (handCount == 14); // 14枚ならツモ牌あり

        // デバッグ: 手牌の枚数と内容を表示
        Debug.Log($"=== 手牌更新 === 枚数: {handCount}, ツモ牌あり: {hasTsumoTile}");
        var tileNames = string.Join(", ", player0.Hand.Select(t => t.Id.ToString()));
        Debug.Log($"手牌内容: {tileNames}");

        // Content Size Fitterを無効化（あれば）
        var sizeFitter = tileButtonContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        // コンテナの幅を必要なサイズに調整
        var containerRect = tileButtonContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            // 牌の幅(30) × 枚数 + 間隔(2) × (枚数-1) + ツモ牌の間隔(15)
            float tileWidth = 30f;
            float spacing = 2f;
            float requiredWidth = (tileWidth * handCount) + (spacing * (handCount - 1));
            if (hasTsumoTile)
            {
                requiredWidth += tsumoTileGap;
            }
            containerRect.sizeDelta = new Vector2(requiredWidth, containerRect.sizeDelta.y);
            Debug.Log($"コンテナ幅を調整: {requiredWidth}px (枚数: {handCount})");
        }

        // 各手牌に対してボタンを生成
        for (int i = 0; i < handCount; i++)
        {
            var tile = player0.Hand[i];
            var btnObj = Instantiate(tileButtonPrefab, tileButtonContainer);
            _tileButtons.Add(btnObj);

            var tileBtn = btnObj.GetComponent<TileButton>();
            if (tileBtn != null)
            {
                // TileId から Sprite を取得
                Sprite tileSprite = GetTileSprite(tile.Id);
                tileBtn.Initialize(i, tileSprite, this);
            }

            // 13枚目（ツモ牌の直前）の場合、右側にスペーサーを追加
            if (hasTsumoTile && i == handCount - 2)
            {
                // スペーサーオブジェクトを作成
                var spacer = new GameObject("TsumoSpacer");
                spacer.transform.SetParent(tileButtonContainer);
                spacer.transform.localScale = Vector3.one;
                var spacerRect = spacer.AddComponent<RectTransform>();
                spacerRect.sizeDelta = new Vector2(tsumoTileGap, 1);
                var spacerLayout = spacer.AddComponent<LayoutElement>();
                spacerLayout.minWidth = tsumoTileGap;
                spacerLayout.preferredWidth = tsumoTileGap;
                _tileButtons.Add(spacer); // 削除時に一緒に消えるように
            }

            // プレイヤーのターンでないときはボタンを無効化
            var button = btnObj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = _waitingForPlayerInput;
            }
        }

        // ベスト打牌をハイライト（14枚のときのみ）
        if (hasTsumoTile && _waitingForPlayerInput)
        {
            HighlightBestDiscard(player0.Hand);
        }
    }

    /// <summary>
    /// ベストな打牌をハイライト表示
    /// </summary>
    private void HighlightBestDiscard(List<Mahjong.Core.Tile> hand)
    {
        int bestIndex = HandAnalyzer.GetBestDiscardIndex(hand);

        if (bestIndex < 0) return;

        Debug.Log($"=== AI推奨打牌: {bestIndex}番目の牌 ({hand[bestIndex].Id}) ===");

        // _tileButtonsにはスペーサーも含まれるので、実際の牌ボタンだけを処理
        int buttonIndex = 0;
        foreach (var btnObj in _tileButtons)
        {
            var tileBtn = btnObj.GetComponent<TileButton>();
            if (tileBtn != null)
            {
                // このボタンがベスト打牌かどうかでハイライト
                tileBtn.SetHighlight(buttonIndex == bestIndex);
                buttonIndex++;
            }
        }
    }

    /// <summary>
    /// 捨て牌（河）を更新
    /// </summary>
    private void UpdateDiscards()
    {
        if (discardContainer == null) return;

        // 使用するプレハブ（専用プレハブがなければtileButtonPrefabを使用）
        var prefab = discardTilePrefab != null ? discardTilePrefab : tileButtonPrefab;
        if (prefab == null) return;

        // 既存の捨て牌を削除
        foreach (var obj in _discardTiles)
        {
            Destroy(obj);
        }
        _discardTiles.Clear();

        // プレイヤー0の捨て牌を表示
        var player0 = _turn.GetPlayer(0);
        foreach (var tile in player0.Discards)
        {
            var tileObj = Instantiate(prefab, discardContainer);
            _discardTiles.Add(tileObj);

            // 画像を設定
            var tileBtn = tileObj.GetComponent<TileButton>();
            if (tileBtn != null)
            {
                Sprite tileSprite = GetTileSprite(tile.Id);
                tileBtn.Initialize(-1, tileSprite, this); // -1でクリック無効
            }
            else
            {
                // TileButtonがない場合は直接Imageを設定
                var image = tileObj.GetComponentInChildren<Image>();
                if (image != null)
                {
                    image.sprite = GetTileSprite(tile.Id);
                }
            }

            // ボタンを無効化（捨て牌はクリックできない）
            var button = tileObj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    /// <summary>
    /// プレイヤーが牌を選択して打牌
    /// </summary>
    public void OnTileClicked(int tileIndex)
    {
        if (!_waitingForPlayerInput)
        {
            Debug.Log("プレイヤーのターンではありません");
            return;
        }

        // 打牌を実行
        _turn.PlayerDiscard(tileIndex);
        _waitingForPlayerInput = false;
        _canTsumo = false;

        // UIを更新（ボタンも再生成される）
        UpdateUI();

        // 次のターンをチェック
        CheckTurn();
    }

    /// <summary>
    /// ツモボタンがクリックされた
    /// </summary>
    public void OnTsumoClicked()
    {
        if (!_canTsumo)
        {
            Debug.Log("ツモアガリできません");
            return;
        }

        Debug.Log("=== ツモアガリ！ ===");
        ShowWin();
    }

    /// <summary>
    /// アガリ画面を表示
    /// </summary>
    private void ShowWin()
    {
        _waitingForPlayerInput = false;
        _canTsumo = false;

        // ツモボタンを非表示
        if (tsumoButton != null)
        {
            tsumoButton.gameObject.SetActive(false);
        }

        // 手牌ボタンを無効化
        foreach (var btn in _tileButtons)
        {
            var button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }

        // アガリパネルを表示
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        // アガリテキストを設定
        if (winText != null)
        {
            var player0 = _turn.GetPlayer(0);
            var handStr = string.Join(" ", player0.Hand.Select(t => FormatTile(t.Id)));
            winText.text = $"ツモ！\n\n手牌: {handStr}\n\nおめでとうございます！";
        }

        // ターン表示を更新
        if (turnText != null)
        {
            turnText.text = "アガリ！";
        }

        if (shantenText != null)
        {
            shantenText.text = "ツモアガリ";
        }
    }

    /// <summary>
    /// 流局画面を表示
    /// </summary>
    private void ShowDrawGame()
    {
        _waitingForPlayerInput = false;
        _canTsumo = false;

        // ツモボタンを非表示
        if (tsumoButton != null)
        {
            tsumoButton.gameObject.SetActive(false);
        }

        // 手牌ボタンを無効化
        foreach (var btn in _tileButtons)
        {
            var button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }

        // アガリパネルを流局表示として使用
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winText != null)
        {
            winText.text = "流局\n\n牌がなくなりました";
        }

        if (turnText != null)
        {
            turnText.text = "流局";
        }
    }

    /// <summary>
    /// Sprite名とTileIdのマッピングを構築
    /// </summary>
    private void BuildSpriteMap()
    {
        _tileSpriteMap = new Dictionary<Mahjong.Core.TileId, Sprite>();

        // Sprite名からTileIdへのマッピング辞書
        var nameToTileId = new Dictionary<string, Mahjong.Core.TileId>
        {
            // 萬子
            { "eman", Mahjong.Core.TileId.Man1 },
            { "ryanman", Mahjong.Core.TileId.Man2 },
            { "sanman", Mahjong.Core.TileId.Man3 },
            { "suman", Mahjong.Core.TileId.Man4 },
            { "uman", Mahjong.Core.TileId.Man5 },
            { "roman", Mahjong.Core.TileId.Man6 },
            { "timan", Mahjong.Core.TileId.Man7 },
            { "paman", Mahjong.Core.TileId.Man8 },
            { "kyuman", Mahjong.Core.TileId.Man9 },
            // 赤萬子（通常の萬子として扱う）
            { "red_uman", Mahjong.Core.TileId.Man5 },
            // 筒子
            { "ipin", Mahjong.Core.TileId.Pin1 },
            { "rynpin", Mahjong.Core.TileId.Pin2 },
            { "sanpin", Mahjong.Core.TileId.Pin3 },
            { "supin", Mahjong.Core.TileId.Pin4 },
            { "upin", Mahjong.Core.TileId.Pin5 },
            { "ropin", Mahjong.Core.TileId.Pin6 },
            { "tipin", Mahjong.Core.TileId.Pin7 },
            { "papin", Mahjong.Core.TileId.Pin8 },
            { "tyupin", Mahjong.Core.TileId.Pin9 },
            // 赤筒子
            { "red_upin", Mahjong.Core.TileId.Pin5 },
            // 索子
            { "eso", Mahjong.Core.TileId.Sou1 },
            { "rynso", Mahjong.Core.TileId.Sou2 },
            { "sanso", Mahjong.Core.TileId.Sou3 },
            { "suso", Mahjong.Core.TileId.Sou4 },
            { "uso", Mahjong.Core.TileId.Sou5 },
            { "roso", Mahjong.Core.TileId.Sou6 },
            { "tiso", Mahjong.Core.TileId.Sou7 },
            { "paso", Mahjong.Core.TileId.Sou8 },
            { "tyuso", Mahjong.Core.TileId.Sou9 },
            // 赤索子
            { "red_uso", Mahjong.Core.TileId.Sou5 },
            // 字牌
            { "Ton", Mahjong.Core.TileId.East },
            { "Nan", Mahjong.Core.TileId.South },
            { "Sya", Mahjong.Core.TileId.West },
            { "Pei", Mahjong.Core.TileId.North },
            { "Haku", Mahjong.Core.TileId.White },
            { "Hatu", Mahjong.Core.TileId.Green },
            { "Tyun", Mahjong.Core.TileId.Red },
        };

        // 各Spriteを名前でマッピング
        if (tileSprites != null)
        {
            foreach (var sprite in tileSprites)
            {
                if (sprite == null) continue;

                string spriteName = sprite.name;
                if (nameToTileId.TryGetValue(spriteName, out var tileId))
                {
                    // 既に登録されていなければ登録（赤牌より通常牌を優先）
                    if (!_tileSpriteMap.ContainsKey(tileId))
                    {
                        _tileSpriteMap[tileId] = sprite;
                    }
                }
            }
        }

        Debug.Log($"Sprite mapping complete: {_tileSpriteMap.Count} tiles mapped");
    }

    /// <summary>
    /// TileId から対応する Sprite を取得
    /// </summary>
    private Sprite GetTileSprite(Mahjong.Core.TileId tileId)
    {
        if (_tileSpriteMap != null && _tileSpriteMap.TryGetValue(tileId, out var sprite))
        {
            return sprite;
        }
        Debug.LogWarning($"Sprite not found for TileId: {tileId}");
        return null;
    }

    private string FormatTile(Mahjong.Core.TileId tileId)
    {
        // 牌を読みやすい形式に変換（例：Man1 → 1m）
        string str = tileId.ToString();
        if (str.StartsWith("Man")) return str.Substring(3) + "m";
        if (str.StartsWith("Pin")) return str.Substring(3) + "p";
        if (str.StartsWith("Sou")) return str.Substring(3) + "s";
        if (str == "East") return "東";
        if (str == "South") return "南";
        if (str == "West") return "西";
        if (str == "North") return "北";
        if (str == "White") return "白";
        if (str == "Green") return "發";
        if (str == "Red") return "中";
        return str;
    }

    /// <summary>
    /// 背景色を雀魂風のダークグリーンに設定
    /// </summary>
    private void SetMahjongSoulBackground()
    {
        // 雀魂風ダークグリーン（卓の色）
        Color mahjongSoulGreen = new Color(0.102f, 0.227f, 0.165f, 1f); // #1A3A2A

        // カメラの背景色を設定
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = mahjongSoulGreen;
        }
        else
        {
            // mainCameraが設定されていない場合はCamera.mainを使用
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = mahjongSoulGreen;
            }
        }

        // 背景パネルがあれば色を設定
        if (backgroundPanel != null)
        {
            backgroundPanel.color = mahjongSoulGreen;
        }

        Debug.Log("背景色を雀魂風に設定しました");
    }
}
