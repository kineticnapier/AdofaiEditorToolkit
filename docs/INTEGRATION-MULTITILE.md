# MultiTileEditor integration plan

`ADOFAIMultiTileEditor` is the first real-world consumer for `ADOFAI.EditorToolkit`.

## Phase 1

Replace the duplicated event plumbing first. Keep MultiTile-specific planning/generation logic unchanged.

Targeted responsibilities:

- resolve `LevelEventInfo` by event name
- create `LevelEvent`
- coerce property values to the metadata/default-value type
- mark configured properties enabled (`disabled[key] = false`)
- route actions vs decorations
- query/remove generated events through the toolkit where practical

Do **not** move the following into the toolkit:

- timeline merge
- virtual repeat expansion
- orbit planning
- compact layout / teleport planning
- floor preview generation
- MultiTile tags and generated-object ownership rules

## Phase 2

Adopt `Editor.BeginTransaction()` around generation so a failed refresh restores the previous `LevelData` and selection.

Before enabling this in release builds, verify in ADOFAI 3.3.1:

1. `RemakePath`
2. `ApplyEventsToFloors`
3. `UpdateDecorationObjects`
4. selection restore
5. PACL2 `OrbitDecoration` immediate playback

## Dependency strategy

During the preview phase, keep `ADOFAI.EditorToolkit` as a separate repository and consume its built DLLs locally. Do not duplicate the toolkit source inside MultiTileEditor.

Once the API is stable, publish versioned release artifacts (and optionally a NuGet package) so MultiTileEditor can pin a toolkit version reproducibly.
