# Guide cho `RoiImageViewerControl.cs`

## Vai trò
`RoiImageViewerControl` là lớp bao ngoài `RoiImageCanvas`, cung cấp:
- Wiring UI create/edit toggle.
- Public API dùng bởi `MainForm`.
- Forward event từ canvas.
- Save/load ROI JSON qua `RoiJsonStorage`.

## API bề mặt chính
- Ảnh/view: `SetImage`, `SetImagePath`, `ResetView`, `Image`.
- ROI data: `GetRois`, `SetRois`, `ClearRois`.
- ROI selection/action: `SelectedRoiIndex`, `SelectRoiByIndex`, `DeleteRoiByIndex`.
- JSON: `SaveRoisToJson`, `LoadRoisFromJson`.
- Behavior: `ManualMode`, `ManualScaleToFull`.

## Event forward (giữ nguyên contract)
- `ManualMeasureChanged`
- `RoiCollectionChanged`
- `SelectedRoiChanged`

Control này không tự xử lý business lớn; chủ yếu bridge từ canvas ra `MainForm`.

## Quy tắc khi sửa
- Toggle create/edit phải loại trừ lẫn nhau (`chkCreate` vs `chkEdit`).
- Khi đổi mode, status message cần rõ và đúng ngữ cảnh.
- Không thay đổi signature public API nếu chưa cập nhật nơi gọi (`MainForm`).
- Nếu chỉnh JSON persistence, giữ tương thích với `RoiPolygon` data contract.

## Lỗi cần tránh
- Dùng `Directory.Exists` để kiểm tra đường dẫn file ảnh (nên kiểm tra file) có thể gây lỗi load ảnh.
- Không cập nhật status sau thao tác save/load hoặc mode switch khiến UX khó theo dõi.
- Thêm logic nặng vào control thay vì để canvas xử lý thao tác đồ họa.

## Checklist tích hợp với MainForm
1. `MainForm` nhận được `RoiCollectionChanged` để refresh `ListBox`.
2. `MainForm` nhận được `SelectedRoiChanged` để đồng bộ item chọn.
3. Chọn item từ `ListBox` gọi lại `SelectRoiByIndex` vào control.
4. Xóa item từ `ListBox` gọi `DeleteRoiByIndex` và danh sách tự cập nhật.
