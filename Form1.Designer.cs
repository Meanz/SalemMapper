using System.Windows.Forms;

namespace SalemMapper
{
    partial class Form1
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
            this.status = new System.Windows.Forms.StatusStrip();
            this.pnlMap = new SalemMapper.MapPanel();
            this.lblUnproc = new System.Windows.Forms.ToolStripStatusLabel();
            this.pgbTotal = new System.Windows.Forms.ToolStripProgressBar();
            this.lblBackproc = new System.Windows.Forms.ToolStripStatusLabel();
            this.status.SuspendLayout();
            this.SuspendLayout();
            // 
            // status
            // 
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pgbTotal,
            this.lblUnproc,
            this.lblBackproc});
            this.status.Location = new System.Drawing.Point(0, 525);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(670, 22);
            this.status.TabIndex = 0;
            this.status.Text = "statusStrip1";
            // 
            // pnlMap
            // 
            this.pnlMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMap.AutoSize = true;
            this.pnlMap.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pnlMap.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.pnlMap.Location = new System.Drawing.Point(12, 12);
            this.pnlMap.Name = "pnlMap";
            this.pnlMap.Size = new System.Drawing.Size(646, 523);
            this.pnlMap.TabIndex = 0;
            // 
            // lblUnproc
            // 
            this.lblUnproc.Name = "lblUnproc";
            this.lblUnproc.Size = new System.Drawing.Size(118, 17);
            this.lblUnproc.Text = "toolStripStatusLabel1";
            // 
            // pgbTotal
            // 
            this.pgbTotal.Name = "pgbTotal";
            this.pgbTotal.Size = new System.Drawing.Size(100, 16);
            // 
            // lblBackproc
            // 
            this.lblBackproc.Name = "lblBackproc";
            this.lblBackproc.Size = new System.Drawing.Size(118, 17);
            this.lblBackproc.Text = "toolStripStatusLabel2";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 547);
            this.Controls.Add(this.status);
            this.Controls.Add(this.pnlMap);
            this.Name = "Form1";
            this.Text = "Form1";
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MapPanel pnlMap;
        private StatusStrip status;
        private ToolStripProgressBar pgbTotal;
        private ToolStripStatusLabel lblUnproc;
        private ToolStripStatusLabel lblBackproc;
    }
}

