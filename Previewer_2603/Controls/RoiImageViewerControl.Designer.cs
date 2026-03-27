namespace Previewer_2603.Controls
{
    partial class RoiImageViewerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.topPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.chkCreate = new System.Windows.Forms.CheckBox();
            this.chkEdit = new System.Windows.Forms.CheckBox();
            this.roiTypeCmb = new System.Windows.Forms.ComboBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.Coordinate_lbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.canvas = new Previewer_2603.Controls.RoiImageCanvas();
            this.topPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.chkCreate);
            this.topPanel.Controls.Add(this.chkEdit);
            this.topPanel.Controls.Add(this.roiTypeCmb);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.topPanel.Size = new System.Drawing.Size(640, 30);
            this.topPanel.TabIndex = 0;
            // 
            // chkCreate
            // 
            this.chkCreate.AutoSize = true;
            this.chkCreate.BackColor = System.Drawing.Color.LightGreen;
            this.chkCreate.Location = new System.Drawing.Point(11, 5);
            this.chkCreate.Name = "chkCreate";
            this.chkCreate.Size = new System.Drawing.Size(54, 16);
            this.chkCreate.TabIndex = 0;
            this.chkCreate.Text = "Create";
            this.chkCreate.UseVisualStyleBackColor = true;
            this.chkCreate.CheckedChanged += new System.EventHandler(this.chkCreate_CheckedChanged);
            // 
            // chkEdit
            // 
            this.chkEdit.AutoSize = true;
            this.chkEdit.BackColor = System.Drawing.Color.LightBlue;
            this.chkEdit.Location = new System.Drawing.Point(71, 5);
            this.chkEdit.Name = "chkEdit";
            this.chkEdit.Size = new System.Drawing.Size(43, 16);
            this.chkEdit.TabIndex = 1;
            this.chkEdit.Text = "Edit";
            this.chkEdit.UseVisualStyleBackColor = true;
            this.chkEdit.CheckedChanged += new System.EventHandler(this.chkEdit_CheckedChanged);
            // 
            // roiTypeCmb
            // 
            this.roiTypeCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.roiTypeCmb.FormattingEnabled = true;
            this.roiTypeCmb.Location = new System.Drawing.Point(120, 3);
            this.roiTypeCmb.Name = "roiTypeCmb";
            this.roiTypeCmb.Size = new System.Drawing.Size(110, 20);
            this.roiTypeCmb.TabIndex = 2;
            this.roiTypeCmb.SelectedIndexChanged += new System.EventHandler(this.roiTypeCmb_SelectedIndexChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.Coordinate_lbl});
            this.statusStrip.Location = new System.Drawing.Point(0, 458);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(640, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // Coordinate_lbl
            // 
            this.Coordinate_lbl.Name = "Coordinate_lbl";
            this.Coordinate_lbl.Size = new System.Drawing.Size(57, 17);
            this.Coordinate_lbl.Text = "X: -, Y: -";
            // 
            // canvas
            // 
            this.canvas.BackColor = System.Drawing.Color.Black;
            this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvas.Location = new System.Drawing.Point(0, 30);
            this.canvas.CreateShape = Previewer_2603.Controls.RoiShapeKind.Polygon;
            this.canvas.ManualMode = false;
            this.canvas.ManualScaleToFull = 1D;
            this.canvas.Mode = Previewer_2603.Controls.RoiImageCanvas.InteractionMode.View;
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(640, 428);
            this.canvas.TabIndex = 2;
            this.canvas.Text = "roiImageCanvas1";
            this.canvas.ImageCoordinateChanged += new System.EventHandler<Previewer_2603.Controls.RoiImageCanvas.ImageCoordinateEventArgs>(this.canvas_ImageCoordinateChanged);
            this.canvas.StatusChanged += new System.EventHandler<string>(this.canvas_StatusChanged);
            // 
            // RoiImageViewerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.canvas);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.topPanel);
            this.Name = "RoiImageViewerControl";
            this.Size = new System.Drawing.Size(640, 480);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel topPanel;
        private System.Windows.Forms.CheckBox chkCreate;
        private System.Windows.Forms.CheckBox chkEdit;
        private System.Windows.Forms.ComboBox roiTypeCmb;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel Coordinate_lbl;
        private RoiImageCanvas canvas;
    }
}
