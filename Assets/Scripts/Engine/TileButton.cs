using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mahjong.Engine
{
    /// <summary>
    /// 手牌のボタンUI
    /// </summary>
    public class TileButton : MonoBehaviour
    {
        public Image tileImage; // 画像表示用
        public Image highlightImage; // ハイライト用（オプション）
        public TextMeshProUGUI tileText; // デバッグ用（オプション）
        private int _tileIndex;
        private GameController _controller;

        // ハイライトの色
        private static readonly Color HighlightColor = new Color(1f, 0.9f, 0.2f, 0.7f); // 黄色
        private static readonly Color NormalColor = new Color(1f, 1f, 1f, 0f); // 透明

        public void Initialize(int index, Sprite tileSprite, GameController controller)
        {
            _tileIndex = index;
            _controller = controller;

            // 牌の画像を設定
            if (tileImage != null && tileSprite != null)
            {
                tileImage.sprite = tileSprite;
            }

            // ハイライトをリセット
            SetHighlight(false);

            // ボタンクリックイベントを設定
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
        }

        /// <summary>
        /// ハイライトの表示/非表示を設定
        /// </summary>
        public void SetHighlight(bool isHighlighted)
        {
            // highlightImageがあればそれを使う
            if (highlightImage != null)
            {
                highlightImage.color = isHighlighted ? HighlightColor : NormalColor;
                return;
            }

            // highlightImageがない場合、tileImageの色を変える
            if (tileImage != null)
            {
                if (isHighlighted)
                {
                    tileImage.color = new Color(1f, 1f, 0.6f, 1f); // 少し黄色っぽく
                }
                else
                {
                    tileImage.color = Color.white; // 通常色
                }
            }
        }

        private void OnClick()
        {
            Debug.Log($"TileButton clicked! Index: {_tileIndex}");
            if (_controller != null)
            {
                _controller.OnTileClicked(_tileIndex);
            }
            else
            {
                Debug.LogWarning("Controller is null!");
            }
        }
    }
}
