# AlignmentImageCanvas

## Mục tiêu

`AlignmentImageCanvas` là lớp kế thừa từ `ImageCanvas`, dùng để hiển thị chồng hai ảnh và cho phép căn chỉnh thủ công ảnh test lên ảnh reference bằng phép biến đổi hình học đơn giản:

- tịnh tiến theo X/Y
- xoay theo một pivot
- xem trực quan trạng thái chồng ảnh bằng tint màu khác nhau
- phát event để lớp ngoài đồng bộ UI hoặc lấy tham số transform

Thành phần này phù hợp cho các bài toán:

- so sánh vị trí giữa ảnh chuẩn và ảnh kiểm tra
- tinh chỉnh thủ công trước khi chạy thuật toán matching/alignment tự động
- tạo UI review để operator kiểm tra sai lệch vị trí/góc quay
- xuất ra ảnh test đã được warp theo transform hiện tại

---

## Vai trò trong kiến trúc hiện tại

Trong `Controls`, quan hệ của lớp này là:

- `ImageCanvas`
  - giữ logic nền: render ảnh, pan/zoom, convert tọa độ, manual measurement, auto brightness, overlay mask
- `AlignmentImageCanvas : ImageCanvas`
  - bổ sung mô hình 2 ảnh: reference + test
  - thêm transform state
  - thêm gizmo tương tác cho alignment
  - expose event `AlignmentChanged`

Ý nghĩa thực tế:

- `ImageCanvas` là reusable base canvas
- `AlignmentImageCanvas` là specialization cho bài toán alignment thủ công
- lớp này không quản lý toolbar/form; UI ngoài sẽ gọi API của canvas và lắng nghe event

---

## Chức năng hiện có

### 1. Quản lý 2 lớp ảnh

Có 4 bitmap nội bộ:

- `_referenceImage`: ảnh chuẩn gốc
- `_testImage`: ảnh kiểm tra gốc
- `_referenceTinted`: bản reference đã tint màu
- `_testTinted`: bản test đã tint màu

Mục đích của tinted cache:

- giúp người dùng nhìn thấy hai lớp ảnh chồng nhau dễ hơn
- reference và test được tô màu khác nhau để nhận ra sai lệch ngay trên canvas
- tránh phải tint lại ở mỗi lần vẽ

Màu hiện tại:

- reference: thiên xanh lam/xanh nhạt
- test: thiên vàng/ấm và alpha thấp hơn

### 2. Hiển thị reference làm nền

Khi gọi `SetReferenceImage(...)`:

- dispose reference cũ
- clone ảnh đầu vào
- tạo tinted cache cho reference
- gọi `SetImage(...)` của base class để lấy reference tinted làm ảnh nền hiển thị

Điểm quan trọng:
`AlignmentImageCanvas` không tự vẽ cả hai ảnh từ đầu. Nó tận dụng `ImageCanvas.Image` làm lớp nền, rồi override `DrawAfterImage(...)` để vẽ lớp test đã transform lên trên.

### 3. Hiển thị test image có transform

`DrawAfterImage(Graphics g)` gọi:

- `base.DrawAfterImage(g)`
- `DrawTransformedImage(g, _testTinted ?? _testImage)`
- `DrawAlignGizmo(g)`

Điều này có nghĩa:

- ảnh nền là reference
- ảnh test được vẽ sau, đã áp transform
- gizmo được vẽ cuối cùng để hiển thị trạng thái tương tác

### 4. Alignment thủ công

Lớp hỗ trợ một mode riêng:

```csharp
public enum AlignMode { View, ManualAlignment }
```

- `View`: chỉ xem
- `ManualAlignment`: cho phép đặt pivot và kéo gizmo để dịch/chỉnh góc

State alignment hiện có:

- `TranslateX`
- `TranslateY`
- `RotationDeg`
- `_pivot`
- `_transformPivot`

Ý nghĩa:

- `_pivot`: pivot mà user đặt và đang dùng cho tương tác
- `_transformPivot`: pivot dùng để áp phép biến đổi khi vẽ
- khi user gọi `SetPivot(...)`, cả hai giá trị được đồng bộ

### 5. Đặt pivot

Luồng tương tác:

- nếu đang ở `ManualAlignment`
- click trái lần đầu trên ảnh
- nếu chưa có pivot thì điểm click sẽ trở thành pivot

Khi đó:

- `_pivot` được gán
- `_transformPivot` được gán
- control `Invalidate()`
- phát `AlignmentChanged`

