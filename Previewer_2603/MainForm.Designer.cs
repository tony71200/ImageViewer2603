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
            this.viewer = new Previewer_2603.Controls.RoiImageViewerControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstb_roi = new System.Windows.Forms.ListBox();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.openDlg_btn);
            this.flowLayoutPanel1.Controls.Add(this.txtb_filename);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(800, 34);
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
            this.txtb_filename.Size = new System.Drawing.Size(347, 22);
            this.txtb_filename.TabIndex = 1;
            // 
            // viewer
            // 
            this.viewer.AutoSize = true;
            this.viewer.BackColor = System.Drawing.Color.Transparent;
            this.viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewer.Location = new System.Drawing.Point(0, 0);
            this.viewer.Name = "viewer";
            this.viewer.Size = new System.Drawing.Size(645, 416);
            this.viewer.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 34);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.viewer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lstb_roi);
            this.splitContainer1.Size = new System.Drawing.Size(800, 416);
            this.splitContainer1.SplitterDistance = 645;
            this.splitContainer1.TabIndex = 2;
            // 
            // lstb_roi
            // 
            this.lstb_roi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstb_roi.FormattingEnabled = true;
            this.lstb_roi.ItemHeight = 12;
            this.lstb_roi.Location = new System.Drawing.Point(0, 0);
            this.lstb_roi.Name = "lstb_roi";
            this.lstb_roi.Size = new System.Drawing.Size(151, 416);
            this.lstb_roi.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "Previewer (insdustrial)";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button openDlg_btn;
        private System.Windows.Forms.TextBox txtb_filename;
        private Controls.RoiImageViewerControl viewer;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstb_roi;
    }
}

