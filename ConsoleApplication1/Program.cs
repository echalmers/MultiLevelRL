using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL;
using MultiResolutionRL.ValueCalculation;

namespace ConsoleApplication1
{
    class model : ActionValue<string, int>
    {
        public model(IEqualityComparer<string> StateComparer, IEqualityComparer<int> ActionComparer, List<int> AvailableActions, string StartState, params object[] parameters) 
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        { }

        public override double[] value(string state, List<int> actions)
        {
            throw new NotImplementedException();
        }

        public override double update(StateTransition<string, int> transition)
        {
            throw new NotImplementedException();
        }

        public override PerformanceStats getStats()
        {
            throw new NotImplementedException();
        }

        public override string PredictNextState(string state, int action)
        {
            switch (state)
            {
                case "A":
                    switch (action)
                    {
                        case 0:
                            return "A"; break;
                        case 1:
                            return "A"; break;
                        case 2:
                            return "C"; break;
                        case 3:
                            return "B"; break;
                    }
                    break;
                case "B":
                    switch (action)
                    {
                        case 0:
                            return "B"; break;
                        case 1:
                            return "A"; break;
                        case 2:
                            return "D"; break;
                        case 3:
                            return "B"; break;
                    }
                    break;
                case "C":
                    switch (action)
                    {
                        case 0:
                            return "A"; break;
                        case 1:
                            return "C"; break;
                        case 2:
                            return "C"; break;
                        case 3:
                            return "D"; break;
                    }
                    break;
                case "D":
                    switch (action)
                    {
                        case 0:
                            return "B"; break;
                        case 1:
                            return "C"; break;
                        case 2:
                            return "F"; break;
                        case 3:
                            return "D"; break;
                    }
                    break;
                case "E":
                    switch (action)
                    {
                        case 0:
                            return "E"; break;
                        case 1:
                            return "E"; break;
                        case 2:
                            return "E"; break;
                        case 3:
                            return "F"; break;
                    }
                    break;
                case "F":
                    switch (action)
                    {
                        case 0:
                            return "D"; break;
                        case 1:
                            return "E"; break;
                        case 2:
                            return "F"; break;
                        case 3:
                            return "F"; break;
                    }
                    break;
            }
            return "Error";
        }
        
        public override Dictionary<string, double> PredictNextStates(string state, int action)
        {
            throw new NotImplementedException();
        }

        public override double PredictReward(string state, int action, string newState)
        {
            throw new NotImplementedException();
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            IEqualityComparer<string> comparer = EqualityComparer<string>.Default;
            List<int> actions = new List<int>();
            actions.AddRange(new int[4] {0,1,2,3});
            model m = new model(comparer, EqualityComparer<int>.Default, actions, "A");
            PathFinder<string, int> pathFinder = new PathFinder<string,int>(comparer);

            Dictionary<string, double> dists = pathFinder.DijkstraDistances("A", m, actions);
            



            // task-switch study
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

            Parallel.For(0, runs, op, (run) =>
            {
                cumModelUse[run] = new List<double>();

                // instantiate world
                World thisWorld = new GridWorld();

                // load 1st map
                thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10.bmp");

                // add agent
                System.Threading.Thread.Sleep(run * 100); // staggered instantiation to avoid identical random number generators

                //thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 8);
                thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));

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
                thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10e.bmp");

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






            //// Lesion study
            //int runs = 8;
            //int goalCt = 25;
            //List<double>[] results = new List<double>[runs];

            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            //ParallelOptions op = new ParallelOptions()
            //{
            //    MaxDegreeOfParallelism = 8
            //};

            //Parallel.For(0, runs, op, (run) =>
            ////for (int run = 0; run < runs; run++)
            //{
            //    // instantiate world
            //    World thisWorld = new GridWorld();

            //    // load map
            //    thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map3LargeMod.bmp");

            //    // load agent
            //    System.Threading.Thread.Sleep(run * 100); // staggered instantiation to avoid identical random number generators
            //    //thisWorld.addAgent(typeof(SoftmaxPolicy<,>), typeof(MultiGridWorldModel<,>), 8, 4);
            //    thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 8, 0);

            //    // run
            //    PerformanceStats stats = new PerformanceStats();
            //    while (stats.stepsToGoal.Count <= goalCt)
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
            //    Agent<int[],int[]> agent = (Agent<int[],int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiGridWorldModel<,>), 8, 4);

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
            //    while (stats.stepsToGoal.Count <= goalCt*2)
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
