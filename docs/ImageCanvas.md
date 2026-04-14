# ImageCanvas

## Mục tiêu tài liệu

Tài liệu này mô tả đầy đủ thành phần `ImageCanvas` trong thư mục `Controls` của dự án `PCM_Inspection_Demo`, theo hướng **tách ý tưởng khỏi dự án hiện tại** để có thể dùng lại ở dự án WinForms khác hoặc để Agent/AI dựng lại component tương đương.

Phạm vi chính của tài liệu:

- Giải thích vai trò của `ImageCanvas` như **lớp nền hiển thị ảnh và xử lý tương tác**.
- Chỉ ra các chức năng đang có.
- Tóm tắt kiến trúc code và các state nền tảng.
- Mô tả các extension point dùng để phát triển các canvas chuyên biệt.
- Đề xuất hướng phát triển tiếp theo từ `ImageCanvas`.
- Cung cấp skeleton code và pattern triển khai để Agent có thể tái tạo trong dự án mới.

---

## 1. Vai trò của `ImageCanvas` trong hệ thống

`ImageCanvas` là **base control** cho các thành phần hiển thị ảnh tương tác trong WinForms.

Nó giải quyết các bài toán nền tảng sau:

1. **Hiển thị ảnh bitmap**
   - Nhận `Bitmap` đầu vào.
   - Giữ ảnh nguồn (`_sourceImage`) và ảnh hiển thị (`_image`) tách biệt.
   - Hỗ trợ thay đổi ảnh hiển thị sau khi xử lý brightness mà không làm mất ảnh gốc.

2. **Biến đổi viewport**
   - Zoom bằng con lăn chuột.
   - Pan bằng kéo chuột trái.
   - Fit ảnh vào cửa sổ.
   - Fit theo chiều rộng / chiều cao.

3. **Chuyển đổi hệ tọa độ**
   - Mapping giữa tọa độ màn hình (`Point`) và tọa độ ảnh (`PointF`).
   - Đây là nền tảng để các lớp dẫn xuất vẽ ROI, gizmo, line đo, alignment handle.

4. **Overlay**
   - Hỗ trợ phủ một `overlay mask` bán trong suốt lên ảnh.

5. **Manual measure**
   - Khi `ManualMode = true`, chuột phải dùng để đo khoảng cách giữa hai điểm trên ảnh.
   - Kết quả được phát ra qua event.

6. **Auto brightness**
   - Tích hợp cơ chế debounce + async processing.
   - Dùng `AutoImageBrightnessLuma.Process(...)` để sinh ảnh hiển thị mới từ ảnh nguồn.

7. **Hook để mở rộng**
   - Cho phép lớp kế thừa vẽ thêm nội dung sau ảnh qua `DrawAfterImage(Graphics g)`.
   - Đây là điểm mở rộng quan trọng nhất.

Nói ngắn gọn: `ImageCanvas` là **image viewport + interaction engine** cho toàn bộ nhóm control xử lý ảnh.

---

## 2. Vị trí của `ImageCanvas` trong kiến trúc `Controls`

Trong folder `Controls`, mối quan hệ đang là:

- `ImageCanvas.cs`
  - lớp nền generic cho hiển thị ảnh và tương tác cơ bản.

- `AlignmentImageCanvas.cs : ImageCanvas`
  - kế thừa để thêm overlay ảnh test/reference, pivot, transform, rotate gizmo.

- `RoiImageCanvas.cs : ImageCanvas`
  - kế thừa để thêm tạo/sửa ROI polygon/rectangle.

- `RoiImageViewerControl.cs`
  - wrapper UI dùng `RoiImageCanvas` làm lõi hiển thị, còn phần toolbar/status/combo nằm ngoài.

- `AutoImageBrightnessByLuma.cs`
  - utility xử lý brightness/luma, được `ImageCanvas` dùng để xử lý ảnh hiển thị.

Mô hình này rất đúng hướng để tái sử dụng:
- `ImageCanvas` giữ logic nền.
- Canvas chuyên biệt chỉ thêm behavior riêng.
- `ViewerControl` chỉ lo UI bao ngoài.

---

## 3. Các state nền tảng của `ImageCanvas`

