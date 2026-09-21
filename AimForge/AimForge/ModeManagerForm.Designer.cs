namespace AimForge
{
    partial class ModeManagerForm
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
            dataGridView1 = new DataGridView();
            label1 = new Label();
            txtName = new TextBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            txtSize = new TextBox();
            txtSpawn = new TextBox();
            txtLifetime = new TextBox();
            txtDuration = new TextBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(55, 48);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(324, 94);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(55, 156);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 1;
            label1.Text = "Name";
            // 
            // txtName
            // 
            txtName.Location = new Point(139, 148);
            txtName.Name = "txtName";
            txtName.Size = new Size(100, 23);
            txtName.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(55, 200);
            label2.Name = "label2";
            label2.Size = new Size(27, 15);
            label2.TabIndex = 3;
            label2.Text = "Size";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(55, 243);
            label3.Name = "label3";
            label3.Size = new Size(61, 15);
            label3.TabIndex = 4;
            label3.Text = "Spawn ms";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(55, 284);
            label4.Name = "label4";
            label4.Size = new Size(50, 15);
            label4.TabIndex = 5;
            label4.Text = "Lifetime";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(55, 327);
            label5.Name = "label5";
            label5.Size = new Size(53, 15);
            label5.TabIndex = 6;
            label5.Text = "Duration";
            // 
            // txtSize
            // 
            txtSize.Location = new Point(139, 192);
            txtSize.Name = "txtSize";
            txtSize.Size = new Size(100, 23);
            txtSize.TabIndex = 7;
            // 
            // txtSpawn
            // 
            txtSpawn.Location = new Point(139, 235);
            txtSpawn.Name = "txtSpawn";
            txtSpawn.Size = new Size(100, 23);
            txtSpawn.TabIndex = 8;
            // 
            // txtLifetime
            // 
            txtLifetime.Location = new Point(139, 276);
            txtLifetime.Name = "txtLifetime";
            txtLifetime.Size = new Size(100, 23);
            txtLifetime.TabIndex = 9;
            txtLifetime.TextChanged += txtLIfetIme_TextChanged;
            // 
            // txtDuration
            // 
            txtDuration.Location = new Point(139, 319);
            txtDuration.Name = "txtDuration";
            txtDuration.Size = new Size(100, 23);
            txtDuration.TabIndex = 10;
            // 
            // button1
            // 
            button1.Location = new Point(55, 375);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 11;
            button1.Text = "Add";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(139, 375);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 12;
            button2.Text = "Update";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(228, 375);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 13;
            button3.Text = "Delete";
            button3.UseVisualStyleBackColor = true;
            // 
            // ModeManagerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(txtDuration);
            Controls.Add(txtLifetime);
            Controls.Add(txtSpawn);
            Controls.Add(txtSize);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtName);
            Controls.Add(label1);
            Controls.Add(dataGridView1);
            Name = "ModeManagerForm";
            Text = "Size";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Label label1;
        private TextBox txtName;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox txtSize;
        private TextBox txtSpawn;
        private TextBox txtLifetime;
        private TextBox txtDuration;
        private Button button1;
        private Button button2;
        private Button button3;
    }
}