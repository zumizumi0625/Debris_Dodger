using UnityEngine;
using Platformer.Mechanics;

namespace Platformer.Mechanics
{
    /// <summary>
    /// 宇宙デブリ（宇宙ゴミ）のコントローラー。
    /// ランダムな方向に移動し、プレイヤーに接触するとダメージを与える。
    /// </summary>
    public class DebrisController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("デブリの移動速度")]
        public float moveSpeed = 2.0f;
        
        [Tooltip("速度のランダム幅")]
        public float speedVariance = 1.0f;
        
        [Tooltip("回転速度（度/秒）")]
        public float rotationSpeed = 50.0f;
        
        [Tooltip("回転速度のランダム幅")]
        public float rotationVariance = 30.0f;
        
        [Header("Collision")]
        [Tooltip("プレイヤーに与えるダメージ")]
        public int damage = 1;
        
        [Tooltip("衝突時に消滅するか")]
        public bool destroyOnCollision = false;
        
        [Tooltip("衝突時に与える衝撃力")]
        public float knockbackForce = 2.0f;
        
        [Header("Lifetime")]
        [Tooltip("デブリの寿命（0で無限）")]
        public float lifetime = 0f;
        
        [Tooltip("画面外で自動消滅するか")]
        public bool destroyOffScreen = true;
        
        [Tooltip("画面外判定のマージン")]
        public float offScreenMargin = 2.0f;
        
        [Header("Visual")]
        [Tooltip("デブリのスプライトリスト（ランダム選択）")]
        public Sprite[] debrisSprites;
        
        [Header("Audio")]
        [Tooltip("衝突時の効果音")]
        public AudioClip collisionAudio;
        
        // === 内部変数 ===
        private Vector2 velocity;
        private float currentRotationSpeed;
        private Camera mainCamera;
        private float lifeTimer;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
            mainCamera = Camera.main;
        }
        
        void Start()
        {
            InitializeDebris();
        }
        
        /// <summary>
        /// デブリの初期化（ランダム化）
        /// </summary>
        void InitializeDebris()
        {
            // ランダムな速度
            float actualSpeed = moveSpeed + Random.Range(-speedVariance, speedVariance);
            
            // 移動方向が設定されていない場合はランダム
            if (velocity == Vector2.zero)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * actualSpeed;
            }
            else
            {
                velocity = velocity.normalized * actualSpeed;
            }
            
            // ランダムな回転速度
            currentRotationSpeed = rotationSpeed + Random.Range(-rotationVariance, rotationVariance);
            if (Random.value > 0.5f) currentRotationSpeed *= -1f;
            
            // ランダムなスプライト
            if (debrisSprites != null && debrisSprites.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = debrisSprites[Random.Range(0, debrisSprites.Length)];
            }
            
            // 寿命タイマー
            lifeTimer = lifetime;
        }
        
        /// <summary>
        /// 移動方向を設定
        /// </summary>
        public void SetDirection(Vector2 direction)
        {
            float actualSpeed = moveSpeed + Random.Range(-speedVariance, speedVariance);
            velocity = direction.normalized * actualSpeed;
        }
        
        /// <summary>
        /// 初期速度を設定
        /// </summary>
        public void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }
        
        void Update()
        {
            // 移動
            transform.position += (Vector3)(velocity * Time.deltaTime);
            
            // 回転
            transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);
            
            // 寿命チェック
            if (lifetime > 0)
            {
                lifeTimer -= Time.deltaTime;
                if (lifeTimer <= 0)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            
            // 画面外チェック
            if (destroyOffScreen)
            {
                CheckOffScreen();
            }
        }
        
        /// <summary>
        /// 画面外判定
        /// </summary>
        void CheckOffScreen()
        {
            if (mainCamera == null) return;
            
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            
            if (viewportPos.x < -offScreenMargin || viewportPos.x > 1 + offScreenMargin ||
                viewportPos.y < -offScreenMargin || viewportPos.y > 1 + offScreenMargin)
            {
                Destroy(gameObject);
            }
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other);
        }
        
        void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.collider);
        }
        
        /// <summary>
        /// 衝突処理
        /// </summary>
        void HandleCollision(Collider2D other)
        {
            // プレイヤー（衛星）との衝突
            SatelliteHealth satelliteHealth = other.GetComponent<SatelliteHealth>();
            
            if (satelliteHealth != null)
            {
                // ダメージを与える
                satelliteHealth.TakeDamage(damage);
                
                // ノックバック
                SatelliteController satelliteController = other.GetComponent<SatelliteController>();
                if (satelliteController != null && knockbackForce > 0)
                {
                    Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                    satelliteController.SetVelocity(satelliteController.Velocity + knockbackDirection * knockbackForce);
                }
                
                // 効果音
                if (collisionAudio != null && audioSource != null)
                {
                    audioSource.PlayOneShot(collisionAudio);
                }
                
                // 衝突時に消滅
                if (destroyOnCollision)
                {
                    Destroy(gameObject, 0.1f); // 効果音再生のために少し遅延
                }
                
                Debug.Log($"Debris hit satellite! Damage: {damage}");
            }
        }
        
        /// <summary>
        /// デブリを消滅させる（エフェクト付き）
        /// </summary>
        public void DestroyDebris()
        {
            // TODO: 消滅エフェクトを追加
            Destroy(gameObject);
        }
    }
}
