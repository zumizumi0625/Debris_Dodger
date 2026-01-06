using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// 宇宙空間で探査衛星を操作するためのプレイヤーコントローラー。
    /// 化学スラスタによる並進移動と電気スラスタによる姿勢制御を実装。
    /// </summary>
    public class SatelliteController : MonoBehaviour
    {
        // === 化学スラスタ（並進移動用） ===
        [Header("Chemical Thruster (Translation)")]
        [Tooltip("1回の噴射で加わる速度")]
        public float thrustPower = 1.0f;
        
        [Tooltip("最大移動速度")]
        public float maxSpeed = 3.0f;
        
        [Tooltip("噴射後のクールダウン時間（秒）")]
        public float thrustCooldown = 0.5f;
        
        // === 電気スラスタ（姿勢制御用） ===
        [Header("Electric Thruster (Attitude Control)")]
        [Tooltip("回転トルク（度/秒）")]
        public float rotationTorque = 90.0f;
        
        [Tooltip("バッテリー消費量（/秒）")]
        public float batteryConsumption = 10.0f;
        
        // === バッテリー ===
        [Header("Battery")]
        [Tooltip("最大バッテリー容量")]
        public float maxBattery = 100.0f;
        
        [Tooltip("現在のバッテリー残量")]
        public float currentBattery = 100.0f;
        
        // === オーディオ ===
        [Header("Audio")]
        public AudioClip thrustAudio;
        public AudioClip rotationAudio;
        public AudioClip deathAudio;
        
        // === 状態 ===
        private Vector2 velocity;           // 現在の速度ベクトル
        private float angularVelocity;      // 現在の角速度（度/秒）
        private float lastThrustTime;       // 最後の噴射時刻
        private bool controlEnabled = true;
        
        // === コンポーネント ===
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private Collider2D collider2d;
        private Animator animator;
        private Camera mainCamera;
        
        // === プロパティ ===
        public Vector2 Velocity => velocity;
        public float AngularVelocity => angularVelocity;
        public float BatteryRatio => currentBattery / maxBattery;
        public bool ControlEnabled
        {
            get => controlEnabled;
            set => controlEnabled = value;
        }
        
        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            mainCamera = Camera.main;
            
            // Rigidbody2Dの設定（キネマティック）
            if (body != null)
            {
                body.isKinematic = true;
                body.gravityScale = 0f; // 無重力
                // 回転制約を解除（重要！）
                body.constraints = RigidbodyConstraints2D.None;
            }
        }

        void Start()
        {
            // 初期状態：静止
            velocity = Vector2.zero;
            angularVelocity = 0f;
            lastThrustTime = -thrustCooldown; // 開始時に即座に噴射可能
        }

        void Update()
        {
            if (controlEnabled)
            {
                HandleRotationInput();
                HandleThrustInput();
            }
        }

        void FixedUpdate()
        {
            // 慣性による移動
            ApplyMovement();
            
            // 回転の適用
            ApplyRotation();
            
            // 画面端チェック
            CheckScreenBounds();
        }

        /// <summary>
        /// 電気スラスタによる姿勢制御入力処理
        /// </summary>
        void HandleRotationInput()
        {
            float rotationInput = 0f;
            
            // A/左キー: 反時計回り（角度増加）
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                rotationInput = 1f;
            }
            // D/右キー: 時計回り（角度減少）
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                rotationInput = -1f;
            }
            
            // バッテリーがある場合のみ姿勢制御可能
            if (currentBattery > 0 && rotationInput != 0f)
            {
                // 角速度を変更
                angularVelocity += rotationInput * rotationTorque * Time.deltaTime;
                
                // バッテリー消費
                currentBattery -= batteryConsumption * Time.deltaTime;
                currentBattery = Mathf.Max(0f, currentBattery);
                
                Debug.Log($"Rotation input: {rotationInput}, Angular velocity: {angularVelocity}");
            }
            // バッテリーがない場合は現在の角速度を維持（宇宙空間では減衰しない）
        }

        /// <summary>
        /// 化学スラスタによる並進移動入力処理
        /// </summary>
        void HandleThrustInput()
        {
            // Spaceキーで噴射（クールダウン確認）
            if (Input.GetButtonDown("Jump") && Time.time >= lastThrustTime + thrustCooldown)
            {
                // 衛星の現在の向きに基づいて推力方向を計算
                // 衛星の「前方」を+Y方向（上）と仮定
                float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector2 thrustDirection = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
                
                // 速度に加算（慣性を維持）
                velocity += thrustDirection * thrustPower;
                
                // 最大速度制限
                if (maxSpeed > 0 && velocity.magnitude > maxSpeed)
                {
                    velocity = velocity.normalized * maxSpeed;
                }
                
                // クールダウン開始
                lastThrustTime = Time.time;
                
                // 効果音再生
                if (thrustAudio != null && audioSource != null)
                {
                    audioSource.PlayOneShot(thrustAudio);
                }
                
                // スラスタ噴射イベント（アニメーション等のトリガーに使用可能）
                OnThrustFired();
                
                Debug.Log($"Thrust fired! Direction: {thrustDirection}, Velocity: {velocity}");
            }
        }

        /// <summary>
        /// 慣性による移動を適用
        /// </summary>
        void ApplyMovement()
        {
            // 宇宙空間なので速度は減衰しない
            Vector2 newPosition = body.position + velocity * Time.fixedDeltaTime;
            body.MovePosition(newPosition);
        }

        /// <summary>
        /// 回転を適用
        /// </summary>
        void ApplyRotation()
        {
            // 宇宙空間なので角速度は減衰しない
            float newAngle = transform.eulerAngles.z + angularVelocity * Time.fixedDeltaTime;
            body.MoveRotation(newAngle);
        }

        /// <summary>
        /// 画面端チェック - 画面外に出たらゲームオーバー
        /// </summary>
        void CheckScreenBounds()
        {
            if (mainCamera == null) return;
            
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            
            // ビューポート座標で0-1の範囲外ならば画面外
            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            {
                OnExitScreen();
            }
        }

        /// <summary>
        /// 画面外に出た時の処理
        /// </summary>
        void OnExitScreen()
        {
            // ゲームオーバー処理
            controlEnabled = false;
            
            // 効果音再生
            if (deathAudio != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathAudio);
            }
            
            // PlayerEnteredDeathZoneイベントをスケジュール
            var deathEvent = Schedule<PlayerEnteredDeathZone>();
            // Note: PlayerEnteredDeathZoneが既存のPlayerControllerを参照している場合、
            // 新しいSatelliteController用のイベントを作成する必要があるかもしれない
        }

        /// <summary>
        /// スラスタ噴射時のコールバック
        /// </summary>
        protected virtual void OnThrustFired()
        {
            // アニメーションやエフェクトのトリガーに使用
            // 派生クラスでオーバーライド可能
        }

        /// <summary>
        /// バッテリーを充電
        /// </summary>
        public void ChargeBattery(float amount)
        {
            currentBattery = Mathf.Min(currentBattery + amount, maxBattery);
        }

        /// <summary>
        /// 衛星をテレポート
        /// </summary>
        public void Teleport(Vector3 position)
        {
            body.position = position;
            velocity = Vector2.zero;
            angularVelocity = 0f;
        }

        /// <summary>
        /// 外部から速度を設定
        /// </summary>
        public void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
            
            // 最大速度制限
            if (maxSpeed > 0 && velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
        }

        /// <summary>
        /// 外部から角速度を設定
        /// </summary>
        public void SetAngularVelocity(float newAngularVelocity)
        {
            angularVelocity = newAngularVelocity;
        }

        /// <summary>
        /// クールダウンが完了しているか
        /// </summary>
        public bool CanThrust()
        {
            return Time.time >= lastThrustTime + thrustCooldown;
        }

        /// <summary>
        /// クールダウンの残り時間（秒）
        /// </summary>
        public float GetThrustCooldownRemaining()
        {
            float remaining = (lastThrustTime + thrustCooldown) - Time.time;
            return Mathf.Max(0f, remaining);
        }
    }
}
