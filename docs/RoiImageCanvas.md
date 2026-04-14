# RoiImageCanvas

## Mục tiêu

`RoiImageCanvas` là lớp kế thừa từ `ImageCanvas`, dùng để tạo, hiển thị, chỉnh sửa và quản lý ROI trực tiếp trên ảnh trong WinForms.

Thành phần này giải quyết các nhu cầu chính:

- hiển thị ảnh với pan/zoom
- tạo ROI polygon hoặc rectangle bằng thao tác chuột
- chọn ROI và chỉnh từng vertex
- kéo dịch toàn bộ ROI trong giới hạn ảnh
- phát event để UI ngoài đồng bộ danh sách ROI, selection, lưu/đọc JSON

Đây là một canvas tương tác mức nghiệp vụ, phù hợp cho:

- đánh dấu vùng kiểm tra
- tạo recipe vùng đo
- editor ROI cho vision/inspection
- review annotation trên ảnh

---

## Vai trò trong kiến trúc hiện tại

Quan hệ trong `Controls`:

- `ImageCanvas`
  - cung cấp nền tảng render ảnh, pan/zoom, chuyển đổi tọa độ, manual measure, overlay, brightness
- `RoiImageCanvas : ImageCanvas`
  - quản lý dữ liệu ROI
  - xử lý mode tạo/sửa ROI
  - render polygon/rectangle, label, handle
  - raise event thay đổi collection/selection
- `RoiImageViewerControl`
  - là lớp bọc UI dùng `RoiImageCanvas`
  - thêm checkbox Create/Edit, combobox loại ROI, status, save/load JSON

Nói ngắn gọn:

- `ImageCanvas` là base visual engine
- `RoiImageCanvas` là domain editor
- `RoiImageViewerControl` là host control để nhúng vào màn hình

---

## Mô hình dữ liệu ROI

### 1. `RoiPoint`

```csharp
[DataContract]
public sealed class RoiPoint
{
    [DataMember(Order = 0)] public float X { get; set; }
    [DataMember(Order = 1)] public float Y { get; set; }
}
```

Vai trò:

- đại diện một điểm trong image space
- tương thích serialization
- có helper chuyển đổi `PointF`

### 2. `RoiPolygon`

```csharp
[DataContract]
public class RoiPolygon
{
    [DataMember(Order = 0)] public string Id { get; set; }
    [DataMember(Order = 1)] public string Name { get; set; }
    [DataMember(Order = 2)] public List<RoiPoint> Points { get; set; }
    [DataMember(Order = 3)] public string ShapeType { get; set; }
}
```

Ý nghĩa:

- là model ROI tổng quát
- dùng cho cả polygon lẫn rectangle
- clone sâu để tránh lộ state nội bộ ra ngoài

### 3. `RoiRectangle`

```csharp
public sealed class RoiRectangle : RoiPolygon
```

Bản chất vẫn lưu bằng 4 điểm polygon.
Điều này rất hữu ích vì renderer và editor không cần tách logic vẽ riêng cho rectangle.

### 4. `RoiShapeKind`

```csharp
public enum RoiShapeKind { Polygon, Rectangle }
```

Dùng để:

- quyết định mode tạo ROI
- lưu `ShapeType`
- đồng bộ UI combobox

---

## Chức năng hiện có

## 1. Quản lý mode tương tác

```csharp
public enum InteractionMode { View, Create, Edit }
```

- `View`: chỉ xem ảnh, vẫn dùng được pan/zoom/manual measure từ base
- `Create`: tạo ROI mới
- `Edit`: chọn, kéo handle, kéo cả ROI, xóa ROI

State chính:

- `Mode`
- `CreateShape`

### `CreateShape`
```csharp
public RoiShapeKind CreateShape { get; set; } = RoiShapeKind.Polygon;
```

Cho phép tạo:
- polygon nhiều điểm
- rectangle kéo-thả

---

## 2. Quản lý danh sách ROI

Danh sách nội bộ:

```csharp
private readonly List<RoiPolygon> _rois = new List<RoiPolygon>();
```

API công khai:

```csharp
public IReadOnlyList<RoiPolygon> GetRois()
public void SetRois(IEnumerable<RoiPolygon> rois)
public void ClearRois()
public int SelectedRoiIndex => _selectedRoiIndex;
public bool SelectRoiByIndex(int index)
public bool DeleteRoiByIndex(int index)
```

Điểm rất đúng trong thiết kế hiện tại:
`GetRois()` và `SetRois(...)` đều clone dữ liệu.
Điều này ngăn UI ngoài sửa trực tiếp state nội bộ của canvas.

---

