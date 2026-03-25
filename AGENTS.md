# AGENTS.md

## Scope
This file applies to the whole repository.

## Working rules for this repo
- Keep WinForms UI element wiring/layout in `*.Designer.cs` unless logic-only change is required.
- Preserve existing ROI features when modifying code:
  - pan/zoom
  - polygon ROI create/edit
  - ROI selection + handle editing
  - ROI JSON save/load
  - status messaging
- When adding/removing ROI behaviors, ensure synchronization path remains valid:
  `RoiImageCanvas` -> `RoiImageViewerControl` -> `MainForm` (`ListBox`).
- Prefer minimal, focused patches and avoid broad refactors.

## Validation expectations
- Try to run a local build command.
- If build tools are missing, report the exact failed command and reason.

## Docs expectations
- Keep `README.md` bilingual (English + Traditional Chinese) for any behavior/API updates.
- Keep keyboard shortcuts documented when input handling changes.
