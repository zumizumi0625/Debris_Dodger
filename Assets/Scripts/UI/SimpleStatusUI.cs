using UnityEngine;
using TMPro;
using Platformer.Mechanics;

namespace Platformer.UI
{
    /// <summary>
    /// HPとバッテリーをシンプルなテキストで表示するUI。
    /// 素早くセットアップしたい場合に使用。
    /// </summary>
    public class SimpleStatusUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("HP表示用テキスト（例: HP: 3/3）")]
        public TextMeshProUGUI hpText;
        
        [Tooltip("バッテリー表示用テキスト（例: Battery: 100%）")]
        public TextMeshProUGUI batteryText;
        
        [Header("Display Format")]
        [Tooltip("HPの表示フォーマット")]
        public string hpFormat = "HP: {0}/{1}";
        
        [Tooltip("バッテリーの表示フォーマット")]
        public string batteryFormat = "Battery: {0}%";
        
        [Header("Colors")]
        [Tooltip("HP満タン時の色")]
        public Color hpFullColor = Color.green;
        
        [Tooltip("HP低下時の色")]
        public Color hpLowColor = Color.red;
        
        [Tooltip("バッテリー満タン時の色")]
        public Color batteryFullColor = Color.cyan;
        
        [Tooltip("バッテリー低下時の色")]
        public Color batteryLowColor = Color.red;
        
        // === コンポーネント ===
        private SatelliteHealth satelliteHealth;
        private SatelliteController satelliteController;
        
        void Start()
        {
            // 自動で参照を検索
            satelliteHealth = FindObjectOfType<SatelliteHealth>();
            satelliteController = FindObjectOfType<SatelliteController>();
            
            if (satelliteHealth == null)
            {
                Debug.LogWarning("SimpleStatusUI: SatelliteHealth not found!");
            }
            
            if (satelliteController == null)
            {
                Debug.LogWarning("SimpleStatusUI: SatelliteController not found!");
            }
        }
        
        void Update()
        {
            UpdateHPDisplay();
            UpdateBatteryDisplay();
        }
        
        /// <summary>
        /// HP表示を更新
        /// </summary>
        void UpdateHPDisplay()
        {
            if (hpText == null || satelliteHealth == null) return;
            
            int currentHP = satelliteHealth.CurrentHP;
            int maxHP = satelliteHealth.MaxHP;
            
            hpText.text = string.Format(hpFormat, currentHP, maxHP);
            
            // HPに応じて色を変更
            float hpRatio = (float)currentHP / maxHP;
            hpText.color = Color.Lerp(hpLowColor, hpFullColor, hpRatio);
        }
        
        /// <summary>
        /// バッテリー表示を更新
        /// </summary>
        void UpdateBatteryDisplay()
        {
            if (batteryText == null || satelliteController == null) return;
            
            float batteryRatio = satelliteController.BatteryRatio;
            int percentage = Mathf.RoundToInt(batteryRatio * 100f);
            
            batteryText.text = string.Format(batteryFormat, percentage);
            
            // バッテリーに応じて色を変更
            batteryText.color = Color.Lerp(batteryLowColor, batteryFullColor, batteryRatio);
        }
    }
}