### 6. Kéo pivot để translate

Sau khi đã có pivot:

- click gần tâm pivot sẽ vào trạng thái `_dragPivot = true`
- kéo chuột sẽ cộng delta ảnh vào `TranslateX`, `TranslateY`

Chi tiết quan trọng:
delta được tính trong image space, không phải screen space:

```csharp
TranslateX += img.X - _lastDragImage.X;
TranslateY += img.Y - _lastDragImage.Y;
```

Cách làm này giúp transform ổn định dù đang zoom.

### 7. Kéo cung tròn để rotate

Nếu click không trúng pivot nhưng nằm gần cung tròn gizmo:

- `_dragRotate = true`

Khi kéo:

- tính góc từ pivot tới điểm chuột trước đó
- tính góc tới vị trí chuột mới
- lấy hiệu góc rồi cộng vào `RotationDeg`

Đây là cách rotate trực quan, phù hợp với editor/tooling UI.

### 8. Reset pivot bằng chuột phải

Trong `ManualAlignment`, click phải:

- hủy trạng thái drag
- xóa `_pivot`
- trả cursor về mặc định
- invalidate
- raise event

Lưu ý:
code hiện tại chỉ reset `_pivot`, không reset `TranslateX`, `TranslateY`, `RotationDeg`, và cũng không xóa `_transformPivot`.
Vì vậy transform đang có vẫn còn hiệu lực. Đây là hành vi cần được ghi rõ khi tái sử dụng.

### 9. Xuất ảnh test đã căn chỉnh

`GetAlignedTestImage()` tạo bitmap output mới và vẽ `_testImage` qua `DrawTransformedImage(...)`.

Đây là API quan trọng nếu muốn:

- lưu kết quả preview
- chuyển ảnh đã align sang pipeline khác
- debug kết quả alignment thủ công

Kích thước output:

- ưu tiên theo kích thước `_referenceImage`
- nếu không có reference thì dùng kích thước `_testImage`

### 10. Phát event đồng bộ UI

```csharp
public event EventHandler<AlignmentChangedEventArgs> AlignmentChanged;
```

Payload gồm:

- `HasPivot`
- `Pivot`
- `OffsetX`
- `OffsetY`
- `AngleDeg`

UI ngoài có thể dùng event này để:

- cập nhật textbox/numeric control
- lưu transform
- hiển thị thông số debug
- đồng bộ với hệ thống recipe/job

---

## API chính

### Public properties

```csharp
public AlignMode Mode { get; set; } = AlignMode.View;
public float TranslateX { get; private set; }
public float TranslateY { get; private set; }
public float RotationDeg { get; private set; }
public bool HasPivot => _pivot.HasValue;
public PointF Pivot => _pivot ?? _transformPivot ?? PointF.Empty;
```

### Public methods

```csharp
public void SetReferenceImage(Bitmap image)
public void SetTestImage(Bitmap image)
public void SetPivot(float pivotX, float pivotY)
public void SetTransform(float offsetX, float offsetY, float angleDeg)
public Bitmap GetAlignedTestImage()
```

### Event args

```csharp
public sealed class AlignmentChangedEventArgs : EventArgs
{
    public bool HasPivot { get; }
    public PointF Pivot { get; }
    public float OffsetX { get; }
    public float OffsetY { get; }
    public float AngleDeg { get; }
}
```

---

## Luồng render

### Lớp nền từ `ImageCanvas`

Render của base class đã làm:

1. clear nền
2. áp `_offset`, `_scale`
3. vẽ `Image`
4. vẽ overlay mask
5. vẽ manual measure
6. gọi `DrawAfterImage(...)`

### Phần mở rộng của `AlignmentImageCanvas`

`DrawAfterImage(...)` dùng đúng extension point của base:

```csharp
protected override void DrawAfterImage(Graphics g)
{
    base.DrawAfterImage(g);
    DrawTransformedImage(g, _testTinted ?? _testImage);
    DrawAlignGizmo(g);
}
```

Đây là pattern quan trọng để Agent tái sử dụng trong dự án mới:

- base canvas giữ transform màn hình
- derived class thêm layer chuyên biệt sau khi ảnh nền đã được vẽ

---

## Cơ chế transform

### Áp transform khi vẽ

```csharp
private void ApplyTransform(Graphics g)
{
    if (!_transformPivot.HasValue) return;
    g.TranslateTransform(_transformPivot.Value.X + TranslateX, _transformPivot.Value.Y + TranslateY);
    g.RotateTransform(RotationDeg);
    g.TranslateTransform(-_transformPivot.Value.X, -_transformPivot.Value.Y);
}
```

