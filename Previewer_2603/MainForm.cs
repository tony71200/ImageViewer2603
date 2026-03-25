using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Previewer_2603
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
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
                
                viewer.SetImage((Bitmap)Image.FromFile(txtb_filename.Text), false);
            }
        }
    }
}
