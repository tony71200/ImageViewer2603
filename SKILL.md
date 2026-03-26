# ImageViewer2603 Development Skill

## Goal
Provide a compact workflow for editing and validating the WinForms ROI viewer in this repository.

## Project map
- `Previewer_2603/MainForm.*`: host form, file-open flow, external ROI list UI.
- `Previewer_2603/Controls/RoiImageViewerControl.*`: reusable UserControl shell (top/create-edit, middle canvas, bottom status strip).
- `Previewer_2603/Controls/RoiImageCanvas.cs`: drawing engine (pan/zoom, create/edit polygon ROI, events, keyboard/mouse behavior).

## Preferred workflow
1. Read `MainForm.cs`, `RoiImageViewerControl.cs`, `RoiImageCanvas.cs` before patching.
2. Keep visual-element layout changes in `*.Designer.cs` files whenever possible.
3. For ROI behavior changes, update canvas first, then forward APIs/events from viewer control, then connect in form.
4. Update `README.md` whenever shortcuts/API surface changes.
5. Run available build checks; if unavailable, report clearly.

## ROI behavior checklist
- Create polygon ROI with click + double-click finalize.
- Ctrl-constrained segment orientation while creating.
- Edit mode supports selection, vertex handles, drag handle, drag whole ROI.
- External list can select/delete ROI and stays synchronized with canvas.
- Save/load ROI JSON remains functional.

## Documentation checklist
- English + Traditional Chinese sections should stay in sync.
- Include purpose, usage steps, API tree list, and keyboard shortcuts.
