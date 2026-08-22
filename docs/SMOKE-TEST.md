# ADOFAI 3.3.1 smoke test

Run these checks before replacing MultiTileEditor's existing event plumbing.

## Load

- Load both `ADOFAI.EditorToolkit.dll` and `ADOFAI.EditorToolkit.ADOFAI.dll` from a minimal UMM test mod.
- Call `ADOFAIEditorBackend.ConfigureToolkit()` once.
- Confirm `Editor.Current`, `Editor.Level`, and `Editor.Selection` are accessible while the stock editor is open.

## Stock events

Inside a transaction, create on a disposable copy of a chart:

1. `Twirl`
2. `SetSpeed`
3. `MoveDecorations`

Commit and verify that they appear in the editor and affect immediate playback without save/reload.

## Rollback

- Start a transaction.
- Create an event.
- Dispose without calling `Commit()`.
- Verify the original `LevelData` and selected floors are restored.

Then force refresh to fail and verify rollback still occurs.

## PACL2

With PACL2 loaded:

- Resolve `OrbitDecoration` by name.
- Create one with `duration`, `ease`, and tags through the fluent API.
- Verify immediate playback works before saving/reloading.

This specifically guards against the historical property-type mismatch where generated events only worked after save/reload.

## Multi-selection

- Select multiple floors in the stock editor.
- Snapshot.
- Modify/rebuild the level.
- Restore.
- Verify every selected floor is restored, not only the first one.
