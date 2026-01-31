using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// 衛星とデブリの衝突イベント。
    /// </summary>
    public class SatelliteDebrisCollision : Simulation.Event<SatelliteDebrisCollision>
    {
        public SatelliteHealth satellite;
        public DebrisController debris;
        
        public override void Execute()
        {
            if (satellite == null || debris == null) return;
            
            // ダメージ処理
            satellite.TakeDamage(debris.damage);
            
            // デブリの消滅処理（オプション）
            if (debris.destroyOnCollision)
            {
                debris.DestroyDebris();
            }
        }
    }
}
