namespace SmoothPursuit
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
            this.pcbControl = new System.Windows.Forms.PictureBox();
            this.lblTargetColor = new System.Windows.Forms.Label();
            this.lblColor = new System.Windows.Forms.Label();
            this.sfdSaveData = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pcbControl)).BeginInit();
            this.SuspendLayout();
            // 
            // pcbControl
            // 
            this.pcbControl.Location = new System.Drawing.Point(0, 0);
            this.pcbControl.Margin = new System.Windows.Forms.Padding(0);
            this.pcbControl.Name = "pcbControl";
            this.pcbControl.Size = new System.Drawing.Size(500, 500);
            this.pcbControl.TabIndex = 0;
            this.pcbControl.TabStop = false;
            this.pcbControl.Paint += new System.Windows.Forms.PaintEventHandler(this.pcbControl_Paint);
            // 
            // lblTargetColor
            // 
            this.lblTargetColor.BackColor = System.Drawing.Color.Red;
            this.lblTargetColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTargetColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTargetColor.Location = new System.Drawing.Point(200, 195);
            this.lblTargetColor.Name = "lblTargetColor";
            this.lblTargetColor.Size = new System.Drawing.Size(100, 50);
            this.lblTargetColor.TabIndex = 1;
            this.lblTargetColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblColor
            // 
            this.lblColor.BackColor = System.Drawing.Color.White;
            this.lblColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblColor.Location = new System.Drawing.Point(200, 255);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(100, 50);
            this.lblColor.TabIndex = 2;
            this.lblColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sfdSaveData
            // 
            this.sfdSaveData.DefaultExt = "txt";
            this.sfdSaveData.Filter = "Text files|*.txt";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 500);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.lblTargetColor);
            this.Controls.Add(this.pcbControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "Smooth Volume";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pcbControl)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pcbControl;
        private System.Windows.Forms.Label lblTargetColor;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.SaveFileDialog sfdSaveData;
    }
}

