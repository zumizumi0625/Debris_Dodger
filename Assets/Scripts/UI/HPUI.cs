using UnityEngine;
using UnityEngine.UI;
using Platformer.Mechanics;
using TMPro;
using System.Collections.Generic;

namespace Platformer.UI
{
    /// <summary>
    /// HP（衛星の耐久力）を表示するUIコントローラー。
    /// ハートアイコンまたはバー形式でHPを視覚化。
    /// </summary>
    public class HPUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SatelliteHealthへの参照（未設定の場合は自動検索）")]
        public SatelliteHealth satelliteHealth;
        
        [Header("Display Mode")]
        [Tooltip("表示モード")]
        public DisplayMode displayMode = DisplayMode.Hearts;
        
        [Header("Heart Display")]
        [Tooltip("ハートアイコンのプレハブ")]
        public GameObject heartPrefab;
        
        [Tooltip("ハートを配置する親オブジェクト")]
        public Transform heartContainer;
        
        [Tooltip("満タンのハートスプライト")]
        public Sprite fullHeartSprite;
        
        [Tooltip("空のハートスプライト")]
        public Sprite emptyHeartSprite;
        
        [Header("Bar Display")]
        [Tooltip("HPバーのスライダー")]
        public Slider hpSlider;
        
        [Tooltip("HPバーのFillイメージ")]
        public Image hpFillImage;
        
        [Tooltip("HP数値を表示するテキスト")]
        public TextMeshProUGUI hpText;
        
        [Header("Color Settings")]
        [Tooltip("HP満タン時の色")]
        public Color fullColor = new Color(0.2f, 0.8f, 0.2f);
        
        [Tooltip("HP中間時の色")]
        public Color midColor = new Color(0.9f, 0.7f, 0.1f);
        
        [Tooltip("HP低下時の色")]
        public Color lowColor = new Color(0.9f, 0.2f, 0.2f);
        
        [Header("Animation")]
        [Tooltip("ダメージ時のアニメーション")]
        public bool animateOnDamage = true;
        
        [Tooltip("ダメージアニメーションの時間")]
        public float damageAnimationDuration = 0.3f;
        
        // === 内部変数 ===
        private List<Image> heartImages = new List<Image>();
        private float damageAnimationTimer;
        private bool isAnimatingDamage;
        private Vector3 originalScale;
        
        public enum DisplayMode
        {
            Hearts,    // ハートアイコン形式
            Bar,       // バー形式
            Both       // 両方
        }
        
        void Start()
        {
            // SatelliteHealthが未設定の場合は自動検索
            if (satelliteHealth == null)
            {
                satelliteHealth = FindObjectOfType<SatelliteHealth>();
                
                if (satelliteHealth == null)
                {
                    Debug.LogWarning("HPUI: SatelliteHealth not found in scene!");
                    return;
                }
            }
            
            // イベント登録
            satelliteHealth.OnHealthChanged += OnHealthChanged;
            satelliteHealth.OnDamaged += OnDamaged;
            
            // 初期化
            originalScale = transform.localScale;
            InitializeHearts();
            UpdateDisplay(satelliteHealth.CurrentHP, satelliteHealth.MaxHP);
        }
        
        void OnDestroy()
        {
            if (satelliteHealth != null)
            {
                satelliteHealth.OnHealthChanged -= OnHealthChanged;
                satelliteHealth.OnDamaged -= OnDamaged;
            }
        }
        
        void Update()
        {
            // ダメージアニメーション
            if (isAnimatingDamage)
            {
                damageAnimationTimer -= Time.deltaTime;
                
                float progress = 1f - (damageAnimationTimer / damageAnimationDuration);
                float shake = Mathf.Sin(progress * Mathf.PI * 4f) * (1f - progress) * 0.1f;
                transform.localScale = originalScale * (1f + shake);
                
                if (damageAnimationTimer <= 0)
                {
                    isAnimatingDamage = false;
                    transform.localScale = originalScale;
                }
            }
        }
        
        /// <summary>
        /// ハートアイコンを初期化
        /// </summary>
        void InitializeHearts()
        {
            if (displayMode == DisplayMode.Bar) return;
            if (heartContainer == null || heartPrefab == null) return;
            
            // 既存のハートをクリア
            foreach (Transform child in heartContainer)
            {
                Destroy(child.gameObject);
            }
            heartImages.Clear();
            
            // 最大HP分のハートを生成
            for (int i = 0; i < satelliteHealth.MaxHP; i++)
            {
                GameObject heart = Instantiate(heartPrefab, heartContainer);
                Image heartImage = heart.GetComponent<Image>();
                
                if (heartImage != null)
                {
                    heartImages.Add(heartImage);
                }
            }
        }
        
        /// <summary>
        /// HP変更時のコールバック
        /// </summary>
        void OnHealthChanged(int currentHP, int maxHP)
        {
            UpdateDisplay(currentHP, maxHP);
        }
        
        /// <summary>
        /// ダメージ時のコールバック
        /// </summary>
        void OnDamaged()
        {
            if (animateOnDamage)
            {
                isAnimatingDamage = true;
                damageAnimationTimer = damageAnimationDuration;
            }
        }
        
        /// <summary>
        /// 表示を更新
        /// </summary>
        void UpdateDisplay(int currentHP, int maxHP)
        {
            float ratio = (float)currentHP / maxHP;
            
            // ハート表示の更新
            if (displayMode != DisplayMode.Bar)
            {
                UpdateHearts(currentHP);
            }
            
            // バー表示の更新
            if (displayMode != DisplayMode.Hearts)
            {
                UpdateBar(ratio);
            }
            
            // テキスト表示の更新
            if (hpText != null)
            {
                hpText.text = $"{currentHP}/{maxHP}";
            }
            
            // 色の更新
            UpdateColor(ratio);
        }
        
        /// <summary>
        /// ハートアイコンを更新
        /// </summary>
        void UpdateHearts(int currentHP)
        {
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] != null)
                {
                    // スプライトが設定されている場合のみ変更
                    if (fullHeartSprite != null && emptyHeartSprite != null)
                    {
                        heartImages[i].sprite = (i < currentHP) ? fullHeartSprite : emptyHeartSprite;
                    }
                    
                    // HPがない場合は透明度を下げる
                    Color color = heartImages[i].color;
                    color.a = (i < currentHP) ? 1f : 0.3f;
                    heartImages[i].color = color;
                }
            }
        }
        
        /// <summary>
        /// バーを更新
        /// </summary>
        void UpdateBar(float ratio)
        {
            if (hpSlider != null)
            {
                hpSlider.value = ratio;
            }
            
            if (hpFillImage != null)
            {
                hpFillImage.fillAmount = ratio;
            }
        }
        
        /// <summary>
        /// 色を更新
        /// </summary>
        void UpdateColor(float ratio)
        {
            Color targetColor;
            
            if (ratio > 0.6f)
            {
                targetColor = fullColor;
            }
            else if (ratio > 0.3f)
            {
                float t = (ratio - 0.3f) / 0.3f;
                targetColor = Color.Lerp(midColor, fullColor, t);
            }
            else
            {
                float t = ratio / 0.3f;
                targetColor = Color.Lerp(lowColor, midColor, t);
            }
            
            // バーの色を適用
            if (hpFillImage != null)
            {
                hpFillImage.color = targetColor;
            }
            
            if (hpSlider != null && hpSlider.fillRect != null)
            {
                Image fillImage = hpSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = targetColor;
                }
            }
        }
    }
}
