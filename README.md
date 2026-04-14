# Previewer_2603

## Multi-canvas Workspace (English)

### What changed
The main form now contains a **3-tab tag container** (`tagContainer`) so each MD-described component is available in one place:
1. **Image Canvas** tab: base `ImageCanvas` viewer (pan/zoom/manual measure).
2. **Alignment Canvas** tab: `AlignmentImageCanvas` for reference/test overlay and manual alignment.
3. **ROI Canvas** tab: existing `RoiImageViewerControl` + ROI `ListBox` synchronization.

### Components overview
- `ImageCanvas.cs`
  - Base image display control with pan/zoom, fit/reset, cursor coordinate event, and manual measurement event.
- `AlignmentImageCanvas.cs`
  - Inherits `ImageCanvas`, adds pivot/translate/rotate state and `AlignmentChanged` event.
- `RoiImageCanvas.cs` + `RoiImageViewerControl.cs`
  - Existing ROI create/edit/save/load flow, still synchronized with `MainForm` ROI list.

### Keyboard / mouse shortcuts
#### Image Canvas
- `Space + Left Drag`: pan.
- `Mouse Wheel`: zoom in/out at cursor.
- `Alt + Mouse Wheel`: vertical move.
- `Shift + Alt + Mouse Wheel`: horizontal move.
- `R`: reset view.
- `Q`: clear measurement line.
- `Right Drag` (`ManualMode=true`): manual measurement.

#### Alignment Canvas
- Enable **Manual Alignment Mode** checkbox.
- `Left Click`: set pivot (first click).
- `Drag pivot`: translate test image.
- `Drag orange arc`: rotate test image.
- `Right Click`: clear pivot (keeps current transform values).

#### ROI Canvas
- `Ctrl` (while creating polygon): constrain next segment horizontal/vertical.
- `Right-click` (Create mode): finalize polygon ROI.
- `Rectangle` mode (Create): left-drag to generate rectangle ROI.
- `Esc` (Create mode): cancel current ROI creation.
- `Delete` (Edit mode or ROI ListBox): delete selected ROI.

---

## 多畫布工作區（繁體中文）

### 變更內容
主畫面已改為 **3 個分頁的 tag 容器**（`tagContainer`），將文件描述的元件整合在同一視窗：
1. **Image Canvas**：基礎 `ImageCanvas`（平移/縮放/手動量測）。
2. **Alignment Canvas**：`AlignmentImageCanvas`（參考圖/測試圖疊合與手動對位）。
3. **ROI Canvas**：原有 `RoiImageViewerControl` + `ListBox` 同步。

### 元件說明
- `ImageCanvas.cs`
  - 提供基礎影像顯示、平移縮放、視圖重設、座標事件、手動量測事件。
- `AlignmentImageCanvas.cs`
  - 繼承 `ImageCanvas`，增加 pivot / 平移 / 旋轉與 `AlignmentChanged` 事件。
- `RoiImageCanvas.cs` + `RoiImageViewerControl.cs`
  - 保留原 ROI 建立/編輯/JSON 載入儲存流程，並維持與 `MainForm` ROI 清單同步。

### 快捷鍵與滑鼠操作
#### Image Canvas
- `Space + 滑鼠左鍵拖曳`：平移。
- `滑鼠滾輪`：以游標為中心縮放。
- `Alt + 滑鼠滾輪`：上下移動。
- `Shift + Alt + 滑鼠滾輪`：左右移動。
- `R`：重設視圖。
- `Q`：清除量測線。
- `滑鼠右鍵拖曳`（`ManualMode=true`）：手動量測。

#### Alignment Canvas
- 勾選 **Manual Alignment Mode**。
- `滑鼠左鍵點擊`：設定 pivot（第一次點擊）。
- `拖曳 pivot`：平移測試影像。
- `拖曳橘色圓弧`：旋轉測試影像。
- `滑鼠右鍵點擊`：清除 pivot（保留目前 transform 值）。

#### ROI Canvas
- `Ctrl`（建立多邊形時）：限制下一段為水平/垂直。
- `滑鼠右鍵`（Create 模式）：完成多邊形 ROI。
- `Rectangle`（Create 模式）：滑鼠左鍵拖曳建立矩形 ROI。
- `Esc`（Create 模式）：取消目前建立流程。
- `Delete`（Edit 模式或 ROI 清單）：刪除選取 ROI。
