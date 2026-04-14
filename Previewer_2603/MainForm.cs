using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Previewer_2603.Controls;

namespace Previewer_2603
{
    public partial class MainForm : Form
    {
        private string Filename = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            viewer.RoiCollectionChanged += Viewer_RoiCollectionChanged;
            viewer.SelectedRoiChanged += Viewer_SelectedRoiChanged;
            lstb_roi.SelectedIndexChanged += lstb_roi_SelectedIndexChanged;
            lstb_roi.KeyDown += lstb_roi_KeyDown;
            alignmentCanvas.AlignmentChanged += alignmentCanvas_AlignmentChanged;
        }

        private void openDlg_btn_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff",
                Title = "Select an image"
            })
            {
                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                txtb_filename.Text = openFileDialog.FileName;
                Filename = Path.ChangeExtension(openFileDialog.FileName, null);

                using (var loaded = new Bitmap(openFileDialog.FileName))
                {
                    imageCanvas.SetImage(new Bitmap(loaded), false);
                    alignmentCanvas.SetReferenceImage(new Bitmap(loaded));
                    alignmentCanvas.SetTestImage(new Bitmap(loaded));
                    viewer.SetImage(new Bitmap(loaded), false);
                    viewer.ClearRois();
                }

                chkManualAlign.Checked = false;
                alignmentCanvas.SetTransform(0, 0, 0);
                LoadROI(Filename);
                RefreshRoiList();
            }
        }

        private void Viewer_RoiCollectionChanged(object sender, EventArgs e)
        {
            RefreshRoiList();
        }

        private void Viewer_SelectedRoiChanged(object sender, EventArgs e)
        {
            if (lstb_roi.SelectedIndex != viewer.SelectedRoiIndex)
            {
                lstb_roi.SelectedIndex = viewer.SelectedRoiIndex;
            }
        }

        private void lstb_roi_SelectedIndexChanged(object sender, EventArgs e)
        {
            viewer.SelectRoiByIndex(lstb_roi.SelectedIndex);
        }

        private void lstb_roi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;
            if (lstb_roi.SelectedIndex < 0) return;

            viewer.DeleteRoiByIndex(lstb_roi.SelectedIndex);
            e.Handled = true;
        }

        private void RefreshRoiList()
        {
            var rois = viewer.GetRois();
            lstb_roi.BeginUpdate();
            lstb_roi.Items.Clear();
            foreach (var roi in rois)
            {
                lstb_roi.Items.Add(FormatRoiDisplay(roi));
            }
            lstb_roi.EndUpdate();

            if (viewer.SelectedRoiIndex >= 0 && viewer.SelectedRoiIndex < lstb_roi.Items.Count)
            {
                lstb_roi.SelectedIndex = viewer.SelectedRoiIndex;
            }
        }

        private void LoadROI(string pathBase)
        {
            var jsonPath = Path.HasExtension(pathBase)
                ? Path.ChangeExtension(pathBase, "json")
                : pathBase + ".json";

            if (File.Exists(jsonPath))
            {
                viewer.LoadRoisFromJson(jsonPath);
                RefreshRoiList();
            }
        }

        private void SaveROI()
        {
            var jsonPath = Path.HasExtension(Filename)
                ? Path.ChangeExtension(Filename, "json")
                : Filename + ".json";
            viewer.SaveRoisToJson(jsonPath);
        }

        private static string FormatRoiDisplay(RoiPolygon roi)
        {
            if (roi == null) return string.Empty;
            return $"{roi.Name} ({roi.Points.Count} pts)";
        }

        private void btn_loadROI_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Filename)) return;
            LoadROI(Filename);
        }

        private void btn_saveROI_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Filename)) return;
            SaveROI();
        }

        private void chkManualAlign_CheckedChanged(object sender, EventArgs e)
        {
            alignmentCanvas.Mode = chkManualAlign.Checked
                ? AlignmentImageCanvas.AlignMode.ManualAlignment
                : AlignmentImageCanvas.AlignMode.View;
        }

        private void alignmentCanvas_AlignmentChanged(object sender, AlignmentChangedEventArgs e)
        {
            var pivot = e.HasPivot ? $"({e.Pivot.X:F1}, {e.Pivot.Y:F1})" : "-";
            alignmentInfo.Text =
                $"Pivot: {pivot}{Environment.NewLine}" +
                $"Offset: ({e.OffsetX:F1}, {e.OffsetY:F1}){Environment.NewLine}" +
                $"Angle: {e.AngleDeg:F2}°";
        }
    }
}
