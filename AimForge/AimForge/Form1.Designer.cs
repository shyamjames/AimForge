namespace AimForge
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cboModes = new ComboBox();
            btnPlay = new Button();
            btnModes = new Button();
            btnStats = new Button();
            SuspendLayout();
            // 
            // cboModes
            // 
            cboModes.FormattingEnabled = true;
            cboModes.Location = new Point(315, 181);
            cboModes.Name = "cboModes";
            cboModes.Size = new Size(121, 23);
            cboModes.TabIndex = 0;
            cboModes.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(225, 256);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(75, 23);
            btnPlay.TabIndex = 1;
            btnPlay.Text = "Play";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // btnModes
            // 
            btnModes.Location = new Point(343, 256);
            btnModes.Name = "btnModes";
            btnModes.Size = new Size(75, 23);
            btnModes.TabIndex = 2;
            btnModes.Text = "Modes";
            btnModes.UseVisualStyleBackColor = true;
            btnModes.Click += btnModes_Click;
            // 
            // btnStats
            // 
            btnStats.Location = new Point(450, 256);
            btnStats.Name = "btnStats";
            btnStats.Size = new Size(75, 23);
            btnStats.TabIndex = 3;
            btnStats.Text = "Stats";
            btnStats.UseVisualStyleBackColor = true;
            btnStats.Click += btnModes_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnStats);
            Controls.Add(btnModes);
            Controls.Add(btnPlay);
            Controls.Add(cboModes);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private ComboBox cboModes;
        private Button btnPlay;
        private Button btnModes;
        private Button btnStats;
    }
}
