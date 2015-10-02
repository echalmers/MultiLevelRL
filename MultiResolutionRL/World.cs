using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using MultiResolutionRL.ValueCalculation;

namespace MultiResolutionRL
{
    public interface World
    {
        void addAgent(Type policyType, Type actionValueType, params int[] actionValueParameters);
        double stepAgent();
        void Load(string filename);
        Bitmap showState(int width, int height, bool showPath = false);
    }

    public class GridWorld : World
    {
        public Bitmap mapBmp;
        private int[,] map;
        private int[] startState;
        public Agent<int[], int[]> agent;
        List<int[]> availableActions;

        List<int[]> visitedStates = new List<int[]>();
        
        public GridWorld()
        {
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
                
        public double stepAgent()
        {
            int[] state = agent.state;
            if (!visitedStates.Contains(agent.state,new IntArrayComparer()))
                visitedStates.Add(agent.state);
            int[] action = agent.selectAction();

            int[] newState = new int[2];
            double reward = 0;
            bool absorbingStateReached = false;

            // get the type of the new location
            int newStateType = map[state[0] + action[0], state[1] + action[1]];
            switch (newStateType)
            {
                case 0: // open space
                    newState = new int[2] {state[0] + action[0], state[1] + action[1]};
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
                    newState = new int[2] {startState[0], startState[1]};
                    reward = 10;
                    absorbingStateReached = true;
                    break;
            }
            
            agent.logEvent(new StateTransition<int[], int[]>(state, action, reward, newState, absorbingStateReached));
            return agent.cumulativeReward.Last();
        }

        public void addAgent(Type policyType, Type actionValueType, params int[] actionValueParameters)
        {
            policyType = policyType.MakeGenericType(typeof(int[]), typeof(int[]));
            Policy<int[], int[]> newPolicy = (Policy<int[],int[]>)Activator.CreateInstance(policyType);

            actionValueType = actionValueType.MakeGenericType(typeof(int[]), typeof(int[]));
            ActionValue<int[], int[]> newActionValue = (ActionValue<int[], int[]>)Activator.CreateInstance(actionValueType, new IntArrayComparer(), new IntArrayComparer(), availableActions, startState, actionValueParameters);
            
            agent = new Agent<int[], int[]>(startState, newPolicy, newActionValue, availableActions);
        }


        public Bitmap showState(int width, int height, bool showPath = false)
        {
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
                    if (text.IndexOf("null") != -1)
                        continue;
                    int start = text.IndexOf("Level ") + 6;
                    string goalLevelString = text.Substring(start, 1);
                    int goalLevel = Convert.ToInt32(goalLevelString);
                    start = text.IndexOf("at ") + 3;
                    string[] goalString = text.Substring(start).Split(',');
                    int[] goal = new int[2];
                    goal[0] = Convert.ToInt32(goalString[0]);
                    goal[1] = Convert.ToInt32(goalString[1]);

                    for (int i = 0; i < (1 << goalLevel); i++)
                    {
                        for (int j = 0; j < (1 << goalLevel); j++)
                        {
                            int x = i + (goal[0] << goalLevel);
                            int y = j + (goal[1] << goalLevel);
                            if ((x < modMap.Width) && (x >= 0) && (y < modMap.Height) && (y >= 0))
                            {
                                int r = Math.Min(mapBmp.GetPixel(x, y).R + 50, 255);
                                int g = Math.Max(mapBmp.GetPixel(x, y).G - 50, 0);
                                int b = Math.Max(mapBmp.GetPixel(x, y).B - 50, 0);
                                Color c = Color.FromArgb(r, g, b);
                                modMap.SetPixel(x, y, c);
                            }
                        }
                    }
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
    }

}
