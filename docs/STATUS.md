# Status

## 0.1.0 scaffold

実装済み:

- Core public API
- backend abstraction
- fluent Event API
- metadata-driven value conversion
- enum / numeric / component conversion
- snapshot / restore
- transaction commit / rollback
- refresh failure rollback
- query / remove
- dependency-free console test harness
- MultiTileEditor v0.10.6の実コードと照合したADOFAI 3.x adapter
- `GCS.levelEventsInfo`のexact / normalized / Mod-prefix suffix解決
- `propertiesInfo.value_default`を優先するproperty型解決
- `levelEvents` / `decorations`両collection対応
- `LevelEventInfo.isDecoration`による追加先の自動判定
- 提供されたADOFAI Managed DLL一式のmetadataでadapterの参照型・memberを照合
- Coreとconsole test harnessのRelease build成功
- console test 7件成功
- 提供された`Assembly-CSharp.dll` / `RDTools.dll` / `UnityEngine.CoreModule.dll`に対するadapter compile成功

未検証:

- ADOFAI 3.3.1での実行
- PACL2 `OrbitDecoration`のmetadata解決
- `scnEditor` selectionの保存・復元
- refresh呼出順

提供DLLで確認済みの主なmember:

- `ADOBase.editor`
- `scnEditor.levelData` / `selectedFloors`
- `scnEditor.RemakePath` / `ApplyEventsToFloors` / `UpdateDecorationObjects`
- `LevelData.Copy` / `levelEvents` / `decorations`
- `LevelEventInfo.name` / `type` / `propertiesInfo` / `isDecoration`
- `LevelEvent` constructor / indexer / `disabled`
- `PropertyInfo.value_default`

実機検証に追加で必要な入力:

- 可能ならPACL2 sourceまたはDLL
