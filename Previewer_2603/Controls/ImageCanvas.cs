using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Previewer_2603.Controls
{
    public class ImageCanvas : Control
    {
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

        public sealed class ImageCoordinateEventArgs : EventArgs
        {
            public bool HasImage { get; }
            public PointF Point { get; }

            public ImageCoordinateEventArgs(bool hasImage, PointF point)
            {
                HasImage = hasImage;
                Point = point;
            }
        }

        private Bitmap _image;
        private float _scale = 1.0f;
        private PointF _offset = PointF.Empty;
        private bool _panning;
        private Point _lastMouse;
        private bool _measuring;
        private PointF? _measureStartImage;
        private PointF? _measureEndImage;

        public ImageCanvas()
        {
            DoubleBuffered = true;
            TabStop = true;
            BackColor = Color.Black;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        public bool ManualMode { get; set; }
        public double ManualScaleToFull { get; set; } = 1.0;
        public Bitmap Image => _image;

        public event EventHandler<ManualMeasureEventArgs> ManualMeasureChanged;
        public event EventHandler<ImageCoordinateEventArgs> ImageCoordinateChanged;
        public event EventHandler<string> StatusChanged;

        public void SetImage(Bitmap image, bool preserveView = true)
        {
            var hadImage = _image != null;
            _image?.Dispose();
            _image = image;

            _measureStartImage = null;
            _measureEndImage = null;
            _measuring = false;

            if (!preserveView || !hadImage)
            {
                FitToWindow();
            }

            Invalidate();
            RaiseImageCoordinateChanged(PointF.Empty, _image != null);
        }

        public void ResetView()
        {
            FitToWindow();
            Invalidate();
            RaiseStatus("View reset");
        }

        public void FitWidth()
        {
            if (_image == null || Width <= 0) return;
            _scale = Math.Max(0.01f, Width / (float)_image.Width);
            var drawH = _image.Height * _scale;
            _offset = new PointF(0f, (Height - drawH) / 2f);
            Invalidate();
        }

        public void FitHeight()
        {
            if (_image == null || Height <= 0) return;
            _scale = Math.Max(0.01f, Height / (float)_image.Height);
            var drawW = _image.Width * _scale;
            _offset = new PointF((Width - drawW) / 2f, 0f);
            Invalidate();
        }

        public void ZoomIn() => ZoomAt(new Point(Width / 2, Height / 2), 1.1f);
        public void ZoomOut() => ZoomAt(new Point(Width / 2, Height / 2), 0.9f);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _image?.Dispose();
                _image = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);
            if (_image == null) return;

            var isInteractive = _panning || _measuring;
            e.Graphics.SmoothingMode = isInteractive ? SmoothingMode.HighSpeed : SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = isInteractive ? InterpolationMode.Bilinear : InterpolationMode.HighQualityBicubic;

            e.Graphics.TranslateTransform(_offset.X, _offset.Y);
            e.Graphics.ScaleTransform(_scale, _scale);

            e.Graphics.DrawImage(_image, 0, 0, _image.Width, _image.Height);

            DrawMeasure(e.Graphics);
            DrawAfterImage(e.Graphics);
        }

        protected virtual void DrawAfterImage(Graphics g)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_image != null) FitToWindow();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (_image == null) return;

            if (ManualMode && e.Button == MouseButtons.Right)
            {
                _measuring = true;
                _measureStartImage = ClampPointToImage(ScreenToImage(e.Location));
                _measureEndImage = _measureStartImage;
                Invalidate();
                return;
            }

            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Space) == Keys.Space)
            {
                _panning = true;
                _lastMouse = e.Location;
                Cursor = Cursors.Hand;
                Capture = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_image == null)
            {
                RaiseImageCoordinateChanged(PointF.Empty, false);
                return;
            }

            var img = ScreenToImage(e.Location);
            RaiseImageCoordinateChanged(img, IsPointInsideImage(img));

            if (_panning)
            {
                var dx = e.Location.X - _lastMouse.X;
                var dy = e.Location.Y - _lastMouse.Y;
                _offset = new PointF(_offset.X + dx, _offset.Y + dy);
                _lastMouse = e.Location;
                Invalidate();
                return;
            }

            if (_measuring)
            {
                _measureEndImage = ClampPointToImage(img);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_panning && e.Button == MouseButtons.Left)
            {
                _panning = false;
                Cursor = Cursors.Default;
                Capture = false;
            }

            if (_measuring && e.Button == MouseButtons.Right)
            {
                _measuring = false;
                if (_measureStartImage.HasValue && _measureEndImage.HasValue)
                {
                    var start = _measureStartImage.Value;
                    var end = _measureEndImage.Value;
                    var dx = (end.X - start.X) * ManualScaleToFull;
                    var dy = (end.Y - start.Y) * ManualScaleToFull;
                    ManualMeasureChanged?.Invoke(this, new ManualMeasureEventArgs(start, end, dx, dy));
                    RaiseStatus($"Measure dx={dx:F2}, dy={dy:F2}");
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_image == null) return;

            if ((ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                var delta = e.Delta > 0 ? 20f : -20f;
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    _offset = new PointF(_offset.X + delta, _offset.Y);
                }
                else
                {
                    _offset = new PointF(_offset.X, _offset.Y + delta);
                }
                Invalidate();
                return;
            }

            var factor = e.Delta > 0 ? 1.1f : 0.9f;
            ZoomAt(e.Location, factor);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_image == null) return;

            if (e.KeyCode == Keys.R)
            {
                ResetView();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Q)
            {
                _measureStartImage = null;
                _measureEndImage = null;
                _measuring = false;
                Invalidate();
                RaiseStatus("Manual measure cleared");
                e.Handled = true;
            }
        }

        protected PointF ScreenToImage(Point point)
        {
            return new PointF(
                (point.X - _offset.X) / Math.Max(0.0001f, _scale),
                (point.Y - _offset.Y) / Math.Max(0.0001f, _scale));
        }

        protected PointF ClampPointToImage(PointF point)
        {
            if (_image == null) return point;
            return new PointF(
                Math.Max(0f, Math.Min(_image.Width - 1, point.X)),
                Math.Max(0f, Math.Min(_image.Height - 1, point.Y)));
        }

        protected bool IsPointInsideImage(PointF point)
        {
            return _image != null && point.X >= 0 && point.Y >= 0 && point.X < _image.Width && point.Y < _image.Height;
        }

        protected float SafeScale() => Math.Max(0.01f, _scale);

        protected void RaiseStatus(string message)
        {
            StatusChanged?.Invoke(this, message ?? string.Empty);
        }

        private void DrawMeasure(Graphics g)
        {
            if (!_measureStartImage.HasValue || !_measureEndImage.HasValue) return;
            using (var pen = new Pen(Color.Orange, 2f / SafeScale()))
            using (var brush = new SolidBrush(Color.Orange))
            {
                var p1 = _measureStartImage.Value;
                var p2 = _measureEndImage.Value;
                g.DrawLine(pen, p1, p2);

                var r = 3.5f / SafeScale();
                g.FillEllipse(brush, p1.X - r, p1.Y - r, r * 2, r * 2);
                g.FillEllipse(brush, p2.X - r, p2.Y - r, r * 2, r * 2);
            }
        }

        private void RaiseImageCoordinateChanged(PointF point, bool hasImage)
        {
            ImageCoordinateChanged?.Invoke(this, new ImageCoordinateEventArgs(hasImage, point));
        }

        private void FitToWindow()
        {
            if (_image == null || Width <= 0 || Height <= 0) return;

            var scaleX = Width / (float)_image.Width;
            var scaleY = Height / (float)_image.Height;
            _scale = Math.Max(0.01f, Math.Min(scaleX, scaleY));

            var drawW = _image.Width * _scale;
            var drawH = _image.Height * _scale;
            _offset = new PointF((Width - drawW) / 2f, (Height - drawH) / 2f);
        }

        private void ZoomAt(Point cursor, float factor)
        {
            if (_image == null) return;

            var oldScale = _scale;
            _scale = Math.Max(0.1f, Math.Min(10f, _scale * factor));
            var ratio = _scale / oldScale;

            var dx = cursor.X - _offset.X;
            var dy = cursor.Y - _offset.Y;
            _offset = new PointF(cursor.X - dx * ratio, cursor.Y - dy * ratio);
            Invalidate();
        }
    }
}
