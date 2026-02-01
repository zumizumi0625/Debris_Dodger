# Debris Dodger 仕様書 (Ver 1.0)

## 1. ゲーム概要
宇宙空間を進む衛星を操作し、デブリ（宇宙ゴミ）や小惑星を回避しながら進み続けるアクションゲーム。
上方向へのスクロール制で、進んだ距離がスコアとなる。

---

## 2. コア・ゲームプレイ

### 勝利/敗北条件
*   **勝利条件**: なし（エンドレスラン形式、または特定高度到達）
    *   *現状の実装はエンドレスで、HPが尽きるまで続く。*
*   **敗北条件**: HPが0になる（デブリに3回衝突する）。

### スコアシステム
*   **計算方法**: 上方向にスクロールした距離（メートル相当）× 倍率。
*   **表示**:
    *   プレイ中画面（HUD）に常時表示。
    *   ゲームオーバー画面に最終結果を表示。

---

## 3. プレイヤーキャラクター（衛星）

| 項目 | 仕様詳細 | 担当スクリプト |
| :--- | :--- | :--- |
| **HP（耐久力）** | 最大HPは **3**。<br>デブリ衝突ごとに1減少。ダメージ後に1.5秒の無敵時間あり。 | `SatelliteHealth` |
| **バッテリー** | 最大100。<br>姿勢制御（回転）を行うと消費する。<br>0になると回転操作不能。 | `SatelliteController` |
| **移動操作** | **Spaceキー**: 現在の機首方向へ化学スラスタを噴射して加速（慣性あり）。 | `SatelliteController` |
| **回転操作** | **A / ←**: 反時計回り<br>**D / →**: 時計回り<br>バッテリーを消費する。 | `SatelliteController` |
| **エフェクト** | **噴射時**: パーティクルとライト点灯。<br>**ダメージ時**: 機体点滅。<br>**低HP時**: 警告エフェクト（赤点滅・煙）。 | `SatelliteThrusterEffect`<br>`SatelliteDamageEffect` |

---

## 4. 環境・敵キャラクター

### カメラ・スクロール
*   **方向**: 上方向へ自動スクロール。
*   **速度**: 時間経過とともに徐々に加速する。
*   **追従**: プレイヤーもカメラに合わせて上方向へ移動する。
    *   *担当: `VerticalScroller.cs`*

### デブリ・小惑星
*   **発生**: 画面上部からランダムな位置・間隔でスポーンする。
*   **種類**:
    1.  **デブリ**: 小さめ、速い、回転が速い。
    2.  **小惑星**: 大きめ、遅い。
*   **挙動**:
    *   物理挙動（Rigidbody2D）を持ち、衝突時にプレイヤーを弾く（ノックバック）。
    *   プレイヤーに1ダメージを与える。
*   **難易度**: 時間経過とともにスポーン間隔が短くなる。
    *   *担当: `DebrisSpawner.cs`, `DebrisController.cs`*

---

## 5. UI（ユーザーインターフェース）

### HUD（ヘッドアップディスプレイ）
*   **HPゲージ**: ハートアイコン3つで表示。現在HP分だけ赤く点灯。(`HPUI`)
*   **バッテリー**: バーおよび％で残量を表示。(`BatteryUI`)
*   **スコア**: 現在の到達距離を表示。(`ScoreUI`)

### ゲームオーバー画面
*   プレイヤー死亡時にフェードイン表示。
*   **表示内容**: "GAME OVER" テキスト、最終スコア。
*   **機能**:
    *   **RETRY**: ゲームをリロードして再開。
    *   **TITLE**: タイトル画面へ戻る。

---

## 6. セットアップガイドまとめ

### シーン構成 (Scene Hierarchy)
```
Main Camera
 └─ VerticalScroller (Scroll Speed: 1.0, Accelerate: ON)
     └─ Background (子オブジェクトとして追従)

Player (衛星)
 ├─ SatelliteController
 ├─ SatelliteHealth (Max HP: 3)
 ├─ SatelliteThrusterEffect
 └─ Rigidbody2D / Collider2D

DebrisSpawner
 └─ DebrisSpawner (Direction: Top, Prefabs: Asteroid, Debris)

GameManager (空オブジェクト)
 └─ ScoreManager

Canvas
 ├─ HPContainer (HPUI + Horizontal Layout)
 ├─ BatteryContainer (BatteryUI)
 ├─ ScoreText (ScoreUI)
 └─ GameOverPanel (GameOverUI, 初期は非表示)
```