### 3.1. Quản lý ảnh

```csharp
private Bitmap _image;
private Bitmap _sourceImage;
private Bitmap _overlayMask;
private float _overlayOpacity = 0.3f;
```

Ý nghĩa:

- `_sourceImage`
  - ảnh gốc được nạp vào control.
  - không dùng để vẽ trực tiếp khi auto brightness bật.

- `_image`
  - ảnh đang được render lên màn hình.
  - có thể là clone của `_sourceImage`, hoặc ảnh đã qua xử lý brightness.

- `_overlayMask`
  - lớp bitmap phủ lên ảnh chính.

- `_overlayOpacity`
  - alpha của overlay.

### 3.2. View transform

```csharp
private float _scale = 1.0f;
private PointF _offset = new PointF(0, 0);
```

Ý nghĩa:

- `_scale`
  - hệ số zoom hiện tại.

- `_offset`
  - độ dời của ảnh trên control sau khi zoom/pan.

Tất cả đối tượng vẽ thêm phải tuân theo transform này.

### 3.3. Trạng thái tương tác chuột

```csharp
private bool _panning;
private Point _lastMouse;

private bool _measuring;
private PointF? _measureStartImage;
private PointF? _measureEndImage;
```

Ý nghĩa:

- `_panning`
  - đang kéo để pan ảnh.

- `_measuring`
  - đang đo thủ công bằng chuột phải.

- `_measureStartImage`, `_measureEndImage`
  - 2 điểm đo trong hệ tọa độ ảnh.

### 3.4. Trạng thái brightness async

```csharp
private readonly Timer _brightnessDebounceTimer;
private CancellationTokenSource _brightnessCts;
private int _brightnessRequestVersion;
private bool _autoBrightnessEnabled;
private ImageLumaBrightnessOptions _brightnessOptions;
```

Ý nghĩa:

- debounce để tránh xử lý brightness quá dày.
- cancel request cũ khi có request mới.
- version number tránh race condition.
- `BrightnessOptions` là cấu hình pipeline brightness.

Đây là một design khá tốt cho WinForms vì xử lý ảnh thường tốn CPU.

---

## 4. Public API hiện có

### 4.1. Thuộc tính

```csharp
public bool ManualMode { get; set; }
public double ManualScaleToFull { get; set; } = 1.0;
public Bitmap Image => _image;
public float OverlayOpacity => _overlayOpacity;
public bool AutoBrightnessEnabled { get; set; }
public ImageLumaBrightnessOptions BrightnessOptions => _brightnessOptions.Clone();
```

Ý nghĩa:

- `ManualMode`
  - bật chế độ đo thủ công bằng chuột phải.

- `ManualScaleToFull`
  - scale chuyển đổi kết quả đo sang đơn vị mong muốn của hệ thống ngoài.

- `Image`
  - ảnh đang hiển thị.

- `OverlayOpacity`
  - alpha overlay hiện tại.

- `AutoBrightnessEnabled`
  - bật/tắt xử lý brightness tự động.

- `BrightnessOptions`
  - trả về clone để tránh lộ state trực tiếp.

### 4.2. Event

```csharp
public event EventHandler<ManualMeasureEventArgs> ManualMeasureChanged;
public event EventHandler<ImageCoordinateEventArgs> ImageCoordinateChanged;
public event EventHandler<string> StatusChanged;
```

Ý nghĩa:

- `ManualMeasureChanged`
  - trả về điểm đầu, điểm cuối, dx, dy sau khi kết thúc đo.

- `ImageCoordinateChanged`
  - bắn liên tục khi rê chuột, để UI ngoài cập nhật tọa độ.

- `StatusChanged`
  - cho control con hoặc host form hiển thị message trạng thái.

### 4.3. Method thao tác ảnh / view

```csharp
public void SetImage(Bitmap image, bool preserveView = true)
public void SetBrightnessOptions(ImageLumaBrightnessOptions options, bool immediate = false)
public void SetOverlayMask(Bitmap mask)
public void ClearOverlayMask()
public void SetOverlayOpacity(float opacity)
public void ResetView()
public void FitWidth()
public void FitHeight()
public void ZoomIn()
public void ZoomOut()
```

