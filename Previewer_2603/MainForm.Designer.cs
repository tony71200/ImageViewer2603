namespace Previewer_2603
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.openDlg_btn = new System.Windows.Forms.Button();
            this.txtb_filename = new System.Windows.Forms.TextBox();
            this.btn_loadROI = new System.Windows.Forms.Button();
            this.btn_saveROI = new System.Windows.Forms.Button();
            this.tagContainer = new System.Windows.Forms.TabControl();
            this.tabImageCanvas = new System.Windows.Forms.TabPage();
            this.imageCanvas = new Previewer_2603.Controls.ImageCanvas();
            this.tabAlignmentCanvas = new System.Windows.Forms.TabPage();
            this.splitContainerAlignment = new System.Windows.Forms.SplitContainer();
            this.alignmentCanvas = new Previewer_2603.Controls.AlignmentImageCanvas();
            this.alignmentInfo = new System.Windows.Forms.Label();
            this.chkManualAlign = new System.Windows.Forms.CheckBox();
            this.tabRoiCanvas = new System.Windows.Forms.TabPage();
            this.splitContainerRoi = new System.Windows.Forms.SplitContainer();
            this.viewer = new Previewer_2603.Controls.RoiImageViewerControl();
            this.lstb_roi = new System.Windows.Forms.ListBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.tagContainer.SuspendLayout();
            this.tabImageCanvas.SuspendLayout();
            this.tabAlignmentCanvas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAlignment)).BeginInit();
            this.splitContainerAlignment.Panel1.SuspendLayout();
            this.splitContainerAlignment.Panel2.SuspendLayout();
            this.splitContainerAlignment.SuspendLayout();
            this.tabRoiCanvas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRoi)).BeginInit();
            this.splitContainerRoi.Panel1.SuspendLayout();
            this.splitContainerRoi.Panel2.SuspendLayout();
            this.splitContainerRoi.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.openDlg_btn);
            this.flowLayoutPanel1.Controls.Add(this.txtb_filename);
            this.flowLayoutPanel1.Controls.Add(this.btn_loadROI);
            this.flowLayoutPanel1.Controls.Add(this.btn_saveROI);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(980, 34);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // openDlg_btn
            // 
            this.openDlg_btn.Location = new System.Drawing.Point(3, 3);
            this.openDlg_btn.Name = "openDlg_btn";
            this.openDlg_btn.Size = new System.Drawing.Size(75, 23);
            this.openDlg_btn.TabIndex = 0;
            this.openDlg_btn.Text = "Open";
            this.openDlg_btn.UseVisualStyleBackColor = true;
            this.openDlg_btn.Click += new System.EventHandler(this.openDlg_btn_Click);
            // 
            // txtb_filename
            // 
            this.txtb_filename.Location = new System.Drawing.Point(84, 3);
            this.txtb_filename.Name = "txtb_filename";
            this.txtb_filename.ReadOnly = true;
            this.txtb_filename.Size = new System.Drawing.Size(600, 22);
            this.txtb_filename.TabIndex = 1;
            // 
            // btn_loadROI
            // 
            this.btn_loadROI.Location = new System.Drawing.Point(690, 3);
            this.btn_loadROI.Name = "btn_loadROI";
            this.btn_loadROI.Size = new System.Drawing.Size(95, 23);
            this.btn_loadROI.TabIndex = 2;
            this.btn_loadROI.Text = "Load ROI";
            this.btn_loadROI.UseVisualStyleBackColor = true;
            this.btn_loadROI.Click += new System.EventHandler(this.btn_loadROI_Click);
            // 
            // btn_saveROI
            // 
            this.btn_saveROI.Location = new System.Drawing.Point(791, 3);
            this.btn_saveROI.Name = "btn_saveROI";
            this.btn_saveROI.Size = new System.Drawing.Size(95, 23);
            this.btn_saveROI.TabIndex = 3;
            this.btn_saveROI.Text = "Save ROI";
            this.btn_saveROI.UseVisualStyleBackColor = true;
            this.btn_saveROI.Click += new System.EventHandler(this.btn_saveROI_Click);
            // 
            // tagContainer
            // 
            this.tagContainer.Controls.Add(this.tabImageCanvas);
            this.tagContainer.Controls.Add(this.tabAlignmentCanvas);
            this.tagContainer.Controls.Add(this.tabRoiCanvas);
            this.tagContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tagContainer.Location = new System.Drawing.Point(0, 34);
            this.tagContainer.Name = "tagContainer";
            this.tagContainer.SelectedIndex = 0;
            this.tagContainer.Size = new System.Drawing.Size(980, 506);
            this.tagContainer.TabIndex = 1;
            // 
            // tabImageCanvas
            // 
            this.tabImageCanvas.Controls.Add(this.imageCanvas);
            this.tabImageCanvas.Location = new System.Drawing.Point(4, 22);
            this.tabImageCanvas.Name = "tabImageCanvas";
            this.tabImageCanvas.Padding = new System.Windows.Forms.Padding(3);
            this.tabImageCanvas.Size = new System.Drawing.Size(972, 480);
            this.tabImageCanvas.TabIndex = 0;
            this.tabImageCanvas.Text = "Image Canvas";
            this.tabImageCanvas.UseVisualStyleBackColor = true;
            // 
            // imageCanvas
            // 
            this.imageCanvas.BackColor = System.Drawing.Color.Black;
            this.imageCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageCanvas.Location = new System.Drawing.Point(3, 3);
            this.imageCanvas.ManualMode = false;
            this.imageCanvas.ManualScaleToFull = 1D;
            this.imageCanvas.Name = "imageCanvas";
            this.imageCanvas.Size = new System.Drawing.Size(966, 474);
            this.imageCanvas.TabIndex = 0;
            this.imageCanvas.Text = "imageCanvas";
            // 
            // tabAlignmentCanvas
            // 
            this.tabAlignmentCanvas.Controls.Add(this.splitContainerAlignment);
            this.tabAlignmentCanvas.Location = new System.Drawing.Point(4, 22);
            this.tabAlignmentCanvas.Name = "tabAlignmentCanvas";
            this.tabAlignmentCanvas.Padding = new System.Windows.Forms.Padding(3);
            this.tabAlignmentCanvas.Size = new System.Drawing.Size(972, 480);
            this.tabAlignmentCanvas.TabIndex = 1;
            this.tabAlignmentCanvas.Text = "Alignment Canvas";
            this.tabAlignmentCanvas.UseVisualStyleBackColor = true;
            // 
            // splitContainerAlignment
            // 
            this.splitContainerAlignment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerAlignment.Location = new System.Drawing.Point(3, 3);
            this.splitContainerAlignment.Name = "splitContainerAlignment";
            // 
            // splitContainerAlignment.Panel1
            // 
            this.splitContainerAlignment.Panel1.Controls.Add(this.alignmentCanvas);
            // 
            // splitContainerAlignment.Panel2
            // 
            this.splitContainerAlignment.Panel2.Controls.Add(this.alignmentInfo);
            this.splitContainerAlignment.Panel2.Controls.Add(this.chkManualAlign);
            this.splitContainerAlignment.Size = new System.Drawing.Size(966, 474);
            this.splitContainerAlignment.SplitterDistance = 744;
            this.splitContainerAlignment.TabIndex = 0;
            // 
            // alignmentCanvas
            // 
            this.alignmentCanvas.BackColor = System.Drawing.Color.Black;
            this.alignmentCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alignmentCanvas.Location = new System.Drawing.Point(0, 0);
            this.alignmentCanvas.ManualMode = false;
            this.alignmentCanvas.ManualScaleToFull = 1D;
            this.alignmentCanvas.Mode = Previewer_2603.Controls.AlignmentImageCanvas.AlignMode.View;
            this.alignmentCanvas.Name = "alignmentCanvas";
            this.alignmentCanvas.Size = new System.Drawing.Size(744, 474);
            this.alignmentCanvas.TabIndex = 0;
            this.alignmentCanvas.Text = "alignmentCanvas";
            // 
            // alignmentInfo
            // 
            this.alignmentInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.alignmentInfo.Location = new System.Drawing.Point(0, 25);
            this.alignmentInfo.Name = "alignmentInfo";
            this.alignmentInfo.Padding = new System.Windows.Forms.Padding(8);
            this.alignmentInfo.Size = new System.Drawing.Size(218, 131);
            this.alignmentInfo.TabIndex = 1;
            this.alignmentInfo.Text = "Pivot: -\r\nOffset: (0, 0)\r\nAngle: 0";
            // 
            // chkManualAlign
            // 
            this.chkManualAlign.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkManualAlign.Location = new System.Drawing.Point(0, 0);
            this.chkManualAlign.Name = "chkManualAlign";
            this.chkManualAlign.Padding = new System.Windows.Forms.Padding(8, 2, 0, 0);
            this.chkManualAlign.Size = new System.Drawing.Size(218, 25);
            this.chkManualAlign.TabIndex = 0;
            this.chkManualAlign.Text = "Manual Alignment Mode";
            this.chkManualAlign.UseVisualStyleBackColor = true;
            this.chkManualAlign.CheckedChanged += new System.EventHandler(this.chkManualAlign_CheckedChanged);
            // 
            // tabRoiCanvas
            // 
            this.tabRoiCanvas.Controls.Add(this.splitContainerRoi);
            this.tabRoiCanvas.Location = new System.Drawing.Point(4, 22);
            this.tabRoiCanvas.Name = "tabRoiCanvas";
            this.tabRoiCanvas.Padding = new System.Windows.Forms.Padding(3);
            this.tabRoiCanvas.Size = new System.Drawing.Size(972, 480);
            this.tabRoiCanvas.TabIndex = 2;
            this.tabRoiCanvas.Text = "ROI Canvas";
            this.tabRoiCanvas.UseVisualStyleBackColor = true;
            // 
            // splitContainerRoi
            // 
            this.splitContainerRoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRoi.Location = new System.Drawing.Point(3, 3);
            this.splitContainerRoi.Name = "splitContainerRoi";
            // 
            // splitContainerRoi.Panel1
            // 
            this.splitContainerRoi.Panel1.Controls.Add(this.viewer);
            // 
            // splitContainerRoi.Panel2
            // 
            this.splitContainerRoi.Panel2.Controls.Add(this.lstb_roi);
            this.splitContainerRoi.Size = new System.Drawing.Size(966, 474);
            this.splitContainerRoi.SplitterDistance = 779;
            this.splitContainerRoi.TabIndex = 0;
            // 
            // viewer
            // 
            this.viewer.AutoSize = true;
            this.viewer.BackColor = System.Drawing.Color.Transparent;
            this.viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewer.Location = new System.Drawing.Point(0, 0);
            this.viewer.Name = "viewer";
            this.viewer.Size = new System.Drawing.Size(779, 474);
            this.viewer.TabIndex = 0;
            // 
            // lstb_roi
            // 
            this.lstb_roi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstb_roi.FormattingEnabled = true;
            this.lstb_roi.ItemHeight = 12;
            this.lstb_roi.Location = new System.Drawing.Point(0, 0);
            this.lstb_roi.Name = "lstb_roi";
            this.lstb_roi.Size = new System.Drawing.Size(183, 474);
            this.lstb_roi.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 540);
            this.Controls.Add(this.tagContainer);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "Previewer (industrial)";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tagContainer.ResumeLayout(false);
            this.tabImageCanvas.ResumeLayout(false);
            this.tabAlignmentCanvas.ResumeLayout(false);
            this.splitContainerAlignment.Panel1.ResumeLayout(false);
            this.splitContainerAlignment.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAlignment)).EndInit();
            this.splitContainerAlignment.ResumeLayout(false);
            this.tabRoiCanvas.ResumeLayout(false);
            this.splitContainerRoi.Panel1.ResumeLayout(false);
            this.splitContainerRoi.Panel1.PerformLayout();
            this.splitContainerRoi.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRoi)).EndInit();
            this.splitContainerRoi.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button openDlg_btn;
        private System.Windows.Forms.TextBox txtb_filename;
        private System.Windows.Forms.Button btn_loadROI;
        private System.Windows.Forms.Button btn_saveROI;
        private System.Windows.Forms.TabControl tagContainer;
        private System.Windows.Forms.TabPage tabImageCanvas;
        private System.Windows.Forms.TabPage tabAlignmentCanvas;
        private System.Windows.Forms.TabPage tabRoiCanvas;
        private Controls.ImageCanvas imageCanvas;
        private System.Windows.Forms.SplitContainer splitContainerAlignment;
        private Controls.AlignmentImageCanvas alignmentCanvas;
        private System.Windows.Forms.CheckBox chkManualAlign;
        private System.Windows.Forms.Label alignmentInfo;
        private System.Windows.Forms.SplitContainer splitContainerRoi;
        private Controls.RoiImageViewerControl viewer;
        private System.Windows.Forms.ListBox lstb_roi;
    }
}
