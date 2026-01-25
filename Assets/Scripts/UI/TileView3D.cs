// Assets/Scripts/UI/TileView3D.cs みたいな場所
using System;
using UnityEngine;

namespace Mahjong.UI
{
    public class TileView3D : MonoBehaviour
    {
        private int _index;
        private Action<int> _onClicked;

        public void Setup(int index, Action<int> onClicked)
        {
            _index = index;
            _onClicked = onClicked;
        }

        // マウスクリックで呼ばれる（スマホならあとでタップ対応に変える）
        private void OnMouseDown()
        {
            _onClicked?.Invoke(_index);
        }
    }
}
