using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Previewer_2603.Controls
{
    public sealed class AlignmentChangedEventArgs : EventArgs
    {
        public bool HasPivot { get; }
        public PointF Pivot { get; }
        public float OffsetX { get; }
        public float OffsetY { get; }
        public float AngleDeg { get; }

        public AlignmentChangedEventArgs(bool hasPivot, PointF pivot, float offsetX, float offsetY, float angleDeg)
        {
            HasPivot = hasPivot;
            Pivot = pivot;
            OffsetX = offsetX;
            OffsetY = offsetY;
            AngleDeg = angleDeg;
        }
    }

    public sealed class AlignmentImageCanvas : ImageCanvas
    {
        public enum AlignMode { View, ManualAlignment }

        private Bitmap _referenceImage;
        private Bitmap _testImage;
        private PointF? _pivot;
        private PointF? _transformPivot;
        private bool _dragPivot;
        private bool _dragRotate;
        private PointF _lastDragImage;

        public AlignMode Mode { get; set; } = AlignMode.View;
        public float TranslateX { get; private set; }
        public float TranslateY { get; private set; }
        public float RotationDeg { get; private set; }
        public bool HasPivot => _pivot.HasValue;
        public PointF Pivot => _pivot ?? _transformPivot ?? PointF.Empty;

        public event EventHandler<AlignmentChangedEventArgs> AlignmentChanged;

        public void SetReferenceImage(Bitmap image)
        {
            _referenceImage?.Dispose();
            _referenceImage = image;
            base.SetImage(image == null ? null : (Bitmap)image.Clone(), false);
        }

        public void SetTestImage(Bitmap image)
        {
            _testImage?.Dispose();
            _testImage = image;
            Invalidate();
        }

        public void SetPivot(float pivotX, float pivotY)
        {
            _pivot = new PointF(pivotX, pivotY);
            _transformPivot = _pivot;
            Invalidate();
            RaiseAlignmentChanged();
        }

        public void SetTransform(float offsetX, float offsetY, float angleDeg)
        {
            TranslateX = offsetX;
            TranslateY = offsetY;
            RotationDeg = angleDeg;
            Invalidate();
            RaiseAlignmentChanged();
        }

        public Bitmap GetAlignedTestImage()
        {
            if (_testImage == null) return null;
            var width = _referenceImage?.Width ?? _testImage.Width;
            var height = _referenceImage?.Height ?? _testImage.Height;

            var output = new Bitmap(width, height);
            using (var g = Graphics.FromImage(output))
            {
                g.Clear(Color.Black);
                DrawTransformedImage(g, _testImage);
            }

            return output;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _referenceImage?.Dispose();
                _referenceImage = null;
                _testImage?.Dispose();
                _testImage = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Mode != AlignMode.ManualAlignment || Image == null) return;

            var img = ScreenToImage(e.Location);
            if (e.Button == MouseButtons.Right)
            {
                _dragPivot = false;
                _dragRotate = false;
                _pivot = null;
                Invalidate();
                RaiseAlignmentChanged();
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            if (!_pivot.HasValue)
            {
                _pivot = ClampPointToImage(img);
                _transformPivot = _pivot;
                Invalidate();
                RaiseAlignmentChanged();
                return;
            }

            var pivotNow = TransformPoint(_pivot.Value);
            var hit = 9f / SafeScale();
            if (Dist(pivotNow, img) <= hit)
            {
                _dragPivot = true;
                _lastDragImage = img;
                Cursor = Cursors.Hand;
                return;
            }

            if (IsNearArc(img, pivotNow))
            {
                _dragRotate = true;
                _lastDragImage = img;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Mode == AlignMode.ManualAlignment && _pivot.HasValue)
            {
                var img = ScreenToImage(e.Location);
                var changed = false;

                if (_dragPivot)
                {
                    TranslateX += img.X - _lastDragImage.X;
                    TranslateY += img.Y - _lastDragImage.Y;
                    _lastDragImage = img;
                    changed = true;
                }
                else if (_dragRotate)
                {
                    var pivotNow = TransformPoint(_pivot.Value);
                    var prevAngle = (float)Math.Atan2(_lastDragImage.Y - pivotNow.Y, _lastDragImage.X - pivotNow.X);
                    var nowAngle = (float)Math.Atan2(img.Y - pivotNow.Y, img.X - pivotNow.X);
                    RotationDeg += (nowAngle - prevAngle) * 180f / (float)Math.PI;
                    _lastDragImage = img;
                    changed = true;
                }

                if (changed)
                {
                    Invalidate();
                    RaiseAlignmentChanged();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragPivot = false;
                _dragRotate = false;
                Cursor = Cursors.Default;
            }
            base.OnMouseUp(e);
        }

        protected override void DrawAfterImage(Graphics g)
        {
            base.DrawAfterImage(g);
            DrawTransformedImage(g, _testImage);
            DrawAlignGizmo(g);
        }

        private void DrawTransformedImage(Graphics g, Bitmap image)
        {
            if (image == null) return;

            var state = g.Save();
            ApplyTransform(g);
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            g.Restore(state);
        }

        private void ApplyTransform(Graphics g)
        {
            if (!_transformPivot.HasValue) return;
            g.TranslateTransform(_transformPivot.Value.X + TranslateX, _transformPivot.Value.Y + TranslateY);
            g.RotateTransform(RotationDeg);
            g.TranslateTransform(-_transformPivot.Value.X, -_transformPivot.Value.Y);
        }

        private PointF TransformPoint(PointF point)
        {
            if (!_transformPivot.HasValue) return point;

            var matrix = new Matrix();
            matrix.Translate(_transformPivot.Value.X + TranslateX, _transformPivot.Value.Y + TranslateY);
            matrix.Rotate(RotationDeg);
            matrix.Translate(-_transformPivot.Value.X, -_transformPivot.Value.Y);
            var points = new[] { point };
            matrix.TransformPoints(points);
            return points[0];
        }

        private void DrawAlignGizmo(Graphics g)
        {
            if (!_pivot.HasValue) return;

            var pivotNow = TransformPoint(_pivot.Value);
            var length = 70f / SafeScale();
            var axisX = TransformPoint(new PointF(_pivot.Value.X + length, _pivot.Value.Y));
            var axisY = TransformPoint(new PointF(_pivot.Value.X, _pivot.Value.Y + length));

            using (var px = new Pen(Color.Red, 2f / SafeScale()))
            using (var py = new Pen(Color.Lime, 2f / SafeScale()))
            using (var arcPen = new Pen(Color.Orange, 2f / SafeScale()) { DashStyle = DashStyle.Dash })
            using (var pivotBrush = new SolidBrush(Color.Yellow))
            {
                g.DrawLine(px, pivotNow, axisX);
                g.DrawLine(py, pivotNow, axisY);

                var r = length;
                var rect = new RectangleF(pivotNow.X - r, pivotNow.Y - r, r * 2, r * 2);
                g.DrawArc(arcPen, rect, 0f, 90f);

                var pr = 5f / SafeScale();
                g.FillEllipse(pivotBrush, pivotNow.X - pr, pivotNow.Y - pr, pr * 2, pr * 2);
            }
        }

        private bool IsNearArc(PointF point, PointF pivot)
        {
            var radius = 70f / SafeScale();
            var d = Dist(point, pivot);
            return Math.Abs(d - radius) <= 10f / SafeScale() && point.X >= pivot.X && point.Y >= pivot.Y;
        }

        private static float Dist(PointF a, PointF b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void RaiseAlignmentChanged()
        {
            AlignmentChanged?.Invoke(this,
                new AlignmentChangedEventArgs(
                    _pivot.HasValue,
                    Pivot,
                    TranslateX,
                    TranslateY,
                    RotationDeg));
        }
    }
}
