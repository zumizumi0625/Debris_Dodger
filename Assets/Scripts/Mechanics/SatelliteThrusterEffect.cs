using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// 衛星のスラスタエフェクトコントローラー。
    /// 化学スラスタと電気スラスタ使用時のパーティクルエフェクトを管理。
    /// </summary>
    [RequireComponent(typeof(SatelliteController))]
    public class SatelliteThrusterEffect : MonoBehaviour
    {
        [Header("Chemical Thruster (Main)")]
        [Tooltip("化学スラスタのパーティクルシステム")]
        public ParticleSystem mainThrusterParticle;
        
        [Tooltip("噴射時間（秒）")]
        public float thrustDuration = 0.3f;
        
        [Tooltip("噴射時のライト（オプション）")]
        public Light2D thrusterLight;
        
        [Tooltip("噴射時のライト強度")]
        public float thrusterLightIntensity = 2.0f;
        
        [Header("Electric Thruster (Rotation)")]
        [Tooltip("左回転時のパーティクルシステム")]
        public ParticleSystem leftRotationParticle;
        
        [Tooltip("右回転時のパーティクルシステム")]
        public ParticleSystem rightRotationParticle;
        
        [Header("Visual Settings")]
        [Tooltip("スラスタの色")]
        public Color thrusterColor = new Color(1f, 0.6f, 0.2f);
        
        [Tooltip("電気スラスタの色")]
        public Color electricThrusterColor = new Color(0.4f, 0.6f, 1f);
        
        [Header("Trail Effect")]
        [Tooltip("移動時のトレイルエフェクト")]
        public TrailRenderer moveTrail;
        
        [Tooltip("トレイルを表示する最小速度")]
        public float minSpeedForTrail = 1.0f;
        
        // === コンポーネント ===
        private SatelliteController satelliteController;
        private float thrustTimer;
        private bool isThrusting;
        
        void Awake()
        {
            satelliteController = GetComponent<SatelliteController>();
        }
        
        void Start()
        {
            // パーティクルの初期化
            InitializeParticles();
            
            // ライトの初期化
            if (thrusterLight != null)
            {
                thrusterLight.intensity = 0f;
            }
        }
        
        void Update()
        {
            HandleMainThruster();
            HandleRotationThrusters();
            HandleTrailEffect();
        }
        
        /// <summary>
        /// パーティクルの初期化
        /// </summary>
        void InitializeParticles()
        {
            // 化学スラスタ
            if (mainThrusterParticle != null)
            {
                var main = mainThrusterParticle.main;
                main.startColor = thrusterColor;
                mainThrusterParticle.Stop();
            }
            
            // 電気スラスタ
            if (leftRotationParticle != null)
            {
                var main = leftRotationParticle.main;
                main.startColor = electricThrusterColor;
                leftRotationParticle.Stop();
            }
            
            if (rightRotationParticle != null)
            {
                var main = rightRotationParticle.main;
                main.startColor = electricThrusterColor;
                rightRotationParticle.Stop();
            }
        }
        
        /// <summary>
        /// 化学スラスタのエフェクト処理
        /// </summary>
        void HandleMainThruster()
        {
            // スペースキーで噴射
            if (Input.GetButtonDown("Jump") && satelliteController.CanThrust() && satelliteController.ControlEnabled)
            {
                FireMainThruster();
            }
            
            // 噴射タイマー
            if (isThrusting)
            {
                thrustTimer -= Time.deltaTime;
                
                // ライトのフェードアウト
                if (thrusterLight != null)
                {
                    float progress = thrustTimer / thrustDuration;
                    thrusterLight.intensity = thrusterLightIntensity * progress;
                }
                
                if (thrustTimer <= 0)
                {
                    StopMainThruster();
                }
            }
        }
        
        /// <summary>
        /// 化学スラスタを発射
        /// </summary>
        public void FireMainThruster()
        {
            isThrusting = true;
            thrustTimer = thrustDuration;
            
            // パーティクルを発射
            if (mainThrusterParticle != null)
            {
                mainThrusterParticle.Play();
            }
            
            // ライトを点灯
            if (thrusterLight != null)
            {
                thrusterLight.intensity = thrusterLightIntensity;
            }
        }
        
        /// <summary>
        /// 化学スラスタを停止
        /// </summary>
        void StopMainThruster()
        {
            isThrusting = false;
            
            if (mainThrusterParticle != null && mainThrusterParticle.isPlaying)
            {
                mainThrusterParticle.Stop();
            }
            
            if (thrusterLight != null)
            {
                thrusterLight.intensity = 0f;
            }
        }
        
        /// <summary>
        /// 回転スラスタのエフェクト処理
        /// </summary>
        void HandleRotationThrusters()
        {
            if (!satelliteController.ControlEnabled) return;
            if (satelliteController.currentBattery <= 0) return; // バッテリーがない場合は表示しない
            
            bool rotatingLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            bool rotatingRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            
            // 左回転エフェクト
            if (leftRotationParticle != null)
            {
                if (rotatingLeft && !leftRotationParticle.isPlaying)
                {
                    leftRotationParticle.Play();
                }
                else if (!rotatingLeft && leftRotationParticle.isPlaying)
                {
                    leftRotationParticle.Stop();
                }
            }
            
            // 右回転エフェクト
            if (rightRotationParticle != null)
            {
                if (rotatingRight && !rightRotationParticle.isPlaying)
                {
                    rightRotationParticle.Play();
                }
                else if (!rotatingRight && rightRotationParticle.isPlaying)
                {
                    rightRotationParticle.Stop();
                }
            }
        }
        
        /// <summary>
        /// トレイルエフェクトの処理
        /// </summary>
        void HandleTrailEffect()
        {
            if (moveTrail == null) return;
            
            float speed = satelliteController.Velocity.magnitude;
            
            // 速度に応じてトレイルを表示/非表示
            if (speed >= minSpeedForTrail)
            {
                if (!moveTrail.emitting)
                {
                    moveTrail.emitting = true;
                }
                
                // 速度に応じてトレイルの幅を変更
                float widthMultiplier = Mathf.Clamp01((speed - minSpeedForTrail) / (satelliteController.maxSpeed - minSpeedForTrail));
                moveTrail.widthMultiplier = 0.1f + (widthMultiplier * 0.2f);
            }
            else
            {
                if (moveTrail.emitting)
                {
                    moveTrail.emitting = false;
                }
            }
        }
        
        /// <summary>
        /// 全てのエフェクトを停止
        /// </summary>
        public void StopAllEffects()
        {
            StopMainThruster();
            
            if (leftRotationParticle != null) leftRotationParticle.Stop();
            if (rightRotationParticle != null) rightRotationParticle.Stop();
            if (moveTrail != null) moveTrail.emitting = false;
        }
    }
    
    /// <summary>
    /// Light2D用のプレースホルダー（Universal RPの2D Lightが無い場合のフォールバック）
    /// </summary>
    public class Light2D : MonoBehaviour
    {
        public float intensity;
    }
}
