using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// 衛星のダメージエフェクトコントローラー。
    /// ダメージ時やHPが低い時の視覚的なフィードバックを提供。
    /// </summary>
    [RequireComponent(typeof(SatelliteHealth))]
    public class SatelliteDamageEffect : MonoBehaviour
    {
        [Header("Flash Effect")]
        [Tooltip("ダメージ時のフラッシュ色")]
        public Color flashColor = Color.red;
        
        [Tooltip("フラッシュの持続時間")]
        public float flashDuration = 0.1f;
        
        [Header("Low HP Effect")]
        [Tooltip("低HPエフェクトを有効にするか")]
        public bool enableLowHPEffect = true;
        
        [Tooltip("低HP警告のしきい値（0-1）")]
        [Range(0f, 1f)]
        public float lowHPThreshold = 0.3f;
        
        [Tooltip("低HP時のパーティクル（煙など）")]
        public ParticleSystem lowHPParticle;
        
        [Tooltip("低HP時の点滅速度")]
        public float lowHPBlinkSpeed = 2f;
        
        [Header("Death Effect")]
        [Tooltip("死亡時の爆発パーティクル")]
        public ParticleSystem explosionParticle;
        
        [Tooltip("死亡時のカメラシェイク強度")]
        public float deathShakeIntensity = 0.5f;
        
        [Tooltip("カメラシェイクの持続時間")]
        public float deathShakeDuration = 0.3f;
        
        // === コンポーネント ===
        private SatelliteHealth satelliteHealth;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private float flashTimer;
        private bool isFlashing;
        private bool isLowHP;
        
        void Awake()
        {
            satelliteHealth = GetComponent<SatelliteHealth>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        void Start()
        {
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            
            // イベント登録
            if (satelliteHealth != null)
            {
                satelliteHealth.OnDamaged += OnDamaged;
                satelliteHealth.OnDeath += OnDeath;
                satelliteHealth.OnHealthChanged += OnHealthChanged;
            }
            
            // 低HPパーティクルを停止しておく
            if (lowHPParticle != null)
            {
                lowHPParticle.Stop();
            }
        }
        
        void OnDestroy()
        {
            if (satelliteHealth != null)
            {
                satelliteHealth.OnDamaged -= OnDamaged;
                satelliteHealth.OnDeath -= OnDeath;
                satelliteHealth.OnHealthChanged -= OnHealthChanged;
            }
        }
        
        void Update()
        {
            // フラッシュエフェクト
            if (isFlashing)
            {
                flashTimer -= Time.deltaTime;
                
                if (flashTimer <= 0)
                {
                    isFlashing = false;
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = originalColor;
                    }
                }
            }
            
            // 低HPエフェクト
            if (enableLowHPEffect && isLowHP && !satelliteHealth.IsInvincible)
            {
                UpdateLowHPEffect();
            }
        }
        
        /// <summary>
        /// ダメージ時のコールバック
        /// </summary>
        void OnDamaged()
        {
            // フラッシュエフェクト開始
            StartFlash();
            
            // カメラシェイク（軽め）
            ShakeCamera(0.2f, 0.1f);
        }
        
        /// <summary>
        /// 死亡時のコールバック
        /// </summary>
        void OnDeath()
        {
            // 爆発パーティクル
            if (explosionParticle != null)
            {
                explosionParticle.Play();
            }
            
            // カメラシェイク（強め）
            ShakeCamera(deathShakeIntensity, deathShakeDuration);
            
            // 低HPパーティクルを停止
            if (lowHPParticle != null)
            {
                lowHPParticle.Stop();
            }
            
            // スプライトを非表示
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }
        
        /// <summary>
        /// HP変更時のコールバック
        /// </summary>
        void OnHealthChanged(int currentHP, int maxHP)
        {
            float ratio = (float)currentHP / maxHP;
            bool wasLowHP = isLowHP;
            isLowHP = ratio <= lowHPThreshold && ratio > 0;
            
            // 低HPになった時にパーティクル開始
            if (isLowHP && !wasLowHP && lowHPParticle != null)
            {
                lowHPParticle.Play();
            }
            // 低HPを脱した時にパーティクル停止
            else if (!isLowHP && wasLowHP && lowHPParticle != null)
            {
                lowHPParticle.Stop();
            }
        }
        
        /// <summary>
        /// フラッシュエフェクト開始
        /// </summary>
        void StartFlash()
        {
            isFlashing = true;
            flashTimer = flashDuration;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = flashColor;
            }
        }
        
        /// <summary>
        /// 低HPエフェクトの更新
        /// </summary>
        void UpdateLowHPEffect()
        {
            if (spriteRenderer == null) return;
            
            // 赤く点滅
            float blink = (Mathf.Sin(Time.time * lowHPBlinkSpeed * Mathf.PI * 2f) + 1f) / 2f;
            Color blinkColor = Color.Lerp(originalColor, new Color(1f, 0.5f, 0.5f), blink * 0.3f);
            spriteRenderer.color = blinkColor;
        }
        
        /// <summary>
        /// カメラシェイク
        /// </summary>
        void ShakeCamera(float intensity, float duration)
        {
            // カメラシェイクコンポーネントを探して実行
            var shaker = Camera.main?.GetComponent<CameraShake>();
            
            if (shaker != null)
            {
                shaker.Shake(intensity, duration);
            }
            else
            {
                // 簡易シェイク（CameraShakeコンポーネントがない場合）
                StartCoroutine(SimpleShake(intensity, duration));
            }
        }
        
        /// <summary>
        /// 簡易カメラシェイク
        /// </summary>
        System.Collections.IEnumerator SimpleShake(float intensity, float duration)
        {
            if (Camera.main == null) yield break;
            
            Vector3 originalPos = Camera.main.transform.position;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float currentIntensity = intensity * (1f - progress);
                
                Vector3 offset = new Vector3(
                    Random.Range(-currentIntensity, currentIntensity),
                    Random.Range(-currentIntensity, currentIntensity),
                    0f
                );
                
                Camera.main.transform.position = originalPos + offset;
                yield return null;
            }
            
            Camera.main.transform.position = originalPos;
        }
    }
    
    /// <summary>
    /// カメラシェイクコンポーネント（スタブ）
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public virtual void Shake(float intensity, float duration)
        {
            // オーバーライドして実装
        }
    }
}
