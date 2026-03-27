# Guide cho `RoiImageCanvas.cs`

## Vai trò
`RoiImageCanvas` là lõi xử lý đồ họa và tương tác:
- Render ảnh + ROI.
- Quản lý transform pan/zoom (`_scale`, `_offset`).
- Quản lý mode tương tác: `View`, `Create`, `Edit`.
- Phát event ra ngoài để control cha đồng bộ UI.

## Cấu trúc state quan trọng
- Ảnh và transform: `_image`, `_scale`, `_offset`.
- Manual measure: `_measuring`, `_measureStartImage`, `_measureEndImage`.
- ROI collection: `_rois`.
- Create state: `_creatingPoints`, `_creatingActive`, `_creatingMouseImage`.
- Edit state: `_selectedRoiIndex`, `_selectedHandleIndex`, `_movingRoi`, `_moveStartPoints`.

## Event contract (không nên phá)
- `ManualMeasureChanged`: cập nhật kết quả đo thủ công.
- `StatusChanged`: status text cho lớp cha hiển thị.
- `RoiCollectionChanged`: báo danh sách ROI thay đổi (add/delete/set/clear).
- `SelectedRoiChanged`: báo ROI được chọn thay đổi.

Nếu thêm hành vi mới làm đổi ROI hoặc selection, phải raise event tương ứng.

## Luồng input chính
- Mouse wheel: zoom theo con trỏ.
- Space + Left drag: pan (mọi mode).
- `Create` mode:
  - Left click: thêm điểm.
  - Right click: finalize ROI.
  - Ctrl: khóa phương ngang/dọc với điểm trước.
- `Edit` mode:
  - Click handle: kéo đỉnh.
  - Click trong polygon: kéo ROI.
  - Delete: xóa ROI được chọn.
- Phím:
  - `R`: reset view.
  - `Q`: clear manual measure.
  - `Esc` (Create): hủy create.

## Rủi ro thường gặp khi sửa
- Quên `Invalidate()` sau khi cập nhật state -> UI không refresh.
- Quên `RaiseRoiCollectionChanged()` khi add/remove/set ROI.
- Quên `RaiseSelectedRoiChanged()` khi select/deselect.
- Chạm vào `FitToWindow()`/`ScreenToImage()` dễ làm sai toàn bộ thao tác kéo thả.

## Khuyến nghị mở rộng
- Nếu thêm mode mới, tách xử lý theo pattern hiện tại: `Handle<Mode>MouseDown/Move/...`.
- Nếu thêm metadata ROI, giữ tương thích clone + JSON serialize.
- Duy trì hit-test ưu tiên ROI trên cùng (duyệt ngược danh sách).
