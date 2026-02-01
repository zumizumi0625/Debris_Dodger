# HP・バッテリーUI セットアップガイド

## 概要

このガイドでは、HPゲージ（ハートアイコン）とバッテリーUIの設定方法を説明します。

---

## Part 1: ハートアイコン（HP UI）の設定

### Step 1: Canvasの作成

1. **Hierarchy**で右クリック → **UI** → **Canvas** を選択
2. Canvas の設定:
   - **Render Mode**: `Screen Space - Overlay`
   - **Canvas Scaler**:
     - **UI Scale Mode**: `Scale With Screen Size`
     - **Reference Resolution**: `1920 x 1080`

---

### Step 2: HP用のコンテナ作成

1. **Canvas**を右クリック → **Create Empty** → 名前を `HPContainer` に変更
2. **RectTransform**の設定:
   - **Anchor**: 左上（上の段の左端をクリック）
   - **Pivot**: `(0, 1)`
   - **Position**: `X: 20, Y: -20`

3. **Horizontal Layout Group**コンポーネントを追加:
   - **Spacing**: `10`（ハート間の間隔）
   - **Child Alignment**: `Upper Left`
   - **Child Force Expand**: Width と Height のチェックを外す

---

### Step 3: ハートアイコンのプレハブ作成

1. **HPContainer**を右クリック → **UI** → **Image**
2. 名前を `Heart` に変更
3. **Image**コンポーネントの設定:
   - **Source Image**: ハートのスプライト（下記参照）
   - **Set Native Size**ボタンをクリック
4. **RectTransform**の設定:
   - **Width**: `40`
   - **Height**: `40`

5. **プレハブ化**:
   - `Heart`オブジェクトを**Project**ウィンドウの**Assets/Prefabs**フォルダにドラッグ
   - Hierarchyの`Heart`オブジェクトは削除

---

### Step 4: ハートスプライトがない場合

Unityの標準アセットを使う方法:

1. **Project**ウィンドウで右クリック → **Import Package** → **2D**
2. または、以下のように自作:

```
1. Project ウィンドウで右クリック
2. Create → 2D → Sprites → Circle (丸いスプライトを作成)
3. 色を赤に変更して使用
```

**おすすめ**: Unity Asset Store で "Heart Icon" を検索してダウンロード（無料あり）

---

### Step 5: HPUIコンポーネントの設定

1. **HPContainer**に**HPUI**コンポーネントを追加（Add Component → HPUI）
2. 設定:

| プロパティ | 値 |
|-----------|-----|
| **Satellite Health** | (空でOK、自動検索されます) |
| **Display Mode** | `Hearts` |
| **Heart Prefab** | 作成したHeartプレハブをドラッグ |
| **Heart Container** | HPContainer自身をドラッグ |
| **Full Heart Sprite** | ハートの満タンスプライト |
| **Empty Heart Sprite** | ハートの空スプライト (グレーなど) |

---

## Part 2: バッテリーUIの設定

### Step 1: バッテリー用のコンテナ作成

1. **Canvas**を右クリック → **Create Empty** → 名前を `BatteryContainer` に変更
2. **RectTransform**の設定:
   - **Anchor**: 右上
   - **Pivot**: `(1, 1)`
   - **Position**: `X: -20, Y: -20`
   - **Width**: `200`
   - **Height**: `30`

---

### Step 2: バッテリーバー（Slider方式）

1. **BatteryContainer**を右クリック → **UI** → **Slider**
2. 名前を `BatterySlider` に変更
3. **Slider**コンポーネントの設定:
   - **Interactable**: ☐（チェックを外す）
   - **Min Value**: `0`
   - **Max Value**: `1`
   - **Value**: `1`

4. **子オブジェクトの調整**:
   - `Handle Slide Area` を削除（不要）
   - `Background` → 灰色などに設定
   - `Fill Area` → `Fill` の色を緑に設定

---

### Step 3: バッテリーアイコン追加（オプション）

1. **BatteryContainer**を右クリック → **UI** → **Image**
2. 名前を `BatteryIcon` に変更
3. 電池のアイコンスプライトを設定
4. 位置をスライダーの左側に配置

---

### Step 4: バッテリーパーセント表示（オプション）

1. **BatteryContainer**を右クリック → **UI** → **Text - TextMeshPro**
2. 名前を `BatteryText` に変更
3. 設定:
   - **Text**: `100%`
   - **Font Size**: `20`
   - **Alignment**: 中央揃え
4. 位置をスライダーの右側に配置

---

### Step 5: BatteryUIコンポーネントの設定

1. **BatteryContainer**に**BatteryUI**コンポーネントを追加
2. 設定:

| プロパティ | 値 |
|-----------|-----|
| **Satellite Controller** | (空でOK、自動検索されます) |
| **Battery Slider** | 作成したBatterySliderをドラッグ |
| **Battery Fill Image** | Slider内のFillイメージ |
| **Battery Text** | BatteryTextをドラッグ（ある場合） |
| **Full Color** | 緑 (`0, 255, 0`) |
| **Half Color** | 黄色 (`255, 255, 0`) |
| **Low Color** | 赤 (`255, 0, 0`) |
| **Low Battery Threshold** | `0.2` |
| **Blink On Low** | ☑ |

---

## 完成形のHierarchy構造

```
Canvas
├── HPContainer
│   ├── HPUI (コンポーネント)
│   └── (ゲーム開始時にハートが自動生成されます)
│
├── BatteryContainer
│   ├── BatteryUI (コンポーネント)
│   ├── BatteryIcon (Image) [オプション]
│   ├── BatterySlider (Slider)
│   │   ├── Background
│   │   └── Fill Area
│   │       └── Fill
│   └── BatteryText (TextMeshPro) [オプション]
│
└── EventSystem (自動生成)
```

---

## トラブルシューティング

### ハートが表示されない

1. **Heart Prefab**が設定されているか確認
2. **Heart Container**が正しく設定されているか確認
3. **Full Heart Sprite**が設定されているか確認

### バッテリーバーが動かない

1. **Satellite Controller**がシーン内に存在するか確認
2. **Battery Slider**が正しくリンクされているか確認
3. PlayモードでA/Dキーを押してバッテリーが消費されるか確認

### UIが小さすぎる/大きすぎる

1. **Canvas Scaler**の**Reference Resolution**を調整
2. 各UIオブジェクトの**RectTransform**サイズを調整

---

## 簡易版（最小構成）

時間がない場合の最小構成:

### HP表示（テキストのみ）

1. Canvas → UI → Text - TextMeshPro を作成
2. 名前を `HPText` に変更
3. 左上にアンカー設定
4. 以下のスクリプトをアタッチ:

```csharp
// シンプルなHP表示
using UnityEngine;
using TMPro;
using Platformer.Mechanics;

public class SimpleHPText : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    private SatelliteHealth health;
    
    void Start()
    {
        health = FindObjectOfType<SatelliteHealth>();
    }
    
    void Update()
    {
        if (health != null && hpText != null)
        {
            hpText.text = $"HP: {health.CurrentHP}/{health.MaxHP}";
        }
    }
}
```

### バッテリー表示（テキストのみ）

既存の`BatteryUI`コンポーネントで`batteryText`だけ設定すればOK。
