using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
// ============================================================================
// File: Controls/RoiImageCanvas.cs
// Implements: pan/zoom + manual measure (from your ImagePreviewControl) + ROI create/edit
// Notes:
// - Pan: Space + Left drag (any mode)
// - Manual measure: Right drag when ManualMode=true
// - Create: click to add points; Ctrl locks horizontal/vertical; double-click to finish
// - Edit: select ROI => show handles; drag handle or move ROI
// ============================================================================
namespace Previewer_2603.Controls
{
    [DataContract]
    public sealed class RoiPolygon
    {
        [DataMember(Order = 0)]
        public string Id {  get; set; } = Guid.NewGuid().ToString("N");
        [DataMember(Order = 1)]
        public string Name { get; set; } = string.Empty;
        [DataMember(Order = 2)]
        public List<RoiPoint> Points { get; set; } = new List<RoiPoint>();

        public RoiPolygon Clone()
        {
            return new RoiPolygon
            {
                Id = Id,
                Name = Name,
                Points = Points.Select(p => new RoiPoint { X = p.X, Y = p.Y }).ToList()
            };
        }
        public override string ToString() => $"{Name} ({Points.Count} pts)";
    }
    [DataContract]
    public sealed class RoiPoint
    {
        [DataMember (Order = 0)]
        public float X { get; set; }
        [DataMember (Order = 1)]
        public float Y { get; set; }
        public PointF ToPointF() => new PointF(X, Y);
        public static RoiPoint From(PointF p) => new RoiPoint
        {
            X = p.X,
            Y = p.Y,
        };
    }
    public sealed class RoiImageCanvas : Control
    {
        public enum InteractionMode { View, Create, Edit}

        private Bitmap _image;
        private float _scale = 1.0f;
        private PointF _offset = new PointF(0, 0);

        private bool _panning;
        private Point _lastMouse;

        private bool _measuring;
        private PointF? _measureStartImage;
        private PointF? _measureEndImage;

        private readonly List<RoiPolygon> _rois = new List<RoiPolygon>();

        // Create mode state
        private readonly List<PointF> _creatingPoints = new List<PointF>();
        private bool _creatingActive;
        private PointF? _creatingMouseImage;

        // Edit mode state
        private int _selectedRoiIndex = -1;
        private int _selectedHandleIndex = -1; // vertex index
        private bool _movingRoi;
        private PointF _moveStartImage;
        private List<PointF> _moveStartPoints;

        public RoiImageCanvas()
        {
            DoubleBuffered = true;
            TabStop = true;
            BackColor = Color.Black;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        public bool ManualMode { get; set; }
        public double ManualScaleToFull {  get; set; } = 1.0;
        public InteractionMode Mode { get; set; } = InteractionMode.View;
        public Bitmap Image => _image;

        public event EventHandler<ManualMeasureEventArgs> ManualMeasureChanged;
        public event EventHandler<string> StatusChanged;
        public event EventHandler RoiCollectionChanged;
        public event EventHandler SelectedRoiChanged;

        public void SetImage(Bitmap image, bool preserveView = true)
        {
            var hadImage = _image != null;
            _image?.Dispose();
            _image = image;

            _measureStartImage = null;
            _measureEndImage = null;

            CancelCreate();
            Deselect();

            if (!preserveView || !hadImage) FitToWindow();
            Invalidate();
        }

        public void ResetView()
        {
            FitToWindow();
            Invalidate();
        }

        public IReadOnlyList<RoiPolygon> GetRois() => _rois.Select(r => r.Clone()).ToList();

        public void SetRois(IEnumerable<RoiPolygon> rois)
        {
            _rois.Clear();
            if (rois != null) _rois.AddRange(rois.Select(r => r.Clone()));
            CancelCreate();
            Deselect();
            RaiseRoiCollectionChanged();
            Invalidate();
        }

        public void ClearRois()
        {
            _rois?.Clear();
            CancelCreate();
            Deselect();
            RaiseRoiCollectionChanged();
            Invalidate();
        }

        public int SelectedRoiIndex => _selectedRoiIndex;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image?.Dispose();
                _image = null;
            }
            base.Dispose(disposing);
        }

