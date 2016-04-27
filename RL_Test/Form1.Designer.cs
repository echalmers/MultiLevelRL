namespace RL_Test
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.imageContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImagesFromThisRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RunButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.stepsUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.displayCheckBox = new System.Windows.Forms.CheckBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadMapButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.actionTextBox = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.steps2goalContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.button2 = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.fromMdlButton = new System.Windows.Forms.Button();
            this.learnerTypeComboBox = new System.Windows.Forms.ComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.imageContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stepsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.chartContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).BeginInit();
            this.steps2goalContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.ContextMenuStrip = this.imageContextMenu;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(200, 200);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // imageContextMenu
            // 
            this.imageContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyImageToolStripMenuItem,
            this.saveImagesFromThisRunToolStripMenuItem});
            this.imageContextMenu.Name = "imageContextMenu";
            this.imageContextMenu.Size = new System.Drawing.Size(140, 48);
            this.imageContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.imageContextMenu_Opening);
            // 
            // copyImageToolStripMenuItem
            // 
            this.copyImageToolStripMenuItem.Name = "copyImageToolStripMenuItem";
            this.copyImageToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.copyImageToolStripMenuItem.Text = "Copy Image";
            this.copyImageToolStripMenuItem.Click += new System.EventHandler(this.copyImageToolStripMenuItem_Click);
            // 
            // saveImagesFromThisRunToolStripMenuItem
            // 
            this.saveImagesFromThisRunToolStripMenuItem.Name = "saveImagesFromThisRunToolStripMenuItem";
            this.saveImagesFromThisRunToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.saveImagesFromThisRunToolStripMenuItem.Text = "Save Images";
            this.saveImagesFromThisRunToolStripMenuItem.Click += new System.EventHandler(this.saveImagesFromThisRunToolStripMenuItem_Click);
            // 
            // RunButton
            // 
            this.RunButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunButton.Location = new System.Drawing.Point(293, 187);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(75, 23);
            this.RunButton.TabIndex = 1;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(218, 199);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // stepsUpDown
            // 
            this.stepsUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stepsUpDown.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.stepsUpDown.Location = new System.Drawing.Point(326, 138);
            this.stepsUpDown.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.stepsUpDown.Name = "stepsUpDown";
            this.stepsUpDown.Size = new System.Drawing.Size(120, 20);
            this.stepsUpDown.TabIndex = 4;
            this.stepsUpDown.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(232, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Number of Steps";
            // 
            // displayCheckBox
            // 
            this.displayCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.displayCheckBox.AutoSize = true;
            this.displayCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.displayCheckBox.Location = new System.Drawing.Point(235, 164);
            this.displayCheckBox.Name = "displayCheckBox";
            this.displayCheckBox.Size = new System.Drawing.Size(133, 17);
            this.displayCheckBox.TabIndex = 6;
            this.displayCheckBox.Text = "Display While Running";
            this.displayCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.displayCheckBox.UseVisualStyleBackColor = true;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisY.Title = "Reward";
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.ContextMenuStrip = this.chartContextMenu;
            this.chart1.Location = new System.Drawing.Point(12, 218);
            this.chart1.Name = "chart1";
            this.chart1.Size = new System.Drawing.Size(543, 122);
            this.chart1.TabIndex = 7;
            this.chart1.Text = "chart1";
            // 
            // chartContextMenu
            // 
            this.chartContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.exportAsCSVToolStripMenuItem});
            this.chartContextMenu.Name = "chartContextMenu";
            this.chartContextMenu.Size = new System.Drawing.Size(158, 48);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.exportToolStripMenuItem.Text = "Export as Image";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // exportAsCSVToolStripMenuItem
            // 
            this.exportAsCSVToolStripMenuItem.Name = "exportAsCSVToolStripMenuItem";
            this.exportAsCSVToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.exportAsCSVToolStripMenuItem.Text = "Export as CSV";
            this.exportAsCSVToolStripMenuItem.Click += new System.EventHandler(this.exportAsCSVToolStripMenuItem_Click);
            // 
            // loadMapButton
            // 
            this.loadMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.loadMapButton.Location = new System.Drawing.Point(221, 41);
            this.loadMapButton.Name = "loadMapButton";
            this.loadMapButton.Size = new System.Drawing.Size(75, 23);
            this.loadMapButton.TabIndex = 8;
            this.loadMapButton.Text = "Load Map";
            this.loadMapButton.UseVisualStyleBackColor = true;
            this.loadMapButton.Click += new System.EventHandler(this.loadMapButton_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(218, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // actionTextBox
            // 
            this.actionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.actionTextBox.Location = new System.Drawing.Point(375, 187);
            this.actionTextBox.Name = "actionTextBox";
            this.actionTextBox.Size = new System.Drawing.Size(71, 20);
            this.actionTextBox.TabIndex = 15;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "GridWorld",
            "Mountain Car",
            "Taxi",
            "Stochastic"});
            this.comboBox1.Location = new System.Drawing.Point(221, 14);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(75, 21);
            this.comboBox1.TabIndex = 16;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // chart2
            // 
            this.chart2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.AxisY.Title = "Model Use";
            chartArea2.Name = "ChartArea1";
            this.chart2.ChartAreas.Add(chartArea2);
            this.chart2.ContextMenuStrip = this.chartContextMenu;
            this.chart2.Location = new System.Drawing.Point(12, 338);
            this.chart2.Name = "chart2";
            this.chart2.Size = new System.Drawing.Size(543, 122);
            this.chart2.TabIndex = 17;
            this.chart2.Text = "chart2";
            // 
            // chart3
            // 
            this.chart3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea3.AxisY.IsLogarithmic = true;
            chartArea3.AxisY.Title = "Steps to Goal";
            chartArea3.Name = "ChartArea1";
            this.chart3.ChartAreas.Add(chartArea3);
            this.chart3.ContextMenuStrip = this.steps2goalContextMenu;
            this.chart3.Location = new System.Drawing.Point(12, 460);
            this.chart3.Name = "chart3";
            this.chart3.Size = new System.Drawing.Size(543, 122);
            this.chart3.TabIndex = 18;
            this.chart3.Text = "chart3";
            // 
            // steps2goalContextMenu
            // 
            this.steps2goalContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.steps2goalContextMenu.Name = "steps2goalContextMenu";
            this.steps2goalContextMenu.Size = new System.Drawing.Size(146, 26);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(145, 22);
            this.toolStripMenuItem1.Text = "Export to CSV";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(218, 99);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.Location = new System.Drawing.Point(481, 70);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 20;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // fromMdlButton
            // 
            this.fromMdlButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fromMdlButton.Location = new System.Drawing.Point(481, 99);
            this.fromMdlButton.Name = "fromMdlButton";
            this.fromMdlButton.Size = new System.Drawing.Size(75, 23);
            this.fromMdlButton.TabIndex = 21;
            this.fromMdlButton.Text = "From Model";
            this.fromMdlButton.UseVisualStyleBackColor = true;
            this.fromMdlButton.Click += new System.EventHandler(this.fromMdlButton_Click);
            // 
            // learnerTypeComboBox
            // 
            this.learnerTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.learnerTypeComboBox.FormattingEnabled = true;
            this.learnerTypeComboBox.Items.AddRange(new object[] {
            "Q learning",
            "Model based",
            "Tracking ModelBased",
            "Multi-resolution",
            "Context switcher (hierarchical)",
            "Context switcher",
            "EgoAllo(initialValue)",
            "EgoAllo(fullPrediction)",
            "LinearFA",
            "LinearEgoAlloFA",
            "Load"});
            this.learnerTypeComboBox.Location = new System.Drawing.Point(326, 14);
            this.learnerTypeComboBox.Name = "learnerTypeComboBox";
            this.learnerTypeComboBox.Size = new System.Drawing.Size(121, 21);
            this.learnerTypeComboBox.TabIndex = 27;
            this.learnerTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.learnerTypeComboBox_SelectedIndexChanged);
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(479, 12);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 28;
            this.saveButton.Text = "Save learner";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 597);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.learnerTypeComboBox);
            this.Controls.Add(this.fromMdlButton);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.chart3);
            this.Controls.Add(this.chart2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.actionTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.loadMapButton);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.displayCheckBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.stepsUpDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.imageContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.stepsUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.chartContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).EndInit();
            this.steps2goalContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown stepsUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox displayCheckBox;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Button loadMapButton;
        private System.Windows.Forms.ContextMenuStrip imageContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveImagesFromThisRunToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip chartContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox actionTextBox;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.ToolStripMenuItem exportAsCSVToolStripMenuItem;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart3;
        private System.Windows.Forms.ContextMenuStrip steps2goalContextMenu;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button fromMdlButton;
        private System.Windows.Forms.ComboBox learnerTypeComboBox;
        private System.Windows.Forms.Button saveButton;
    }
}

