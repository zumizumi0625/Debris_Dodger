using UnityEngine;
using System.Collections.Generic;

namespace Platformer.Mechanics
{
    /// <summary>
    /// 動的に星空を生成するコンポーネント。
    /// 背景画像がない場合に星のパーティクルを生成。
    /// </summary>
    public class StarfieldGenerator : MonoBehaviour
    {
        [Header("Star Settings")]
        [Tooltip("生成する星の数")]
        public int starCount = 200;
        
        [Tooltip("星を配置する範囲")]
        public Vector2 fieldSize = new Vector2(30f, 20f);
        
        [Tooltip("星のプレハブ（未設定の場合は自動生成）")]
        public GameObject starPrefab;
        
        [Header("Star Appearance")]
        [Tooltip("星の最小サイズ")]
        public float minStarSize = 0.02f;
        
        [Tooltip("星の最大サイズ")]
        public float maxStarSize = 0.1f;
        
        [Tooltip("星の色のバリエーション")]
        public Color[] starColors = new Color[]
        {
            Color.white,
            new Color(0.9f, 0.9f, 1f),      // 青白い
            new Color(1f, 0.95f, 0.9f),     // 暖色系
            new Color(0.8f, 0.8f, 1f),      // 青み
            new Color(1f, 1f, 0.9f)         // 黄色み
        };
        
        [Header("Twinkle Effect")]
        [Tooltip("星の瞬きを有効にするか")]
        public bool enableTwinkle = true;
        
        [Tooltip("瞬きの速度")]
        public float twinkleSpeed = 2f;
        
        [Header("Camera Follow")]
        [Tooltip("カメラに追従するか")]
        public bool followCamera = true;
        
        [Tooltip("追従するカメラ")]
        public Camera targetCamera;
        
        [Tooltip("Z座標（背景として後ろに配置）")]
        public float zPosition = 10f;
        
        // === 内部変数 ===
        private List<StarData> stars = new List<StarData>();
        private Transform starContainer;
        
        private class StarData
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public float twinkleOffset;
            public float baseAlpha;
        }
        
        void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            
            CreateStarContainer();
            GenerateStars();
        }
        
        void Update()
        {
            if (followCamera && targetCamera != null)
            {
                // カメラに追従
                Vector3 cameraPos = targetCamera.transform.position;
                starContainer.position = new Vector3(cameraPos.x, cameraPos.y, zPosition);
            }
            
            if (enableTwinkle)
            {
                UpdateTwinkle();
            }
        }
        
        /// <summary>
        /// 星を格納するコンテナを作成
        /// </summary>
        void CreateStarContainer()
        {
            GameObject container = new GameObject("Stars");
            starContainer = container.transform;
            starContainer.SetParent(transform);
            starContainer.localPosition = Vector3.zero;
        }
        
        /// <summary>
        /// 星を生成
        /// </summary>
        void GenerateStars()
        {
            for (int i = 0; i < starCount; i++)
            {
                CreateStar();
            }
        }
        
        /// <summary>
        /// 個別の星を作成
        /// </summary>
        void CreateStar()
        {
            GameObject star;
            
            if (starPrefab != null)
            {
                star = Instantiate(starPrefab, starContainer);
            }
            else
            {
                // プレハブがない場合は自動生成
                star = CreateDefaultStar();
            }
            
            // ランダムな位置
            float x = Random.Range(-fieldSize.x / 2f, fieldSize.x / 2f);
            float y = Random.Range(-fieldSize.y / 2f, fieldSize.y / 2f);
            star.transform.localPosition = new Vector3(x, y, 0f);
            
            // ランダムなサイズ
            float size = Random.Range(minStarSize, maxStarSize);
            star.transform.localScale = new Vector3(size, size, 1f);
            
            // ランダムな色
            SpriteRenderer renderer = star.GetComponent<SpriteRenderer>();
            if (renderer != null && starColors.Length > 0)
            {
                Color color = starColors[Random.Range(0, starColors.Length)];
                float alpha = Random.Range(0.5f, 1f);
                color.a = alpha;
                renderer.color = color;
                
                // 星データを保存
                StarData data = new StarData
                {
                    transform = star.transform,
                    renderer = renderer,
                    twinkleOffset = Random.Range(0f, Mathf.PI * 2f),
                    baseAlpha = alpha
                };
                stars.Add(data);
            }
        }
        
        /// <summary>
        /// デフォルトの星を作成
        /// </summary>
        GameObject CreateDefaultStar()
        {
            GameObject star = new GameObject("Star");
            star.transform.SetParent(starContainer);
            
            SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
            
            // シンプルな白い円スプライトを作成
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            
            Vector2 center = new Vector2(16f, 16f);
            float radius = 14f;
            
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    
                    if (distance <= radius)
                    {
                        // グラデーションで柔らかい円を描画
                        float alpha = 1f - (distance / radius);
                        alpha = alpha * alpha; // より中心が明るく
                        pixels[y * 32 + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        pixels[y * 32 + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            renderer.sprite = sprite;
            renderer.sortingOrder = -100; // 背景として最背面
            
            return star;
        }
        
        /// <summary>
        /// 星の瞬きを更新
        /// </summary>
        void UpdateTwinkle()
        {
            foreach (var star in stars)
            {
                if (star.renderer == null) continue;
                
                float twinkle = Mathf.Sin(Time.time * twinkleSpeed + star.twinkleOffset);
                twinkle = (twinkle + 1f) / 2f; // 0-1に正規化
                
                Color color = star.renderer.color;
                color.a = star.baseAlpha * Mathf.Lerp(0.6f, 1f, twinkle);
                star.renderer.color = color;
            }
        }
        
        /// <summary>
        /// 星を再生成
        /// </summary>
        public void RegenerateStars()
        {
            // 既存の星を削除
            foreach (var star in stars)
            {
                if (star.transform != null)
                {
                    Destroy(star.transform.gameObject);
                }
            }
            stars.Clear();
            
            // 新しい星を生成
            GenerateStars();
        }
    }
}
