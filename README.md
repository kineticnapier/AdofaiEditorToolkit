# ADOFAI.EditorToolkit

実行中のADOFAI stock editorをModから安全に操作するための小さなC#ライブラリです。

`.adofai`ファイルのparserではありません。`scnEditor`、`LevelData`、`LevelEvent`を直接扱うときに繰り返し必要になる処理をまとめます。

## v0.1の範囲

- `Editor.Current` / `Editor.Level` / `Editor.Selection`
- `Editor.Snapshot()` / `Editor.Restore(...)`
- `Editor.BeginTransaction()`
  - `Commit()`時にeditorをrefresh
  - 未Commitの`Dispose()`時にrollback
  - refresh失敗時にもrollback
- `Editor.Events.Create(...)`
- property metadataに基づく値の型変換
- property設定時の`disabled = false`
- eventのquery / remove
- metadataに基づくaction (`levelEvents`) / decoration (`decorations`) の自動振り分け
- `Vector2`、`Vector3`、`Color`をUnityEngine参照なしで渡すcomponent value
- ADOFAI固有部を隔離する`IEditorBackend` / `IEventBackend`
- ADOFAI 3.x向け`ADOFAIEditorBackend` / `ADOFAIEventBackend`

## 想定API

```csharp
using ADOFAI.EditorToolkit;

using (var tx = Editor.BeginTransaction())
{
    Editor.Events.Create("OrbitDecoration", floor: 10)
        .Set("duration", 1.0)
        .Set("ease", "Linear")
        .Set("position", EventValues.Vector2(2f, -1f))
        .Set("tag", "PlanetA");

    tx.Commit();
}
```

追加先は`LevelEventInfo.isDecoration`から自動判定します。

```csharp
Editor.Events.Create("AddObject", 10)
    .Set("objectType", "Floor");
```

metadataが特殊なModイベントでは、必要に応じて`EventCollection.Actions`または`EventCollection.Decorations`を明示して上書きできます。

`Create`はイベントをLevelDataへ直ちに追加します。複数propertyを設定する処理はTransaction内で行うことを推奨します。`EventCollection.Auto`はCreate専用で、Queryは`All`（既定）、`Actions`、`Decorations`を使います。

```csharp
var speeds = Editor.Events.Query("SetSpeed");
var removed = Editor.Events.Remove("Twirl", floor: 42);
```

## ADOFAIとの接続

Coreは`Assembly-CSharp.dll`を直接参照しません。ゲームバージョン固有のアクセスは、次の2 interfaceを実装するadapterへ置きます。`ADOFAI.EditorToolkit.ADOFAI` projectには、MultiTileEditor v0.10.6の実装と照合したADOFAI 3.x向けadapterが入っています。

```text
ADOFAI / PACL2
      ↓
IEditorBackend + IEventBackend
      ↓
ADOFAI.EditorToolkit
      ↓
MultiTileEditor / EditorQoL / other editor mods
```

ModのLoad時に一度だけ初期化します。

```csharp
using ADOFAI.EditorToolkit.Game;

ADOFAIEditorBackend.ConfigureToolkit();
```

別バージョンへ接続したい場合は、`DelegateEditorBackend`と`DelegateEventBackend`を使えば独立したadapter classを作らずに差し替えられます。接続例は[samples/ADOFAIAdapterSketch.cs](samples/ADOFAIAdapterSketch.cs)にあります。

## ビルド

必要環境は.NET SDK 8以降です。ライブラリ出力自体は`netstandard2.0`です。

```powershell
dotnet build .\src\ADOFAI.EditorToolkit\ADOFAI.EditorToolkit.csproj -c Release
dotnet run --project .\tests\ADOFAI.EditorToolkit.Tests\ADOFAI.EditorToolkit.Tests.csproj -c Release
```

ADOFAI adapterは`.NET Framework 4.8` projectです。Visual StudioのMSBuildで、ゲームのManaged directoryを指定してビルドします。

```powershell
msbuild .\src\ADOFAI.EditorToolkit.ADOFAI\ADOFAI.EditorToolkit.ADOFAI.csproj `
  /p:Configuration=Release `
  /p:GameManagedDir="C:\Program Files (x86)\Steam\steamapps\common\A Dance of Fire and Ice\A Dance of Fire and Ice_Data\Managed"
```

`artifacts/Release`には、提供されたADOFAI Managed DLL一式を参照してコンパイル確認したpreview DLLが入っています。

- `ADOFAI.EditorToolkit.dll`
- `ADOFAI.EditorToolkit.ADOFAI.dll`
- `ADOFAI.EditorToolkit.xml`

現時点ではGit repository化、NuGet package化、ライセンス確定は行っていません。

## 次の段階

1. ADOFAI 3.3.1上でadapterをビルド・ロードする
2. `RemakePath`、`ApplyEventsToFloors`、`UpdateDecorationObjects`の順序を実機検証する
3. 複数Floor selectionの保存・復元を実機検証する
4. PACL2 `OrbitDecoration`の即時再生を回帰テストする
5. MultiTileEditor内の重複Event APIを置き換える
