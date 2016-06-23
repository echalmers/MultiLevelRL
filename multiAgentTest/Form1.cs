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
            GridWorld world = new GridWorld();
            world.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\waterMazeSim\\Open.bmp");
            world.addAgent(typeof(OptimalPolicy<,>), typeof(MultiResValue<,>), 10, 5);

            PerformanceStats stats = new PerformanceStats();
            while(stats.stepsToGoal.Count < 6)
            {
                stats = world.stepAgent();
                pictureBox1.Image = world.showState(100, 100);
                pictureBox1.Refresh();
                System.Threading.Thread.Sleep(10);
            }

            world.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\waterMazeSim\\Third.bmp");
            while (stats.stepsToGoal.Count < 11)
            {
                stats = world.stepAgent();
                pictureBox1.Image = world.showState(100, 100);
                pictureBox1.Refresh();
                System.Threading.Thread.Sleep(10);

                if (stats.stepsToGoal.Last() >= 500)
                {
                    stats.TallyStepsToGoal(true);
                    world.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\waterMazeSim\\Third.bmp");
                }
            }

            foreach (double d in stats.stepsToGoal)
            {
                textBox1.AppendText(d + Environment.NewLine);
            }
        }
    }
}
