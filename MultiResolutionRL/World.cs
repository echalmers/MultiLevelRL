using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MultiResolutionRL.ValueCalculation;



namespace MultiResolutionRL
{


    public interface World
    {
        object addAgent(Type policyType, Type actionValueType, params object[] actionValueParameters);
        PerformanceStats stepAgent(string userAction = "");
        void Load(string filename);
        Bitmap showState(int width, int height, bool showPath = false);
        bool useFolder();
    }

    public class stochasticRewardGridworld : World
    {
        public Bitmap mapBmp;
        private int[,] map;
        private int[] startState;
        public Agent<int[], int[]> agent;
        List<int[]> availableActions;
        int[] currentRewardSite;
        Random rnd = new Random();
        IntArrayComparer comparer = new IntArrayComparer();
        HashSet<int[]> rewardSites;

        OutputData OD;
        DirectoryInfo DI;

        public stochasticRewardGridworld()
        {

            OD = new OutputData();
            DI = Directory.CreateDirectory(OD.getAddress("outputData"));

            availableActions = new List<int[]>();
            availableActions.Add(new int[2] { -1, 0 });
            availableActions.Add(new int[2] { 0, -1 });
            availableActions.Add(new int[2] { 1, 0 });
            availableActions.Add(new int[2] { 0, 1 });

            rewardSites = new HashSet<int[]>(comparer);

            // set the default agent
            startState = new int[2] { 1, 1 };

            Policy<int[], int[]> policy = new EGreedyPolicy<int[], int[]>();
            ActionValue<int[], int[]> value = new ModelFreeValue<int[], int[]>(new IntArrayComparer(), new IntArrayComparer(), availableActions, startState);
            agent = new Agent<int[], int[]>(startState, policy, value, availableActions);
        }

        public object addAgent(Type policyType, Type actionValueType, params object[] actionValueParameters)
        {
            policyType = policyType.MakeGenericType(typeof(int[]), typeof(int[]));
            Policy<int[], int[]> newPolicy = (Policy<int[], int[]>)Activator.CreateInstance(policyType);

            actionValueType = actionValueType.MakeGenericType(typeof(int[]), typeof(int[]));
            ActionValue<int[], int[]> newActionValue = (ActionValue<int[], int[]>)Activator.CreateInstance(actionValueType, new IntArrayComparer(), new IntArrayComparer(), availableActions, startState, actionValueParameters);

            agent = new Agent<int[], int[]>(startState, newPolicy, newActionValue, availableActions);
            return agent;
        }

        public void Load(string filename)
        {
            mapBmp = new Bitmap(filename);
            map = new int[mapBmp.Width, mapBmp.Height];

            for (int i = 0; i < mapBmp.Width; i++)
            {
                for (int j = 0; j < mapBmp.Height; j++)
                {
                    Color thisPixel = mapBmp.GetPixel(i, j);
                    if (thisPixel == Color.FromArgb(0, 0, 0))
                    {
                        startState = new int[2] { i, j };
                        mapBmp.SetPixel(i, j, Color.Yellow);
                    }

                    if (thisPixel == Color.FromArgb(0, 0, 255))
                        map[i, j] = 1;
                    else if (thisPixel == Color.FromArgb(255, 0, 0))
                        map[i, j] = 2;
                    else if (thisPixel == Color.FromArgb(255, 0, 255) || thisPixel == Color.FromArgb(0, 255, 0))
                    {
                        map[i, j] = 3;
                        rewardSites.Add(new int[2] { i, j });
                        mapBmp.SetPixel(i, j, Color.White);
                    }
                    else
                        map[i, j] = 0;
                }
            }

            agent.state = startState;
            currentRewardSite = rewardSites.ElementAt(rnd.Next(rewardSites.Count - 1));
        }

        public Bitmap showState(int width, int height, bool showPath = false)
        {
            width = 100; height = 100;
            Bitmap modMap = new Bitmap(mapBmp);

            modMap.SetPixel(agent.state[0], agent.state[1], Color.Black);
            modMap.SetPixel(currentRewardSite[0], currentRewardSite[1], Color.Green);

            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(modMap, 0, 0, width, height);
            }
            return resized;
        }

