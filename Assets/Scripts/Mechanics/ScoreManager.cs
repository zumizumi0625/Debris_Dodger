using UnityEngine;
using Platformer.Mechanics;

namespace Platformer.Mechanics
{
    /// <summary>
    /// ゲームのスコアを管理するクラス。
    /// 上方向への移動距離をスコアとして計算する。
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("高さ（Y座標）を計測する対象。通常はPlayerまたはCamera")]
        public Transform targetTransform;
        
        [Header("Score Settings")]
        [Tooltip("距離1メートルあたりのスコア")]
        public float scoreMultiplier = 10f;
        
        [Tooltip("スコア計測の開始Y座標")]
        public float startY = 0f;
        
        // 現在のスコア（整数）
        public int Score => Mathf.FloorToInt(distanceTraveled * scoreMultiplier);
        
        // 内部変数
        private float distanceTraveled;
        
        void Start()
        {
            if (targetTransform == null)
            {
                // デフォルトでカメラまたはプレイヤーを探す
                if (Camera.main != null) 
                    targetTransform = Camera.main.transform;
                else
                    targetTransform = FindObjectOfType<SatelliteController>()?.transform;
            }
            
            if (targetTransform != null)
            {
                startY = targetTransform.position.y;
            }
        }

        void Update()
        {
            if (targetTransform != null)
            {
                // 現在のY座標と開始位置の差分をスコアとする
                float currentY = targetTransform.position.y;
                distanceTraveled = Mathf.Max(0f, currentY - startY);
            }
        }
    }
}
