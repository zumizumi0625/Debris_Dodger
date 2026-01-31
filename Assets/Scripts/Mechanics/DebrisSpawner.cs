using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// デブリのスポナー。
    /// 画面端からデブリを生成し続ける。
    /// </summary>
    public class DebrisSpawner : MonoBehaviour
    {
        [Header("Spawning")]
        [Tooltip("デブリのプレハブ")]
        public GameObject debrisPrefab;
        
        [Tooltip("複数のデブリプレハブ（ランダム選択）")]
        public GameObject[] debrisPrefabs;
        
        [Tooltip("スポーン間隔（秒）")]
        public float spawnInterval = 2.0f;
        
        [Tooltip("スポーン間隔のランダム幅")]
        public float spawnIntervalVariance = 1.0f;
        
        [Tooltip("同時に存在できる最大デブリ数")]
        public int maxDebrisCount = 20;
        
        [Header("Spawn Area")]
        [Tooltip("スポーン位置のオフセット（画面端からの距離）")]
        public float spawnOffset = 1.0f;
        
        [Tooltip("スポーンする方向")]
        public SpawnDirection spawnDirection = SpawnDirection.All;
        
        [Header("Difficulty Scaling")]
        [Tooltip("時間経過でスポーン間隔を短くするか")]
        public bool scaleDifficulty = true;
        
        [Tooltip("最小スポーン間隔")]
        public float minSpawnInterval = 0.5f;
        
        [Tooltip("難易度上昇の速さ")]
        public float difficultyScaleRate = 0.01f;
        
        [Header("Debug")]
        [Tooltip("スポーンを有効にするか")]
        public bool spawnEnabled = true;
        
        // === 内部変数 ===
        private Camera mainCamera;
        private float spawnTimer;
        private float currentInterval;
        private float gameTime;
        private List<GameObject> activeDebris = new List<GameObject>();
        
        public enum SpawnDirection
        {
            All,        // 全方向
            Top,        // 上からのみ
            Sides,      // 左右からのみ
            TopAndSides // 上と左右
        }
        
        void Start()
        {
            mainCamera = Camera.main;
            currentInterval = spawnInterval;
            spawnTimer = currentInterval;
        }
        
        void Update()
        {
            if (!spawnEnabled) return;
            
            gameTime += Time.deltaTime;
            
            // 難易度スケーリング
            if (scaleDifficulty)
            {
                currentInterval = Mathf.Max(minSpawnInterval, 
                    spawnInterval - (gameTime * difficultyScaleRate));
            }
            
            // スポーンタイマー
            spawnTimer -= Time.deltaTime;
            
            if (spawnTimer <= 0)
            {
                // 破棄されたデブリを管理リストから削除
                CleanupDestroyedDebris();
                
                // 最大数チェック
                if (activeDebris.Count < maxDebrisCount)
                {
                    SpawnDebris();
                }
                
                // 次のスポーンまでの時間を設定
                spawnTimer = currentInterval + Random.Range(-spawnIntervalVariance, spawnIntervalVariance);
            }
        }
        
        /// <summary>
        /// デブリをスポーン
        /// </summary>
        void SpawnDebris()
        {
            if (mainCamera == null) return;
            
            // スポーン位置と移動方向を決定
            Vector3 spawnPosition;
            Vector2 direction;
            
            GetSpawnPositionAndDirection(out spawnPosition, out direction);
            
            // プレハブを選択
            GameObject prefabToSpawn = GetRandomPrefab();
            if (prefabToSpawn == null) return;
            
            // デブリを生成
            GameObject debris = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            // 方向を設定
            DebrisController controller = debris.GetComponent<DebrisController>();
            if (controller != null)
            {
                controller.SetDirection(direction);
            }
            
            activeDebris.Add(debris);
            
            Debug.Log($"Debris spawned at {spawnPosition}, direction: {direction}");
        }
        
        /// <summary>
        /// スポーン位置と移動方向を取得
        /// </summary>
        void GetSpawnPositionAndDirection(out Vector3 position, out Vector2 direction)
        {
            // カメラのビューポート境界を取得
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 10));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 10));
            
            float left = bottomLeft.x - spawnOffset;
            float right = topRight.x + spawnOffset;
            float bottom = bottomLeft.y - spawnOffset;
            float top = topRight.y + spawnOffset;
            
            // スポーン方向に基づいて位置を決定
            int side;
            switch (spawnDirection)
            {
                case SpawnDirection.Top:
                    side = 0; // 上
                    break;
                case SpawnDirection.Sides:
                    side = Random.value > 0.5f ? 1 : 2; // 左または右
                    break;
                case SpawnDirection.TopAndSides:
                    side = Random.Range(0, 3); // 上、左、右
                    break;
                case SpawnDirection.All:
                default:
                    side = Random.Range(0, 4); // 全方向
                    break;
            }
            
            switch (side)
            {
                case 0: // 上
                    position = new Vector3(Random.Range(bottomLeft.x, topRight.x), top, 0);
                    direction = new Vector2(Random.Range(-0.3f, 0.3f), -1f).normalized;
                    break;
                case 1: // 左
                    position = new Vector3(left, Random.Range(bottomLeft.y, topRight.y), 0);
                    direction = new Vector2(1f, Random.Range(-0.3f, 0.3f)).normalized;
                    break;
                case 2: // 右
                    position = new Vector3(right, Random.Range(bottomLeft.y, topRight.y), 0);
                    direction = new Vector2(-1f, Random.Range(-0.3f, 0.3f)).normalized;
                    break;
                case 3: // 下
                default:
                    position = new Vector3(Random.Range(bottomLeft.x, topRight.x), bottom, 0);
                    direction = new Vector2(Random.Range(-0.3f, 0.3f), 1f).normalized;
                    break;
            }
        }
        
        /// <summary>
        /// ランダムなプレハブを取得
        /// </summary>
        GameObject GetRandomPrefab()
        {
            if (debrisPrefabs != null && debrisPrefabs.Length > 0)
            {
                return debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];
            }
            
            return debrisPrefab;
        }
        
        /// <summary>
        /// 破棄されたデブリを管理リストから削除
        /// </summary>
        void CleanupDestroyedDebris()
        {
            activeDebris.RemoveAll(debris => debris == null);
        }
        
        /// <summary>
        /// 全てのデブリを削除
        /// </summary>
        public void ClearAllDebris()
        {
            foreach (var debris in activeDebris)
            {
                if (debris != null)
                {
                    Destroy(debris);
                }
            }
            activeDebris.Clear();
        }
        
        /// <summary>
        /// スポーンを一時停止/再開
        /// </summary>
        public void SetSpawnEnabled(bool enabled)
        {
            spawnEnabled = enabled;
        }
        
        /// <summary>
        /// 難易度をリセット
        /// </summary>
        public void ResetDifficulty()
        {
            gameTime = 0;
            currentInterval = spawnInterval;
        }
    }
}
