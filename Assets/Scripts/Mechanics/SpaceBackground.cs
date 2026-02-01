using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// 宇宙空間の背景をカメラに追従させるコンポーネント。
    /// パララックス効果（視差効果）もオプションで設定可能。
    /// </summary>
    public class SpaceBackground : MonoBehaviour
    {
        [Header("Follow Settings")]
        [Tooltip("追従するカメラ（未設定の場合はメインカメラ）")]
        public Camera targetCamera;
        
        [Tooltip("追従モード")]
        public FollowMode followMode = FollowMode.FullFollow;
        
        [Header("Parallax Settings")]
        [Tooltip("パララックス効果の強さ（0=完全追従、1=静止）")]
        [Range(0f, 1f)]
        public float parallaxEffectX = 0f;
        
        [Range(0f, 1f)]
        public float parallaxEffectY = 0f;
        
        [Header("Offset")]
        [Tooltip("カメラからのZ軸オフセット（背景なので大きな正の値）")]
        public float zOffset = 10f;
        
        [Header("Tiling (Infinite Background)")]
        [Tooltip("タイリングを有効にするか")]
        public bool enableTiling = false;
        
        [Tooltip("タイルのサイズ")]
        public Vector2 tileSize = new Vector2(20f, 20f);
        
        // === 内部変数 ===
        private Vector3 startPosition;
        private Vector3 lastCameraPosition;
        private SpriteRenderer spriteRenderer;
        
        public enum FollowMode
        {
            FullFollow,     // 完全に追従（背景が常に画面中央）
            Parallax,       // パララックス効果あり
            Static          // 静止（追従しない）
        }
        
        void Start()
        {
            // カメラの取得
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            
            if (targetCamera == null)
            {
                Debug.LogWarning("SpaceBackground: Camera not found!");
                return;
            }
            
            startPosition = transform.position;
            lastCameraPosition = targetCamera.transform.position;
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // 初期位置を設定
            UpdatePosition();
        }
        
        void LateUpdate()
        {
            if (targetCamera == null) return;
            
            UpdatePosition();
            
            if (enableTiling)
            {
                UpdateTiling();
            }
            
            lastCameraPosition = targetCamera.transform.position;
        }
        
        /// <summary>
        /// 背景の位置を更新
        /// </summary>
        void UpdatePosition()
        {
            Vector3 cameraPos = targetCamera.transform.position;
            Vector3 newPosition;
            
            switch (followMode)
            {
                case FollowMode.FullFollow:
                    // 完全にカメラに追従
                    newPosition = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z + zOffset);
                    break;
                    
                case FollowMode.Parallax:
                    // パララックス効果
                    Vector3 deltaMovement = cameraPos - lastCameraPosition;
                    float parallaxX = deltaMovement.x * (1f - parallaxEffectX);
                    float parallaxY = deltaMovement.y * (1f - parallaxEffectY);
                    
                    newPosition = transform.position + new Vector3(parallaxX, parallaxY, 0f);
                    newPosition.z = cameraPos.z + zOffset;
                    break;
                    
                case FollowMode.Static:
                default:
                    // 静止
                    return;
            }
            
            transform.position = newPosition;
        }
        
        /// <summary>
        /// タイリングの更新（無限スクロール用）
        /// </summary>
        void UpdateTiling()
        {
            if (spriteRenderer == null) return;
            
            Vector3 cameraPos = targetCamera.transform.position;
            
            // カメラ位置に応じてタイルをオフセット
            float offsetX = (cameraPos.x - startPosition.x) % tileSize.x;
            float offsetY = (cameraPos.y - startPosition.y) % tileSize.y;
            
            // マテリアルのオフセットを更新
            if (spriteRenderer.material != null)
            {
                spriteRenderer.material.mainTextureOffset = new Vector2(
                    offsetX / tileSize.x,
                    offsetY / tileSize.y
                );
            }
        }
        
        /// <summary>
        /// 背景をカメラのビューに合わせてスケール
        /// </summary>
        public void FitToCamera()
        {
            if (targetCamera == null || spriteRenderer == null) return;
            
            // カメラのビューサイズを取得
            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * targetCamera.aspect;
            
            // スプライトのサイズを取得
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            
            // スケールを計算（少し大きめにして端が見えないように）
            float scaleX = (cameraWidth / spriteSize.x) * 1.2f;
            float scaleY = (cameraHeight / spriteSize.y) * 1.2f;
            float scale = Mathf.Max(scaleX, scaleY);
            
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