Vai trò:
- `SetImage` là điểm vào chính.
- `SetBrightnessOptions` cập nhật cấu hình brightness.
- nhóm overlay quản lý lớp phủ.
- nhóm fit/zoom/reset quản lý viewport.

---

## 5. Luồng hoạt động chính

## 5.1. Nạp ảnh

Luồng của `SetImage(...)`:

1. Dispose `_sourceImage` cũ.
2. Gán ảnh mới cho `_sourceImage`.
3. Clone `_sourceImage` sang `_image`.
4. Reset trạng thái measure.
5. Clear overlay mask.
6. Nếu chưa có ảnh trước đó hoặc `preserveView = false` thì `FitToWindow()`.
7. Gọi `RequestBrightnessRefresh(true)`.
8. Phát `ImageCoordinateChanged` với trạng thái hiện tại.

Điểm đáng chú ý:
- `SetImage` đang **nhận ownership của `Bitmap` truyền vào** vì sau đó control sẽ `Dispose()` ảnh đó.
- Khi tái sử dụng ở dự án khác, cần thống nhất rõ ownership để tránh dispose nhầm ở ngoài.

## 5.2. Paint pipeline

Trong `OnPaint(...)`:

1. Clear nền.
2. Nếu không có ảnh thì return.
3. Chọn chất lượng render tùy trạng thái tương tác:
   - đang pan/measure hoặc ảnh quá lớn -> ưu tiên tốc độ.
   - bình thường -> ưu tiên chất lượng.
4. Áp `TranslateTransform(_offset.X, _offset.Y)`.
5. Áp `ScaleTransform(_scale, _scale)`.
6. Vẽ `_image`.
7. Vẽ overlay mask.
8. Vẽ line đo.
9. Gọi `DrawAfterImage(g)`.

Ý nghĩa thiết kế:
- Toàn bộ lớp dẫn xuất được hưởng sẵn transform ảnh.
- Lớp con chỉ việc vẽ trực tiếp theo **tọa độ ảnh**.

## 5.3. Zoom

`OnMouseWheel(...)`:
- zoom quanh vị trí con trỏ chuột.
- scale bị clamp trong `[0.1, 10]`.

Công thức giữ điểm dưới con trỏ không bị trôi là một phần rất quan trọng để user cảm thấy canvas tự nhiên.

## 5.4. Pan

- Chuột trái down -> `_panning = true`
- Mouse move -> cộng delta vào `_offset`
- Mouse up -> `_panning = false`

## 5.5. Manual measure

Khi `ManualMode = true`:

- chuột phải down -> bắt đầu đo.
- mouse move -> cập nhật `_measureEndImage`.
- chuột phải up -> phát `ManualMeasureChanged`.

Kết quả đo:

```csharp
Dx = (end.X - start.X) * ManualScaleToFull
Dy = (end.Y - start.Y) * ManualScaleToFull
```

Control hiện tại mới trả `Dx`, `Dy`, chưa trả độ dài Euclidean trực tiếp.

## 5.6. Brightness async

Luồng:

1. `RequestBrightnessRefresh(immediate)`
2. Nếu immediate -> chạy ngay.
3. Nếu không -> debounce 180ms.
4. `ProcessBrightnessAsync()` clone `_sourceImage`.
5. Nếu brightness tắt -> trả clone gốc.
6. Nếu bật -> gọi `AutoImageBrightnessLuma.Process(...)`.
7. Nếu request hiện tại đã cũ hoặc đã bị cancel -> bỏ kết quả.
8. `BeginInvoke(...)` để swap `_image` trên UI thread.

Đây là một pattern an toàn cho WinForms khi có xử lý nền.

---

## 6. Các hàm nền tảng quan trọng

### 6.1. Chuyển hệ tọa độ

```csharp
protected PointF ScreenToImage(Point point)
```

Đây là API trọng tâm để các lớp kế thừa làm hit-test, tạo ROI, kéo pivot, v.v.

Công thức:

```csharp
(point.X - _offset.X) / _scale
(point.Y - _offset.Y) / _scale
```

### 6.2. Kiểm tra biên ảnh

```csharp
protected bool IsPointInsideImage(PointF point)
protected PointF ClampPointToImage(PointF point)
```

