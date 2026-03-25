using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Previewer_2603.Controls
{
    public partial class RoiImageViewerControl : UserControl
    {
        public RoiImageViewerControl()
        {
            InitializeComponent();

        }

        private void chkCreate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCreate.Checked) chkEdit.Checked = false;
            canvas.Mode = chkCreate.Checked ? RoiImageCanvas.InteractionMode.Create :
                          chkEdit.Checked ? RoiImageCanvas.InteractionMode.Edit :
                          RoiImageCanvas.InteractionMode.View;
            SetStatus(canvas.Mode == RoiImageCanvas.InteractionMode.Create ?
                "Create ROI: Click to set points, double-click to finish. Hold Ctrl to lock horizontal/vertical orientation." :
                "Ready");
            
        }

        private void chkEdit_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEdit.Checked) chkCreate.Checked = false;
            canvas.Mode = chkEdit.Checked ? RoiImageCanvas.InteractionMode.Edit :
                          chkCreate.Checked ? RoiImageCanvas.InteractionMode.Create :
                          RoiImageCanvas.InteractionMode.View;
            SetStatus(canvas.Mode == RoiImageCanvas.InteractionMode.Edit ?
                "Edit ROI: Click ROI to select, drag handle to edit, drag within ROI to move." :
                "Ready");
        }

        private void canvas_StatusChanged(object sender, string e)
        {
            SetStatus(e);
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null) statusLabel.Text = message ?? string.Empty;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap Image => canvas.Image;

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ManualMode
        {
            get => canvas.ManualMode;
            set => canvas.ManualMode = value;
        }

        [Category("Behavior")]
        [DefaultValue(1.0)]
        public double ManualScaleToFull
        {
            get => canvas.ManualScaleToFull;
            set => canvas.ManualScaleToFull = value;
        }

        public event EventHandler<RoiImageCanvas.ManualMeasureEventArgs> ManualMeasureChanged
        {
            add => canvas.ManualMeasureChanged += value;
            remove => canvas.ManualMeasureChanged -= value;
        }
        public event EventHandler RoiCollectionChanged
        {
            add => canvas.RoiCollectionChanged += value;
            remove => canvas.RoiCollectionChanged -= value;
        }
        public event EventHandler SelectedRoiChanged
        {
            add => canvas.SelectedRoiChanged += value;
            remove => canvas.SelectedRoiChanged -= value;
        }

        public void SetImage(Bitmap image, bool preserveView = true) => canvas.SetImage(image, preserveView);
        public void ResetView() => canvas.ResetView();
        public IReadOnlyList<RoiPolygon> GetRois() => canvas.GetRois();
        public void SetRois(IEnumerable<RoiPolygon> rois) => canvas.SetRois(rois);
        public void ClearRois() => canvas.ClearRois();
        public int SelectedRoiIndex => canvas.SelectedRoiIndex;
        public bool SelectRoiByIndex(int index) => canvas.SelectRoiByIndex(index);
        public bool DeleteRoiByIndex(int index) => canvas.DeleteRoiByIndex(index);
        public void SaveRoisToJson(string filePath)
        {
            var rois = canvas.GetRois().ToList();
            RoiJsonStorage.Save(filePath, rois);
            SetStatus($"Saved {rois.Count} ROI -> {Path.GetFileName(filePath)}");
        }

        public void LoadRoisFromJson(string filePath)
        {
            var rois = RoiJsonStorage.Load(filePath);
            canvas.SetRois(rois);
            SetStatus($"Loaded {rois.Count} ROI <- {Path.GetFileName(filePath)}");
        }

    }

    internal static class RoiJsonStorage
    {
        public static void Save(string filePath, List<RoiPolygon> rois)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".");
            var serializer = new DataContractJsonSerializer(typeof(List<RoiPolygon>));
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                serializer.WriteObject(fs, rois ?? new List<RoiPolygon>());
            }
        }

        public static List<RoiPolygon> Load(string filePath)
        {
            if (!File.Exists(filePath)) return new List<RoiPolygon>();
            var serializer = new DataContractJsonSerializer(typeof (List<RoiPolygon>));
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (serializer.ReadObject(fs) as List<RoiPolygon>) ?? new List<RoiPolygon>();
            }
        }
    }
}
