namespace MlogCompiler
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
            if(disposing && (components != null))
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.compileButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.importButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.AcceptsTab = true;
            this.textBox1.Location = new System.Drawing.Point(12, 32);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(385, 348);
            this.textBox1.TabIndex = 0;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(403, 32);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(385, 348);
            this.textBox2.TabIndex = 1;
            this.textBox2.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Code";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(403, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Compiled";
            // 
            // compileButton
            // 
            this.compileButton.Location = new System.Drawing.Point(12, 409);
            this.compileButton.Name = "compileButton";
            this.compileButton.Size = new System.Drawing.Size(94, 29);
            this.compileButton.TabIndex = 4;
            this.compileButton.Text = "Compile";
            this.compileButton.UseVisualStyleBackColor = true;
            this.compileButton.Click += new System.EventHandler(this.compileButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(12, 383);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(50, 20);
            this.statusLabel.TabIndex = 5;
            this.statusLabel.Text = "Ready";
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(112, 409);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(94, 29);
            this.importButton.TabIndex = 6;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(212, 409);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(94, 29);
            this.exportButton.TabIndex = 7;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "hlog";
            this.openFileDialog1.Filter = "High-level Mindustry Logic files (*.hlog)|*.hlog|Text files (*.txt)|*.txt|All fil" +
    "es (*.*)|*.*";
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "hlog";
            this.saveFileDialog1.Filter = "Mindustry Logic files (*.mlog)|*.mlog|Text files (*.txt)|*.txt|All files (*.*)|*." +
    "*";
            this.saveFileDialog1.RestoreDirectory = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.compileButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Mindustry Logic Compiler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox textBox1;
        private TextBox textBox2;
        private Label label1;
        private Label label2;
        private Button compileButton;
        private Label statusLabel;
        private Button importButton;
        private Button exportButton;
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
    }
}