Dùng để:
- không cho tương tác vượt biên.
- clamp điểm kéo về trong ảnh.
- đảm bảo ROI/measure/alignment không bị out-of-bounds.

### 6.3. Scale an toàn

```csharp
protected float SafeScale() => Math.Max(0.01f, _scale);
```

Dùng khi tính độ dày nét, kích thước handle:
- nét luôn ổn định theo zoom.
- tránh chia cho 0.

### 6.4. Hook mở rộng

```csharp
protected virtual void DrawAfterImage(Graphics g) { }
```

Đây là extension point chính.
Các lớp dẫn xuất đã dùng nó như sau:

- `AlignmentImageCanvas`
  - vẽ ảnh test đã transform.
  - vẽ alignment gizmo.

- `RoiImageCanvas`
  - vẽ ROI.
  - vẽ preview khi đang create ROI.

---

## 7. Cách các lớp dẫn xuất đang tái sử dụng `ImageCanvas`

## 7.1. `AlignmentImageCanvas`

Mục tiêu:
- chồng ảnh test lên ảnh reference.
- cho phép người dùng đặt pivot.
- kéo pivot để translate.
- kéo cung tròn để rotate.

Tận dụng từ `ImageCanvas`:
- viewport zoom/pan có sẵn.
- hệ tọa độ ảnh có sẵn.
- render transform nền có sẵn qua `DrawAfterImage`.
- status / mouse flow dùng lại một phần.

Phần mở rộng thêm:
- `SetReferenceImage`, `SetTestImage`
- `SetPivot`, `SetTransform`
- `GetAlignedTestImage`
- event `AlignmentChanged`
- gizmo pivot + axis + arc quay

Kết luận:
`AlignmentImageCanvas` chứng minh `ImageCanvas` đã đủ tốt để làm base cho một editor tương tác phức tạp hơn.

## 7.2. `RoiImageCanvas`

Mục tiêu:
- tạo ROI polygon hoặc rectangle.
- chỉnh sửa ROI bằng handle.
- di chuyển ROI.
- chọn / xóa ROI.

Tận dụng từ `ImageCanvas`:
- render ảnh nền.
- transform màn hình <-> ảnh.
- pan/zoom.
- manual mode và event nền.

Phần mở rộng thêm:
- mode `View/Create/Edit`
- state tạo ROI
- state chọn ROI / handle
- draw ROI / handle / preview
- event `RoiCollectionChanged`, `SelectedRoiChanged`

Kết luận:
`RoiImageCanvas` là ví dụ rất rõ cho cách dùng `ImageCanvas` làm **interaction host**.

---

## 8. Code nền tảng cần giữ khi dựng lại ở dự án mới

Khi port sang dự án khác, các thành phần sau nên giữ nguyên hoặc giữ gần tương đương.

### 8.1. Khung class base

```csharp
public class ImageCanvas : Control
{
    private Bitmap _image;
    private Bitmap _sourceImage;
    private Bitmap _overlayMask;

    private float _overlayOpacity = 0.3f;
    private float _scale = 1.0f;
    private PointF _offset = new PointF(0, 0);

    private bool _panning;
    private Point _lastMouse;

    private bool _measuring;
    private PointF? _measureStartImage;
    private PointF? _measureEndImage;

    protected virtual void DrawAfterImage(Graphics g) { }
}
```

### 8.2. Paint pipeline nền

```csharp
protected override void OnPaint(PaintEventArgs e)
{
    base.OnPaint(e);
    e.Graphics.Clear(BackColor);
    if (_image == null) return;

    e.Graphics.TranslateTransform(_offset.X, _offset.Y);
    e.Graphics.ScaleTransform(_scale, _scale);

    e.Graphics.DrawImage(_image, 0, 0, _image.Width, _image.Height);
    DrawOverlayMask(e.Graphics);
    DrawMeasure(e.Graphics);
    DrawAfterImage(e.Graphics);
}
```

### 8.3. Tọa độ màn hình -> ảnh

```csharp
protected PointF ScreenToImage(Point point)
{
    return new PointF(
        (point.X - _offset.X) / Math.Max(1e-6f, _scale),
        (point.Y - _offset.Y) / Math.Max(1e-6f, _scale));
}
```

