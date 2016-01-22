using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL;
using MultiResolutionRL.ValueCalculation;
//using feudalRL_Library;

namespace FeudalTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int wS = 48;    //WorldSize p[0]
            bool RL = true;    //RLMethod p[1];  'F' for QL, 'T' For MB
            double a = 0.1; //alpha p[2];
            double g = 0.8; //Gamma p[3];
            int tO = wS;     //timeOut p[4];
            double mR = 1; //Manager Rewards p[5];
            Policy<int[], int[]> cP = new EGreedyPolicy<int[], int[]>(); //chosen Policy p[6]

            // task-switch test
            int runs = 48;
            int goalCt = 10;
            List<double>[] stepsToGoal = new List<double>[runs];
            List<double>[] cumModelUse = new List<double>[runs];

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ParallelOptions op = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8
            };

            //for (int run = 0; run < runs; run++)
            Parallel.For(0, runs, op, run =>
            {
                cumModelUse[run] = new List<double>();

                // instantiate world
                World thisWorld = new GridWorld();

                // load 1st map
                thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10.bmp");

                // add agent
                System.Threading.Thread.Sleep(run * 100); // staggered instantiation to avoid identical random number generators

                //thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 8);
              //  thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(Boss<,>), wS, RL, a, g, tO, mR, cP);

                // run
                PerformanceStats stats = new PerformanceStats();
                while (stats.stepsToGoal.Count <= goalCt)
                {
                    stats = thisWorld.stepAgent("");
                    if (stats.stepsToGoal.Last() == 0)
                    {
                        cumModelUse[run].Add(stats.modelAccesses + stats.modelUpdates);
                        Console.WriteLine("run " + run.ToString() + " goal count: " + stats.stepsToGoal.Count);
                    }
                }

                // switch task
                thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10a.bmp");

                // run again
                while (stats.stepsToGoal.Count <= goalCt * 2)
                {
                    stats = thisWorld.stepAgent("");
                    if (stats.stepsToGoal.Last() == 0)
                    {
                        cumModelUse[run].Add(stats.modelAccesses + stats.modelUpdates);
                        Console.WriteLine("run " + run.ToString() + " goal count: " + stats.stepsToGoal.Count);
                    }
                }

                stepsToGoal[run] = stats.stepsToGoal;
            });

            System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\stepsToGoal.csv");
            for (int i = 0; i < stepsToGoal[0].Count; i++)
            {
                List<string> line = new List<string>();
                foreach (List<double> series in stepsToGoal)
                {
                    line.Add(series[i].ToString());
                }
                writer.WriteLine(string.Join(",", line));
            }
            writer.Flush();
            writer.Close();
            writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\modelUse.csv");
            for (int i = 0; i < cumModelUse[0].Count; i++)
            {
                List<string> line = new List<string>();
                foreach (List<double> series in cumModelUse)
                {
                    line.Add(series[i].ToString());
                }
                writer.WriteLine(string.Join(",", line));
            }
            writer.Flush();
            writer.Close();
        }
    }
}