Ý nghĩa:

- dịch gốc tọa độ tới pivot + translation
- xoay quanh điểm đó
- dịch ngược lại

### Transform một điểm

```csharp
private PointF TransformPoint(PointF point)
```

Hàm này dùng để:

- biến đổi pivot sang vị trí hiện tại trên ảnh
- tính endpoint của trục X/Y của gizmo
- xác định vùng hit-test cho pivot và arc

---

## Gizmo tương tác

`DrawAlignGizmo(...)` vẽ 3 phần:

- trục X màu đỏ
- trục Y màu xanh
- cung tròn 90 độ để biểu diễn vùng rotate
- điểm pivot màu vàng

Kích thước gizmo không cố định theo pixel màn hình mà phụ thuộc vào:

- kích thước ảnh
- `SafeScale()`

Điều này giúp gizmo:

- nhìn được khi zoom lớn/nhỏ
- không quá bé trên ảnh lớn

Các helper chính:

- `GetGizmoLength()`
- `DrawArrow(...)`
- `IsNearArc(...)`
- `Dist(...)`
- `Cross(...)`

### Cách hit-test

- hit pivot: khoảng cách từ chuột tới pivot transformed nhỏ hơn `baseHit`
- hit rotate: điểm chuột nằm gần cung tròn giữa trục X và trục Y

`IsNearArc(...)` kiểm tra cả:

- khoảng cách gần bán kính
- hướng vector nằm trong góc phần tư hợp lệ giữa X và Y

Đây là một nền tảng tốt để phát triển editor-like manipulation tool.

---

## Nền tảng code cần giữ khi port sang dự án khác

### 1. Pattern kế thừa từ base canvas

Giữ lại kiến trúc:

```text
CanvasBase
 ├─ quản lý zoom/pan/screen transform
 ├─ render base image
 ├─ event coordinate/status
 └─ virtual DrawAfterImage()

AlignmentCanvas : CanvasBase
 ├─ 2 ảnh
 ├─ transform state
 ├─ gizmo
 └─ alignment event
```

Đây là phần quan trọng nhất để Agent có thể xây lại component ở dự án mới mà không bị trộn UI với logic vẽ.

### 2. Tách image gốc và tinted cache

Không nên chỉ giữ 1 bitmap.
Nên giữ:

- source/reference thật
- test thật
- display cache đã tint

Lý do:

- tránh phá dữ liệu ảnh gốc
- render nhanh hơn
- dễ đổi style hiển thị
- có thể xuất ảnh aligned từ bản gốc thay vì bản tinted

### 3. Biến đổi trong image space

Toàn bộ translate/rotate nên tính trong hệ tọa độ ảnh.
Base class chịu trách nhiệm chuyển đổi giữa screen space và image space.

Điều này làm cho:

- behavior ổn định khi zoom
- code dễ test hơn
- event trả về dữ liệu có ý nghĩa nghiệp vụ

### 4. Event contract

Khi dựng lại trong dự án mới, cần giữ contract kiểu này:

- khi state alignment đổi -> raise event
- event luôn mang đủ pivot/offset/angle
- UI ngoài không chọc trực tiếp state nội bộ

---

## Mẫu code nền tảng để Agent tái tạo

## 1. Khung class

```csharp
public class AlignmentCanvas : ImageCanvasBase
{
    private Bitmap _referenceImage;
    private Bitmap _testImage;
    private Bitmap _referenceDisplay;
    private Bitmap _testDisplay;

    private PointF? _pivot;
    private PointF? _transformPivot;
    private bool _dragPivot;
    private bool _dragRotate;
    private PointF _lastDragImage;

    public float TranslateX { get; private set; }
    public float TranslateY { get; private set; }
    public float RotationDeg { get; private set; }

    public event EventHandler<AlignmentChangedEventArgs> AlignmentChanged;

    public void SetReferenceImage(Bitmap image) { ... }
    public void SetTestImage(Bitmap image) { ... }
    public void SetPivot(float x, float y) { ... }
    public void SetTransform(float dx, float dy, float angleDeg) { ... }

    protected override void DrawAfterImage(Graphics g)
    {
        DrawTransformedImage(g, _testDisplay ?? _testImage);
        DrawAlignGizmo(g);
    }
}
```

## 2. Vẽ ảnh test có transform

