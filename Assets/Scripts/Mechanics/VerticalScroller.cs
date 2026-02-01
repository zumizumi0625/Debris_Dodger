using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// カメラを上方向にスクロールさせるコンポーネント。
    /// プレイヤーは画面内で相対的に下に流されていくように見える。
    /// </summary>
    public class VerticalScroller : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [Tooltip("スクロール速度")]
        public float scrollSpeed = 1.0f;
        
        [Tooltip("時間経過で速度を上げるか")]
        public bool accelerateOverTime = true;
        
        [Tooltip("最大スクロール速度")]
        public float maxScrollSpeed = 3.0f;
        
        [Tooltip("加速率（秒あたり）")]
        public float accelerationRate = 0.05f;
        
        [Header("Target")]
        [Tooltip("スクロールさせるカメラ（未設定の場合はメインカメラ）")]
        public Camera targetCamera;
        
        [Tooltip("プレイヤーも一緒に動かすか")]
        public bool movePlayerWithCamera = false;
        
        [Tooltip("プレイヤー参照")]
        public Transform player;

        [Header("Target Offset")]
        [Tooltip("ターゲット（プレイヤー）とのY軸オフセットを固定維持するか")]
        public bool maintainFixedOffset = false;
        
        [Header("Bounds")]
        [Tooltip("スクロールを停止するY座標（0で無限）")]
        public float maxY = 0f;
        
        [Header("Control")]
        [Tooltip("スクロールを有効にするか")]
        public bool scrollEnabled = true;

        [Header("Debug Info")]
        [Tooltip("現在のスクロール速度")]
        [SerializeField] private float currentSpeed;
        [Tooltip("総移動距離")]
        [SerializeField] private float totalDistance;
        
        // 開始時のオフセット（カメラY - プレイヤーY）
        private float initialOffsetY;
        
        // === プロパティ ===
        public float CurrentSpeed => currentSpeed;
        public float TotalDistance => totalDistance;
        
        void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            
            if (player == null)
            {
                var satelliteController = FindObjectOfType<SatelliteController>();
                if (satelliteController != null)
                {
                    player = satelliteController.transform;
                }
            }
            
            // オフセットを計算（カメラが上、プレイヤーが下なら正の値）
            if (player != null && targetCamera != null)
            {
                initialOffsetY = targetCamera.transform.position.y - player.position.y;
            }
            
            currentSpeed = scrollSpeed;
        }
        
        void LateUpdate()
        {
            if (!scrollEnabled) return;
            if (targetCamera == null) return;
            
            // 加速
            if (accelerateOverTime)
            {
                currentSpeed = Mathf.Min(currentSpeed + accelerationRate * Time.deltaTime, maxScrollSpeed);
            }
            
            // 上限チェック
            if (maxY > 0 && targetCamera.transform.position.y >= maxY)
            {
                return;
            }
            
            // カメラをスクロール
            float deltaY = currentSpeed * Time.deltaTime;
            Vector3 cameraPos = targetCamera.transform.position;
            cameraPos.y += deltaY;
            targetCamera.transform.position = cameraPos;
            
            totalDistance += deltaY;
            
            // プレイヤーも一緒に動かす場合
            if (movePlayerWithCamera && player != null)
            {
                if (maintainFixedOffset)
                {
                    // 強制的にオフセット位置に固定（Xはそのまま、Yはカメラ追従）
                    // これで「画面内の定位置」をキープします
                    Vector3 playerPos = player.position;
                    playerPos.y = targetCamera.transform.position.y - initialOffsetY;
                    player.position = playerPos;
                }
                else
                {
                    // デルタ加算（プレイヤー自身の上下移動も許容）
                    Vector3 playerPos = player.position;
                    playerPos.y += deltaY;
                    player.position = playerPos;
                }
            }
        }
        
        /// <summary>
        /// スクロールを一時停止
        /// </summary>
        public void Pause()
        {
            scrollEnabled = false;
        }
        
        /// <summary>
        /// スクロールを再開
        /// </summary>
        public void Resume()
        {
            scrollEnabled = true;
        }
        
        /// <summary>
        /// スクロールをリセット
        /// </summary>
        public void Reset()
        {
            currentSpeed = scrollSpeed;
            totalDistance = 0f;
        }
        
        /// <summary>
        /// 速度を設定
        /// </summary>
        public void SetSpeed(float speed)
        {
            currentSpeed = speed;
        }
    }
}