        public PerformanceStats stepAgent(string userAction = "")
        {
            int[] state = agent.state;
            int[] action;
            if (userAction == "")
                action = agent.selectAction();
            else
            {
                string[] act = userAction.Split(',');
                action = new int[2];
                action[0] = Convert.ToInt32(act[0]);
                action[1] = Convert.ToInt32(act[1]);
            }

            int[] newState = new int[2];
            double reward = 0;
            bool absorbingStateReached = false;

            // get the type of the new location
            int[] potentialNewState = new int[2] { state[0] + action[0], state[1] + action[1] };
            int newStateType = map[potentialNewState[0], potentialNewState[1]];
            switch (newStateType)
            {
                case 0: // open space
                    newState = new int[2] { state[0] + action[0], state[1] + action[1] };
                    reward = 0;
                    break;
                case 1: // wall
                    newState = new int[2] { state[0], state[1] };
                    reward = 0;
                    break;
                case 2: // lava
                    newState = new int[2] { state[0] + action[0], state[1] + action[1] };
                    reward = -0.5;
                    break;
                case 3: // reward site
                    if (comparer.Equals(currentRewardSite, potentialNewState))
                    {
                        currentRewardSite = rewardSites.ElementAt(rnd.Next(rewardSites.Count));
                        reward = 10;
                        absorbingStateReached = true;
                    }
                    else
                    {
                        //newState = new int[2] { state[0] + action[0], state[1] + action[1] };
                        reward = -0.01;
                    }
                    newState = new int[2] { startState[0], startState[1] };
                    break;
            }

            //LSValue<int[], int[]> av = (LSValue<int[], int[]>)((Agent<int[], int[]>)agent)._actionValue;
            //Console.Write(Math.Round(av.value(new int[2] { 1, 2 }, new int[2] { 0, -1 }), 2) + ",  ");
            //Console.Write(Math.Round(av.PredictReward(new int[2] { 1, 2 }, new int[2] { 0, -1 }, new int[2] { 5, 5 }), 2) + ",   ");
            //Console.Write(Math.Round(av.PredictReward(new int[2] { 1, 2 }, new int[2] { 0, -1 }, new int[2] { 1, 1 }), 2));

            //Console.Write("    |    " + Math.Round(av.value(new int[2] { 9, 8 }, new int[2] { 0, 1 }), 2) + ",  ");
            //Console.Write(Math.Round(av.PredictReward(new int[2] { 9, 8 }, new int[2] { 0, 1 }, new int[2] { 5, 5 }), 2) + ",   ");
            //Console.WriteLine(Math.Round(av.PredictReward(new int[2] { 9, 8 }, new int[2] { 0, 1 }, new int[2] { 9, 9 }), 2));

            agent.getStats().TallyStepsToGoal(reward > 0);

            agent.logEvent(new StateTransition<int[], int[]>(state, action, reward, newState, absorbingStateReached));
            return agent.getStats();
        }

