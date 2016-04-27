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



            //// stochastic reward study
            //int runs = 48;
            //int goalCt = 100;
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
            //    World thisWorld = new StochasticRewardGridWorld();

            //    // load map
            //    thisWorld.Load("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map4choiceB.bmp");

            //    // add agent
            //    System.Threading.Thread.Sleep(run * 100); // staggered instantiation to avoid identical random number generators

            //    thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));
            //    //thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ContextSwitchValue<,>), 8, 100);

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
                
            //    stepsToGoal[run] = stats.stepsToGoal;
            //});

            //System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\stepsToGoalStochasticMBRL.csv");
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
            //writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\modelUseStochasticMBRL.csv");
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
            //    thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(MultiResValue<,>), 1, 0);

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
            ///********TESTING FOR EGOALO***********///
            /// 
            //Set up the world.

            string testResultsLocation = "C:/Users/User/Desktop/MultiLevelRL/TestData/";
            string mapLocation = "C:/Users/User/Desktop/MultiLevelRL/TestMaps/EgoAloTest/";

            int runs = 10;
            int testTypeCount = 6;
            int qStepCount = 25000;
            int mBStepCount = 5000;
            string testmap = "HallwayTest.bmp";
            string trainmap = "mapEgoAlloTrain.bmp";


            List<double>[] cumRwrd = new List<double>[testTypeCount];
            for (int i = 0; i < testTypeCount; i++)
            {
                if (i > 2)
                {
                    cumRwrd[i] = new List<double>(mBStepCount * runs + 1);
                    for (int j = 0; j <= mBStepCount * runs; j++)
                        cumRwrd[i].Add(0);
                }
                else
                {
                    cumRwrd[i] = new List<double>(qStepCount * runs + 1);
                    for (int j = 0; j <= qStepCount * runs; j++)
                        cumRwrd[i].Add(0);
                }

            }


            ParallelOptions op = new ParallelOptions();
            {
                op.MaxDegreeOfParallelism = testTypeCount;
            };

            ParallelOptions op2 = new ParallelOptions();
            {
                op2.MaxDegreeOfParallelism = 8;
            };


            //Testing the in parallel each test type
            Parallel.For(0, testTypeCount, op, (testtype) =>
            {
                System.Threading.Thread.Sleep(testtype * 100); // staggered instantiation to avoid identical random number generators
                switch ((testTypes)testtype)
                {
                    case testTypes.InitQ://Use Q-learning that is initilized with ego information
                        {
                            Parallel.For(0, runs, op2, (run) =>
                            {
                                World thisWorld = new EgoAlloGridWorld();
                                Agent<int[], int[]> agent = (Agent<int[], int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearEgoAlloValue<,>), false, 1, false);
                                System.Threading.Thread.Sleep(run * 100);
                                //run the training map
                                PerformanceStats stats = new PerformanceStats();
                                thisWorld.Load(mapLocation + trainmap);
                                int iterator = 0;
                                while (iterator++ <= qStepCount)
                                    thisWorld.stepAgent("");

                                LinearEgoAlloValue<int[], int[]> temp = (LinearEgoAlloValue<int[], int[]>)(agent._actionValue);
                                temp.ResetAllocentric(false);

                                //run the Testing Map
                                iterator = 0;
                                thisWorld.Load(mapLocation + testmap);
                                while (iterator < qStepCount)
                                {
                                    cumRwrd[testtype][run * qStepCount + iterator] = (thisWorld.stepAgent("")).cumulativeReward;
                                    iterator++;
                                }

                                Console.Write((testTypes)testtype);
                                Console.Write(" runs " + run + " ");
                                Console.WriteLine(stats.cumulativeReward);

                            });//end inner loop
                            break;
                        } //q-table initilization method case

                    case testTypes.OurQ:
                        {
                            Parallel.For(0, runs, op2, (run) =>
                            {
                                World thisWorld = new EgoAlloGridWorld();
                                Agent<int[], int[]> agent = agent = (Agent<int[], int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearEgoAlloValue<,>), true, 1000, false);
                                System.Threading.Thread.Sleep(run * 100);
                                //run the training map

                                PerformanceStats stats = new PerformanceStats();
                                thisWorld.Load(mapLocation + trainmap);
                                int iterator = 0;
                                while (iterator++ <= qStepCount)
                                    thisWorld.stepAgent("");

                                LinearEgoAlloValue<int[], int[]> temp = (LinearEgoAlloValue<int[], int[]>)(agent._actionValue);
                                temp.ResetAllocentric(false);

                                //run the Testing Map
                                thisWorld.Load(mapLocation + testmap);
                                iterator = 0;
                                while (iterator < qStepCount)
                                {
                                    cumRwrd[testtype][run * qStepCount + iterator] = (thisWorld.stepAgent("")).cumulativeReward;
                                    iterator++;
                                }

                            });//end inner loop
                            Console.WriteLine((testTypes)testtype + " is Complete");
                            break;

                        } //q-table our prediction method
                    case
                        testTypes.StandardQ:
                        {
                            Parallel.For(0, runs, op2, (run) =>
                            {
                                World thisWorld = new EgoAlloGridWorld();
                                Agent<int[], int[]> agent = agent = (Agent<int[], int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelFreeValue<,>));
                                System.Threading.Thread.Sleep(run * 100);

                                PerformanceStats stats = new PerformanceStats();
                                //run the Testing Map
                                thisWorld.Load(mapLocation + testmap);
                                int iterator = 0;
                                while (iterator < qStepCount)
                                {
                                    cumRwrd[testtype][run * qStepCount + iterator] = (thisWorld.stepAgent("")).cumulativeReward;
                                    iterator++;
                                }

                            });//end inner loop
                            Console.WriteLine((testTypes)testtype + " is Complete");
                            break;
                        } // Standard q learning

                    case testTypes.InitMB:
                        {
                            Parallel.For(0, runs, op2, (run) =>
                            {
                                World thisWorld = new EgoAlloGridWorld();
                                Agent<int[], int[]> agent = agent = (Agent<int[], int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearEgoAlloValue<,>), false, 1, true);
                                System.Threading.Thread.Sleep(run * 100);

                                //run the training map
                                thisWorld.Load(mapLocation + trainmap);
                                int iterator = 0;
                                while (iterator++ <= mBStepCount)
                                    thisWorld.stepAgent("");

                                LinearEgoAlloValue<int[], int[]> temp = (LinearEgoAlloValue<int[], int[]>)(agent._actionValue);
                                temp.ResetAllocentric(true);

                                //run the Testing Map
                                PerformanceStats stats = new PerformanceStats();
                                thisWorld.Load(mapLocation + testmap);
                                iterator = 0;
                                while (iterator < mBStepCount)
                                {
                                    cumRwrd[testtype][run * mBStepCount + iterator] = (thisWorld.stepAgent("")).cumulativeReward;
                                    iterator++;
                                }

                                if (stats.cumulativeReward == 0)
                                    Console.WriteLine((testTypes)testtype + " on thread: " + run + " reward is: " + stats.cumulativeReward);


                            });//end inner loop
                            Console.WriteLine((testTypes)testtype + " is Complete");
                            break;
                        }//Model-Based with initilization 

                    case testTypes.OurMB:
                        {
                            Parallel.For(0, runs, op2, (run) =>
                            {
                                World thisWorld = new EgoAlloGridWorld();
                                Agent<int[], int[]> agent = agent = (Agent<int[], int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(LinearEgoAlloValue<,>), true, 10, true);
                                System.Threading.Thread.Sleep(run * 100);

                                //run the training map
                                thisWorld.Load(mapLocation + trainmap);
                                int iterator = 0;
                                while (iterator++ <= mBStepCount)
                                    thisWorld.stepAgent("");

                                LinearEgoAlloValue<int[], int[]> temp = (LinearEgoAlloValue<int[], int[]>)(agent._actionValue);
                                temp.ResetAllocentric(true);

                                //run the Testing Map
                                PerformanceStats stats = new PerformanceStats();
                                thisWorld.Load(mapLocation + testmap);
                                iterator = 0;
                                while (iterator < mBStepCount)
                                {
                                    cumRwrd[testtype][run * mBStepCount + iterator] = (thisWorld.stepAgent("")).cumulativeReward;
                                    iterator++;
                                }

                            });//end inner loop
                            Console.WriteLine((testTypes)testtype + " is Complete");
                            break;
                        }//Model-Based with full predicition method

                    case testTypes.StandardMB:
                        {
                            Parallel.For(0, runs, op2, (run) =>
                            {
                                World thisWorld = new EgoAlloGridWorld();
                                Agent<int[], int[]> agent = agent = (Agent<int[], int[]>)thisWorld.addAgent(typeof(EGreedyPolicy<,>), typeof(ModelBasedValue<,>));
                                System.Threading.Thread.Sleep(run * 100);


                                PerformanceStats stats = new PerformanceStats();
                                //run the Testing Map
                                thisWorld.Load(mapLocation + testmap);
                                int iterator = 0;
                                while (iterator < mBStepCount)
                                {
                                    cumRwrd[testtype][run * mBStepCount + iterator] = (thisWorld.stepAgent("")).cumulativeReward;
                                    iterator++;
                                }

                            });//end inner loop
                            Console.WriteLine((testTypes)testtype + " is Complete");
                            break;
                        }//standard Model-Based without transfer

                }//END SWITCH
            }); //end outer-For loop

            //Write everything to output csv

            for (int testtype = 0; testtype < testTypeCount; testtype++)
            {
                System.IO.StreamWriter writer;
                writer = new System.IO.StreamWriter(testResultsLocation + (testTypes)testtype + "cumReward.csv");
                if (testtype <= 2)
                    for (int i = 0; i < runs * qStepCount; i++)
                    {
                        writer.WriteLine(cumRwrd[testtype][i].ToString());
                    }
                else
                    for (int i = 0; i < runs * mBStepCount; i++)
                        writer.WriteLine(cumRwrd[testtype][i].ToString());

                writer.Flush();
                writer.Close();
            }

        }
    }
}
