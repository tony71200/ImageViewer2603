# ROI Controls Skill (Previewer_2603/Controls)

## Mục tiêu
Tài liệu này hướng dẫn agent chỉnh sửa an toàn phần ROI viewer trong thư mục `Controls`, tránh phá vỡ các hành vi cốt lõi.

## Phạm vi source
- `RoiImageCanvas.cs`: engine tương tác ảnh + ROI (pan/zoom, create/edit, measure, hit-test).
- `RoiImageViewerControl.cs`: lớp `UserControl` bọc canvas, toolbar trạng thái, API public cho `MainForm`.
- `RoiImageViewerControl.Designer.cs`: wiring UI (checkbox create/edit, status strip, canvas host).

## Luồng đồng bộ bắt buộc
Khi thay đổi hành vi ROI, luôn kiểm tra đủ 3 tầng:
1. `RoiImageCanvas` (state + event)
2. `RoiImageViewerControl` (forward API/event + status)
3. `MainForm` (đồng bộ danh sách ROI ngoài `ListBox`)

> Chuỗi đồng bộ chuẩn: `RoiImageCanvas` -> `RoiImageViewerControl` -> `MainForm` (`ListBox`).

## Hành vi không được làm mất
- Pan/zoom hoạt động ổn định khi ảnh lớn.
- Tạo polygon ROI: thêm điểm, kết thúc đúng, hỗ trợ Ctrl lock ngang/dọc.
- Edit ROI: chọn ROI, kéo handle, kéo toàn ROI.
- Save/Load ROI JSON vẫn tương thích với `RoiPolygon`/`RoiPoint`.
- Status message phản ánh thao tác chính xác.

## Quy tắc sửa code trong Controls
- Ưu tiên patch nhỏ, tập trung, tránh refactor rộng.
- Nếu đổi layout/wiring UI, sửa trong `*.Designer.cs`; logic xử lý để ở file code-behind.
- Không đổi schema JSON (`DataContract`/`DataMember`) trừ khi có yêu cầu rõ ràng.
- Khi thêm input mới (phím tắt/chuột), cập nhật docs liên quan ở `README.md` (EN + 繁中).

## Checklist trước khi kết thúc
1. Build solution/project thành công (nếu tool có sẵn).
2. Tự kiểm tra nhanh các thao tác:
   - View mode: pan, zoom, reset.
   - Create mode: thêm điểm, kết thúc ROI.
   - Edit mode: chọn, kéo đỉnh, kéo ROI, xóa ROI.
   - Save/load JSON.
3. Đảm bảo status text không bị sai ngữ cảnh sau thao tác.
