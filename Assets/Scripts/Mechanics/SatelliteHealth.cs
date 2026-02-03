using System;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// 衛星の耐久力（HP）を管理するコンポーネント。
    /// デブリとの衝突でダメージを受け、HPが0になるとゲームオーバー。
    /// </summary>
    public class SatelliteHealth : MonoBehaviour
    {
        [Header("HP Settings")]
        [Tooltip("最大HP")]
        public int maxHP = 3;
        
        [Tooltip("ダメージ後の無敵時間（秒）")]
        public float invincibilityDuration = 1.5f;
        
        [Tooltip("ゲーム開始時の無敵時間（秒）")]
        public float startInvincibilityDuration = 5.0f;
        
        [Header("Visual Feedback")]
        [Tooltip("ダメージ時の点滅回数")]
        public int blinkCount = 5;
        
        [Header("Audio")]
        [Tooltip("ダメージ時の効果音")]
        public AudioClip damageAudio;
        
        [Tooltip("HP回復時の効果音")]
        public AudioClip healAudio;
        
        // === 状態 ===
        private int currentHP;
        private bool isInvincible;
        private float invincibilityTimer;
        
        // === コンポーネント ===
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private SatelliteController satelliteController;
        
        // === イベント ===
        public event Action<int, int> OnHealthChanged; // (currentHP, maxHP)
        public event Action OnDeath;
        public event Action OnDamaged;
        
        // === プロパティ ===
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public float HealthRatio => (float)currentHP / maxHP;
        public bool IsAlive => currentHP > 0;
        public bool IsInvincible => isInvincible;
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
            satelliteController = GetComponent<SatelliteController>();
        }
        
        void Start()
        {
            currentHP = maxHP;
            OnHealthChanged?.Invoke(currentHP, maxHP);
            
            // ゲーム開始時の無敵時間を設定
            if (startInvincibilityDuration > 0)
            {
                isInvincible = true;
                invincibilityTimer = startInvincibilityDuration;
                Debug.Log($"Start invincibility activated for {startInvincibilityDuration} seconds");
            }
        }
        
        void Update()
        {
            // 無敵時間の処理
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                
                // 点滅エフェクト
                if (spriteRenderer != null)
                {
                    float blinkValue = Mathf.Sin(invincibilityTimer * blinkCount * Mathf.PI * 2f / invincibilityDuration);
                    spriteRenderer.enabled = blinkValue > 0;
                }
                
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage = 1)
        {
            if (!IsAlive || isInvincible) return;
            
            currentHP = Mathf.Max(0, currentHP - damage);
            OnHealthChanged?.Invoke(currentHP, maxHP);
            OnDamaged?.Invoke();
            
            // 効果音
            if (damageAudio != null && audioSource != null)
            {
                audioSource.PlayOneShot(damageAudio);
            }
            
            // 無敵時間開始
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            
            Debug.Log($"Satellite damaged! HP: {currentHP}/{maxHP}");
            
            // 死亡判定
            if (currentHP <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// HPを回復
        /// </summary>
        public void Heal(int amount = 1)
        {
            if (!IsAlive) return;
            
            int previousHP = currentHP;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            
            if (currentHP != previousHP)
            {
                OnHealthChanged?.Invoke(currentHP, maxHP);
                
                // 効果音
                if (healAudio != null && audioSource != null)
                {
                    audioSource.PlayOneShot(healAudio);
                }
                
                Debug.Log($"Satellite healed! HP: {currentHP}/{maxHP}");
            }
        }
        
        /// <summary>
        /// HPを全回復
        /// </summary>
        public void FullHeal()
        {
            currentHP = maxHP;
            OnHealthChanged?.Invoke(currentHP, maxHP);
        }
        
        /// <summary>
        /// 死亡処理
        /// </summary>
        void Die()
        {
            OnDeath?.Invoke();
            
            // 操作無効化
            if (satelliteController != null)
            {
                satelliteController.ControlEnabled = false;
            }
            
            Debug.Log("Satellite destroyed!");
            
            // 直接GameOverUIを表示
            // includeInactive = true で非アクティブなオブジェクトも検索
            var gameOverUI = FindObjectOfType<Platformer.UI.GameOverUI>(true);
            if (gameOverUI != null)
            {
                // 少し遅延させて表示
                StartCoroutine(ShowGameOverDelayed(gameOverUI, 0.5f));
            }
            else
            {
                Debug.LogWarning("GameOverUI not found in scene! Make sure GameOverPanel exists in Canvas.");
            }
            
            // 旧イベントシステム（バックアップ）
            // var deathEvent = Schedule<SatelliteDeath>();
            // deathEvent.satelliteHealth = this;
        }
        
        /// <summary>
        /// ゲームオーバー表示を遅延実行
        /// </summary>
        System.Collections.IEnumerator ShowGameOverDelayed(Platformer.UI.GameOverUI gameOverUI, float delay)
        {
            yield return new WaitForSeconds(delay);
            gameOverUI.Show();
        }
        
        /// <summary>
        /// リセット（リスポーン用）
        /// </summary>
        public void Reset()
        {
            currentHP = maxHP;
            isInvincible = false;
            invincibilityTimer = 0f;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            
            OnHealthChanged?.Invoke(currentHP, maxHP);
        }
    }
}
