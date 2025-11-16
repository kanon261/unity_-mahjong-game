using Mahjong.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Mahjong.UI
{
    public class TileView : MonoBehaviour
    {
        public Image tileImage;  // ボタンの見た目
        private int _indexInHand;
        private System.Action<int> _onClicked;

        // GameController から呼ばれる初期化
        public void Setup(int indexInHand, System.Action<int> onClicked)
        {
            _indexInHand = indexInHand;
            _onClicked = onClicked;
        }

        // Button の OnClick から呼ばせる用
        public void OnClick()
        {
            _onClicked?.Invoke(_indexInHand);
        }
    }
}
