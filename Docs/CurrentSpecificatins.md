# Debris Dodger 仕様書 (Ver 2.0)

> 最終更新: 2026-02-03

---

## 1. ゲーム概要
宇宙空間を進む衛星を操作し、デブリ（宇宙ゴミ）や小惑星を回避しながら進み続けるアクションゲーム。
上方向へのスクロール制で、進んだ距離がスコアとなる。

---

## 2. コア・ゲームプレイ

### 勝利/敗北条件

| 条件 | 内容 |
|:---|:---|
| **勝利条件** | なし（エンドレスラン形式） |
| **敗北条件** | HPが0になる（デブリに3回衝突する）、または画面外に出てHPが0になる |

### スコアシステム

| 項目 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **計算方法** | 上方向への移動距離（Y座標）× 倍率（デフォルト: 10） | `ScoreManager` |
| **計測対象** | カメラまたはプレイヤーのY座標 | `ScoreManager` |
| **HUD表示** | プレイ中画面に常時表示 | `ScoreUI` |
| **結果表示** | ゲームオーバー画面に最終スコアを表示 | `GameOverUI` |

---

## 3. プレイヤーキャラクター（衛星）

### 基本パラメータ

| 項目 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **HP（耐久力）** | 最大HP: **3**<br>デブリ衝突ごとに1減少。<br>ダメージ後に**1.5秒**の無敵時間（点滅エフェクト）。 | `SatelliteHealth` |
| **開始時無敵時間** | ゲーム開始直後**5秒間**は無敵状態。<br>開始直後のデブリ衝突を防止。 | `SatelliteHealth` |
| **バッテリー** | 最大: 100<br>姿勢制御（回転）を行うと消費。<br>消費量: **10/秒**（操作中）<br>0になると回転操作不能、現在の角速度を維持。 | `SatelliteController` |

### 操作系

| 操作 | キー | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|:---|
| **並進移動（化学スラスタ）** | Spaceキー | 現在の機首方向へ推力を加える（1回押下で1.0の速度加算）。<br>慣性あり（宇宙空間を模擬）。<br>クールダウン: **0.5秒**。<br>最大速度: **3.0**。 | `SatelliteController` |
| **回転（電気スラスタ）** | A / 左矢印 | 反時計回りに回転（角速度を増加） | `SatelliteController` |
| | D / 右矢印 | 時計回りに回転（角速度を減少） | `SatelliteController` |

**物理挙動**:
- 宇宙空間を模擬するため、速度・角速度ともに減衰しない
- 衛星の「前方」を+Y方向（上）として、噴射方向を計算
- Rigidbody2Dをキネマティックモードで使用

### 画面外処理

| 状況 | 処理 | 担当スクリプト |
|:---|:---|:---|
| 画面外への移動 | 1ダメージを受け、画面中央にリスポーン。<br>速度・角速度はリセット。<br>無敵中は再リスポーンなし。 | `SatelliteController.OnExitScreen()` |

### エフェクト

| タイミング | エフェクト | 担当スクリプト |
|:---|:---|:---|
| **噴射時** | パーティクルエフェクト + ライト点灯 | `SatelliteThrusterEffect` |
| **ダメージ時** | 機体点滅（無敵時間中、5回点滅） | `SatelliteHealth` |
| **低HP時** | 警告エフェクト（赤点滅・煙パーティクル） | `SatelliteDamageEffect` |

---

## 4. カメラ・スクロール

| 項目 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **追従方向** | Y軸のみ（プレイヤーの上昇に追従） | `CameraFollowY` |
| **追従方式** | 滑らかに追従（SmoothDamp使用、smoothTime: **0.3秒**） | `CameraFollowY` |
| **一方向スクロール** | 有効（プレイヤーが下がってもカメラは下がらない） | `CameraFollowY` |
| **オフセット** | プレイヤーより上方向に**3ユニット**の位置にカメラ | `CameraFollowY` |

---

## 5. デブリ・障害物

### デブリの挙動

| 項目 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **移動** | 一定方向に移動（速度: 2.0 ± 1.0のランダム） | `DebrisController` |
| **回転** | 自転あり（50° ± 30°/秒のランダム） | `DebrisController` |
| **ダメージ** | プレイヤーに1ダメージ | `DebrisController` |
| **ノックバック** | 衝突時にプレイヤーを弾く（力: 2.0） | `DebrisController` |
| **寿命** | 画面外で自動消滅（マージン: 2.0ユニット） | `DebrisController` |

### スポーン設定

| 項目 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **発生位置** | 画面端から（設定可能: 上のみ / 左右 / 全方向） | `DebrisSpawner` |
| **スポーン間隔** | 2.0秒 ± 1.0秒のランダム | `DebrisSpawner` |
| **スポーン速度倍率** | 調整可能（speedMultiplier: 0.1〜3.0） | `DebrisSpawner` |
| **最大同時存在数** | 20体 | `DebrisSpawner` |

### 難易度スケーリング

| 項目 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **有効/無効** | 設定可能 | `DebrisSpawner` |
| **スケーリング方法** | 時間経過でスポーン間隔が短縮 | `DebrisSpawner` |
| **スケーリング速度** | 0.01/秒の減少 | `DebrisSpawner` |
| **最小間隔** | 0.5秒（これ以上は短くならない） | `DebrisSpawner` |

---

## 6. UI（ユーザーインターフェース）

### HUD（ヘッドアップディスプレイ）

