# Previewer_2603

## ROI Image Viewer Control (English)

### Purpose of `RoiImageViewerControl.cs`
`RoiImageViewerControl` is a reusable WinForms user control that wraps:
- A toolbar-like mode selector (`Create`, `Edit`).
- A drawing canvas (`RoiImageCanvas`) for image display, pan/zoom, ROI creation/editing, and manual measurement.
- Status text updates for user feedback.

It acts as a higher-level API over the canvas, so forms like `MainForm` can load images, manage ROI data, and react to ROI selection/list changes without touching canvas internals.

### How to use
1. Add `RoiImageViewerControl` to your form.
2. Call `SetImage(Bitmap image, bool preserveView = true)` after the user selects an image file.
3. Subscribe to events if needed:
   - `RoiCollectionChanged`: update external ROI UI (for example, a `ListBox`).
   - `SelectedRoiChanged`: synchronize selected ROI between canvas and external UI.
   - `ManualMeasureChanged`: receive measurement data.
4. Use external controls (like `ListBox`) to call:
   - `SelectRoiByIndex(int index)` to select an ROI.
   - `DeleteRoiByIndex(int index)` to remove an ROI.
5. Use JSON helper methods when needed:
   - `SaveRoisToJson(...)`
   - `LoadRoisFromJson(...)`

### Tree List (API in `RoiImageViewerControl.cs`)
- `SetImage(Bitmap image, bool preserveView = true)`: Load/replace image in the viewer.
- `ResetView()`: Fit image to control and reset viewport.
- `GetRois()`: Get cloned read-only list of current ROIs.
- `SetRois(IEnumerable<RoiPolygon> rois)`: Replace all ROIs from external source.
- `ClearRois()`: Remove all ROIs.
- `SelectRoiByIndex(int index)`: Select ROI by index from external UI.
- `DeleteRoiByIndex(int index)`: Delete ROI by index from external UI.
- `SaveRoisToJson(string filePath)`: Save current ROIs to JSON file.
- `LoadRoisFromJson(string filePath)`: Load ROIs from JSON file.
- `Image` (property): Current loaded bitmap.
- `ManualMode` (property): Enable/disable manual measurement mode.
- `ManualScaleToFull` (property): Measurement scale factor.
- `SelectedRoiIndex` (property): Currently selected ROI index.
- `ManualMeasureChanged` (event): Emits measurement result.
- `RoiCollectionChanged` (event): Emits when ROI set changes (add/delete/set/clear).
- `SelectedRoiChanged` (event): Emits when selected ROI changes.

### Keyboard shortcuts
- `Ctrl` (while creating polygon): constrain next segment to horizontal/vertical.
- `Right-click` (Create mode): finalize polygon ROI.
- `Esc` (Create mode): cancel the current polygon creation.
- `Delete` (Edit mode): delete selected ROI in canvas.
- `Delete` (on external ROI ListBox in `MainForm`): delete selected ROI from list/canvas.
- `R`: reset view (fit image).
- `Q`: clear manual measurement line.
- `Space + Left Drag`: pan image.
- `Right Drag` (when `ManualMode=true`): manual measurement.

---

## ROI 影像檢視控制項（繁體中文）

### `RoiImageViewerControl.cs` 的目的
`RoiImageViewerControl` 是可重複使用的 WinForms 使用者控制項，整合了：
- 模式切換（`Create`、`Edit`）。
- 繪圖畫布（`RoiImageCanvas`），可進行影像顯示、縮放/平移、ROI 建立與編輯、手動量測。
- 狀態列訊息顯示。

此控制項提供比 `RoiImageCanvas` 更高階的操作介面，讓 `MainForm` 這類表單可以直接載入影像、同步 ROI 清單與選取狀態，而不需要耦合到畫布內部細節。

### 使用方式
1. 將 `RoiImageViewerControl` 放到表單上。
2. 使用者選圖後呼叫 `SetImage(Bitmap image, bool preserveView = true)`。
3. 依需求訂閱事件：
   - `RoiCollectionChanged`：更新外部 ROI 清單（例如 `ListBox`）。
   - `SelectedRoiChanged`：同步畫布與外部清單的選取項目。
   - `ManualMeasureChanged`：接收量測結果。
4. 外部控制項可呼叫：
   - `SelectRoiByIndex(int index)`：由清單選取 ROI。
   - `DeleteRoiByIndex(int index)`：由清單刪除 ROI。
5. 如需儲存/載入 ROI，可使用：
   - `SaveRoisToJson(...)`
   - `LoadRoisFromJson(...)`

### Tree List（`RoiImageViewerControl.cs` 可用函式）
- `SetImage(Bitmap image, bool preserveView = true)`：載入/替換影像。
- `ResetView()`：重設檢視並自動縮放符合控制項。
- `GetRois()`：取得目前 ROI（複本、唯讀）。
- `SetRois(IEnumerable<RoiPolygon> rois)`：以外部資料一次替換全部 ROI。
- `ClearRois()`：清空所有 ROI。
- `SelectRoiByIndex(int index)`：從外部 UI 依索引選取 ROI。
- `DeleteRoiByIndex(int index)`：從外部 UI 依索引刪除 ROI。
- `SaveRoisToJson(string filePath)`：將 ROI 儲存成 JSON。
- `LoadRoisFromJson(string filePath)`：從 JSON 載入 ROI。
- `Image`（屬性）：目前載入影像。
- `ManualMode`（屬性）：是否啟用手動量測模式。
- `ManualScaleToFull`（屬性）：量測比例係數。
- `SelectedRoiIndex`（屬性）：目前選取 ROI 索引。
- `ManualMeasureChanged`（事件）：量測結果事件。
- `RoiCollectionChanged`（事件）：ROI 集合改變事件（新增/刪除/設定/清空）。
- `SelectedRoiChanged`（事件）：選取 ROI 改變事件。

### 快捷鍵說明
- `Ctrl`（建立多邊形時）：限制下一條線段只能水平或垂直。
- `滑鼠右鍵`（Create 模式）：完成多邊形 ROI。
- `Esc`（Create 模式）：取消目前的建立流程。
- `Delete`（Edit 模式）：刪除目前選取 ROI。
- `Delete`（`MainForm` 外部 ROI 清單 ListBox）：刪除清單中選取 ROI（同步到畫布）。
- `R`：重設視圖（自動符合視窗）。
- `Q`：清除手動量測線。
- `Space + 滑鼠左鍵拖曳`：平移影像。
- `滑鼠右鍵拖曳`（`ManualMode=true`）：手動量測。