```csharp
private void DrawTransformedImage(Graphics g, Bitmap image)
{
    if (image == null) return;

    var state = g.Save();
    ApplyTransform(g);
    g.DrawImage(
        image,
        new Rectangle(0, 0, image.Width, image.Height),
        0, 0, image.Width, image.Height,
        GraphicsUnit.Pixel);
    g.Restore(state);
}
```

## 3. Áp transform quanh pivot

```csharp
private void ApplyTransform(Graphics g)
{
    if (!_transformPivot.HasValue) return;

    g.TranslateTransform(_transformPivot.Value.X + TranslateX, _transformPivot.Value.Y + TranslateY);
    g.RotateTransform(RotationDeg);
    g.TranslateTransform(-_transformPivot.Value.X, -_transformPivot.Value.Y);
}
```

## 4. Luồng drag rotate

```csharp
var pivotNow = TransformPoint(_pivot.Value);
var prevAngle = (float)Math.Atan2(_lastDragImage.Y - pivotNow.Y, _lastDragImage.X - pivotNow.X);
var nowAngle = (float)Math.Atan2(img.Y - pivotNow.Y, img.X - pivotNow.X);
RotationDeg += (nowAngle - prevAngle) * 180f / (float)Math.PI;
```

---

## Điểm mạnh của thiết kế hiện tại

- tận dụng tốt `ImageCanvas` làm base
- render layer rõ ràng
- UI interaction đơn giản, dễ hiểu
- event contract gọn
- dễ mang sang WinForms project khác
- đủ linh hoạt để tích hợp thêm alignment auto/manual hybrid

---

## Giới hạn và rủi ro hiện tại

### 1. Chỉ hỗ trợ rigid transform đơn giản
Hiện tại chỉ có:

- translate
- rotate quanh một pivot

Chưa có:

- scale
- non-uniform scale
- skew
- flip
- affine matrix tổng quát

### 2. Reset pivot chưa reset transform
Right click xóa `_pivot` nhưng transform hiện tại vẫn còn.
Nếu chuyển project khác, nên quyết định rõ một trong hai hành vi:

- reset pivot only
- reset toàn bộ alignment state

### 3. `SetReferenceImage(...)` đang thay ảnh nền của base class
Cách này tiện nhưng coupling khá chặt giữa:

- reference image
- display image của base class

Nếu sau này cần nhiều layer hơn, nên tách khái niệm `BaseImage`/`DisplayLayers`.

### 4. Chưa có serialization state
Chưa có DTO hoặc method lưu/khôi phục đầy đủ alignment state ngoài các setter cơ bản.

### 5. Chưa có API lấy matrix
Trong dự án mới, thường sẽ cần:

- `Matrix GetTransformMatrix()`
- `Matrix GetInverseTransformMatrix()`
- `AlignmentState ExportState()`

---

## Hướng phát triển khuyến nghị

## 1. Chuẩn hóa state object

Nên thêm model riêng:

```csharp
public sealed class AlignmentState
{
    public bool HasPivot { get; set; }
    public PointF Pivot { get; set; }
    public float TranslateX { get; set; }
    public float TranslateY { get; set; }
    public float RotationDeg { get; set; }
}
```

API nên có:

```csharp
public AlignmentState GetState()
public void SetState(AlignmentState state)
public void ResetAlignment()
```

Lợi ích:

- dễ lưu recipe
- dễ undo/redo
- dễ sync giữa UI và backend

## 2. Hỗ trợ auto alignment + manual refine

Mở rộng workflow:

1. thuật toán auto matching tính transform sơ bộ
2. gọi `SetPivot(...)`, `SetTransform(...)`
3. user tinh chỉnh thủ công qua gizmo

Code nền tảng:

```csharp
public void ApplyAutoAlignmentResult(PointF pivot, float dx, float dy, float angleDeg)
{
    SetPivot(pivot.X, pivot.Y);
    SetTransform(dx, dy, angleDeg);
}
```

## 3. Tách renderer khỏi interaction

Cho project lớn hơn, nên tách:

- `AlignmentCanvasState`
- `AlignmentRenderer`
- `AlignmentInteractionController`

`Control` chỉ còn:
- nhận input
- gọi renderer
- raise event

Điều này giúp viết unit test dễ hơn.

## 4. Hỗ trợ nhiều lớp test/overlay

Ví dụ:

- reference
- test current
- edge map
- defect mask
- feature points

Có thể chuyển sang danh sách layer:

