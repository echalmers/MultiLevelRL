using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiResolutionRL;
using MultiResolutionRL.ValueCalculation;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.IO;

namespace RL_Test
{
    public partial class Form1 : Form
    {
        //System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Presentation Sept 28\\cumulativeReward.txt");
        System.IO.StreamWriter trajWriter;
        World world;
        bool saveImages = false;
        string saveFolder; int numSavedImages = 0;
        object agent;

        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("******");

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            PerformanceStats stats = new PerformanceStats();
            for (int i=0; i<(int)stepsUpDown.Value; i++)
            {
                stats = world.stepAgent(actionTextBox.Text);

                //trajWriter.WriteLine(string.Join(",", ((Agent<int[], int[]>)agent).state));

                label1.Text = i.ToString();
                label1.Refresh();

                chart1.Series.Last().Points.AddY(stats.cumulativeReward);
                chart2.Series.Last().Points.AddY(stats.modelAccesses + stats.modelUpdates);
                //writer.WriteLine("Reward: " + stats.cumulativeReward);

                if (displayCheckBox.Checked)
                {
                    pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height, true);
                    pictureBox1.Refresh();
                    Thread.Sleep(20);

                    if (saveImages)
                    {
                        pictureBox1.Image.Save(saveFolder + numSavedImages.ToString() + ".bmp");
                        numSavedImages++;
                    }
                }
            }
            sw.Stop();
            chart3.Series.Last().Points.Clear();
            foreach (double d in stats.stepsToGoal)
            {
                if (d <= 0)
                    break;
                chart3.Series.Last().Points.Add(d);
            }
            label1.Text = Math.Round(sw.Elapsed.TotalSeconds,1) + "s";

            pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height, true);
           
        }

        private void loadMapButton_Click(object sender, EventArgs e)
        {
            try
            {
                ((ContextSwitchValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue).resetHistory();
                ((ContextSwitchValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue).currentModel = null;
            }
            catch { }

            OpenFileDialog of = new OpenFileDialog();
            of.Title = "Load map";
            of.ShowDialog();
            of.Filter = "*.bmp|*.bmp";
            if (System.IO.File.Exists(of.FileName))
            {
                world.Load(of.FileName);
                pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height);
            }
            if (trajWriter!=null && trajWriter.BaseStream != null)
            {
                trajWriter.Flush(); trajWriter.Close();
            }
           // trajWriter = new System.IO.StreamWriter("C:\\Users\\Eric\\Desktop\\trajectory" + of.SafeFileName + ".csv");
        }

        
        private void copyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(pictureBox1.Image);
        }
        

        private void imageContextMenu_Opening(object sender, CancelEventArgs e)
        {
            imageContextMenu.Items[1].Text = "Save Images (" + saveImages + ")";
        }

        private void saveImagesFromThisRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveImages = !saveImages;
            if (saveImages)
            {
                saveFolder = "C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Images\\" + DateTime.Now.ToString("ddMMM-hh-mm") + "\\";
                System.IO.Directory.CreateDirectory(saveFolder);

                displayCheckBox.Checked = true;
                displayCheckBox.Enabled = false;
            }
            else
            {
                displayCheckBox.Enabled = true;
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart1.SaveImage("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Images\\chart_" + DateTime.Now.ToString("ddMMM-hh-mm") + ".Tiff", System.Drawing.Imaging.ImageFormat.Tiff);
        }

        
        private void button1_Click_1(object sender, EventArgs e)
        {
            //((StochasticRewardGridWorld)world).ExportGradients();
            ((GridWorld)world).ExportGradients(0);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox1.Text)
            {

                case "EgoAlloGridWorldMulti":
                    world = new EgoAlloGridWorldMulti();
                    loadMapButton.Enabled = true;
                    loadMapButton.PerformClick();
                    break;

                case "GridWorld":
                    world = new GridWorld();
                    loadMapButton.Enabled = true;
                    loadMapButton.PerformClick();
                    break;
                case "Mountain Car":
                    world = new MountainCar();
                    loadMapButton.Enabled = false;
                    break;
                case "Taxi":
                    world = new Taxi();
                    loadMapButton.Enabled = true;
                    loadMapButton.PerformClick();
                    break;
                case "Stochastic":
                    world = new StochasticRewardGridWorld();
                    loadMapButton.Enabled = true;
                    loadMapButton.PerformClick();
                    break;
            }
        }

        private void exportAsCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.Cancel)
                return;

            System.IO.StreamWriter writer = new System.IO.StreamWriter(sf.FileName, false);
            List<string> header = new List<string>();
            int maxPoints = 0;
            foreach (System.Windows.Forms.DataVisualization.Charting.Series s in chart1.Series)
            {
                maxPoints = Math.Max(maxPoints, s.Points.Count);
                header.Add(s.Name);
            }
            writer.WriteLine("," + String.Join(",", header) + ",," + String.Join(",", header));

            for (int i=0; i<maxPoints; i++)
            {
                List<string> rewards = new List<string>();
                List<string> accesses = new List<string>();

                for (int s = 0; s < chart1.Series.Count; s++)
                {
                    try { rewards.Add(chart1.Series[s].Points[i].YValues[0].ToString()); }
                    catch { rewards.Add(""); }

                    try { accesses.Add(chart2.Series[s].Points[i].YValues[0].ToString()); }
                    catch { accesses.Add(""); }
                }
                writer.WriteLine("," + String.Join(",", rewards) + ",," + String.Join(",", accesses));
            }
            writer.Flush(); writer.Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
             SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.Cancel)
                return;

            System.IO.StreamWriter writer = new System.IO.StreamWriter(sf.FileName, false);
            List<string> header = new List<string>();
            int maxPoints = 0;
            foreach (System.Windows.Forms.DataVisualization.Charting.Series s in chart3.Series)
            {
                maxPoints = Math.Max(maxPoints, s.Points.Count);
                header.Add(s.Name);
            }
            writer.WriteLine("," + String.Join(",", header));

            for (int i=0; i<maxPoints; i++)
            {
                List<string> steps = new List<string>();

                for (int s = 0; s < chart3.Series.Count; s++)
                {
                    try { steps.Add(chart3.Series[s].Points[i].YValues[0].ToString()); }
                    catch { steps.Add(""); }

                }
                writer.WriteLine("," + String.Join(",", steps));
            }
            writer.Flush(); writer.Close();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ((LinearEgoAlloValue<int[], int[]>)((Agent<int[],int[]>)agent)._actionValue).ResetAllocentric(true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //world = new EgoAlloGridWorld();
            loadMapButton.Enabled = true;
            world.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10a.bmp");
            pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            ((GridWorld)world).ExportAdjacencies();
        }

        private void fromMdlButton_Click(object sender, EventArgs e)
        {
            //ModelBasedValue<int[], int[]> singleModel = (ModelBasedValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue;
            //multiModelButton.PerformClick();
            //MultiResValue<int[],int[]> multiModel = (MultiResValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue;
            //multiModel.models[0] = singleModel;
            //multiModel.stateTree = new MultiResolutionRL.StateManagement.learnedStateTree();
        }
        
        
        
        private void learnerTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (learnerTypeComboBox.Text)
            {
                case "Q learning":
                    world.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelFreeValue<,>));
                    break;
                case "Model based":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));
                    break;
                case "Tracking ModelBased":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>), true);
                    break;
                case "Multi-resolution":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiResValue<,>), 10);
                    break;
                case "Context switcher (hierarchical)":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(ContextSwitchValue<,>), 8, 100);
                    break;
                case "Context switcher":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(ContextSwitchValue<,>), 1, 100);
                    break;
                case "EgoAllo(initialValue)":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearEgoAlloValue<,>), false, 1000, false);
                    break;
                case "EgoAllo(fullPrediction)":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearEgoAlloValue<,>), true, 10, false);//True = MB , false = MF
                    break;
                case "LinearFA":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearFAValue<,>));
                    break;
                //case "LinearEgoAlloFA":
                  //  agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearEgoAlloFAValue<,>));
                   // break;
                case "Load":
                    agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));
                    learnerTypeComboBox.Text = "Context switcher";

                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream("savedModel.mdl", FileMode.Open,FileAccess.Read,FileShare.Read);
                    ((Agent<int[], int[]>)agent)._actionValue = (ContextSwitchValue<int[], int[]>)formatter.Deserialize(stream);
                    stream.Close();

                    break;
            }

            chart1.Series.Add(chart1.Series.Count + " " + learnerTypeComboBox.Text);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add(chart2.Series.Count + " " + learnerTypeComboBox.Text);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add(chart3.Series.Count + " " + learnerTypeComboBox.Text);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            ContextSwitchValue<int[], int[]> mdl = (ContextSwitchValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue;
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("savedModel.mdl", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, mdl);
            stream.Close();
        }
    }
}
