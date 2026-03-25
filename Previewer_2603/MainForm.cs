using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Previewer_2603.Controls;

namespace Previewer_2603
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            viewer.RoiCollectionChanged += Viewer_RoiCollectionChanged;
            viewer.SelectedRoiChanged += Viewer_SelectedRoiChanged;
            lstb_roi.SelectedIndexChanged += lstb_roi_SelectedIndexChanged;
            lstb_roi.KeyDown += lstb_roi_KeyDown;
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

                using (var loaded = new Bitmap(txtb_filename.Text))
                {
                    viewer.SetImage(new Bitmap(loaded), false);
                }
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

        private static string FormatRoiDisplay(RoiPolygon roi)
        {
            if (roi == null) return string.Empty;
            return $"{roi.Name} ({roi.Points.Count} pts)";
        }
    }
}
