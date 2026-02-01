using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// プレイヤーのY軸（縦）の動きに合わせてカメラを追従させるスクリプト。
    /// X軸（横）は固定。
    /// </summary>
    public class CameraFollowY : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("追従対象（プレイヤー）")]
        public Transform target;
        
        [Header("Settings")]
        [Tooltip("カメラとターゲットのY軸オフセット")]
        public float yOffset = 3f;
        
        [Tooltip("追従の滑らかさ (0=即座, 大きいほど遅れる)")]
        public float smoothTime = 0.3f;
        
        [Tooltip("一番下に戻れる限界ライン（これ以上カメラは下がらない）")]
        public float minY = 0f;

        [Tooltip("ターゲットがオフセットより下にいる場合追従しない")]
        public bool oneWayScroll = true;

        // 内部変数
        private Vector3 velocity = Vector3.zero;

        void Start()
        {
            if (target == null)
            {
                var player = FindObjectOfType<SatelliteController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
            
            // 現在のオフセットを初期値として採用する場合のコード（必要ならコメントアウト解除）
            // if (target != null) yOffset = transform.position.y - target.position.y;
        }

        void LateUpdate()
        {
            if (target == null) return;

            // 目標とするカメラ位置
            float targetY = target.position.y + yOffset;
            
            // 一方向スクロール（戻らない）の場合
            if (oneWayScroll)
            {
                // 現在位置より下には行かない
                if (targetY < transform.position.y)
                {
                    targetY = transform.position.y;
                }
            }
            
            // 最低ラインチェック
            if (targetY < minY) targetY = minY;

            // スムーズに移動
            Vector3 newPos = transform.position;
            newPos.y = Mathf.SmoothDamp(transform.position.y, targetY, ref velocity.y, smoothTime);
            transform.position = newPos;
        }
    }
}
