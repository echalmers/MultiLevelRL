using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL;
using MultiResolutionRL.ValueCalculation;

namespace ConsoleApplication1
{
    
    class Program
    {
        static void Main(string[] args)
        {

            //MultiResolutionRL.StateManagement.learnedStateTree tree = new MultiResolutionRL.StateManagement.learnedStateTree();
            //IntArrayComparer comparer = new IntArrayComparer();

            //System.IO.StreamReader rdr = new System.IO.StreamReader("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Fuzzy Place Field Test\\parents1.csv");
            //rdr.ReadLine();
            //string thisline;
            //while ((thisline = rdr.ReadLine()) != null)
            //{
            //    string[] elements = thisline.Split(',');
            //    int[] thisState = new int[2] { Convert.ToInt32(elements[0]), Convert.ToInt32(elements[1]) };
            //    int[] parent = tree.GetParentState(thisState, 3);
            //    if (comparer.Equals(parent, new int[2] { 3, 5 }))
            //    {
            //        int a = 0;
            //    }
            //    List<int[]> children = tree.GetLevel0Children(parent, 3);
            //}


            //// task-switch study
            //int runs = 48;
            //int goalCt = 10;
            //List<double>[] stepsToGoal = new List<double>[runs];
            //List<double>[] cumModelUse = new List<double>[runs];

            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            //ParallelOptions op = new ParallelOptions()
            //{
            //    MaxDegreeOfParallelism = 8
            //};

            //Parallel.For(0, runs, op, (run) =>
            //{
            //    cumModelUse[run] = new List<double>();

            //    // instantiate world
            //    World thisWorld = new GridWorld();

            //    // load 1st map
            //    thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10.bmp");

            //    // add agent
            //    System.Threading.Thread.Sleep(run * 100); // staggered instantiation to avoid identical random number generators

            //    //thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 8);
            //    thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));

            //    // run
            //    PerformanceStats stats = new PerformanceStats();
            //    while (stats.stepsToGoal.Count <= goalCt)
            //    {
            //        stats = thisWorld.stepAgent("");
            //        if (stats.stepsToGoal.Last() == 0)
            //        {
            //            cumModelUse[run].Add(stats.modelAccesses + stats.modelUpdates);
            //            Console.WriteLine("run " + run.ToString() + " goal count: " + stats.stepsToGoal.Count);
            //        }
            //    }

            //    // switch task
            //    thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10e.bmp");

            //    // run again
            //    while (stats.stepsToGoal.Count <= goalCt * 2)
            //    {
            //        stats = thisWorld.stepAgent("");
            //        if (stats.stepsToGoal.Last() == 0)
            //        {
            //            cumModelUse[run].Add(stats.modelAccesses + stats.modelUpdates);
            //            Console.WriteLine("run " + run.ToString() + " goal count: " + stats.stepsToGoal.Count);
            //        }
            //    }

            //    stepsToGoal[run] = stats.stepsToGoal;
            //});

            //System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\stepsToGoal.csv");
            //for (int i = 0; i < stepsToGoal[0].Count; i++)
            //{
            //    List<string> line = new List<string>();
            //    foreach (List<double> series in stepsToGoal)
            //    {
            //        line.Add(series[i].ToString());
            //    }
            //    writer.WriteLine(string.Join(",", line));
            //}
            //writer.Flush();
            //writer.Close();
            //writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\modelUse.csv");
            //for (int i = 0; i < cumModelUse[0].Count; i++)
            //{
            //    List<string> line = new List<string>();
            //    foreach (List<double> series in cumModelUse)
            //    {
            //        line.Add(series[i].ToString());
            //    }
            //    writer.WriteLine(string.Join(",", line));
            //}
            //writer.Flush();
            //writer.Close();






            // Lesion study
            int runs = 8;
            int goalCt = 25;
            List<double>[] results = new List<double>[runs];

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ParallelOptions op = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8
            };

            Parallel.For(0, runs, op, (run) =>
            //for (int run = 0; run < runs; run++)
            {
                // instantiate world
                World thisWorld = new GridWorld();

                // load map
                thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map3LargeMod.bmp");

                // load agent
                System.Threading.Thread.Sleep(run * 100); // staggered instantiation to avoid identical random number generators
                //thisWorld.addAgent(typeof(SoftmaxPolicy<,>), typeof(MultiGridWorldModel<,>), 8, 4);
                thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiResValue<,>), 1, 0);

                // run
                PerformanceStats stats = new PerformanceStats();
                while (stats.stepsToGoal.Count <= goalCt)
                {
                    stats = thisWorld.stepAgent("");
                }

                results[run] = stats.stepsToGoal;
            });

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());

           // System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\data.csv");
            for (int i = 0; i < goalCt; i++)
            {
                List<string> line = new List<string>();
                foreach (List<double> series in results)
                {
                    line.Add(series[i].ToString());
                }
               // writer.WriteLine(string.Join(",", line));
            }
           // writer.Flush();
            //writer.Close();


            //// Post-training Lesion study
            //int runs = 7;
            //int goalCt = 2;
            //List<double>[] results = new List<double>[runs];

            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            //Parallel.For(0, runs, (run) =>
            ////for (int run = 0; run < runs; run++)
            //{
            //    // instantiate world
            //    World thisWorld = new GridWorld();

            //    // load map
            //    thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map3.bmp");

            //    // load agent
            //    System.Threading.Thread.Sleep(run * 100); // staggered instantiation to avoid identical random number generators
            //    //thisWorld.addAgent(typeof(SoftMaxPolicy<,>), typeof(MultiGridWorldModel<,>), 8, 4);
            //    Agent<int[], int[]> agent = (Agent<int[], int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 8, 4);

            //    // run
            //    PerformanceStats stats = new PerformanceStats();
            //    while (stats.stepsToGoal.Count <= goalCt)
            //    {
            //        stats = thisWorld.stepAgent("");
            //    }

            //    // change environment
            //    thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map3b.bmp");

            //    // lesion vH
            //    MultiGridWorldModel<int[], int[]> model = (MultiGridWorldModel<int[], int[]>)agent._actionValue;
            //    model.LesionVH(1);

            //    // run
            //    while (stats.stepsToGoal.Count <= goalCt * 2)
            //    {
            //        stats = thisWorld.stepAgent("");
            //    }

            //    results[run] = stats.stepsToGoal;
            //});

            //sw.Stop();
            //Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());

            //System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\data.csv");
            //for (int i = 0; i < goalCt; i++)
            //{
            //    List<string> line = new List<string>();
            //    foreach (List<double> series in results)
            //    {
            //        line.Add(series[i].ToString());
            //    }
            //    writer.WriteLine(string.Join(",", line));
            //}
            //writer.Flush();
            //writer.Close();
        }
    }
}
