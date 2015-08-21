namespace SmoothVolume
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
            this.pcbKnob = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pcbKnob)).BeginInit();
            this.SuspendLayout();
            // 
            // pcbKnob
            // 
            this.pcbKnob.Image = global::SmoothVolume.Properties.Resources.knob;
            this.pcbKnob.Location = new System.Drawing.Point(0, 0);
            this.pcbKnob.Name = "pcbKnob";
            this.pcbKnob.Size = new System.Drawing.Size(500, 500);
            this.pcbKnob.TabIndex = 0;
            this.pcbKnob.TabStop = false;
            this.pcbKnob.Paint += new System.Windows.Forms.PaintEventHandler(this.pcbKnob_Paint);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 500);
            this.Controls.Add(this.pcbKnob);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "Smooth Volume";
            ((System.ComponentModel.ISupportInitialize)(this.pcbKnob)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pcbKnob;
    }
}

