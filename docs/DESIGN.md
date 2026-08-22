# Design notes

## 境界

Toolkit本体はADOFAIの型を`object`として保持し、型固有操作をbackendへ委譲する。

この構造にした理由は次の3点。

1. `Assembly-CSharp.dll`の更新でCore全体を再コンパイルしなくてよい
2. PACL2など外部Modイベントも同じEvent APIへ載せられる
3. Fake backendだけでTransactionと型変換をテストできる

## Event生成

`EventService.Create(name, floor)`は次の順で動く。

1. backendがイベント名を`LevelEventInfo`へ解決する
2. backendが生の`LevelEvent`を生成する
3. backendが現在の`levelEvents`へ追加する
4. `EventHandle`を返す

追加先は既定で`EventCollection.Auto`とし、`LevelEventInfo.isDecoration`から`Actions`または`Decorations`を選ぶ。明示指定で上書きできる。Queryは既定で両方を見る。

`EventHandle.Set(key, value)`は次の順。

1. property存在確認
2. backendからtarget typeを取得（ADOFAI adapterでは`propertiesInfo.value_default`を現在値より優先）
3. invariant cultureで値を変換
4. propertyへ設定
5. `disabled[key] = false`

イベントは`Create`時点で追加されるため、一連の生成はTransaction内で行う。途中の`Set`が失敗してもTransaction全体をrollbackできる。

## Transaction保証

### Commit成功

- 編集結果を保持
- 指定されたrefresh処理を実行
- `RestoreSelection`指定時はCommit直前のselectionを維持

### Commit前にDispose

- `CloneLevel()`で保存したsnapshotへ復元
- Transaction開始時のselectionへ復元
- editorをrefresh

### Commit refresh失敗

- snapshotへ復元
- rollback refreshを試行
- 元のrefresh例外を`EditorTransactionException.InnerException`として返す
- rollback自体も失敗した場合は`Data["RollbackException"]`にも保存

### 制限

- nested transactionは非対応
- Unity main threadでの利用を前提とする
- snapshotのdeep copy保証はbackend側の責任

## Refresh flags

`EditorRefreshOptions`は以下を分離する。

- `RemakePath`
- `ApplyEventsToFloors`
- `UpdateDecorationObjects`
- `RestoreSelection`

実際の呼出順はADOFAI adapter側で固定する。Coreは順序を仮定しない。

## まだCoreに入れないもの

- Undo history統合
- 複数LevelData workspace
- Editor Group / split panes
- Harmony patch管理
- logger / settings / updater
- `.adofai` parse / export

これらはEvent APIとTransactionの安定後に必要性を再評価する。
