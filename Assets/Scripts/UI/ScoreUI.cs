using UnityEngine;
using tmpro = TMPro;
using Platformer.Mechanics;

namespace Platformer.UI
{
    /// <summary>
    /// ゲームプレイ中のスコアを表示するUI
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        public ScoreManager scoreManager;
        public tmpro.TextMeshProUGUI scoreText;
        public string format = "Score: {0}";

        void Start()
        {
            if (scoreManager == null)
                scoreManager = FindObjectOfType<ScoreManager>();
        }

        void Update()
        {
            if (scoreManager != null && scoreText != null)
            {
                scoreText.text = string.Format(format, scoreManager.Score);
            }
        }
    }
}
