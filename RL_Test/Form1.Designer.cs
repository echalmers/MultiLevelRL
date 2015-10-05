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
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
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
            this.loadMapButton = new System.Windows.Forms.Button();
            this.worldModelButton = new System.Windows.Forms.Button();
            this.multiModelButton = new System.Windows.Forms.Button();
            this.QLearnButton = new System.Windows.Forms.Button();
            this.oneLayerButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.imageContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stepsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.chartContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.imageContextMenu;
            this.pictureBox1.Location = new System.Drawing.Point(38, 24);
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
            this.RunButton.Location = new System.Drawing.Point(319, 199);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(75, 23);
            this.RunButton.TabIndex = 1;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(244, 211);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // stepsUpDown
            // 
            this.stepsUpDown.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.stepsUpDown.Location = new System.Drawing.Point(352, 150);
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
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(258, 152);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Number of Steps";
            // 
            // displayCheckBox
            // 
            this.displayCheckBox.AutoSize = true;
            this.displayCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.displayCheckBox.Location = new System.Drawing.Point(261, 176);
            this.displayCheckBox.Name = "displayCheckBox";
            this.displayCheckBox.Size = new System.Drawing.Size(133, 17);
            this.displayCheckBox.TabIndex = 6;
            this.displayCheckBox.Text = "Display While Running";
            this.displayCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.displayCheckBox.UseVisualStyleBackColor = true;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.ContextMenuStrip = this.chartContextMenu;
            this.chart1.Location = new System.Drawing.Point(38, 230);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(434, 122);
            this.chart1.TabIndex = 7;
            this.chart1.Text = "chart1";
            // 
            // chartContextMenu
            // 
            this.chartContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem});
            this.chartContextMenu.Name = "chartContextMenu";
            this.chartContextMenu.Size = new System.Drawing.Size(108, 26);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // loadMapButton
            // 
            this.loadMapButton.Location = new System.Drawing.Point(247, 24);
            this.loadMapButton.Name = "loadMapButton";
            this.loadMapButton.Size = new System.Drawing.Size(75, 23);
            this.loadMapButton.TabIndex = 8;
            this.loadMapButton.Text = "Load Map";
            this.loadMapButton.UseVisualStyleBackColor = true;
            this.loadMapButton.Click += new System.EventHandler(this.loadMapButton_Click);
            // 
            // worldModelButton
            // 
            this.worldModelButton.Location = new System.Drawing.Point(387, 53);
            this.worldModelButton.Name = "worldModelButton";
            this.worldModelButton.Size = new System.Drawing.Size(85, 23);
            this.worldModelButton.TabIndex = 9;
            this.worldModelButton.Text = "Model-Based";
            this.worldModelButton.UseVisualStyleBackColor = true;
            this.worldModelButton.Click += new System.EventHandler(this.worldModelButton_Click);
            // 
            // multiModelButton
            // 
            this.multiModelButton.Location = new System.Drawing.Point(387, 111);
            this.multiModelButton.Name = "multiModelButton";
            this.multiModelButton.Size = new System.Drawing.Size(85, 23);
            this.multiModelButton.TabIndex = 10;
            this.multiModelButton.Text = "n-layer memory";
            this.multiModelButton.UseVisualStyleBackColor = true;
            this.multiModelButton.Click += new System.EventHandler(this.multiModelButton_Click);
            // 
            // QLearnButton
            // 
            this.QLearnButton.Location = new System.Drawing.Point(387, 24);
            this.QLearnButton.Name = "QLearnButton";
            this.QLearnButton.Size = new System.Drawing.Size(85, 23);
            this.QLearnButton.TabIndex = 11;
            this.QLearnButton.Text = "Q-Learning";
            this.QLearnButton.UseVisualStyleBackColor = true;
            this.QLearnButton.Click += new System.EventHandler(this.QLearnButton_Click);
            // 
            // oneLayerButton
            // 
            this.oneLayerButton.Location = new System.Drawing.Point(387, 82);
            this.oneLayerButton.Name = "oneLayerButton";
            this.oneLayerButton.Size = new System.Drawing.Size(85, 23);
            this.oneLayerButton.TabIndex = 12;
            this.oneLayerButton.Text = "1-layer memory";
            this.oneLayerButton.UseVisualStyleBackColor = true;
            this.oneLayerButton.Click += new System.EventHandler(this.oneLayerButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(269, 95);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(269, 125);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 14;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 359);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.oneLayerButton);
            this.Controls.Add(this.QLearnButton);
            this.Controls.Add(this.multiModelButton);
            this.Controls.Add(this.worldModelButton);
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
        private System.Windows.Forms.Button worldModelButton;
        private System.Windows.Forms.Button multiModelButton;
        private System.Windows.Forms.ContextMenuStrip imageContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyImageToolStripMenuItem;
        private System.Windows.Forms.Button QLearnButton;
        private System.Windows.Forms.ToolStripMenuItem saveImagesFromThisRunToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip chartContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.Button oneLayerButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
    }
}

