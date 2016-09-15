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


namespace multiAgentTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double[] results = new double[8];
            for (int num_agents = 1; num_agents <= 8; num_agents++)
            {
                // instantiate problem
                EgoAlloGridWorldMulti world = new EgoAlloGridWorldMulti() { numAgents = num_agents };
                List<Agent<int[], int[]>> agents = (List<Agent<int[], int[]>>)world.addAgent(typeof(OptimalPolicy<,>), typeof(LinearEgoAlloValue<,>), true, 1, true);

                world.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\mapEgoAlloTrain.bmp");
                for (int i = 0; i < 2000; i++)
                {
                    world.stepAgent();
                    pictureBox1.Image = world.showState(100, 100);
                    pictureBox1.Refresh();
                    System.Threading.Thread.Sleep(10);
                }
                foreach (Agent<int[], int[]> a in agents)
                    ((LinearEgoAlloValue<int[], int[]>)a._actionValue).ResetAllocentric(true);


                world.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\mapEgoAlloTest.bmp");
                PerformanceStats stats = new PerformanceStats();
                int step = 0;
                for (step = 0; step < 5000; step++)
                {
                    stats = world.stepAgent();
                    pictureBox1.Image = world.showState(100, 100);
                    pictureBox1.Refresh();
                    System.Threading.Thread.Sleep(10);
                    if (stats.cumulativeReward > 0)
                        break;
                }

                results[num_agents - 1] = step;
            }

            foreach (double d in results)
                textBox1.AppendText(d.ToString() + Environment.NewLine);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int runs = 64;
            int trials = 50;
            int stepCap = 200;
            double[][] results = new double[trials * 2][];
            for (int i = 0; i < (trials * 2); i++)
                results[i] = new double[runs];

            int scale = 2;

            Bitmap map = new Bitmap("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\waterMazeSim\\Open.bmp");
            Bitmap resized = new Bitmap(map.Width * 3, map.Height * 3);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(map, 0, 0, map.Width * scale, map.Height * scale);
            }
            resized.Save("temp.bmp");

            map = new Bitmap("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\waterMazeSim\\Third.bmp");
            resized = new Bitmap(map.Width * 3, map.Height * 3);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(map, 0, 0, map.Width * scale, map.Height * scale);
            }
            resized.Save("temp2.bmp");



            Parallel.For(0, runs, run =>  // for (int run = 0; run < runs; run++)
            {
                GridWorld world = new GridWorld();
                
                world.Load("temp.bmp");
                world.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiResValue<,>), 1, 0);

                PerformanceStats stats = new PerformanceStats();
                while (stats.stepsToGoal.Count < (trials + 1))
                {
                    stats = world.stepAgent();
                    //pictureBox1.Image = world.showState(300, 300);
                    //pictureBox1.Refresh();
                    //System.Threading.Thread.Sleep(1);

                    if (stats.stepsToGoal.Last() >= stepCap)
                    {
                        stats.TallyStepsToGoal(true);
                        world.Load("temp.bmp");
                    }
                }

                
                world.Load("temp2.bmp");
                while (stats.stepsToGoal.Count < (trials * 2 + 1))
                {
                    stats = world.stepAgent();
                    //pictureBox1.Image = world.showState(300, 300);
                    //pictureBox1.Refresh();
                    //System.Threading.Thread.Sleep(1);

                    if (stats.stepsToGoal.Last() >= stepCap)
                    {
                        stats.TallyStepsToGoal(true);
                        world.Load("temp2.bmp");
                    }
                }

                for (int i = 0; i < trials * 2; i++)
                {
                    results[i][run] = stats.stepsToGoal[i];
                }
            });

            for (int i = 0; i < trials * 2; i++)
            {
                textBox1.AppendText(results[i].Average() + Environment.NewLine);
            }
        }
    }
}
