using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// 衛星が破壊された時に発火するイベント。
    /// ゲームオーバー処理とリスポーンをスケジュールする。
    /// </summary>
    public class SatelliteDeath : Simulation.Event<SatelliteDeath>
    {
        public SatelliteHealth satelliteHealth;
        
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            if (satelliteHealth == null) return;
            
            var satelliteController = satelliteHealth.GetComponent<SatelliteController>();
            
            if (satelliteController != null)
            {
                // 操作を無効化
                satelliteController.ControlEnabled = false;
                
                // 死亡効果音
                var audioSource = satelliteController.GetComponent<AudioSource>();
                if (audioSource != null && satelliteController.deathAudio != null)
                {
                    audioSource.PlayOneShot(satelliteController.deathAudio);
                }
            }
            
            // ゲームオーバーUIイベントをスケジュール
            Simulation.Schedule<GameOver>(0.5f);
            
            Debug.Log("SatelliteDeath event executed. GameOver scheduled.");
        }
    }
}