### 8.4. Zoom theo con trỏ

```csharp
protected override void OnMouseWheel(MouseEventArgs e)
{
    if (_image == null) return;

    var oldScale = _scale;
    var delta = e.Delta > 0 ? 1.1f : 0.9f;
    _scale = Math.Max(0.1f, Math.Min(10f, _scale * delta));

    var dx = e.Location.X - _offset.X;
    var dy = e.Location.Y - _offset.Y;
    var ratio = _scale / oldScale;

    _offset = new PointF(
        e.Location.X - dx * ratio,
        e.Location.Y - dy * ratio);

    Invalidate();
}
```

### 8.5. Hook mở rộng cho lớp con

```csharp
protected virtual void DrawAfterImage(Graphics g) { }
```

Nếu phải giữ đúng một ý tưởng cốt lõi của `ImageCanvas`, thì đây là điểm quan trọng nhất.

---

## 9. Những quyết định thiết kế đáng giữ lại

## 9.1. Giữ ảnh nguồn và ảnh hiển thị riêng nhau

Lý do:
- ảnh hiển thị có thể bị thay đổi bởi brightness, tint, overlay, preprocess.
- ảnh nguồn cần được bảo toàn cho các xử lý khác.

## 9.2. Mọi canvas con làm việc theo tọa độ ảnh

Lý do:
- logic ROI/alignment không phụ thuộc zoom hay pan.
- code dễ lý giải hơn.
- vẽ và hit-test thống nhất.

## 9.3. Base class không ôm logic nghiệp vụ

`ImageCanvas` chỉ lo:
- image rendering
- transform
- interaction cơ bản
- async brightness

Nó không biết ROI là gì, alignment là gì.
Đây là thiết kế tốt để tái sử dụng.

## 9.4. Event để giao tiếp với UI ngoài

`StatusChanged`, `ImageCoordinateChanged`, `ManualMeasureChanged` là cầu nối nhẹ giữa canvas và UI host.

---

## 10. Những điểm còn hạn chế trong bản hiện tại

Các điểm này nên được hiểu rõ khi tái sử dụng hoặc nâng cấp.

### 10.1. `SetImage` ownership chưa được mô tả rõ
Hiện tại control sẽ dispose `_sourceImage`, nghĩa là ảnh truyền vào coi như đã chuyển ownership cho control.

Khuyến nghị:
- ghi rõ contract này trong XML doc, hoặc
- luôn clone input bên trong và không giữ trực tiếp object do bên ngoài tạo.

### 10.2. `OnResize` luôn gọi `FitToWindow`
```csharp
protected override void OnResize(EventArgs e) { base.OnResize(e); FitToWindow(); }
```

Hệ quả:
- user đang zoom/pan mà control resize thì view bị reset fit lại.
- không phù hợp nếu muốn preserve view.

Khuyến nghị:
- thêm cờ `AutoFitOnResize`.
- hoặc preserve center/scale hiện tại khi resize.

### 10.3. Overlay mask bị clear khi `SetImage`
Hiện tại mỗi lần set ảnh mới, overlay bị xóa:

```csharp
ClearOverlayMask();
```

Điều này hợp lý ở một số flow, nhưng không phải mọi flow.

Khuyến nghị:
- thêm option `clearOverlay = true/false`.

### 10.4. Chưa có API lấy transform hiện tại
Hiện chưa có:
- `Scale`
- `Offset`
- `ImageToScreen(...)`
- `GetViewState()` / `SetViewState(...)`

Thiếu các API này sẽ hạn chế việc đồng bộ nhiều canvas hoặc lưu/khôi phục view.

### 10.5. Chưa có virtual hook cho mouse flow ở mức nhỏ
Hiện lớp con override `OnMouseDown/Move/Up` trực tiếp.
Điều này vẫn ổn, nhưng nếu số canvas con tăng thì nên tách thêm hook như:
- `OnImageMouseDown(...)`
- `OnImageMouseMove(...)`
- `OnImageMouseUp(...)`

