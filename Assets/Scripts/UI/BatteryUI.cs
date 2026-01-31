using UnityEngine;
using UnityEngine.UI;
using Platformer.Mechanics;
using TMPro;

namespace Platformer.UI
{
    /// <summary>
    /// バッテリー残量を表示するUIコントローラー。
    /// スライダーまたはイメージのfillAmountでバッテリー残量を視覚化。
    /// </summary>
    public class BatteryUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SatelliteControllerへの参照（未設定の場合は自動検索）")]
        public SatelliteController satelliteController;
        
        [Header("UI Elements")]
        [Tooltip("バッテリー残量を表示するスライダー（オプション）")]
        public Slider batterySlider;
        
        [Tooltip("バッテリー残量を表示するイメージ（Fill方式、オプション）")]
        public Image batteryFillImage;
        
        [Tooltip("バッテリーパーセンテージを表示するテキスト（オプション）")]
        public TextMeshProUGUI batteryText;
        
        [Tooltip("バッテリー残量に応じて色を変えるイメージ（オプション）")]
        public Image batteryColorImage;
        
        [Header("Color Settings")]
        [Tooltip("バッテリー満タン時の色")]
        public Color fullColor = Color.green;
        
        [Tooltip("バッテリー半分時の色")]
        public Color halfColor = Color.yellow;
        
        [Tooltip("バッテリー低下時の色")]
        public Color lowColor = Color.red;
        
        [Tooltip("低バッテリー警告のしきい値（0-1）")]
        [Range(0f, 1f)]
        public float lowBatteryThreshold = 0.2f;
        
        [Header("Animation")]
        [Tooltip("低バッテリー時に点滅させるか")]
        public bool blinkOnLow = true;
        
        [Tooltip("点滅の速度")]
        public float blinkSpeed = 2f;
        
        private float blinkTimer;
        private bool isBlinking;
        
        void Start()
        {
            // SatelliteControllerが未設定の場合は自動検索
            if (satelliteController == null)
            {
                satelliteController = FindObjectOfType<SatelliteController>();
                
                if (satelliteController == null)
                {
                    Debug.LogWarning("BatteryUI: SatelliteController not found in scene!");
                }
            }
            
            // スライダーの初期設定
            if (batterySlider != null)
            {
                batterySlider.minValue = 0f;
                batterySlider.maxValue = 1f;
            }
        }
        
        void Update()
        {
            if (satelliteController == null) return;
            
            float batteryRatio = satelliteController.BatteryRatio;
            
            // スライダーを更新
            if (batterySlider != null)
            {
                batterySlider.value = batteryRatio;
            }
            
            // Fillイメージを更新
            if (batteryFillImage != null)
            {
                batteryFillImage.fillAmount = batteryRatio;
            }
            
            // テキストを更新
            if (batteryText != null)
            {
                int percentage = Mathf.RoundToInt(batteryRatio * 100f);
                batteryText.text = $"{percentage}%";
            }
            
            // 色を更新
            UpdateBatteryColor(batteryRatio);
            
            // 低バッテリー時の点滅
            if (blinkOnLow && batteryRatio <= lowBatteryThreshold)
            {
                HandleLowBatteryBlink();
            }
            else
            {
                isBlinking = false;
            }
        }
        
        /// <summary>
        /// バッテリー残量に応じた色を計算・適用
        /// </summary>
        void UpdateBatteryColor(float ratio)
        {
            Color targetColor;
            
            if (ratio > 0.5f)
            {
                // 満タン〜半分: 緑から黄色へ
                float t = (ratio - 0.5f) * 2f;
                targetColor = Color.Lerp(halfColor, fullColor, t);
            }
            else
            {
                // 半分〜空: 黄色から赤へ
                float t = ratio * 2f;
                targetColor = Color.Lerp(lowColor, halfColor, t);
            }
            
            // 点滅中はアルファを変動
            if (isBlinking)
            {
                float alpha = (Mathf.Sin(blinkTimer * blinkSpeed * Mathf.PI * 2f) + 1f) / 2f;
                alpha = Mathf.Lerp(0.3f, 1f, alpha);
                targetColor.a = alpha;
            }
            
            // 色を適用
            if (batteryColorImage != null)
            {
                batteryColorImage.color = targetColor;
            }
            
            if (batteryFillImage != null)
            {
                batteryFillImage.color = targetColor;
            }
            
            if (batterySlider != null && batterySlider.fillRect != null)
            {
                Image fillImage = batterySlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = targetColor;
                }
            }
        }
        
        /// <summary>
        /// 低バッテリー時の点滅処理
        /// </summary>
        void HandleLowBatteryBlink()
        {
            isBlinking = true;
            blinkTimer += Time.deltaTime;
        }
    }
}
