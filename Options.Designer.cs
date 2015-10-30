namespace SmoothPursuit
{
    partial class Options
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gpbPursueDetector = new System.Windows.Forms.GroupBox();
            this.rdbPursueDetector_OffsetXY = new System.Windows.Forms.RadioButton();
            this.rdbPursueDetector_OffsetDist = new System.Windows.Forms.RadioButton();
            this.gpbWidget = new System.Windows.Forms.GroupBox();
            this.rdbWidgetStatic = new System.Windows.Forms.RadioButton();
            this.rdbWidgetScrollbar = new System.Windows.Forms.RadioButton();
            this.rdbWidgetKnob = new System.Windows.Forms.RadioButton();
            this.gpbPursueDetector.SuspendLayout();
            this.gpbWidget.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(18, 203);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(109, 203);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // gpbPursueDetector
            // 
            this.gpbPursueDetector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gpbPursueDetector.Controls.Add(this.rdbPursueDetector_OffsetXY);
            this.gpbPursueDetector.Controls.Add(this.rdbPursueDetector_OffsetDist);
            this.gpbPursueDetector.Location = new System.Drawing.Point(12, 12);
            this.gpbPursueDetector.Name = "gpbPursueDetector";
            this.gpbPursueDetector.Size = new System.Drawing.Size(183, 73);
            this.gpbPursueDetector.TabIndex = 2;
            this.gpbPursueDetector.TabStop = false;
            this.gpbPursueDetector.Text = "Pursue detector";
            // 
            // rdbPursueDetector_OffsetXY
            // 
            this.rdbPursueDetector_OffsetXY.AutoSize = true;
            this.rdbPursueDetector_OffsetXY.Location = new System.Drawing.Point(6, 42);
            this.rdbPursueDetector_OffsetXY.Name = "rdbPursueDetector_OffsetXY";
            this.rdbPursueDetector_OffsetXY.Size = new System.Drawing.Size(76, 17);
            this.rdbPursueDetector_OffsetXY.TabIndex = 1;
            this.rdbPursueDetector_OffsetXY.TabStop = true;
            this.rdbPursueDetector_OffsetXY.Text = "Offset, X-Y";
            this.rdbPursueDetector_OffsetXY.UseVisualStyleBackColor = true;
            // 
            // rdbPursueDetector_OffsetDist
            // 
            this.rdbPursueDetector_OffsetDist.AutoSize = true;
            this.rdbPursueDetector_OffsetDist.Location = new System.Drawing.Point(6, 19);
            this.rdbPursueDetector_OffsetDist.Name = "rdbPursueDetector_OffsetDist";
            this.rdbPursueDetector_OffsetDist.Size = new System.Drawing.Size(99, 17);
            this.rdbPursueDetector_OffsetDist.TabIndex = 0;
            this.rdbPursueDetector_OffsetDist.TabStop = true;
            this.rdbPursueDetector_OffsetDist.Text = "Offset, distance";
            this.rdbPursueDetector_OffsetDist.UseVisualStyleBackColor = true;
            // 
            // gpbWidget
            // 
            this.gpbWidget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gpbWidget.Controls.Add(this.rdbWidgetStatic);
            this.gpbWidget.Controls.Add(this.rdbWidgetScrollbar);
            this.gpbWidget.Controls.Add(this.rdbWidgetKnob);
            this.gpbWidget.Location = new System.Drawing.Point(12, 91);
            this.gpbWidget.Name = "gpbWidget";
            this.gpbWidget.Size = new System.Drawing.Size(183, 95);
            this.gpbWidget.TabIndex = 3;
            this.gpbWidget.TabStop = false;
            this.gpbWidget.Text = "Widget";
            // 
            // rdbWidgetStatic
            // 
            this.rdbWidgetStatic.AutoSize = true;
            this.rdbWidgetStatic.Location = new System.Drawing.Point(6, 65);
            this.rdbWidgetStatic.Name = "rdbWidgetStatic";
            this.rdbWidgetStatic.Size = new System.Drawing.Size(52, 17);
            this.rdbWidgetStatic.TabIndex = 2;
            this.rdbWidgetStatic.TabStop = true;
            this.rdbWidgetStatic.Text = "Static";
            this.rdbWidgetStatic.UseVisualStyleBackColor = true;
            // 
            // rdbWidgetScrollbar
            // 
            this.rdbWidgetScrollbar.AutoSize = true;
            this.rdbWidgetScrollbar.Location = new System.Drawing.Point(6, 42);
            this.rdbWidgetScrollbar.Name = "rdbWidgetScrollbar";
            this.rdbWidgetScrollbar.Size = new System.Drawing.Size(66, 17);
            this.rdbWidgetScrollbar.TabIndex = 1;
            this.rdbWidgetScrollbar.TabStop = true;
            this.rdbWidgetScrollbar.Text = "Scrollbar";
            this.rdbWidgetScrollbar.UseVisualStyleBackColor = true;
            // 
            // rdbWidgetKnob
            // 
            this.rdbWidgetKnob.AutoSize = true;
            this.rdbWidgetKnob.Location = new System.Drawing.Point(6, 19);
            this.rdbWidgetKnob.Name = "rdbWidgetKnob";
            this.rdbWidgetKnob.Size = new System.Drawing.Size(50, 17);
            this.rdbWidgetKnob.TabIndex = 0;
            this.rdbWidgetKnob.TabStop = true;
            this.rdbWidgetKnob.Text = "Knob";
            this.rdbWidgetKnob.UseVisualStyleBackColor = true;
            // 
            // Options
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(207, 238);
            this.Controls.Add(this.gpbWidget);
            this.Controls.Add(this.gpbPursueDetector);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Options";
            this.Text = "Options";
            this.gpbPursueDetector.ResumeLayout(false);
            this.gpbPursueDetector.PerformLayout();
            this.gpbWidget.ResumeLayout(false);
            this.gpbWidget.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gpbPursueDetector;
        private System.Windows.Forms.RadioButton rdbPursueDetector_OffsetXY;
        private System.Windows.Forms.RadioButton rdbPursueDetector_OffsetDist;
        private System.Windows.Forms.GroupBox gpbWidget;
        private System.Windows.Forms.RadioButton rdbWidgetStatic;
        private System.Windows.Forms.RadioButton rdbWidgetScrollbar;
        private System.Windows.Forms.RadioButton rdbWidgetKnob;
    }
}