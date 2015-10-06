﻿using System;
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

namespace RL_Test
{
    public partial class Form1 : Form
    {
        System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Presentation Sept 28\\cumulativeReward.txt");
        World world;
        bool saveImages = false;
        string saveFolder; int numSavedImages = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            world = new GridWorld();
            loadMapButton.PerformClick();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i=0; i<(int)stepsUpDown.Value; i++)
            {
                double cumulativeReward = world.stepAgent();
                label1.Text = i.ToString();
                label1.Refresh();

                chart1.Series.Last().Points.AddY(cumulativeReward);
                writer.WriteLine("Reward: " + cumulativeReward);

                if (displayCheckBox.Checked)
                {
                    pictureBox1.Image = world.showState(pictureBox1.Width, pictureBox1.Height, true);
                    pictureBox1.Refresh();
                    Thread.Sleep(10);

                    pictureBox1.Image.Save(saveFolder + numSavedImages.ToString() + ".bmp");
                    numSavedImages++;
                }
            }
            sw.Stop();
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
        }

        private void worldModelButton_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter w = new System.IO.StreamWriter("log.txt", false);
            w.Close();

            world.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));
            chart1.Series.Add(chart1.Series.Last().Name + "1");
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void multiModelButton_Click(object sender, EventArgs e)
        {
            world.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 8);
            chart1.Series.Add(chart1.Series.Last().Name + "1");
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
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
            chart1.Series.Add(chart1.Series.Last().Name + "1");
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
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
            world.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 1);
            chart1.Series.Add(chart1.Series.Last().Name + "1");
            chart1.Series.Last().ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MultiResolutionRL.StateManagement.StateTree<int[]> stateTree = new MultiResolutionRL.StateManagement.StateTree<int[]>(new IntArrayComparer(), new int[2] {0,0});
            int[] x = new int[8];
            int[] y = new int[8];

            while (x[5] == 0 && y[5] == 0)
            {
                int thisi = 0, thisj = 0;
                for (int l = 0; l < 7; l++)
                {
                    thisi += x[l] * (int)Math.Pow(2, l);
                    thisj += y[l] * (int)Math.Pow(2, l);
                }
                stateTree.AddState(new int[2] { thisi, thisj });

                x[0]++;
                for (int l = 0; l < 6; l++)
                {
                    if (x[l] == 2)
                    {
                        x[l] = 0;
                        y[l]++;
                    }
                    if (y[l] == 2)
                    {
                        y[l] = 0;
                        x[l + 1]++;
                    }
                }
            }

            string[] inp = textBox1.Text.Split(',');
            int[] state = new int[2] {Convert.ToInt32(inp[0]), Convert.ToInt32(inp[1])};
            List<int[]> children = stateTree.GetChildren(state, Convert.ToInt32(inp[2]));
            string childrenString = "";
            string parentString = "";
            foreach(int[] c in children)
            {
                parentString += (String.Join(",", stateTree.GetParentState(c, Convert.ToInt32(inp[2]))) + Environment.NewLine);
                childrenString += (String.Join(",", c) + Environment.NewLine);
            }
            MessageBox.Show(childrenString);
            MessageBox.Show(parentString);
        }

        
    }
}