| 要素 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **HPゲージ** | ハートアイコン3つで表示。現在HP分だけ表示を変更。 | `HPUI` |
| **バッテリー** | バーおよび％で残量を表示 | `BatteryUI` |
| **スコア** | 現在の到達距離を数値で表示 | `ScoreUI` |

### ゲームオーバー画面

| 要素 | 仕様詳細 | 担当スクリプト |
|:---|:---|:---|
| **表示タイミング** | プレイヤー死亡の0.5秒後にフェードイン | `GameOverUI` |
| **フェードイン時間** | 0.5秒 | `GameOverUI` |
| **表示内容** | "GAME OVER" テキスト、最終スコア | `GameOverUI` |
| **RETRYボタン** | 現在のシーンをリロードして再開 | `GameOverUI` |
| **TITLEボタン** | タイトルシーン（"Title"）へ遷移 | `GameOverUI` |
| **ゲーム停止** | 表示中はTime.timeScale = 0で一時停止 | `GameOverUI` |

---

## 7. シーン構成

### シーン一覧

| シーン名 | 内容 |
|:---|:---|
| **Title** | タイトル画面 |
| **Play** | メインゲームシーン |

### シーン階層構造 (Play Scene)

```
Main Camera
 └─ CameraFollowY (Target: Player)

Player (衛星)
 ├─ SatelliteController
 │   ├─ Thrust Power: 1.0
 │   ├─ Max Speed: 3.0
 │   ├─ Thrust Cooldown: 0.5
 │   ├─ Rotation Torque: 90.0
 │   ├─ Battery Consumption: 10.0
 │   └─ Max Battery: 100.0
 ├─ SatelliteHealth (Max HP: 3, Invincibility: 1.5s)
 ├─ SatelliteThrusterEffect
 ├─ SatelliteDamageEffect
 ├─ Rigidbody2D (Kinematic, GravityScale: 0)
 ├─ Collider2D
 ├─ SpriteRenderer
 └─ AudioSource

DebrisSpawner
 └─ DebrisSpawner
     ├─ Spawn Interval: 2.0
     ├─ Spawn Direction: Top または All
     ├─ Max Debris Count: 20
     └─ Speed Multiplier: 1.0

GameManager (空オブジェクト)
 └─ ScoreManager (Score Multiplier: 10)

Canvas
 ├─ HPContainer (HPUI)
 ├─ BatteryContainer (BatteryUI)
 ├─ ScoreText (ScoreUI)
 └─ GameOverPanel (GameOverUI, 初期は非表示)
```

---

## 8. スクリプト一覧

### プレイヤー関連

| スクリプト | 機能概要 |
|:---|:---|
| `SatelliteController.cs` | 衛星の移動・回転制御、バッテリー管理、画面外処理 |
| `SatelliteHealth.cs` | HP管理、ダメージ処理、無敵時間、死亡判定 |
| `SatelliteThrusterEffect.cs` | スラスタのパーティクル・ライトエフェクト |
| `SatelliteDamageEffect.cs` | ダメージ時・低HP時のエフェクト |

### デブリ関連

| スクリプト | 機能概要 |
|:---|:---|
| `DebrisController.cs` | デブリの移動・回転、衝突時のダメージ・ノックバック |
| `DebrisSpawner.cs` | デブリの生成、難易度スケーリング |

### システム関連

| スクリプト | 機能概要 |
|:---|:---|
| `ScoreManager.cs` | スコア計算（Y座標ベース） |
| `CameraFollowY.cs` | カメラのY軸追従 |
| `GameController.cs` | シミュレーションのティック処理 |

### UI関連

| スクリプト | 機能概要 |
|:---|:---|
| `HPUI.cs` | HPゲージ（ハートアイコン）表示 |
| `BatteryUI.cs` | バッテリー残量表示 |
| `ScoreUI.cs` | スコア表示 |
| `GameOverUI.cs` | ゲームオーバー画面、リトライ・タイトル遷移 |

---

## 9. パラメータ調整ガイド

### 難易度調整例

| パラメータ | 低難易度 | 標準 | 高難易度 |
|:---|:---:|:---:|:---:|
| Player Max HP | 5 | 3 | 1 |
| Invincibility Duration | 2.0s | 1.5s | 1.0s |
| Debris Spawn Interval | 3.0s | 2.0s | 1.0s |
| Debris Move Speed | 1.5 | 2.0 | 3.0 |
| Debris Knockback Force | 1.0 | 2.0 | 3.0 |
| Difficulty Scale Rate | 0.005 | 0.01 | 0.02 |
| Min Spawn Interval | 1.0s | 0.5s | 0.3s |

---

## 10. 未実装・今後の計画

### 未実装機能（当初仕様書より）

| 機能 | 状態 | 備考 |
|:---|:---:|:---|
| 太陽電池パネルによるバッテリー充電 | 未実装 | 太陽方向への姿勢維持によるジレンマ要素 |
| アイテム（クリスタル・サンプル） | 未実装 | スコアボーナス用 |
| 横スクロール形式 | 変更 | 縦スクロールに変更済み |

### 今後の改善点

- [ ] デブリの種類追加（サイズ・速度のバリエーション）
- [ ] パーティクルエフェクトの調整
- [ ] 効果音・BGMの追加
- [ ] ハイスコア保存機能
- [ ] オプション画面（音量調整等）

---

## 変更履歴

| バージョン | 日付 | 変更内容 |
|:---|:---|:---|
| 1.0 | - | 初版作成 |
| 2.0 | 2026-02-03 | 現在の実装に合わせて全面更新。画面外処理、カメラ追従、各種パラメータの詳細を追記。 |