### 10.6. Brightness đang gắn cứng vào base canvas
`ImageCanvas` hiện phụ thuộc `AutoImageBrightnessLuma`.
Nếu muốn reuse rộng hơn, nên tách thành interface/pipeline.

Ví dụ:
```csharp
public interface IBitmapPostProcessor
{
    Bitmap Process(Bitmap source);
}
```

---

## 11. Hướng phát triển từ `ImageCanvas`

Phần này quan trọng nếu dùng tài liệu để Agent xây component mới.

## 11.1. Hướng 1: Tách `ImageCanvas` thành framework nhỏ cho image editor

Nên tách thành các lớp:

- `ImageCanvasBase`
  - render ảnh, zoom/pan, coordinate transform

- `ImageCanvasOverlayLayer`
  - layer vẽ thêm

- `ImageCanvasInteractionTool`
  - tool create/edit/select/measure

- `ImageCanvasViewState`
  - scale, offset, selected tool, options

Lợi ích:
- dễ mở rộng.
- tránh một lớp base quá lớn.
- hỗ trợ nhiều tool song song.

## 11.2. Hướng 2: Chuẩn hóa hệ layer

Hiện tại base đang có:
- ảnh nền
- overlay mask
- measure overlay
- custom overlay trong `DrawAfterImage`

Có thể nâng thành hệ layer rõ ràng:

1. Background image layer
2. Processed overlay layer
3. Annotation layer
4. Interaction preview layer
5. HUD / status layer

Lợi ích:
- dễ bật/tắt layer.
- dễ quản lý z-index.
- dễ chụp ảnh export.

## 11.3. Hướng 3: Bổ sung view state serialization

Cần class:

```csharp
public sealed class ImageCanvasViewState
{
    public float Scale { get; set; }
    public PointF Offset { get; set; }
}
```

Và API:

```csharp
public ImageCanvasViewState GetViewState();
public void SetViewState(ImageCanvasViewState state);
```

Ứng dụng:
- lưu view hiện tại.
- đồng bộ nhiều canvas.
- mở ảnh lại vẫn giữ vị trí trước đó.

## 11.4. Hướng 4: Tool-based interaction

Thay vì boolean/mode rời rạc, có thể chuyển sang pattern tool:

```csharp
public interface IImageCanvasTool
{
    void OnMouseDown(ImageCanvas canvas, MouseEventArgs e);
    void OnMouseMove(ImageCanvas canvas, MouseEventArgs e);
    void OnMouseUp(ImageCanvas canvas, MouseEventArgs e);
    void OnPaint(ImageCanvas canvas, Graphics g);
}
```

Các tool:
- PanTool
- MeasureTool
- RoiCreateTool
- RoiEditTool
- AlignmentTool
- PixelProbeTool

Đây là hướng rất mạnh nếu dự án sau có nhiều tương tác.

## 11.5. Hướng 5: Đồng bộ nhiều canvas

Ví dụ dự án inspection thường cần:
- canvas ảnh gốc
- canvas ảnh kết quả
- canvas overlay defect
- canvas compare

Nên thêm API:
- sync zoom/pan
- sync cursor position
- sync crosshair
- sync selected ROI

## 11.6. Hướng 6: Hỗ trợ render tối ưu cho ảnh lớn

Có thể phát triển thêm:
- tiled rendering
- cached downsample pyramid
- viewport culling
- lazy redraw

Bản hiện tại đã có chút tối ưu bằng cách hạ chất lượng interpolation khi đang tương tác, nhưng chưa đủ nếu ảnh rất lớn.

---

## 12. Gợi ý triển khai ở dự án mới

## 12.1. Phiên bản base tối thiểu

Khi Agent dựng lại component ở dự án mới, nên triển khai theo thứ tự:

1. `ImageCanvasBase`
   - set image
   - fit to window
   - zoom/pan
   - screen/image transform
   - draw image

2. `Overlay support`
   - overlay mask
   - opacity

3. `Measurement support`
   - manual measure
   - event

4. `Post-processing support`
   - brightness hoặc pipeline filter

5. `Extension hook`
   - draw custom overlay
   - mouse hook/tool hook

## 12.2. Contract API đề xuất

