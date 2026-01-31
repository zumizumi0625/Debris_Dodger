using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// ゲームオーバー時に発火するイベント。
    /// ゲームオーバーUIを表示し、リトライオプションを提供する。
    /// </summary>
    public class GameOver : Simulation.Event<GameOver>
    {
        public override void Execute()
        {
            Debug.Log("Game Over!");
            
            // ゲームオーバーUIを表示
            var gameOverUI = Object.FindObjectOfType<GameOverUI>();
            if (gameOverUI != null)
            {
                gameOverUI.Show();
            }
            else
            {
                Debug.LogWarning("GameOverUI not found in scene!");
            }
            
            // ゲームを一時停止（オプション）
            // Time.timeScale = 0f;
        }
    }
}