## 3. Tạo polygon ROI

Trong mode `Create` và `CreateShape == Polygon`:

- click trái trong ảnh: thêm điểm
- di chuột: hiện đoạn preview từ điểm cuối tới chuột
- nếu giữ `Ctrl`: khóa ngang/dọc theo điểm trước
- click phải: finalize ROI nếu có ít nhất 3 điểm
- `Esc`: hủy tạo

State dùng trong quá trình tạo:

- `_creatingPoints`
- `_creatingActive`
- `_creatingMouseImage`

### Cơ chế khóa ngang/dọc

`ConstrainCreatePoint(...)` kiểm tra `ModifierKeys & Keys.Control`:

- nếu delta X lớn hơn delta Y -> giữ nguyên Y của điểm trước
- ngược lại -> giữ nguyên X

Rất phù hợp khi user cần tạo ROI thẳng hàng nhanh.

---

## 4. Tạo rectangle ROI

Trong mode `Create` và `CreateShape == Rectangle`:

- nhấn chuột trái: lưu điểm bắt đầu
- kéo chuột: cập nhật góc đối diện
- nhả chuột trái: finalize rectangle

State tương ứng:

- `_creatingRectStartImage`
- `_creatingRectCurrentImage`
- `_creatingActive`

Rectangle được build bằng:

```csharp
private static PointF[] BuildRectanglePoints(PointF p1, PointF p2)
```

Kết quả luôn là 4 điểm:
- left-top
- right-top
- right-bottom
- left-bottom

Thiết kế này tốt vì:

- rectangle vẫn đi cùng một pipeline render/edit như polygon
- không cần lớp renderer riêng

---

## 5. Vẽ ROI

Render được đặt trong `DrawAfterImage(Graphics g)`:

```csharp
protected override void DrawAfterImage(Graphics g)
{
    DrawRois(g);
    DrawCreating(g);
}
```

### `DrawRois(g)` làm gì

- duyệt toàn bộ `_rois`
- vẽ polygon/line
- ROI được chọn dùng pen khác màu
- fill nhẹ ROI đang chọn
- vẽ label `ROI {index + 1}`
- nếu ROI đang được chọn -> vẽ handle tại các đỉnh

Style hiện tại:

- ROI thường: LimeGreen
- ROI được chọn: Gold
- handle: OrangeRed
- fill selected: vàng trong suốt

### `DrawCreating(g)` làm gì

- vẽ preview trong lúc tạo ROI
- polygon: vẽ các điểm hiện có, line preview, closing line
- rectangle: vẽ 4 cạnh preview

Đây là điểm rất quan trọng để UX tạo ROI mượt và dễ đoán.

---

## 6. Chọn và sửa ROI

Trong mode `Edit`:

- click vào handle của ROI đang chọn -> kéo sửa đỉnh
- click vào bên trong ROI -> chọn ROI đó và kéo dịch cả ROI
- click ra ngoài -> bỏ chọn
- nhấn `Delete` -> xóa ROI đang chọn

State edit:

- `_selectedRoiIndex`
- `_selectedHandleIndex`
- `_movingRoi`
- `_moveStartImage`
- `_moveStartPoints`

### Sửa từng đỉnh

`HandleEditMouseMove(...)` khi `_selectedHandleIndex >= 0`:

- lấy điểm chuột trong image space
- clamp trong ảnh
- thay thế vertex tương ứng

### Kéo cả ROI

Khi click bên trong ROI:

- lưu snapshot `_moveStartPoints`
- lưu `_moveStartImage`

Trong lúc kéo:

- tính `requestedDx`, `requestedDy`
- lấy bounds của ROI lúc bắt đầu
- clamp delta để toàn bộ ROI không đi ra ngoài biên ảnh
- cập nhật toàn bộ điểm từ snapshot gốc + delta

Điểm này rất tốt:
ROI được dịch dựa trên snapshot lúc bắt đầu drag, nên không bị tích lũy sai số do drag nhiều frame.

---

## 7. Hit-test ROI và handle

### Hit-test ROI

```csharp
private int HitTestRoi(PointF img)
```

- duyệt từ cuối danh sách về đầu
- dùng `GraphicsPath.AddPolygon(...)`
- `path.IsVisible(img)` để kiểm tra click nằm trong ROI nào

Việc duyệt ngược giúp ROI vẽ sau có ưu tiên click cao hơn.

### Hit-test handle

```csharp
private int HitTestHandle(int roiIndex, PointF img)
```

- tính khoảng cách từ chuột tới từng vertex
- radius hit phụ thuộc `SafeScale()`

Điều này giữ cho vùng bắt handle hợp lý khi zoom.