```csharp
public class ImageCanvasBase : Control
{
    public Bitmap SourceImage { get; }
    public Bitmap DisplayImage { get; }
    public float Scale { get; }
    public PointF Offset { get; }

    public bool AutoFitOnResize { get; set; }
    public bool ManualMeasureEnabled { get; set; }

    public event EventHandler<ImageCoordinateChangedEventArgs> ImageCoordinateChanged;
    public event EventHandler<MeasureChangedEventArgs> MeasureChanged;
    public event EventHandler<string> StatusChanged;

    public void SetImage(Bitmap image, bool preserveView = true);
    public void ResetView();
    public void FitWidth();
    public void FitHeight();

    public PointF ScreenToImage(Point point);
    public PointF ImageToScreen(PointF point);

    protected virtual void DrawAfterImage(Graphics g);
}
```

## 12.3. Nếu cần mở rộng ROI

Nên tạo:

- `RoiImageCanvas : ImageCanvasBase`
- model riêng:
  - `RoiShape`
  - `RoiPolygon`
  - `RoiRectangle`
- service riêng:
  - `IRoiSerializer`
  - `IRoiHitTester`

## 12.4. Nếu cần mở rộng alignment

Nên tạo:

- `AlignmentImageCanvas : ImageCanvasBase`
- model riêng:
  - `AlignmentTransform`
  - `PivotHandle`
- service riêng:
  - `IAlignmentRenderer`
  - `ITransformExporter`

---

## 13. Skeleton code phát triển từ `ImageCanvas`

## 13.1. Base canvas có thể tái sử dụng

```csharp
public class ImageCanvasBase : Control
{
    private Bitmap _sourceImage;
    private Bitmap _displayImage;
    private float _scale = 1f;
    private PointF _offset = PointF.Empty;
    private bool _panning;
    private Point _lastMouse;

    public Bitmap SourceImage => _sourceImage;
    public Bitmap DisplayImage => _displayImage;
    public float Scale => _scale;
    public PointF Offset => _offset;

    public ImageCanvasBase()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.ResizeRedraw |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
    }

    public virtual void SetImage(Bitmap image, bool preserveView = true)
    {
        _sourceImage?.Dispose();
        _displayImage?.Dispose();

        _sourceImage = image == null ? null : (Bitmap)image.Clone();
        _displayImage = _sourceImage == null ? null : (Bitmap)_sourceImage.Clone();

        if (!preserveView)
            FitToWindow();

        Invalidate();
    }

    public void ResetView()
    {
        FitToWindow();
        Invalidate();
    }

    protected float SafeScale() => Math.Max(0.01f, _scale);

    public PointF ScreenToImage(Point point)
    {
        return new PointF(
            (point.X - _offset.X) / Math.Max(1e-6f, _scale),
            (point.Y - _offset.Y) / Math.Max(1e-6f, _scale));
    }

    public PointF ImageToScreen(PointF point)
    {
        return new PointF(
            _offset.X + point.X * _scale,
            _offset.Y + point.Y * _scale);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);
        if (_displayImage == null) return;

        e.Graphics.TranslateTransform(_offset.X, _offset.Y);
        e.Graphics.ScaleTransform(_scale, _scale);
        e.Graphics.DrawImage(_displayImage, 0, 0);
        DrawAfterImage(e.Graphics);
    }

    protected virtual void DrawAfterImage(Graphics g) { }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_displayImage == null) return;

        var oldScale = _scale;
        _scale = Math.Max(0.1f, Math.Min(10f, _scale * (e.Delta > 0 ? 1.1f : 0.9f)));

        var dx = e.Location.X - _offset.X;
        var dy = e.Location.Y - _offset.Y;
        var ratio = _scale / oldScale;

        _offset = new PointF(
            e.Location.X - dx * ratio,
            e.Location.Y - dy * ratio);

        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _panning = true;
            _lastMouse = e.Location;
            Capture = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_panning) return;

        var dx = e.Location.X - _lastMouse.X;
        var dy = e.Location.Y - _lastMouse.Y;
        _offset = new PointF(_offset.X + dx, _offset.Y + dy);
        _lastMouse = e.Location;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            _panning = false;
            Capture = false;
        }
    }

    private void FitToWindow()
    {
        if (_displayImage == null || Width <= 0 || Height <= 0) return;

        var scaleX = Width / (float)_displayImage.Width;
        var scaleY = Height / (float)_displayImage.Height;
        _scale = Math.Min(scaleX, scaleY);
        if (_scale <= 0f) _scale = 1f;

        var drawW = _displayImage.Width * _scale;
        var drawH = _displayImage.Height * _scale;
        _offset = new PointF((Width - drawW) / 2f, (Height - drawH) / 2f);
    }
}
```

