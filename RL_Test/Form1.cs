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
using feudalRL_Library;

namespace RL_Test
{
    public partial class Form1 : Form
    {
        System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Presentation Sept 28\\cumulativeReward.txt");
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
                writer.WriteLine("Reward: " + stats.cumulativeReward);

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
            //System.IO.StreamReader r = new System.IO.StreamReader("log.txt");
            //string text = r.ReadLine();
            //if (text==null || (text.IndexOf("null")!=-1))
            //    pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height);
            //else
            //{
            //    int start = text.IndexOf("Level ") + 6;
            //    string goalLevelString = text.Substring(start, 1);
            //    int goalLevel = Convert.ToInt32(goalLevelString);
            //    start = text.IndexOf("at ") + 3;
            //    string[] goalString = text.Substring(start).Split(',');
            //    int[] goal = new int[2];
            //    goal[0] = Convert.ToInt32(goalString[0]);
            //    goal[1] = Convert.ToInt32(goalString[1]);
            //    pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height, true);
            //}
            //r.Close();

            // chart cumulative reward
            //chart1.Series.Last().Points.Clear();
            //for (int i = 0; i < world.agent.cumulativeReward.Count; i++)
            //{
            //    chart1.Series.Last().Points.AddXY(i, world.agent.cumulativeReward[i]);
            //}
            writer.Flush();
            //writer.Close();
        }

        private void loadMapButton_Click(object sender, EventArgs e)
        {
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
            trajWriter = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\trajectory" + of.SafeFileName + ".csv");
        }

        private void worldModelButton_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter w = new System.IO.StreamWriter("log.txt", false);
            w.Close();

            agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));
            chart1.Series.Add("Model-based" + chart1.Series.Count);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("Model-based" + chart2.Series.Count);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add("Model-based" + chart3.Series.Count);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void multiModelButton_Click(object sender, EventArgs e)
        {
            agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiResValue<,>), 8);
            chart1.Series.Add("Multi-layer" + chart1.Series.Count);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("Multi-layer" + chart2.Series.Count);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add("Multi-layer" + chart3.Series.Count);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void copyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(pictureBox1.Image);
        }

        private void QLearnButton_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter w = new System.IO.StreamWriter("log.txt", false);
            w.Close();

            world.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelFreeValue<,>));
            chart1.Series.Add("Model-free" + chart1.Series.Count);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("Model-free" + chart2.Series.Count);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add("Model-free" + chart3.Series.Count);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
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

        private void oneLayerButton_Click(object sender, EventArgs e)
        {
            string[] total_min = textBox1.Text.Split(',');
            agent = world.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiResValue<,>), Convert.ToInt32(total_min[0]), Convert.ToInt32(total_min[1]));
            chart1.Series.Add("1-layer" + chart1.Series.Count);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("1-layer" + chart2.Series.Count);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add("1-layer" + chart3.Series.Count);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ((stochasticRewardGridworld)world).ExportGradients();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox1.Text)
            {
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
                    world = new stochasticRewardGridworld();
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
            world.addAgent(typeof(OptimalPolicy<,>), typeof(FeudalValue<,>));
            chart1.Series.Add("Feudal" + chart1.Series.Count);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("Feudal" + chart2.Series.Count);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add("Feudal" + chart3.Series.Count);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

            //int wS = 48;    //WorldSize p[0]
            //bool RL = true;    //RLMethod p[1];  'F' for QL, 'T' For MB
            //double a = 0.1; //alpha p[2];
            //double g = 0.8; //Gamma p[3];
            //int tO = wS;     //timeOut p[4];
            //double mR = 10; //Manager Rewards p[5];
            //Policy<int[], int[]> cP = new EGreedyPolicy<int[], int[]>(); //chosen Policy p[6]

            //world.addAgent(typeof(EGreedyPolicy<,>), typeof(feudalRL_Library.Boss<,>), wS, RL, a, g, tO, mR, cP);
            //chart1.Series.Add("Feudal" + chart1.Series.Count);
            //chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            //chart2.Series.Add("Feudal" + chart2.Series.Count);
            //chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            //chart3.Series.Add("Feudal" + chart3.Series.Count);
            //chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            world = new stochasticRewardGridworld();
            loadMapButton.Enabled = true;
            world.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\stochastic2.bmp");
            pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            ((GridWorld)world).ExportAdjacencies();
        }

        private void fromMdlButton_Click(object sender, EventArgs e)
        {
            ModelBasedValue<int[], int[]> singleModel = (ModelBasedValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue;
            multiModelButton.PerformClick();
            MultiResValue<int[],int[]> multiModel = (MultiResValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue;
            multiModel.models[0] = singleModel;
            multiModel.stateTree = new MultiResolutionRL.StateManagement.learnedStateTree();
        }

        private void avgRbutton_Click(object sender, EventArgs e)
        {
            world.addAgent(typeof(OptimalPolicy<,>), typeof(ModelBasedAvgRwdValue<,>));
            chart1.Series.Add("AvgRwd" + chart1.Series.Count);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("AvgRwd" + chart2.Series.Count);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add("AvgRwd" + chart3.Series.Count);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void LSWSbutton_Click(object sender, EventArgs e)
        {
            world.addAgent(typeof(OptimalPolicy<,>), typeof(LSValue<,>));
            chart1.Series.Add("AvgRwd" + chart1.Series.Count);
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("AvgRwd" + chart2.Series.Count);
            chart2.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series.Add("AvgRwd" + chart3.Series.Count);
            chart3.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }
    }
}
