using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace Platformer.UI
{
    /// <summary>
    /// ゲームオーバー画面のUIコントローラー。
    /// リトライとタイトルに戻るオプションを提供。
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("ゲームオーバーパネル")]
        public GameObject gameOverPanel;
        
        [Tooltip("ゲームオーバーテキスト")]
        public TextMeshProUGUI gameOverText;
        
        [Tooltip("リトライボタン")]
        public Button retryButton;
        
        [Tooltip("タイトルに戻るボタン")]
        public Button titleButton;
        
        [Header("Animation")]
        [Tooltip("フェードイン時間")]
        public float fadeInDuration = 0.5f;
        
        [Tooltip("フェード用のCanvasGroup")]
        public CanvasGroup canvasGroup;
        
        [Header("Settings")]
        [Tooltip("タイトルシーンの名前")]
        public string titleSceneName = "Title";
        
        [Tooltip("ゲームシーンの名前（リトライ用）")]
        public string gameSceneName = "Play";
        
        private bool isShowing;
        
        void Awake()
        {
            // 初期状態では非表示
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            
            // ボタンイベント設定
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }
            
            if (titleButton != null)
            {
                titleButton.onClick.AddListener(OnTitleClicked);
            }
        }
        
        /// <summary>
        /// ゲームオーバー画面を表示
        /// </summary>
        public void Show()
        {
            if (isShowing) return;
            isShowing = true;
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            StartCoroutine(FadeIn());
        }
        
        /// <summary>
        /// ゲームオーバー画面を非表示
        /// </summary>
        public void Hide()
        {
            if (!isShowing) return;
            isShowing = false;
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
        
        /// <summary>
        /// フェードインコルーチン
        /// </summary>
        IEnumerator FadeIn()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / fadeInDuration;
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
                }
                
                yield return null;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            
            // ゲームを一時停止
            Time.timeScale = 0f;
        }
        
        /// <summary>
        /// リトライボタンクリック時
        /// </summary>
        void OnRetryClicked()
        {
            Time.timeScale = 1f;
            
            // 現在のシーンをリロード
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
        
        /// <summary>
        /// タイトルボタンクリック時
        /// </summary>
        void OnTitleClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(titleSceneName);
        }
        
        void OnDestroy()
        {
            // タイムスケールをリセット
            Time.timeScale = 1f;
        }
    }
}
