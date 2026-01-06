# Debris Dodger - プレイヤー移動システム実装完了

## 実装内容

### 新規作成ファイル

#### [SatelliteController.cs](file:///c:/Users/sprin/Github/dev_unity/Debris_Dodger/Assets/Scripts/Mechanics/SatelliteController.cs)

宇宙空間で探査衛星を操作するためのプレイヤーコントローラー。

## 実装した機能

### 1. 化学スラスタ（並進移動）
- **入力**: Spaceキー
- **動作**: 衛星の現在の向きに基づいて推進力を加える
- **クールダウン**: 連射制限あり（デフォルト0.5秒）
- **最大速度制限**: 設定可能

```csharp
// 衛星の向きから推力方向を計算
float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
Vector2 thrustDirection = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
velocity += thrustDirection * thrustPower;
```

### 2. 電気スラスタ（姿勢制御）
- **入力**: A/←キー（反時計回り）、D/→キー（時計回り）
- **動作**: 押している間、角速度を変更
- **バッテリー消費**: 使用中にバッテリーを消費
- **バッテリー枯渇時**: 姿勢制御不能（現在の回転を維持）

### 3. 慣性移動
- 宇宙空間を再現し、速度・角速度は減衰しない
- 一度加速すると永続的に移動/回転を続ける

### 4. 画面端処理
- 衛星が画面外に出るとゲームオーバー
- `PlayerEnteredDeathZone`イベントをスケジュール

### 5. バッテリーシステム（基本実装）
- 電気スラスタ使用でバッテリー消費
- `ChargeBattery(float amount)`で充電可能

## Inspectorで調整可能なパラメータ

| パラメータ | デフォルト値 | 説明 |
|-----------|-------------|------|
| Thrust Power | 5.0 | 1回の噴射で加わる速度 |
| Max Speed | 8.0 | 最大移動速度 |
| Thrust Cooldown | 0.5 | 噴射間隔（秒） |
| Rotation Torque | 90.0 | 回転速度（度/秒） |
| Battery Consumption | 10.0 | バッテリー消費量（/秒） |
| Max Battery | 100.0 | 最大バッテリー容量 |

## 使用方法

### セットアップ手順

1. **衛星のGameObjectを作成または選択**
2. **必要なコンポーネントをアタッチ**:
   - `Rigidbody2D`（Kinematicに設定される）
   - `Collider2D`（当たり判定用）
   - `SpriteRenderer`（見た目）
   - `AudioSource`（効果音用、オプション）
3. **`SatelliteController`スクリプトをアタッチ**
4. **Inspectorでパラメータを調整**

### 既存のPlayerControllerからの移行

既存のPlayerオブジェクトがある場合：
1. `PlayerController`コンポーネントを無効化または削除
2. 代わりに`SatelliteController`をアタッチ

## 次のステップ

- [ ] 太陽電池パネル充電システムの実装
- [ ] スラスタ噴射時のエフェクト追加
- [ ] Unityエディタでの動作確認とパラメータ調整
- [ ] 衛星用スプライトの作成または設定