```csharp
public sealed class CanvasImageLayer
{
    public string Name { get; set; }
    public Bitmap Image { get; set; }
    public bool Visible { get; set; }
    public float Opacity { get; set; }
    public Matrix Transform { get; set; }
}
```

## 5. Cho phép chỉnh transform bằng numeric input
UI ngoài có thể bind tới:

- `TranslateX`
- `TranslateY`
- `RotationDeg`

Cần thêm API:

```csharp
public void NudgeTranslate(float dx, float dy)
public void NudgeRotate(float dAngle)
```

## 6. Thêm scale mode
Nếu cần alignment dựa trên phóng đại khác nhau:

```csharp
public float ScaleX { get; private set; } = 1f;
public float ScaleY { get; private set; } = 1f;
```

và sửa `ApplyTransform(...)` để có scaling.

## 7. Xuất ảnh aligned với background tùy chọn
Hiện `GetAlignedTestImage()` luôn fill nền đen.
Nên cho phép:

```csharp
public Bitmap GetAlignedTestImage(Color background)
public Bitmap ComposeAlignedPreview(bool includeReference)
```

---

## Mẫu code phát triển đề xuất

## 1. DTO state

```csharp
public sealed class AlignmentState
{
    public bool HasPivot { get; set; }
    public PointF Pivot { get; set; }
    public float TranslateX { get; set; }
    public float TranslateY { get; set; }
    public float RotationDeg { get; set; }
}
```

## 2. Export / import state

```csharp
public AlignmentState GetState()
{
    return new AlignmentState
    {
        HasPivot = _pivot.HasValue,
        Pivot = _pivot ?? _transformPivot ?? PointF.Empty,
        TranslateX = TranslateX,
        TranslateY = TranslateY,
        RotationDeg = RotationDeg
    };
}

public void SetState(AlignmentState state)
{
    if (state == null)
    {
        _pivot = null;
        _transformPivot = null;
        TranslateX = 0f;
        TranslateY = 0f;
        RotationDeg = 0f;
    }
    else
    {
        _pivot = state.HasPivot ? (PointF?)state.Pivot : null;
        _transformPivot = state.HasPivot ? (PointF?)state.Pivot : null;
        TranslateX = state.TranslateX;
        TranslateY = state.TranslateY;
        RotationDeg = state.RotationDeg;
    }

    Invalidate();
    RaiseAlignmentChanged();
}
```

## 3. Reset rõ nghĩa

```csharp
public void ResetAlignment(bool clearPivot = true)
{
    TranslateX = 0f;
    TranslateY = 0f;
    RotationDeg = 0f;

    if (clearPivot)
    {
        _pivot = null;
        _transformPivot = null;
    }

    Invalidate();
    RaiseAlignmentChanged();
}
```

## 4. Public matrix API

```csharp
public Matrix BuildTransformMatrix()
{
    var matrix = new Matrix();
    if (!_transformPivot.HasValue) return matrix;

    matrix.Translate(_transformPivot.Value.X + TranslateX, _transformPivot.Value.Y + TranslateY);
    matrix.Rotate(RotationDeg);
    matrix.Translate(-_transformPivot.Value.X, -_transformPivot.Value.Y);
    return matrix;
}
```

---

## Checklist khi Agent xây lại component này ở dự án mới

- dựng một base canvas có pan/zoom + image-space interaction
- tạo derived canvas cho alignment, không trộn vào form code
- giữ riêng ảnh gốc và display cache
- render reference trước, test sau
- transform phải tính theo image coordinate
- expose event state changed
- thêm API import/export transform state
- quyết định rõ hành vi reset pivot và reset transform
- đảm bảo dispose tất cả bitmap cache
- nếu framework không phải WinForms, vẫn giữ nguyên kiến trúc state + renderer + interaction

---

## Kết luận

`AlignmentImageCanvas` là một specialization tốt của `ImageCanvas` cho bài toán chồng và căn chỉnh hai ảnh.
Giá trị lớn nhất của nó không nằm ở UI hiện tại, mà ở pattern kiến trúc:

- base canvas quản lý view transform
- derived canvas quản lý domain-specific layer + interaction
- state alignment được biểu diễn đơn giản nhưng đủ dùng
- có thể mở rộng thành editor alignment hoàn chỉnh trong dự án khác

Khi port sang project mới, nên giữ nguyên:
- contract event
- image-space transform
- cách tách base/derived
- tinted display cache

Và nên nâng cấp thêm:
- state object
- matrix API
- reset logic rõ ràng
- auto/manual hybrid workflow