## 13.2. Canvas dẫn xuất cho annotation

```csharp
public class AnnotationImageCanvas : ImageCanvasBase
{
    private readonly List<PointF> _points = new List<PointF>();

    protected override void DrawAfterImage(Graphics g)
    {
        base.DrawAfterImage(g);

        using (var pen = new Pen(Color.LimeGreen, 2f / SafeScale()))
        {
            for (int i = 1; i < _points.Count; i++)
                g.DrawLine(pen, _points[i - 1], _points[i]);
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        _points.Add(ScreenToImage(e.Location));
        Invalidate();
    }
}
```

## 13.3. Tách brightness thành processor

```csharp
public interface IImagePostProcessor
{
    Bitmap Process(Bitmap source);
}

public sealed class LumaBrightnessProcessor : IImagePostProcessor
{
    private readonly ImageLumaBrightnessOptions _options;

    public LumaBrightnessProcessor(ImageLumaBrightnessOptions options)
    {
        _options = options;
    }

    public Bitmap Process(Bitmap source)
    {
        return AutoImageBrightnessLuma.Process(source, _options);
    }
}
```

---

## 14. Checklist để Agent dựng lại component trong dự án mới

Agent nên bám đúng checklist này:

### Bước 1: dựng base control
- kế thừa `Control`
- bật `DoubleBuffered`
- lưu `_scale`, `_offset`
- render `Bitmap` với transform

### Bước 2: thêm coordinate transform
- `ScreenToImage`
- `ImageToScreen`
- `ClampPointToImage`
- `IsPointInsideImage`

### Bước 3: thêm interaction nền
- pan
- zoom
- reset view / fit width / fit height

### Bước 4: thêm overlay và measurement
- overlay bitmap + opacity
- line đo
- event measure

### Bước 5: thêm hook mở rộng
- `DrawAfterImage(Graphics g)`
- có thể thêm virtual mouse hook cho lớp con

### Bước 6: thêm post-processing
- brightness async
- debounce + cancellation
- giữ ảnh nguồn và ảnh hiển thị riêng nhau

### Bước 7: tạo canvas chuyên biệt
- ROI canvas
- alignment canvas
- defect canvas
- compare canvas

---

## 15. Kết luận

`ImageCanvas` trong dự án này là một base control được thiết kế đúng hướng để tái sử dụng.

Giá trị lớn nhất của nó không nằm ở việc “vẽ ảnh”, mà nằm ở 4 phần nền tảng:

1. **viewport transform**
2. **screen/image coordinate mapping**
3. **interaction cơ bản (pan/zoom/measure)**
4. **extension point cho canvas chuyên biệt**

Từ lớp này đã phát triển được ít nhất hai nhánh chức năng rõ ràng:

- `AlignmentImageCanvas`
- `RoiImageCanvas`

Do đó, khi chuyển sang dự án mới, nên xem `ImageCanvas` như một **nền tảng chung cho image interaction controls**, không chỉ là một custom picture box.

---

## 16. Phụ lục: mapping nhanh giữa file và vai trò

- `Controls/ImageCanvas.cs`
  - lớp nền hiển thị ảnh, pan/zoom/measure/overlay/brightness

- `Controls/AlignmentImageCanvas.cs`
  - mở rộng cho chỉnh alignment giữa reference và test image

- `Controls/RoiImageCanvas.cs`
  - mở rộng cho tạo/sửa ROI polygon/rectangle

- `Controls/RoiImageViewerControl.cs`
  - UserControl bao ngoài `RoiImageCanvas`, thêm UI

- `Controls/AutoImageBrightnessByLuma.cs`
  - thuật toán brightness theo luma, dùng bởi `ImageCanvas`