        public void ExportGradients()
        {
            /*FilteredValue<int[], int[]>*/
            ActionValue<int[], int[]> av = /*(FilteredValue<int[], int[]>)*/agent._actionValue;
            StateManagement.intStateTree tree = new StateManagement.intStateTree();

            System.IO.StreamWriter valWriter = new System.IO.StreamWriter(OD.getAddress("gradientsVal"));
            for (int i = 0; i < map.GetLength(0); i++)
            {
                double[] thisXLine = new double[map.GetLength(1)];
                double[] thisYLine = new double[map.GetLength(1)];
                double[] thisValLine = new double[map.GetLength(1)];
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    double[] actionVals = av.value(new int[2] { i, j }, availableActions);
                    thisXLine[j] = actionVals[2] - actionVals[0];
                    thisYLine[j] = actionVals[3] - actionVals[1];
                    thisValLine[j] = actionVals.Max();
                }
                valWriter.WriteLine(string.Join(",", thisValLine));
            }
            valWriter.Flush(); valWriter.Close();
        }
        public bool useFolder() { return false; }
    }

    public class GridWorld : World
    {


        public Bitmap mapBmp;
        private int[,] map;
        private int[] startState;
        public Agent<int[], int[]> agent;
        List<int[]> availableActions;

        List<int[]> visitedStates = new List<int[]>();
        OutputData OD;
        DirectoryInfo DI;

        public GridWorld()
        {
            OD = new OutputData();
            DI = Directory.CreateDirectory(OD.getAddress("outputData"));


            availableActions = new List<int[]>();
            availableActions.Add(new int[2] { -1, 0 });
            availableActions.Add(new int[2] { 0, -1 });
            availableActions.Add(new int[2] { 1, 0 });
            availableActions.Add(new int[2] { 0, 1 });
            //availableActions.Add(new int[2] { -1, -1 });
            //availableActions.Add(new int[2] { 1, -1 });
            //availableActions.Add(new int[2] { -1, 1 });
            //availableActions.Add(new int[2] { 1, 1 });

            // set the default agent
            startState = new int[2] { 1, 1 };

            Policy<int[], int[]> policy = new EGreedyPolicy<int[], int[]>();
            ActionValue<int[], int[]> value = new ModelFreeValue<int[], int[]>(new IntArrayComparer(), new IntArrayComparer(), availableActions, startState);
            agent = new Agent<int[], int[]>(startState, policy, value, availableActions);
        }

        public void Load(string bmpFilename)
        {
            mapBmp = new Bitmap(bmpFilename);
            map = new int[mapBmp.Width, mapBmp.Height];

            for (int i = 0; i < mapBmp.Width; i++)
            {
                for (int j = 0; j < mapBmp.Height; j++)
                {
                    Color thisPixel = mapBmp.GetPixel(i, j);
                    if (thisPixel == Color.FromArgb(0, 0, 0))
                    {
                        startState = new int[2] { i, j };
                        mapBmp.SetPixel(i, j, Color.White);
                    }

                    if (thisPixel == Color.FromArgb(0, 0, 255))
                        map[i, j] = 1;
                    else if (thisPixel == Color.FromArgb(255, 0, 0))
                        map[i, j] = 2;
                    else if (thisPixel == Color.FromArgb(0, 255, 0))
                        map[i, j] = 3;
                    else
                        map[i, j] = 0;
                }
            }

            visitedStates.Clear();
            agent.state = startState;
        }

        public PerformanceStats stepAgent(string userAction = "")
        {
            int[] state = agent.state;
            if (!visitedStates.Contains(agent.state, new IntArrayComparer()))
                visitedStates.Add(agent.state);
            int[] action;
            if (userAction == "")
                action = agent.selectAction();
            else
            {
                string[] act = userAction.Split(',');
                action = new int[2];
                action[0] = Convert.ToInt32(act[0]);
                action[1] = Convert.ToInt32(act[1]);
            }

            int[] newState = new int[2];
            double reward = 0;
            bool absorbingStateReached = false;

            // get the type of the new location
            int newStateType = map[state[0] + action[0], state[1] + action[1]];
            switch (newStateType)
            {
                case 0: // open space
                    newState = new int[2] { state[0] + action[0], state[1] + action[1] };
                    reward = -0.01;
                    break;
                case 1: // wall
                    newState = new int[2] { state[0], state[1] };
                    reward = -0.1;
                    break;
                case 2: // lava
                    newState = new int[2] { state[0] + action[0], state[1] + action[1] };
                    reward = -0.5;
                    break;
                case 3: // goal
                    newState = new int[2] { startState[0], startState[1] };
                    reward = 10;
                    absorbingStateReached = true;
                    break;
            }

            agent.getStats().TallyStepsToGoal(reward > 0);
            //if (agent.getStats().stepsToGoal.Last() > 5000)
            //{
            //    agent.getStats().TallyStepsToGoal(true);
            //    newState = new int[2] { startState[0], startState[1] };
            //    absorbingStateReached = true;
            //}
            agent.logEvent(new StateTransition<int[], int[]>(state, action, reward, newState, absorbingStateReached));
            return agent.getStats();
        }

        public object addAgent(Type policyType, Type actionValueType, params object[] actionValueParameters)
        {
            policyType = policyType.MakeGenericType(typeof(int[]), typeof(int[]));
            Policy<int[], int[]> newPolicy = (Policy<int[], int[]>)Activator.CreateInstance(policyType);

            actionValueType = actionValueType.MakeGenericType(typeof(int[]), typeof(int[]));
            ActionValue<int[], int[]> newActionValue = (ActionValue<int[], int[]>)Activator.CreateInstance(actionValueType, new IntArrayComparer(), new IntArrayComparer(), availableActions, startState, actionValueParameters);

            agent = new Agent<int[], int[]>(startState, newPolicy, newActionValue, availableActions);
            return agent;
        }


        public Bitmap showState(int width, int height, bool showPath = false)
        {
            width = 144; height = 48;
            Bitmap modMap = new Bitmap(mapBmp);

            foreach (int[] state in visitedStates)
            {
                modMap.SetPixel(state[0], state[1], Color.FromArgb(mapBmp.GetPixel(state[0], state[1]).R * 3 / 4, mapBmp.GetPixel(state[0], state[1]).G * 3 / 4, mapBmp.GetPixel(state[0], state[1]).B * 3 / 4));
            }


            if (showPath)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader("log.txt");
                string text;
                while ((text = reader.ReadLine()) != null)
                {
                    string[] s = text.Split(',');
                    int x = Convert.ToInt32(s[0]);
                    int y = Convert.ToInt32(s[1]);
                    if (x >= mapBmp.Width || y >= mapBmp.Height)
                        continue;

                    int r = mapBmp.GetPixel(x, y).R;
                    int b = mapBmp.GetPixel(x, y).B;
                    int g = mapBmp.GetPixel(x, y).G;
                    if (s[2] == "p")
                    {
                        b = Math.Max(0, b - 50);
                        g = Math.Max(0, g - 50);
                    }
                    else if (s[2] == "g")
                    {
                        b = Math.Max(0, b - 100);
                        g = Math.Max(0, g - 100);
                    }
                    Color c = Color.FromArgb(r, g, b);
                    modMap.SetPixel(x, y, c);
                }
                reader.Close();
            }
            modMap.SetPixel(agent.state[0], agent.state[1], Color.Black);

            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(modMap, 0, 0, width, height);
            }
            return resized;
        }

        public void ExportGradients()
        {
            MultiResValue<int[], int[]> av = (MultiResValue<int[], int[]>)agent._actionValue;
            StateManagement.intStateTree tree = new StateManagement.intStateTree();


            //System.IO.StreamWriter xWriter = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\gradientsX.csv");
            System.IO.StreamWriter xWriter = new System.IO.StreamWriter(OD.getAddress("gradientsX"));
            // System.IO.StreamWriter yWriter = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\gradientsY.csv");
            System.IO.StreamWriter yWriter = new System.IO.StreamWriter(OD.getAddress("gradientsY"));
            // System.IO.StreamWriter valWriter = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\gradientsVal.csv");
            System.IO.StreamWriter valWriter = new System.IO.StreamWriter(OD.getAddress("gradientsVal"));

            for (int i = 0; i < map.GetLength(0); i++)
            {
                double[] thisXLine = new double[map.GetLength(1)];
                double[] thisYLine = new double[map.GetLength(1)];
                double[] thisValLine = new double[map.GetLength(1)];
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    int[] thisState = tree.GetParentState(new int[2] { i, j }, 0);
                    double[] actionVals = av.models[0].value(thisState, availableActions);
                    thisXLine[j] = actionVals[2] - actionVals[0];
                    thisYLine[j] = actionVals[3] - actionVals[1];
                    thisValLine[j] = actionVals.Max();
                }
                xWriter.WriteLine(string.Join(",", thisXLine));
                yWriter.WriteLine(string.Join(",", thisYLine));
                valWriter.WriteLine(string.Join(",", thisValLine));
            }
            xWriter.Flush(); xWriter.Close();
            yWriter.Flush(); yWriter.Close();
            valWriter.Flush(); valWriter.Close();
        }

        public void ExportAdjacencies()
        {
            System.IO.StreamWriter writerAdj = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Fuzzy Place Field Test\\Adjacencies.csv");
            ModelBasedValue<int[], int[]> model = (ModelBasedValue<int[], int[]>)agent._actionValue;
            IEqualityComparer<int[]> comparer = new IntArrayComparer();

            List<int[]> allStates = model.Qtable.Keys.ToList();

            foreach (int[] state in allStates)
            {
                foreach (int[] action in availableActions)
                {
                    int[] neighbor = model.PredictNextState(state, action);
                    writerAdj.WriteLine(string.Join(",", state) + "," + string.Join(",", neighbor));
                }
            }
            writerAdj.Flush(); writerAdj.Close();
        }
        public bool useFolder() { return false; }

        //public void ExportDistances()
        //{
        //    System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\distances.csv");
        //    ModelBasedValue<int[], int[]> model = (ModelBasedValue<int[], int[]>)agent._actionValue;
        //    PathFinder<int[], int[]> pathfinder = new PathFinder<int[], int[]>(new IntArrayComparer());

        //    double[,] distances = new double[model.Qtable.Keys.Count, model.Qtable.Keys.Count];
        //    List<int[]> allStates = model.Qtable.Keys.ToList();

        //    //for (int i=1; i<map.GetLength(0)-1; i++)
        //    //{
        //    //    for (int j=0; j<map.GetLength(1)-1; j++)
        //    //    {
        //    for (int i=0; i<allStates.Count; i++)
        //    {
        //        int index1 = allStates.IndexOf(allStates[i]);
        //        Dictionary<int[], double> dists = pathfinder.DijkstraDistances(allStates[i]/*new int[2] { i, j }*/, model, availableActions);

        //        foreach (int[] s in dists.Keys)
        //        {
        //            //writer.WriteLine(i + "," + j + "," + s[0] + "," + s[1] + "," + dists[s]);
        //            writer.WriteLine(allStates[i][0] + "," + allStates[i][1] + "," + s[0] + "," + s[1] + "," + dists[s]);
        //        }

        //        for (int j=0; j<allStates.Count; j++)
        //        {
        //            distances[i, j] = dists[allStates[j]];
        //            distances[j, i] = dists[allStates[j]];
        //        }

        //    }
        //    //    }
        //    //} 

        //    writer.Flush();
        //    writer.Close();

        //    writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\distancesMat.csv");
        //    for (int i = 0; i < allStates.Count; i++)
        //    {
        //        for (int j = 0; j < allStates.Count; j++)
        //        {
        //            writer.Write(distances[i,j] + ",");
        //        }
        //        writer.Write(Environment.NewLine);
        //    }
        //    writer.Flush();
        //    writer.Close();

        //    writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\distancesClusters.csv");
        //    StateManagement.SelfAbstractingStateTree<int[]> tree = new StateManagement.SelfAbstractingStateTree<int[]>();
        //    List<int[]> clusters = tree.PerformAbstraction(distances, allStates, new IntArrayComparer());
        //    for (int i= 0; i < clusters.Count; i++)
        //    {
        //        writer.WriteLine(string.Join(",", allStates[i]) + "," + string.Join(",", clusters[i]));
        //    }
        //    writer.Flush();
        //    writer.Close();
        //}

    }

    public class MountainCar : World
    {

        public Agent<int[], int> agent;
        double g = -0.0025;
        List<int> availableActions = new List<int>();
        double _position;
        double _velocity;
        Random rnd = new Random(0);
        Bitmap hill = new Bitmap(18, 101);

        OutputData OD;
        DirectoryInfo DI;

        public MountainCar()
        {
            OD = new OutputData();
            DI = Directory.CreateDirectory(OD.getAddress("outputData"));

            availableActions.Add(-1);
            //availableActions.Add(0);
            availableActions.Add(1);
            rndStartState(out _position, out _velocity);

            // set the default agent
            Policy<int[], int> policy = new EGreedyPolicy<int[], int>();
            ActionValue<int[], int> value = new ModelFreeValue<int[], int>(new IntArrayComparer(), EqualityComparer<int>.Default, availableActions, discretizeState(_position, _velocity));
            agent = new Agent<int[], int>(discretizeState(_position, _velocity), policy, value, availableActions);

            // create the hill bitmap
            for (int i = 0; i < 17; i++)
            {
                double position = (0.5 + 1.2) / (18 - 1) * i - 1.2;
                double amplitude = Math.Sin(3 * position);
                amplitude = amplitude * 50 + 50;
                hill.SetPixel(i, (int)(100 - amplitude), Color.ForestGreen);
            }
        }

        public void Load(string bmpFilename)
        { }

        void rndStartState(out double position, out double velocity)
        {
            position = rnd.NextDouble() * 1.7 - 1.2;
            velocity = rnd.NextDouble() * 0.14 - 0.07;
        }

        int[] discretizeState(double position, double velocity)
        {
            int[] discretized = new int[2];
            discretized[0] = (int)(Math.Round(position + 1.2, 1) * 10);
            discretized[1] = (int)(Math.Round(velocity + 0.07, 2) * 100);
            return discretized;
        }

        public object addAgent(Type policyType, Type actionValueType, params object[] actionValueParameters)
        {
            policyType = policyType.MakeGenericType(typeof(int[]), typeof(int));
            Policy<int[], int> newPolicy = (Policy<int[], int>)Activator.CreateInstance(policyType);

            actionValueType = actionValueType.MakeGenericType(typeof(int[]), typeof(int));
            ActionValue<int[], int> newActionValue = (ActionValue<int[], int>)Activator.CreateInstance(actionValueType, new IntArrayComparer(), EqualityComparer<int>.Default, availableActions, discretizeState(_position, _velocity), actionValueParameters);

            agent = new Agent<int[], int>(discretizeState(_position, _velocity), newPolicy, newActionValue, availableActions);
            return agent;
        }

        public PerformanceStats stepAgent(string userAction = "")
        {
            int action;
            if (userAction == "")
                action = agent.selectAction();
            else
                action = Convert.ToInt32(userAction);

            _velocity += 0.001 * (double)action + g * Math.Cos(3 * _position);
            _velocity = Math.Min(0.07, Math.Max(_velocity, -0.07));
            _position += _velocity;
            if (_position > 0.5 || _position < -1.2)
            {
                _position = Math.Max(-1.2, Math.Min(_position, 0.5));
                _velocity = 0;
            }


            bool absorbingStateReached;
            double reward;
            if (_position >= 0.5)
            {
                reward = 20;
                rndStartState(out _position, out _velocity);
                absorbingStateReached = true;
            }
            else
            {
                reward = -0.01;
                absorbingStateReached = false;
            }

            agent.logEvent(new StateTransition<int[], int>(agent.state, action, reward, discretizeState(_position, _velocity), absorbingStateReached));

            Console.WriteLine(action + ": " + String.Join(",", discretizeState(_position, _velocity)));

            return agent.getStats();
        }

        public Bitmap showState(int width, int height, bool showPath = false)
        {
            int position = agent.state[0];

            double positionDouble = (0.5 + 1.2) / (18 - 1) * position - 1.2;
            double amplitude = Math.Sin(3 * positionDouble);
            amplitude = amplitude * 49 + 50;
            Bitmap map = new Bitmap(hill);
            map.SetPixel(position, (int)(100 - amplitude), Color.Firebrick);
            map.SetPixel(position, (int)(101 - amplitude), Color.Firebrick);
            map.SetPixel(position, (int)(99 - amplitude), Color.Firebrick);

            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(map, 0, 0, width, height);
            }
            return resized;
        }
        public bool useFolder() { return false; }
    }

    public class Taxi : World
    {
        public Bitmap mapBmp;
        private int[,] map;
        private int[] startLocation = new int[2];
        public Agent<int[], int> agent;
        List<int> availableActions = new List<int>();
        List<int[]> dropSites = new List<int[]>();
        IEqualityComparer<int[]> stateComparer = new IntArrayComparer();
        Random rnd = new Random(0);

        double pickupReward = 0;
        double dropReward = 40;


        OutputData OD;
        DirectoryInfo DI;

        public Taxi()
        {
            OD = new OutputData();
            DI = Directory.CreateDirectory(OD.getAddress("outputData"));

            dropSites.Add(null);

            availableActions.Add(1);//left
            availableActions.Add(2);//up
            availableActions.Add(3);//right
            availableActions.Add(4);//down
            availableActions.Add(5);//pickup
            availableActions.Add(6);//drop

            // set the default agent
            Policy<int[], int> policy = new EGreedyPolicy<int[], int>();
            ActionValue<int[], int> value = new ModelFreeValue<int[], int>(new IntArrayComparer(), EqualityComparer<int>.Default, availableActions, new int[4] { 1, 2, 1, 10 });
            agent = new Agent<int[], int>(new int[4] { 1, 2, 10, 1 }, policy, value, availableActions);
        }

        int[] rndStartState()
        {
            int[] state = new int[4] { startLocation[0], startLocation[1], rnd.Next(5, 10), rnd.Next(1, dropSites.Count) };
            //state[2] = 10; //********************
            return state;
        }

        public object addAgent(Type policyType, Type actionValueType, params object[] actionValueParameters)
        {
            int[] startState = rndStartState();

            policyType = policyType.MakeGenericType(typeof(int[]), typeof(int));
            Policy<int[], int> newPolicy = (Policy<int[], int>)Activator.CreateInstance(policyType);

            actionValueType = actionValueType.MakeGenericType(typeof(int[]), typeof(int));
            ActionValue<int[], int> newActionValue = (ActionValue<int[], int>)Activator.CreateInstance(actionValueType, new IntArrayComparer(), EqualityComparer<int>.Default, availableActions, startState, actionValueParameters);

            agent = new Agent<int[], int>(startState, newPolicy, newActionValue, availableActions);
            return agent;
        }

        public PerformanceStats stepAgent(string userAction = "")
        {
            int[] state = agent.state;

            int action;
            if (userAction == "")
                action = agent.selectAction();
            else
                action = Convert.ToInt32(userAction);

            int[] newState = new int[4];
            Array.Copy(state, newState, state.Length);
            double reward = 0;
            bool absorbingStateReached = false;

            if (action > 0 && action < 5)// perform navigation
            {
                absorbingStateReached = performNavigation(action, state, out newState, out reward);
            }
            else if (action == 5) // pickup
            {
                if (state[3] < 0)// if a drop is currently required
                {
                    reward = -10;
                }
                else
                {
                    int[] currentLocation = new int[2] { state[0], state[1] };
                    if (stateComparer.Equals(currentLocation, dropSites[state[3]]))
                    {
                        reward = pickupReward;
                        newState[3] = -rnd.Next(1, dropSites.Count);
                    }
                    else
                        reward = -10;
                }
            }
            else if (action == 6) // drop
            {
                if (state[3] > 0)// if a pickup is currently required
                {
                    reward = -10;
                }
                else
                {
                    int[] currentLocation = new int[2] { state[0], state[1] };
                    if (stateComparer.Equals(currentLocation, dropSites[-state[3]]))
                    {
                        reward = dropReward;
                        newState = rndStartState();
                        absorbingStateReached = true;
                    }
                    else
                        reward = -10;
                }
            }

            //newState[2] = 10; //*****************

            agent.logEvent(new StateTransition<int[], int>(state, action, reward, newState, absorbingStateReached));

            return agent.getStats();
        }

        public bool performNavigation(int action, int[] state, out int[] newState, out double reward)
        {
            newState = new int[4]; Array.Copy(state, newState, state.Length);
            reward = -1;

            switch (action)
            {
                case 1:
                    newState[0] = state[0] - 1; break;
                case 2:
                    newState[1] = state[1] - 1; break;
                case 3:
                    newState[0] = state[0] + 1; break;
                case 4:
                    newState[1] = state[1] + 1; break;
            }

            // get the type of the new location
            int newStateType = map[newState[0], newState[1]];
            switch (newStateType)
            {
                case 0: // open space
                    newState[2] -= 1;
                    break;
                case 1: // wall
                    newState[2] -= 1;
                    newState[0] = state[0]; newState[1] = state[1];
                    break;
                case 4: // gas station
                    newState[0] = state[0]; newState[1] = state[1];
                    newState[2] = 10;
                    break;
            }

            if (newState[2] < 0)
            {
                reward = -20;
                newState = rndStartState();
                return true;
            }
            else
                return false;
        }

        public void Load(string bmpFilename)
        {
            if (bmpFilename.IndexOf("modReward") != -1)
            {
                dropReward = 20; pickupReward = 20;
            }
            else
            {
                dropReward = 40; pickupReward = 0;
            }

            dropSites.Clear();
            dropSites.Add(null);

            mapBmp = new Bitmap(bmpFilename);
            map = new int[mapBmp.Width, mapBmp.Height];

            for (int i = 0; i < mapBmp.Width; i++)
            {
                for (int j = 0; j < mapBmp.Height; j++)
                {
                    Color thisPixel = mapBmp.GetPixel(i, j);
                    if (thisPixel == Color.FromArgb(0, 0, 0))
                    {
                        startLocation[0] = i; startLocation[1] = j;
                        mapBmp.SetPixel(i, j, Color.White);
                    }

                    if (thisPixel == Color.FromArgb(0, 0, 255))
                        map[i, j] = 1;
                    else if (thisPixel == Color.FromArgb(255, 0, 0))
                        map[i, j] = 2;
                    else if (thisPixel == Color.FromArgb(0, 255, 0))
                    {
                        map[i, j] = 3;
                        dropSites.Add(new int[2] { i, j });
                        mapBmp.SetPixel(i, j, Color.White);
                    }
                    else if (thisPixel == Color.FromArgb(255, 255, 0))
                        map[i, j] = 4;
                    else
                        map[i, j] = 0;
                }
            }

            agent.state = rndStartState();
        }

        public Bitmap showState(int width, int height, bool showPath = false)
        {
            double fuel = (double)(agent.state[2]) / 10;
            Color fuelColor = Color.FromArgb((int)((1 - fuel) * 255), (int)(fuel * 255), 50);

            Bitmap modMap = new Bitmap(mapBmp);

            modMap.SetPixel(agent.state[0], agent.state[1], fuelColor);
            int pickupStatus = agent.state[3];
            if (pickupStatus > 0) // pickup required
            {
                modMap.SetPixel(dropSites[pickupStatus][0], dropSites[pickupStatus][1], Color.Green);
            }
            else if (pickupStatus < 0) // drop required
            {
                modMap.SetPixel(dropSites[-pickupStatus][0], dropSites[-pickupStatus][1], Color.Red);
            }

            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(modMap, 0, 0, width, height);
            }
            return resized;
        }
        public bool useFolder() { return false; }
    }

    public class ProceduralGridWorld : World
    {
        public Bitmap[] mapBmp;
        public Bitmap displayMap;
        private int[,] map;
        private int[] startState;
        public Agent<int[], int[]> agent;
        List<int[]> availableActions;

        List<int[]> visitedStates = new List<int[]>();
        OutputData OD;
        DirectoryInfo DI;

        ProceduralGridWorld()
        {
            OD = new OutputData();
            DI = Directory.CreateDirectory(OD.getAddress("outputData"));

            availableActions = new List<int[]>();
            availableActions.Add(new int[2] { -1, 0 });
            availableActions.Add(new int[2] { 0, -1 });
            availableActions.Add(new int[2] { 1, 0 });
            availableActions.Add(new int[2] { 0, 1 });

            startState = new int[2] { 1, 1 };

            Policy<int[], int[]> policy = new EGreedyPolicy<int[], int[]>();
            ActionValue<int[], int[]> value = new ModelFreeValue<int[], int[]>(new IntArrayComparer(), new IntArrayComparer(), availableActions, startState);
            agent = new Agent<int[], int[]>(startState, policy, value, availableActions);

        }

        public object addAgent(Type policyType, Type actionValueType, params object[] actionValueParameters)
        {
            policyType = policyType.MakeGenericType(typeof(int[]), typeof(int[]));
            Policy<int[], int[]> newPolicy = (Policy<int[], int[]>)Activator.CreateInstance(policyType);

            actionValueType = actionValueType.MakeGenericType(typeof(int[]), typeof(int[]));
            ActionValue<int[], int[]> newActionValue = (ActionValue<int[], int[]>)Activator.CreateInstance(actionValueType, new IntArrayComparer(), new IntArrayComparer(), availableActions, startState, actionValueParameters);

            agent = new Agent<int[], int[]>(startState, newPolicy, newActionValue, availableActions);
            return agent;
        }

        //Will load up a set of maps from the folderName
        //Ultimately they will be stitched together and one will have its goal state kept while the others have tehm removed.
        //Will need to pad the maps with a pixel wide blue border,
        //Determine edge maps, and what edge they rest on to give appropriate border
        public void Load(string folderName)
        {
            int numMaps = 4;
            int mapsInFolder = 4;
            int mapW = 0;
            int mapH = 0;
            //First select n-number of maps, 
            for (int i = 0; i < numMaps; i++)
            {
                string m = (new Random()).Next(0, mapsInFolder).ToString();
                mapBmp[i] = new Bitmap(m + ".bmp");
                mapW += mapBmp[i].Width;
                if (mapH < mapBmp[i].Height)
                    mapH = mapBmp[i].Height;
                map = new int[mapW, mapH];
            }

            MapFromBitmap(mapBmp, map, numMaps);
         displayMap = new Bitmap(newDisplayBit(map,mapW,mapH));

            visitedStates.Clear();
            agent.state = startState;
        }

        //STep the agent from one state to another
        public PerformanceStats stepAgent(string userAction = "")
        {
            {
                int[] state = agent.state;
                if (!visitedStates.Contains(agent.state, new IntArrayComparer()))
                    visitedStates.Add(agent.state);
                int[] action;
                if (userAction == "")
                    action = agent.selectAction();
                else
                {
                    string[] act = userAction.Split(',');
                    action = new int[2];
                    action[0] = Convert.ToInt32(act[0]);
                    action[1] = Convert.ToInt32(act[1]);
                }

                int[] newState = new int[2];
                double reward = 0;
                bool absorbingStateReached = false;

                // get the type of the new location
                int newStateType = map[state[0] + action[0], state[1] + action[1]];
                switch (newStateType)
                {
                    case 0: // open space
                        newState = new int[2] { state[0] + action[0], state[1] + action[1] };
                        reward = -0.01;
                        break;
                    case 1: // wall
                        newState = new int[2] { state[0], state[1] };
                        reward = -0.1;
                        break;
                    case 2: // lava
                        newState = new int[2] { state[0] + action[0], state[1] + action[1] };
                        reward = -0.5;
                        break;
                    case 3: // goal
                        newState = new int[2] { startState[0], startState[1] };
                        reward = 10;
                        absorbingStateReached = true;
                        break;
                }

                agent.getStats().TallyStepsToGoal(reward > 0);
                //if (agent.getStats().stepsToGoal.Last() > 5000)
                //{
                //    agent.getStats().TallyStepsToGoal(true);
                //    newState = new int[2] { startState[0], startState[1] };
                //    absorbingStateReached = true;
                //}
                agent.logEvent(new StateTransition<int[], int[]>(state, action, reward, newState, absorbingStateReached));
                return agent.getStats();
            }
        }

        public Bitmap showState(int width, int height, bool showPath = false)
        {
            width = 144; height = 48;
            Bitmap modMap = new Bitmap(displayMap);

            foreach (int[] state in visitedStates)
            {
                modMap.SetPixel(state[0], state[1], Color.FromArgb(displayMap.GetPixel(state[0], state[1]).R * 3 / 4, displayMap.GetPixel(state[0], state[1]).G * 3 / 4, displayMap.GetPixel(state[0], state[1]).B * 3 / 4));
            }


            if (showPath)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader("log.txt");
                string text;
                while ((text = reader.ReadLine()) != null)
                {
                    string[] s = text.Split(',');
                    int x = Convert.ToInt32(s[0]);
                    int y = Convert.ToInt32(s[1]);
                    if (x >= displayMap.Width || y >= displayMap.Height)
                        continue;

                    int r = displayMap.GetPixel(x, y).R;
                    int b = displayMap.GetPixel(x, y).B;
                    int g = displayMap.GetPixel(x, y).G;
                    if (s[2] == "p")
                    {
                        b = Math.Max(0, b - 50);
                        g = Math.Max(0, g - 50);
                    }
                    else if (s[2] == "g")
                    {
                        b = Math.Max(0, b - 100);
                        g = Math.Max(0, g - 100);
                    }
                    Color c = Color.FromArgb(r, g, b);
                    modMap.SetPixel(x, y, c);
                }
                reader.Close();
            }
            modMap.SetPixel(agent.state[0], agent.state[1], Color.Black);

            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(modMap, 0, 0, width, height);
            }
            return resized;
        }
    
       
        public void MapFromBitmap(Bitmap[] mapBmp, int[,] map, int numMaps)
        {
            int ctsI = 0, ctsJ = 0;
            for (int n = 0; n < mapBmp.Length; n++)
                for (int i = 0; i < mapBmp[n].Width; i++)
                    for (int j = 0; j < mapBmp[n].Height; j++)//Assumes they are all the same height at the moment
                    {
                        ctsI += i;
                        ctsJ += j;
                        Color thisPixel = mapBmp[n].GetPixel(i, j);
                        if (thisPixel == Color.FromArgb(0, 0, 0))//If it is the black pixel, Agent
                        {
                            if (n == 0)//Only first state is the start segment right now
                                startState = new int[2] { i, j };


                            mapBmp[n].SetPixel(i, j, Color.White);
                        }
                        else if (thisPixel == Color.FromArgb(0, 0, 255))//Its a wall; 
                            map[i, j] = 1;
                        else if (thisPixel == Color.FromArgb(255, 0, 0))
                            map[i, j] = 2;
                        else if (thisPixel == Color.FromArgb(0, 255, 0))//Its the Goal State
                        {
                            if (n == numMaps)//Only the end state is the goal state;
                                map[i, j] = 3;
                            else map[i, j] = 0;
                        }
                        else
                            map[i, j] = 0;
                    }
        }
        
         //Given the collection of Bitmaps, stitch them together into a new bitmap
        public Bitmap newDisplayBit(int[,] map,int mapW, int mapH)
        {
            Bitmap displayMap = new Bitmap(mapH, mapW);
               
            //Set the colors on the new bitmap according to the int map
            for(int i=1;i< mapW;i++)
                for(int j=1;j<mapH;j++)
            {
                if(map[i,j] == 1)//Is a Wall
                displayMap.SetPixel(i,j, Color.FromArgb(0, 0, 255));
                switch(map[i,j])
                    {
                        case 1://Its a wall
                            displayMap.SetPixel(i, j, Color.FromArgb(0, 0, 255)); break;
                        case 2://Its a Lava
                            displayMap.SetPixel(i, j, Color.FromArgb(255, 0, 0)); break;
                        case 3://Its the Goal
                            displayMap.SetPixel(i, j, Color.FromArgb(0, 255, 0)); break;
                        default://White Space
                            displayMap.SetPixel(i, j, Color.FromArgb(255, 255, 255)); break;
                    }
            }
            //Buffer the blue Border top and bottom
            for (int i = 0; i < mapW; i++)
            {
                displayMap.SetPixel(i, 0, Color.FromArgb(0,0, 255));
                displayMap.SetPixel(i, mapW-1, Color.FromArgb(0,0, 255));
            }
            for (int i = 0; i < mapH; i++)
            {
                displayMap.SetPixel(0, i, Color.FromArgb(0, 0, 255));
                displayMap.SetPixel(mapH-1, i, Color.FromArgb(0, 0, 255));
            }

            return displayMap;
        }
        public bool useFolder() { return true; }

    }


}