        #region Paint + ROI
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);

            if (_image == null) return;

            var isInteractive = _panning || _measuring || _creatingActive || _movingRoi || _selectedHandleIndex >= 0;
            e.Graphics.SmoothingMode = isInteractive ? SmoothingMode.HighSpeed : SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = isInteractive ? InterpolationMode.Bilinear : InterpolationMode.HighQualityBicubic;
            // image space tranform
            e.Graphics.TranslateTransform(_offset.X, _offset.Y);
            e.Graphics.ScaleTransform(_scale, _scale);

            e.Graphics.DrawImage(_image, 0, 0, _image.Width, _image.Height);

            DrawRois(e.Graphics);
            DrawCreating(e.Graphics);
            DrawMeasure(e.Graphics);
            DrawHelpOverlay(e.Graphics);
        }

        private void DrawRois(Graphics g)
        {
            using (var pen = new Pen(Color.LimeGreen, 2f / SafeScale()))
            using (var penSel = new Pen(Color.Gold, 3f / SafeScale()))
            using (var fillSel = new SolidBrush(Color.FromArgb(20, Color.Gold)))
            {
                for (var i = 0; i < _rois.Count; i++)
                {
                    var pts = _rois[i].Points.Select(p => p.ToPointF()).ToArray();
                    if (pts.Length < 2) continue;

                    var isSel = i == _selectedRoiIndex;
                    var drawPen = isSel ? penSel : pen;

                    if (pts.Length >= 3)
                    {
                        using (var path = new GraphicsPath())
                        {
                            path.AddPolygon(pts);
                            if (isSel) g.FillPath(fillSel, path);
                        }
                    }

                    if (pts.Length == 2) g.DrawLine(drawPen, pts[0], pts[1]);
                    else g.DrawPolygon(drawPen, pts);

                    if (isSel) DrawHandles(g,  pts);
                }
            }
        }

        private void DrawHandles(Graphics g,  PointF[] pts)
        {
            var r = 5f / SafeScale();
            using (var b = new SolidBrush(Color.OrangeRed))
            using (var p = new Pen(Color.White, 1f / SafeScale()))
            {
                for (var i = 0; i < pts.Length; i++)
                {
                    var rect = new RectangleF(pts[i].X - r, pts[i].Y - r, r * 2, r * 2);
                    g.FillEllipse(b, rect);
                    g.DrawEllipse(p, rect);
                }
            }
        }

        private void DrawCreating(Graphics g)
        {
            if (Mode != InteractionMode.Create) return;
            if (!_creatingActive && _creatingPoints.Count == 0) return;

            var width = 2f / SafeScale();
            using (var pen = new Pen(Color.DeepSkyBlue, width))
            using (var penDash = new Pen(Color.DeepSkyBlue, width) { DashStyle = DashStyle.Dash })
            using (var brush = new SolidBrush(Color.DeepSkyBlue))
            {
                var pts = _creatingPoints.ToArray();
                if (pts.Length >= 2) g.DrawLines(pen, pts);

                foreach (var p in pts)
                {
                    var r = 4f / SafeScale();
                    g.FillEllipse(brush, p.X - r, p.Y - r, r * 2, r * 2);
                }

                if (pts.Length >= 1 && _creatingMouseImage.HasValue)
                {
                    g.DrawLine(penDash, pts.Last(), _creatingMouseImage.Value);
                }
            }
        }

        private void DrawMeasure(Graphics g)
        {
            if (!_measureStartImage.HasValue || !_measureEndImage.HasValue) return;

            var p1 = _measureStartImage.Value;
            var p2 = _measureEndImage.Value;

            using (var pen = new Pen(Color.LimeGreen, 2f / SafeScale()))
            using (var brush = new SolidBrush(Color.OrangeRed))
            {
                g.DrawLine(pen, p1, p2);
                var r = 4f / SafeScale();
                g.FillEllipse(brush, p1.X - r, p1.Y - r, r * 2, r * 2);
                g.FillEllipse(brush, p2.X - r, p2.Y - r, r * 3, r * 3);
            }
        }

        private void DrawHelpOverlay(Graphics g)
        {
            // Draw in image space; very small and unobtrusive; can be removed if not needed.
            // (No heavy overlay; just status is in StatusStrip.)
        }
        #endregion

        #region Movement
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_image == null) return;

            var oldScale = _scale;
            var delta = e.Delta > 0 ? 1.1f : 0.9f;
            _scale = Math.Max(0.1f, Math.Min(10f, _scale * delta));

            var mousePos = e.Location;
            var dx = mousePos.X - _offset.X;
            var dy = mousePos.Y - _offset.Y;
            var scaleRatio = _scale / oldScale;
            _offset = new PointF(mousePos.X - dx * scaleRatio, mousePos.Y - dy * scaleRatio);

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_image == null) return;

            Focus();

            // Manual measure (keep behavior)
            if (ManualMode && e.Button == MouseButtons.Right)
            {
                _measuring = true;
                _measureStartImage = ScreenToImage(e.Location);
                _measureEndImage = _measureStartImage;
                Invalidate();
                RaiseStatus("Measuring...");
                return;
            }

            // Pan: Space + Left drag (always available)
            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Space) == Keys.Space)
            {
                _panning = true;
                _lastMouse = e.Location;
                RaiseStatus("Panning...");
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            if (Mode == InteractionMode.Create)
            {
                HandleCreateMouseDown(e);
                return;
            }

            if (Mode == InteractionMode.Edit)
            {
                HandleEditMouseDown(e);
                return;
            }

            // View mode: allow pan with Left drag too (matching original) if not Space? optional.
            _panning = true;
            _lastMouse = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_image == null) return;

            if (_measuring)
            {
                _measureEndImage = ScreenToImage(e.Location);
                Invalidate();
                return;
            }

            if (_panning)
            {
                var dx = e.Location.X - _lastMouse.X;
                var dy = e.Location.Y - _lastMouse.Y;
                _offset = new PointF(_offset.X + dx, _offset.Y + dy);
                _lastMouse = e.Location;
                Invalidate();
                return;
            }

            if (Mode == InteractionMode.Create)
            {
                HandleCreateMouseMove(e);
                return;
            }

            if (Mode == InteractionMode.Edit)
            {
                HandleEditMouseMove(e);
                return;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (ManualMode && e.Button == MouseButtons.Right)
            {
                _measuring = false;
                RaiseManualMeasureChanged();
                RaiseStatus("Measure done");
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                _panning = false;

                if (Mode == InteractionMode.Edit)
                {
                    _selectedHandleIndex = -1;
                    _movingRoi = false;
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_image == null) return;

            if (Mode == InteractionMode.Create && e.Button == MouseButtons.Left)
            {
                FinalizeCreate();
            }
        }
        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            FitToWindow();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.R)
            {
                ResetView();
                e.Handled = true;
                RaiseStatus("Reset view");
                return;
            }

            if (e.KeyCode == Keys.Q)
            {
                _measureStartImage = null;
                _measureEndImage = null;
                Invalidate();
                RaiseStatus("Clear measure");
                return;
            }

            if (Mode == InteractionMode.Create && e.KeyCode == Keys.Escape)
            {
                CancelCreate();
                RaiseStatus("Cancel create");
                return;
            }

            if (Mode == InteractionMode.Edit && e.KeyCode == Keys.Delete)
            {
                DeleteSelectedRoi();
                return;
            }
        }

        private void HandleCreateMouseDown(MouseEventArgs e)
        {
            var p = ConstrainCreatePoint(ScreenToImage(e.Location));
            if (!_creatingActive)
            {
                _creatingActive = true;
                _creatingPoints.Clear();
            }

            _creatingPoints.Add(p);
            _creatingMouseImage = p;

            Invalidate();
            RaiseStatus($"Create: {_creatingPoints.Count} point(s)");
        }

        private void HandleCreateMouseMove(MouseEventArgs e)
        {
            if (!_creatingActive && _creatingPoints.Count == 0) return;

            var raw = ScreenToImage(e.Location);
            _creatingMouseImage = ConstrainCreatePoint(raw);

            Invalidate();
        }

        private PointF ConstrainCreatePoint(PointF raw)
        {
            if (_creatingPoints.Count == 0) return raw;

            var last = _creatingPoints[_creatingPoints.Count - 1];

            // Ctrl => horizontal/vertical only relative to last
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                var dx = Math.Abs(raw.X - last.X);
                var dy = Math.Abs(raw.Y - last.Y);
                return dx >= dy
                    ? new PointF(raw.X, last.Y)   // horizontal
                    : new PointF(last.X, raw.Y);  // vertical
            }

            return raw;
        }

        private void FinalizeCreate()
        {
            if (_creatingPoints.Count < 3)
            {
                RaiseStatus("Create: cần >= 3 điểm");
                return;
            }

            var roi = new RoiPolygon
            {
                Name = $"ROI_{_rois.Count + 1}",
                Points = _creatingPoints.Select(RoiPoint.From).ToList()
            };
            _rois.Add(roi);
            RaiseRoiCollectionChanged();

            CancelCreate();
            SelectRoi(_rois.Count - 1);

            Invalidate();
            RaiseStatus($"Created: {roi.Name}");
        }

        private void CancelCreate()
        {
            _creatingActive = false;
            _creatingPoints.Clear();
            _creatingMouseImage = null;
            Invalidate();
        }

        private void HandleEditMouseDown(MouseEventArgs e)
        {
            var img = ScreenToImage(e.Location);

            // If selected ROI exists, check handle hit first
            if (_selectedRoiIndex >= 0)
            {
                var handleIndex = HitTestHandle(_selectedRoiIndex, img);
                if (handleIndex >= 0)
                {
                    _selectedHandleIndex = handleIndex;
                    RaiseStatus($"Edit: dragging handle {handleIndex}");
                    return;
                }
            }

            // Hit test ROI polygons from topmost (last)
            var hit = HitTestRoi(img);
            if (hit >= 0)
            {
                SelectRoi(hit);

                // Start moving whole ROI if click inside (not on handle)
                _movingRoi = true;
                _moveStartImage = img;
                _moveStartPoints = _rois[_selectedRoiIndex].Points.Select(p => p.ToPointF()).ToList();

                RaiseStatus($"Edit: moving {_rois[_selectedRoiIndex].Name}");
                Invalidate();
                return;
            }

            Deselect();
            RaiseStatus("Edit: no selection");
            Invalidate();
        }

        private void HandleEditMouseMove(MouseEventArgs e)
        {
            if (_selectedRoiIndex < 0) return;

            var img = ScreenToImage(e.Location);

            // Dragging a handle
            if (_selectedHandleIndex >= 0)
            {
                var roi = _rois[_selectedRoiIndex];
                if (_selectedHandleIndex < roi.Points.Count)
                {
                    roi.Points[_selectedHandleIndex] = RoiPoint.From(img);
                    Invalidate();
                }
                return;
            }

            // Moving whole ROI
            if (_movingRoi && _moveStartPoints != null)
            {
                var dx = img.X - _moveStartImage.X;
                var dy = img.Y - _moveStartImage.Y;

                var roi = _rois[_selectedRoiIndex];
                for (var i = 0; i < roi.Points.Count; i++)
                {
                    var p0 = _moveStartPoints[i];
                    roi.Points[i] = RoiPoint.From(new PointF(p0.X + dx, p0.Y + dy));
                }

                Invalidate();
            }
        }

        private void DeleteSelectedRoi()
        {
            if (_selectedRoiIndex < 0) return;
            var name = _rois[_selectedRoiIndex].Name;

            _rois.RemoveAt(_selectedRoiIndex);
            Deselect();
            RaiseRoiCollectionChanged();

            Invalidate();
            RaiseStatus($"Deleted: {name}");
        }

        public bool SelectRoiByIndex(int index)
        {
            if (index < 0 || index >= _rois.Count)
            {
                if (_selectedRoiIndex != -1) Deselect();
                return false;
            }

            SelectRoi(index);
            Invalidate();
            return true;
        }

        public bool DeleteRoiByIndex(int index)
        {
            if (index < 0 || index >= _rois.Count) return false;

            SelectRoi(index);
            DeleteSelectedRoi();
            return true;
        }

        private void SelectRoi(int index)
        {
            _selectedRoiIndex = index;
            _selectedHandleIndex = -1;
            _movingRoi = false;
            _moveStartPoints = null;
            RaiseSelectedRoiChanged();
        }

        private void Deselect()
        {
            _selectedRoiIndex = -1;
            _selectedHandleIndex = -1;
            _movingRoi = false;
            _moveStartPoints = null;
            RaiseSelectedRoiChanged();
        }

        private int HitTestRoi(PointF img)
        {
            // Reverse order = topmost first
            for (var i = _rois.Count - 1; i >= 0; i--)
            {
                var pts = _rois[i].Points.Select(p => p.ToPointF()).ToArray();
                if (pts.Length < 3) continue;

                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(pts);
                    if (path.IsVisible(img)) return i;
                }
            }
            return -1;
        }

        private int HitTestHandle(int roiIndex, PointF img)
        {
            if (roiIndex < 0 || roiIndex >= _rois.Count) return -1;
            var pts = _rois[roiIndex].Points.Select(p => p.ToPointF()).ToArray();
            if (pts.Length == 0) return -1;

            var r = 6f / SafeScale();
            for (var i = 0; i < pts.Length; i++)
            {
                var dx = img.X - pts[i].X;
                var dy = img.Y - pts[i].Y;
                if (dx * dx + dy * dy <= r * r) return i;
            }
            return -1;
        }

        #region Utils
        private float SafeScale() => Math.Max(0.01f, _scale);

        private void FitToWindow()
        {
            if (_image == null || Width <= 0 || Height <= 0) return;

            var scaleX = Width / (float)_image.Width;
            var scaleY = Height / (float)_image.Height;
            _scale = Math.Min(scaleX, scaleY);
            if (_scale <= 0) _scale = 1f;

            var drawW = _image.Width * _scale;
            var drawH = _image.Height * _scale;
            _offset = new PointF((Width - drawW) / 2f, (Height - drawH) / 2f);
        }

        private PointF ScreenToImage(Point point)
        {
            var x = (point.X - _offset.X) / Math.Max(1e-6f, _scale);
            var y = (point.Y - _offset.Y) / Math.Max(1e-6f, _scale);
            return new PointF(x, y);
        }
        private void RaiseManualMeasureChanged()
        {
            if (!_measureStartImage.HasValue || !_measureEndImage.HasValue) return;

            var start = _measureStartImage.Value;
            var end = _measureEndImage.Value;
            var dx = (end.X - start.X) * ManualScaleToFull;
            var dy = (end.Y - start.Y) * ManualScaleToFull;

            ManualMeasureChanged?.Invoke(this, new ManualMeasureEventArgs(start, end, dx, dy));
        }

        private void RaiseStatus(string message) => StatusChanged?.Invoke(this, message ?? string.Empty);
        private void RaiseRoiCollectionChanged() => RoiCollectionChanged?.Invoke(this, EventArgs.Empty);
        private void RaiseSelectedRoiChanged() => SelectedRoiChanged?.Invoke(this, EventArgs.Empty);

        public sealed class ManualMeasureEventArgs : EventArgs
        {
            public PointF Start { get; }
            public PointF End { get; }
            public double Dx { get; }
            public double Dy { get; }

            public ManualMeasureEventArgs(PointF start, PointF end, double dx, double dy)
            {
                Start = start;
                End = end;
                Dx = dx;
                Dy = dy;
            }
        }
        #endregion
    }
}