---

## 8. Event contract

```csharp
public event EventHandler RoiCollectionChanged;
public event EventHandler SelectedRoiChanged;
```

Event được raise khi:

- set/clear ROI
- finalize create
- delete ROI
- select/deselect ROI

UI ngoài dùng để:

- refresh ListBox ROI
- đồng bộ item đang chọn
- lưu trạng thái dirty
- bật/tắt action button

Đây là contract quan trọng cần giữ khi port sang project khác.

---

## API chính

### Public properties

```csharp
public InteractionMode Mode { get; set; } = InteractionMode.View;
public RoiShapeKind CreateShape { get; set; } = RoiShapeKind.Polygon;
public int SelectedRoiIndex => _selectedRoiIndex;
```

### Public methods

```csharp
public IReadOnlyList<RoiPolygon> GetRois()
public void SetRois(IEnumerable<RoiPolygon> rois)
public void ClearRois()
public bool SelectRoiByIndex(int index)
public bool DeleteRoiByIndex(int index)
```

### Internal interaction helpers đáng chú ý

- `HandleCreateMouseDown`
- `HandleCreateMouseMove`
- `FinalizeCreate`
- `FinalizeCreateRectangle`
- `HandleEditMouseDown`
- `HandleEditMouseMove`
- `HitTestRoi`
- `HitTestHandle`

---

## Luồng input chi tiết

## 1. OnMouseDown

```csharp
protected override void OnMouseDown(MouseEventArgs e)
```

Thứ tự xử lý hiện tại:

- nếu không có ảnh -> return
- nếu đang `ManualMode` và click phải khi `Create` -> finalize ROI
- nếu đang `Create` và click phải -> finalize ROI
- nếu click trái:
  - `Create` -> `HandleCreateMouseDown(e)`
  - `Edit` -> `HandleEditMouseDown(e)`
- nếu không rơi vào trường hợp trên -> gọi `base.OnMouseDown(e)`

Ý nghĩa:
`RoiImageCanvas` ưu tiên xử lý hành vi ROI trước, rồi mới rơi về pan/measure của base.

## 2. OnMouseMove

- nếu `Create` -> cập nhật preview tạo
- nếu `Edit` -> cập nhật drag handle hoặc drag ROI
- sau đó gọi `base.OnMouseMove(e)` để vẫn có:
  - cập nhật tọa độ ảnh
  - pan
  - manual measure

## 3. OnMouseUp

- nếu đang tạo rectangle và thả trái -> finalize rectangle
- nếu đang edit -> reset state drag
- gọi `base.OnMouseUp(e)`

## 4. OnKeyDown

- `Esc` trong `Create` -> `CancelCreate()`
- `Delete` trong `Edit` -> `DeleteSelectedRoi()`

---

## Nền tảng code cần giữ khi port sang dự án khác

## 1. Tách base canvas và ROI editor

Đây là kiến trúc nên giữ nguyên:

```text
ImageCanvasBase
 ├─ zoom/pan
 ├─ screen <-> image coordinate
 ├─ base rendering
 └─ generic events

RoiCanvas : ImageCanvasBase
 ├─ ROI state
 ├─ create/edit interaction
 ├─ ROI rendering
 └─ ROI events
```

Nếu trộn hết vào một `UserControl` hoặc `Form`, Agent sẽ rất khó tái sử dụng.

## 2. Mọi ROI phải lưu trong image space

Không lưu ROI theo screen pixel.
Mọi điểm đều theo tọa độ ảnh gốc.
Base class chịu trách nhiệm transform lúc vẽ.

Lợi ích:

- zoom/pan không làm sai dữ liệu
- dễ serialize
- dễ dùng cho vision algorithm

## 3. Dữ liệu công khai phải clone

Pattern hiện tại rất tốt:

- `GetRois()` trả clone
- `SetRois(...)` nhận vào rồi clone

Đây là điều bắt buộc nên giữ khi muốn component ổn định ở dự án khác.

## 4. Tạo rectangle bằng polygon 4 điểm

Đây là quyết định thiết kế tốt.
Nó giúp:

- render thống nhất
- hit-test thống nhất
- chỉnh sửa vertex thống nhất
- serialization đơn giản

## 5. Chia luồng interaction theo mode

Pattern đúng là:

- `HandleCreate...`
- `HandleEdit...`
- renderer riêng
- helper hit-test riêng

Agent nên tái tạo y hệt cách tách này.

---

## Mẫu code nền tảng để Agent tái dựng

## 1. Khung class

