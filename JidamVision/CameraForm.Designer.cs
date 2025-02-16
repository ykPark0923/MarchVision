namespace JidamVision
{
    partial class CameraForm
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
            this.picMainView = new System.Windows.Forms.PictureBox();
            this.btnGrab = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picMainView)).BeginInit();
            this.SuspendLayout();
            // 
            // picMainView
            // 
            this.picMainView.Location = new System.Drawing.Point(13, 23);
            this.picMainView.Name = "picMainView";
            this.picMainView.Size = new System.Drawing.Size(455, 241);
            this.picMainView.TabIndex = 0;
            this.picMainView.TabStop = false;
            // 
            // btnGrab
            // 
            this.btnGrab.Location = new System.Drawing.Point(13, 270);
            this.btnGrab.Name = "btnGrab";
            this.btnGrab.Size = new System.Drawing.Size(75, 23);
            this.btnGrab.TabIndex = 1;
            this.btnGrab.Text = "Grab";
            this.btnGrab.UseVisualStyleBackColor = true;
            this.btnGrab.Click += new System.EventHandler(this.btnGrab_Click);
            // 
            // CameraForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 304);
            this.Controls.Add(this.btnGrab);
            this.Controls.Add(this.picMainView);
            this.Name = "CameraForm";
            this.Text = "CameraForm";
            ((System.ComponentModel.ISupportInitialize)(this.picMainView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picMainView;
        private System.Windows.Forms.Button btnGrab;
    }
}