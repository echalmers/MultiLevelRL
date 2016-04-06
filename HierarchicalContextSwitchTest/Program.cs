using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL;
using MultiResolutionRL.ValueCalculation;
using System.IO;

namespace HierarchicalContextSwitchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string stepsToGoalFilename = "C:\\Users\\Eric\\Desktop\\Maps\\stepsToGoal.csv";
            string modelUseFilename = "C:\\Users\\Eric\\Desktop\\Maps\\modelUse.csv";
            string cumRewardFilename = "C:\\Users\\Eric\\Desktop\\Maps\\cumReward.csv";
            string mapsDirectory = "C:\\Users\\Eric\\Desktop\\Maps\\";

            int runs = 96;
            int goalCt = 10;
            List<double>[] stepsToGoal = new List<double>[runs];
            List<double>[] cumModelUse = new List<double>[runs];
            List<double>[] cumReward = new List<double>[runs];

            string[] mapNames = Directory.GetFiles(mapsDirectory, "*.bmp");
            List<string> maps = new List<string>();
            maps.AddRange(mapNames); //maps.AddRange(mapNames);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ParallelOptions op = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8
            };

            //for(int run=0; run< runs; run++)
            Parallel.For(0, runs, op, (run) =>
            {
                cumModelUse[run] = new List<double>();
                cumReward[run] = new List<double>();

                // instantiate world
                World thisWorld = new GridWorld();

                // add agent
                System.Threading.Thread.Sleep(run * 101); // staggered instantiation to avoid identical random number generators
                thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ContextSwitchValue<,>), 8, 100); // this line for context-switch + adaptation
                //thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ContextSwitchValue<,>), 1, 100); // this line for context-switch only
                //thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>)); // this line for standard MBRL

                PerformanceStats stats = new PerformanceStats();

                for (int mapNumber = 0; mapNumber<maps.Count; mapNumber++)
                {
                    // load map
                    thisWorld.Load(maps[mapNumber]);

                    // go
                    while (stats.stepsToGoal.Count < goalCt * (mapNumber+1))
                    {
                        stats = thisWorld.stepAgent("");
                        if (stats.stepsToGoal.Last() == 0)
                        {
                            cumModelUse[run].Add(stats.modelAccesses + stats.modelUpdates);
                            cumReward[run].Add(stats.cumulativeReward);
                            Console.WriteLine("run " + run.ToString() + " goal count: " + (stats.stepsToGoal.Count-1) + " steps: " + stats.stepsToGoal[mapNumber]);
                        }
                    }

                    stepsToGoal[run] = stats.stepsToGoal;
                }

            });
            //}

            saveToCSV(stepsToGoalFilename, stepsToGoal);
            saveToCSV(modelUseFilename, cumModelUse);
            saveToCSV(cumRewardFilename, cumReward);
        }

        public static void saveToCSV(string filename, List<double>[] data)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename);
            for (int i = 0; i < data[0].Count; i++)
            {
                List<string> line = new List<string>();
                foreach (List<double> series in data)
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