```csharp
public sealed class RoiCanvas : ImageCanvasBase
{
    private readonly List<RoiPolygon> _rois = new List<RoiPolygon>();
    private readonly List<PointF> _creatingPoints = new List<PointF>();

    private bool _creatingActive;
    private PointF? _creatingMouseImage;
    private PointF? _creatingRectStartImage;
    private PointF? _creatingRectCurrentImage;

    private int _selectedRoiIndex = -1;
    private int _selectedHandleIndex = -1;
    private bool _movingRoi;
    private PointF _moveStartImage;
    private List<PointF> _moveStartPoints;

    public InteractionMode Mode { get; set; }
    public RoiShapeKind CreateShape { get; set; }

    public event EventHandler RoiCollectionChanged;
    public event EventHandler SelectedRoiChanged;
}
```

## 2. Extension point render

```csharp
protected override void DrawAfterImage(Graphics g)
{
    DrawRois(g);
    DrawCreating(g);
}
```

## 3. Tạo rectangle bằng 4 điểm

```csharp
private static PointF[] BuildRectanglePoints(PointF p1, PointF p2)
{
    var left = Math.Min(p1.X, p2.X);
    var right = Math.Max(p1.X, p2.X);
    var top = Math.Min(p1.Y, p2.Y);
    var bottom = Math.Max(p1.Y, p2.Y);

    return new[]
    {
        new PointF(left, top),
        new PointF(right, top),
        new PointF(right, bottom),
        new PointF(left, bottom)
    };
}
```

## 4. Move ROI theo snapshot

```csharp
var requestedDx = img.X - _moveStartImage.X;
var requestedDy = img.Y - _moveStartImage.Y;

for (var i = 0; i < roi.Points.Count; i++)
{
    var p0 = _moveStartPoints[i];
    roi.Points[i] = RoiPoint.From(new PointF(p0.X + dx, p0.Y + dy));
}
```

---

## Điểm mạnh của thiết kế hiện tại

- kế thừa tốt từ `ImageCanvas`
- mode tạo/sửa tách rõ ràng
- dữ liệu ROI đơn giản, dễ serialize
- rectangle và polygon dùng chung pipeline
- hit-test hợp lý
- drag ROI có clamp trong ảnh
- dễ tích hợp với `ListBox`, `PropertyGrid`, JSON storage

---

## Giới hạn và rủi ro hiện tại

## 1. Event chưa raise khi kéo vertex hoặc kéo ROI
Trong code hiện tại:

- khi sửa đỉnh
- khi kéo ROI

control chỉ `Invalidate()` nhưng không `RaiseRoiCollectionChanged()`.

Điều này có thể làm UI ngoài không biết dữ liệu ROI đã đổi cho tới khi thao tác khác xảy ra.
Nếu dự án mới cần đồng bộ real-time, đây là điểm nên sửa đầu tiên.

## 2. Label đang dùng chỉ số, không dùng `Name`
`DrawRois(...)` đang vẽ:

```csharp
g.DrawString($"ROI {i + 1}", ...)
```

Nếu `Name` có ý nghĩa nghiệp vụ, nên cân nhắc hiển thị `roi.Name`.

## 3. `HitTestRoi` bỏ qua ROI dưới 3 điểm
Phù hợp với polygon kín, nhưng nếu sau này hỗ trợ line/polyline thì phải mở rộng.

## 4. Rectangle sau khi tạo có thể bị sửa thành polygon bất kỳ
Vì rectangle chỉ là 4 điểm bình thường, khi kéo vertex nó không còn đảm bảo vuông/chữ nhật.
Điều này có thể đúng hoặc sai tùy nghiệp vụ.
Trong dự án mới cần quyết định rõ.

## 5. Chưa có undo/redo
Đây là nhu cầu rất thường gặp với editor ROI.

## 6. Chưa có metadata ROI
Hiện chỉ có:

- `Id`
- `Name`
- `ShapeType`
- `Points`

Chưa có:
- category
- enabled
- color
- score
- tags
- recipe params

---

## Hướng phát triển khuyến nghị

## 1. Chuẩn hóa ROI model

Có thể mở rộng:

```csharp
public class RoiPolygon
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ShapeType { get; set; }
    public bool Enabled { get; set; } = true;
    public string ColorHex { get; set; } = "#00FF00";
    public Dictionary<string, string> Metadata { get; set; }
    public List<RoiPoint> Points { get; set; }
}
```

## 2. Raise event khi chỉnh sửa trực tiếp
Nên raise `RoiCollectionChanged` khi:

- drag vertex
- drag ROI
- rename ROI
- đổi shape metadata

Ví dụ:

```csharp
if (_selectedHandleIndex >= 0)
{
    roi.Points[_selectedHandleIndex] = RoiPoint.From(ClampPointToImage(img));
    RaiseRoiCollectionChanged();
    Invalidate();
}
```

## 3. Hỗ trợ undo/redo
Tạo stack snapshot:

```csharp
public sealed class RoiSnapshot
{
    public List<RoiPolygon> Items { get; set; }
    public int SelectedIndex { get; set; }
}
```

Các hành động cần push snapshot:

- finalize create
- delete
- move ROI
- move handle

## 4. Khóa rectangle thật sự
Nếu shape là rectangle và muốn giữ trực giao, nên có editor riêng:

- drag một góc -> tự tính lại các góc còn lại
- hoặc lưu rectangle bằng `RectF + rotation`

## 5. Thêm multi-select
Cho project lớn hơn có thể thêm:

- chọn nhiều ROI
- group move
- copy/paste ROI

## 6. Thêm policy snap
Ví dụ:
- snap vào grid
- snap vào cạnh ảnh
- snap theo góc 45 độ
- snap vertex gần nhau

## 7. Thêm renderer config
Tách style khỏi logic:

```csharp
public sealed class RoiRenderOptions
{
    public Color NormalColor { get; set; }
    public Color SelectedColor { get; set; }
    public Color HandleColor { get; set; }
    public bool ShowLabels { get; set; }
    public bool FillSelected { get; set; }
}
```

---

## Mẫu code phát triển đề xuất

## 1. Event khi data ROI thay đổi lúc edit

```csharp
private void UpdateSelectedHandle(PointF imagePoint)
{
    if (_selectedRoiIndex < 0 || _selectedHandleIndex < 0) return;

    var roi = _rois[_selectedRoiIndex];
    roi.Points[_selectedHandleIndex] = RoiPoint.From(ClampPointToImage(imagePoint));

    RaiseRoiCollectionChanged();
    Invalidate();
}
```

## 2. Export/import DTO

```csharp
public sealed class RoiDocument
{
    public List<RoiPolygon> Items { get; set; } = new List<RoiPolygon>();
}

public RoiDocument ExportDocument()
{
    return new RoiDocument
    {
        Items = _rois.Select(r => r.Clone()).ToList()
    };
}

public void ImportDocument(RoiDocument doc)
{
    SetRois(doc?.Items);
}
```

## 3. Đổi label theo tên ROI

```csharp
var title = string.IsNullOrWhiteSpace(_rois[i].Name)
    ? $"ROI {i + 1}"
    : _rois[i].Name;

g.DrawString(title, font, fontBrush, new PointF(pts[0].X - 15, pts[0].Y - 10));
```

## 4. Reset editor state rõ ràng

```csharp
public void ResetEditorState()
{
    CancelCreate();
    Deselect();
    Invalidate();
}
```

---

## Tích hợp với `RoiImageViewerControl`

`RoiImageViewerControl` đang làm đúng vai trò host:

- bật/tắt mode Create/Edit bằng checkbox
- đổi loại shape bằng combobox
- forward event từ canvas
- save/load JSON
- set status text

Khi mang sang dự án khác, nên giữ pattern:

- `RoiImageCanvas` chỉ lo data + render + interaction
- `RoiImageViewerControl` hoặc `Form` lo UI widget xung quanh

---

## Checklist khi Agent xây component này ở dự án mới

- tạo base image canvas độc lập với form
- lưu ROI trong image coordinate
- tách mode `View/Create/Edit`
- rectangle vẫn dùng 4 điểm polygon
- dùng clone cho API ra/vào
- có hit-test ROI và handle riêng
- drag ROI phải clamp theo biên ảnh
- raise event đầy đủ khi collection hoặc selection đổi
- nên bổ sung event cả khi vertex/ROI đang bị chỉnh
- host UI ngoài mới lo toolbar, list, save/load

---

## Kết luận

`RoiImageCanvas` là một ROI editor nền tảng khá tốt để tái sử dụng ở project khác.
Giá trị lớn nhất của nó là kiến trúc tách lớp hợp lý:

- `ImageCanvas` lo engine hiển thị
- `RoiImageCanvas` lo editor ROI
- `RoiImageViewerControl` lo host UI

Nếu Agent cần dựng lại thành phần này trong dự án mới, nên giữ nguyên các nguyên tắc:

- ROI luôn ở image space
- interaction theo mode
- render qua `DrawAfterImage(...)`
- API công khai trả clone
- host UI không can thiệp sâu vào state canvas

Và nên nâng cấp thêm:

- event khi edit real-time
- undo/redo
- metadata ROI
- rectangle constraint thật sự
- renderer options tách riêng
