namespace MTGRenderer
{
    partial class HiddenForm
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
            this.glView = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // glView
            // 
            this.glView.BackColor = System.Drawing.Color.Black;
            this.glView.Location = new System.Drawing.Point(-4, -3);
            this.glView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.glView.Name = "glView";
            this.glView.Size = new System.Drawing.Size(803, 456);
            this.glView.TabIndex = 0;
            this.glView.VSync = false;
            this.glView.Load += new System.EventHandler(this.glView_Load);
            // 
            // HiddenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.glView);
            this.Name = "HiddenForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "HiddenForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HiddenForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glView;
    }
}