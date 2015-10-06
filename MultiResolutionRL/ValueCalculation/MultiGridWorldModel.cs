using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class MultiGridWorldModel<stateType,actionType> : ActionValue<int[], int[]>
    {
        public Goal<int[], int[]> currentGoal;
        //Dictionary<Goal<int[]>, double> inhibitions;

        ActionValue<int[], int[]>[] models;
        StateTransition<int[], int[]>[] transitions;
        public List<Goal<int[], int[]>>[] subgoals;
        List<int[]> availableActions;
        PathFinder<int[], int[]> pathFinder;
        StateManagement.StateTree<int[]> stateTree;

        Random rnd = new Random(1);

        IEqualityComparer<int[]> stateComparer;
        IEqualityComparer<int[]> actionComparer;

        int steps = 0;

        //Func<int[], List<int[]>> GetLowerLevelStatesMethod = (highLevelState) =>
        //    {
        //        List<int[]> lowLevelStates = new List<int[]>();
        //        lowLevelStates.Add(new int[2] { highLevelState[0] << 1, highLevelState[1] << 1 });
        //        lowLevelStates.Add(new int[2] { (highLevelState[0] << 1) + 1, highLevelState[1] << 1 });
        //        lowLevelStates.Add(new int[2] { highLevelState[0] << 1, (highLevelState[1] << 1) + 1 });
        //        lowLevelStates.Add(new int[2] { (highLevelState[0] << 1) + 1, (highLevelState[1] << 1) + 1 });
        //        return lowLevelStates;
        //    };

        System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\MultiResolutionRL\\multiModelUpdates.txt");

        public MultiGridWorldModel(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params int[] numLayers)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, numLayers)
        {
            models = new ActionValue<int[], int[]>[numLayers[0]];
            transitions = new StateTransition<int[], int[]>[numLayers[0]];
            subgoals = new List<Goal<int[], int[]>>[numLayers[0]];

            availableActions = AvailableActions;
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            stateTree = new StateManagement.StateTree<int[]>(stateComparer, StartState);
            //stateTree = new intStateTree(stateComparer);
            
            //int[] x = new int[8];
            //int[] y = new int[8];

            //while (x[5]==0 && y[5]==0)
            //{
            //    int thisi = 0, thisj = 0;
            //    for (int l = 0; l<7; l++)
            //    {
            //        thisi += x[l] * (int)Math.Pow(2,l);
            //        thisj += y[l] * (int)Math.Pow(2,l);
            //    }
            //    stateTree.AddState(new int[2] { thisi, thisj });

            //    x[0]++;
            //    for (int l = 0; l < 6; l++)
            //    {
            //        if (x[l]==2)
            //        {
            //            x[l] = 0;
            //            y[l]++;
            //        }
            //        if (y[l]==2)
            //        {
            //            y[l] = 0;
            //            x[l + 1]++;
            //        }
            //    }
            //}


                pathFinder = new PathFinder<int[], int[]>(stateComparer);

            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new ModelBasedValue<int[], int[]>(stateComparer, actionComparer, availableActions, StartState) 
                { 
                    writer = writer, 
                    maxUpdates = 10, 
                    defaultQ = i==0 ? 1 : 0,
                    gamma = i==0 ? 0.9 : 0.5
                };
                //models[i] = new WorldMemoryModel<int[], int[]>(StateComparer, ActionComparer, AvailableActions)
                //{
                //    MemorySize = i==0 ? int.MaxValue : (2 << i) - 1, //**************************************
                //    gamma = 0.5,
                //    defaultQ = i==0 ? 1 : 0,
                //    //stateUpdateSelector = (state) =>
                //    //{
                //    //    int[][] states = new int[4][];
                //    //    states[0] = new int[2] { ((state[0] >> 1) << 1), ((state[1] >> 1) << 1) };
                //    //    states[1] = new int[2] { ((state[0] >> 1) << 1) + 1, ((state[1] >> 1) << 1) };
                //    //    states[2] = new int[2] { ((state[0] >> 1) << 1), ((state[1] >> 1) << 1) + 1 };
                //    //    states[3] = new int[2] { ((state[0] >> 1) << 1) + 1, ((state[1] >> 1) << 1) + 1 };
                //    //    return states;
                //    //    //return new int[1][] {state};
                //    //},
                //    writer = writer
                //};
                
                transitions[i] = new StateTransition<int[], int[]>(null, null, 0, null);
                subgoals[i] = new List<Goal<int[], int[]>>();
            }
            currentGoal = new Goal<int[], int[]>(0, null, null, null, 0, stateComparer, actionComparer);
            //inhibitions = new Dictionary<Goal<int[]>, double>(new GoalComparer<int[]>());

        }
       

        private Goal<int[],int[]> selectGoal(int[] state, int maxLevel, List<int[]> availableActions)
        {
            Goal<int[], int[]> newGoal = new Goal<int[],int[]>(-1, null, null, null, double.NegativeInfinity, null, null);

            double[][] values = new double[models.Length][];
            for (int i = 0; i <= maxLevel; i++)
            {
                int[] thisState = stateTree.GetParentState(state, i);
                values[i] = models[i].value(thisState, availableActions);
            }

            // the default new goal will be the best level 0 goal
            for (int i = 0; i < availableActions.Count(); i++)
            {
                if (values[0][i] > newGoal.value || newGoal == null)
                {
                    int[] goalState = models[0].PredictBestNextState(state, availableActions.ElementAt(i)); ///***************************** might need to find best possible next state instead
                    newGoal = new Goal<int[],int[]>(0, state, availableActions.ElementAt(i), goalState, values[0][i], stateComparer, actionComparer);
                }
            }

            // find the best action at any allowed level
            for (int l = 1; l <= maxLevel; l++)
            {
                int[] thisState = stateTree.GetParentState(state, l);
                for (int i = 0; i < availableActions.Count(); i++)
                {
                    int[] thisGoal = models[l].PredictNextState(thisState, availableActions.ElementAt(i));
                    if (thisGoal == null)
                        continue;
                    Goal<int[],int[]> candidateGoal = new Goal<int[],int[]>(l, thisState, availableActions.ElementAt(i), thisGoal, values[l][i], stateComparer, actionComparer);

                    // check if the goal is allowed
                    if ((currentGoal.goalState != null) && (l < currentGoal.level))
                    {
                        int[] temp = stateTree.GetParentState(thisGoal, currentGoal.level - l);
                        if (!stateComparer.Equals(temp, currentGoal.goalState))
                        {
                            continue;
                        }
                    }
                    else if ((currentGoal.goalState != null) && (l == currentGoal.level) && (candidateGoal.value < currentGoal.value))
                        continue;

                    //if (inhibitions.ContainsKey(candidateGoal))
                    //{
                    //    candidateGoal.value = candidateGoal.value + (-1 - candidateGoal.value) * inhibitions[candidateGoal];
                    //}

                    if (candidateGoal.value > newGoal.value)
                    {
                        if (l > 0 && values[l][i] <= 0)//***************************
                            continue;
                        //newGoal.value = values[l][i];
                        //actionIndex = i;
                        //newGoal.level = l;
                        newGoal = candidateGoal;
                    }
                }
            }

            return newGoal;
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            if (currentGoal.goalState == null)
            {
                currentGoal = selectGoal(state, models.Length - 1, actions);
                subgoals[currentGoal.level].Clear();
                subgoals[currentGoal.level].Add(currentGoal);

            }

            for (int l = currentGoal.level - 1; l >= 0; l--)
            {
                if (subgoals[l].Count == 0)
                {
                    // plan the route to the goal
                    List<int[]> goalStates = stateTree.GetChildren(subgoals[l + 1][0].goalState, l + 1);
                    int[] startState = stateTree.GetParentState(state, l);
                    List<Tuple<int[], int[], double>> path = pathFinder.AStar(startState, goalStates, models[l], actions, l!=0);
                    subgoals[l] = path2subgoals(path, l, models[l]);

                    if (subgoals[l].Count == 0) // if no path is known, pass control to lowest level
                    {
                        goalStates = stateTree.GetChildren(subgoals[l + 1][0].goalState, l + 1);
                        int[] currentGoalLevelState = stateTree.GetParentState(state, currentGoal.level);
                        Console.WriteLine("couldn't find a path to level " + currentGoal.level + ": " + currentGoal.goalState[0] + "," + currentGoal.goalState[1] + " at level " + l);
                        goalStates = stateTree.GetChildren(subgoals[l + 1][0].goalState, l + 1);
                        path = pathFinder.AStar(startState, goalStates, models[l], actions, l != 0);

                        //models[0].ForgetBoundaries();
                        currentGoal.goalState = null;
                        for (int i = 0; i < subgoals.Length; i++)
                        {
                            subgoals[i].Clear();
                        }
                        currentGoal = selectGoal(state, 0, actions);
                        subgoals[0].Clear();
                        subgoals[0].Add(currentGoal);
                        break;

                        //if (l == 0)
                        //{
                        //    currentGoal.goalState = null;
                        //    for (int i = 0; i < subgoals.Length; i++)
                        //    {
                        //        subgoals[i].Clear();
                        //    }
                        //    currentGoal = selectGoal(state, 0, actions);
                        //    subgoals[0].Clear();
                        //    subgoals[0].Add(currentGoal);
                        //    break;
                        //}
                        //else
                        //{
                        //    currentGoal.goalState = null;
                        //    for (int i = 0; i < subgoals.Length; i++)
                        //    {
                        //        subgoals[i].Clear();
                        //    }
                        //    currentGoal = selectGoal(state, currentGoal.level - 1, availableActions);
                        //    subgoals[currentGoal.level].Add(currentGoal);
                        //    l = currentGoal.level;
                        //}

                    }

                    // no local path should be more than 2 steps
                    if (subgoals[l].Count > 3)
                    {
                        int[] thisOldState = subgoals[l+1][0].startState; 
                        int[] thisAction = subgoals[l + 1][0].action; 
                        int[] thisNewState = subgoals[l + 1][0].goalState;
                        StateTransition<int[], int[]> t = new StateTransition<int[], int[]>(thisOldState, thisAction, -1, thisNewState); //*********************** size of negative reward?
                        models[l + 1].update(t);
                    }
                }

                System.IO.StreamWriter w = new System.IO.StreamWriter("log.txt", false);
                //w.WriteLine("Goal: Level " + currentGoal.level + ", at " + String.Join(",", currentGoal.goalState));
                for (int k = 0; k <= currentGoal.level; k++)
                {
                    foreach (Goal<int[], int[]> step in subgoals[k])
                    {
                        if (step.goalState != null)
                        {
                            foreach (int[] s in stateTree.GetLevel0Children(step.goalState, k))
                            {
                                w.WriteLine(string.Join(",", s) + "," + (k==currentGoal.level ? "g": "p"));
                            }
                            //w.WriteLine("Goal: Level " + Math.Max(0, l) + ", at " + String.Join(",", step.goalState));
                        }
                    }
                }
                w.Flush(); w.Close();
            }

            double[] response = new double[actions.Count()];
            for (int i = 0; i < availableActions.Count; i++)
            {
                if (actionComparer.Equals(subgoals[0][0].action, availableActions.ElementAt(i)))
                    response[i] = 1;
            }

            return response;
        }

        private List<Goal<int[], int[]>> path2subgoals(List<Tuple<int[], int[], double>> path, int level, ActionValue<int[], int[]> model)
        {
            List<Goal<int[],int[]>> subgoals = new List<Goal<int[],int[]>>();
            for (int i = 0; i < path.Count-1; i++)
            {
                subgoals.Add(new Goal<int[],int[]>(level, path[i].Item1, path[i].Item2, path[i+1].Item1, path[i].Item3, stateComparer, actionComparer));
            }
            return subgoals;
        }

        public override int[] PredictNextState(int[] state, int[] action)
        {
            throw new NotImplementedException();
        }

        public override int[] PredictBestNextState(int[] state, int[] action)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<int[], double> PredictNextStates(int[] state, int[] action)
        {
            throw new NotImplementedException();
        }

        public override double PredictReward(int[] state, int[] action, int[] newState)
        {
            throw new NotImplementedException();
        }

        public override void update(StateTransition<int[], int[]> transition)
        {
            steps++;
            if (steps % 3000 == 0)
            {
                for (int i = 1; i < 7; i++)
                {
                    Dictionary<int[], System.Drawing.Color> colors = new Dictionary<int[], System.Drawing.Color>(new IntArrayComparer());
                    System.Drawing.Bitmap map = new System.Drawing.Bitmap(21, 21);
                    for (int row = 0; row < 21; row++)
                    {
                        for (int col = 0; col < 21; col++)
                        {
                            try
                            {
                                int[] thisState = stateTree.GetParentState(new int[2] { row, col }, i);
                                if (!colors.ContainsKey(thisState))
                                    colors.Add(thisState, System.Drawing.Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)));
                                map.SetPixel(row, col, colors[thisState]);
                            }
                            catch (Exception ex)
                            { }
                        }
                    }

                    System.Drawing.Bitmap resized = new System.Drawing.Bitmap(500, 500);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(resized))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                        g.DrawImage(map, 0, 0, 500, 500);
                    }
                    resized.Save("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\MultiResolutionRL\\level" + i + ".bmp");
                }
            }


            ////System.IO.StreamWriter w = new System.IO.StreamWriter("log.txt",false);
            if (currentGoal.goalState == null)
            {
                Console.WriteLine("Goal: null");
            }
            else
            {
                Console.WriteLine("Goal: Level " + currentGoal.level + ", at " + String.Join(",", currentGoal.goalState) + ", value: " + currentGoal.value);
            }

            ////decay inhibitions
            //Goal<int[]>[] gs = inhibitions.Keys.ToArray();
            //foreach (Goal<int[]> g in gs)
            //{
            //    inhibitions[g] *= 0.99;
            //    if (inhibitions[g] < 0.01)
            //        inhibitions.Remove(g);
            //}

            // update the stateTree
            stateTree.AddState(transition.newState);

            // perform model updates
            models[0].update(transition);

            if (false)//(transition.absorbingStateReached) // if this was the end of the simulation // ****** do we need this?
            {
                for (int i = 1; i < models.Length; i++)
                {
                    if (transitions[i].oldState != null)
                    {
                        transitions[i].reward += transition.reward;
                        models[i].update(transitions[i]);
                        transitions[i] = new StateTransition<int[], int[]>(null, null, 0, null);
                    }
                }

                currentGoal.goalState = null;
                for (int i = 0; i < subgoals.Length; i++)
                {
                    subgoals[i].Clear();
                }
                return;
            }
            else
            {
                for (int i = 1; i < models.Length; i++)
                {
                    // get the representation of state at this level
                    int[] thisOldState = stateTree.GetParentState(transition.oldState, i);
                    int[] thisNewState = stateTree.GetParentState(transition.newState, i);

                    // check if the local transition represents a transition at this level
                    if ((!stateComparer.Equals(thisOldState, thisNewState)) || i == 0)// transition - update the model at this level, and start a new accumulation of reward
                    {
                        if (transitions[i].oldState != null)// availableActions.Contains(transitions[i].action, actionComparer))
                        {
                            //double rewardError = transitions[i].reward - models[i].PredictReward(transitions[i].oldState, transitions[i].action, transitions[i].newState);
                            //if (rewardError < -0.1)
                            //{
                            //    Goal<int[]> inhibition = new Goal<int[]>(i, transitions[i].oldState, transitions[i].newState, 0, stateComparer);
                            //    if (!inhibitions.ContainsKey(inhibition))
                            //        inhibitions.Add(inhibition, 0);
                            //    inhibitions[inhibition] -= rewardError;
                            //    Goal<int[]> disInhibition = new Goal<int[]>(i, transitions[i].newState, transitions[i].oldState, 0, stateComparer);
                            //    if (!inhibitions.ContainsKey(disInhibition))
                            //        inhibitions.Add(disInhibition, 0);
                            //    inhibitions[disInhibition] += rewardError;
                            //    Console.WriteLine("Inhibited: level " + inhibition.level + " from " + String.Join(",", inhibition.startState) + " to " + String.Join(",", inhibition.goalState));

                            //    currentGoal.goalState = null;
                            //    for (int j = 0; j < subgoals.Length; j++)
                            //    {
                            //        subgoals[j].Clear();
                            //    }
                            //}
                            models[i].update(transitions[i]);

                        }
                        transitions[i] = new StateTransition<int[], int[]>(thisOldState, transition.action, transition.reward, thisNewState); // this line might only apply to grid-worlds

                    }
                    else
                    {
                        // accumulate the reward
                        transitions[i].reward += transition.reward;
                    }
                }
            }


            if (currentGoal.goalState == null)
                return;

            if (stateComparer.Equals(stateTree.GetParentState(transition.newState, currentGoal.level), currentGoal.goalState)) // if the current goal has been reached
            {
                for (int i = 0; i < subgoals.Length; i++)
                {
                    subgoals[i].Clear();
                }

                if (currentGoal.level == 0) // if the current goal is at level 0, select a new goal from scratch
                {
                    currentGoal.goalState = null;
                }
                else // otherwise do a contrained selection of a new goal
                {
                    currentGoal = selectGoal(transition.newState, currentGoal.level, availableActions);
                    subgoals[currentGoal.level].Add(currentGoal);
                }
            }
            else if (stateComparer.Equals(subgoals[0][0].goalState, transition.newState)) // if the next step at level 0 has been reached successfully
            {
                for (int i = 0; i < currentGoal.level; i++)
                {
                    subgoals[i].RemoveAt(0);
                    if (subgoals[i].Count > 0)
                        break;
                }
            }
            else if (!stateComparer.Equals(subgoals[0][0].goalState, transition.newState)) // if we're not where we expected to be at level 0
            {
                if (currentGoal.level == 0)
                {
                    currentGoal.goalState = null;
                    for (int i = 0; i < subgoals.Length; i++)
                    {
                        subgoals[i].Clear();
                    }
                }
                else
                {
                    for (int i = 0; i <= currentGoal.level; i++)
                    {
                        subgoals[i].Clear();
                    }
                    subgoals[currentGoal.level].Add(currentGoal);
                }
            }
        }


        //private int[] quantizeState(int[] state, int resolution)
        //{
        //    int[] newState = new int[state.Length];
        //    for (int i = 0; i < newState.Length; i++)
        //    {
        //        newState[i] = state[i] >> resolution;
        //    }
        //    return newState;
        //}


    }



    //public class WorldMemoryModel<stateType, actionType> : ActionValue<stateType, actionType>
    //{
    //    int modelAccesses = 0;

    //    public double defaultQ = 1;
    //    public double gamma = 0.9;
    //    int c = 1, numUpdates = 0;
    //    public Func<stateType, IEnumerable<stateType>> stateUpdateSelector = null;

    //    TransitionMemory<stateType, actionType> R;
    //    Dictionary<stateType, Dictionary<actionType, double>> Qtable;
    //    IEqualityComparer<actionType> actionComparer;
    //    IEqualityComparer<stateType> stateComparer;
    //    int memorySize = 1;
    //    public int MemorySize
    //    {
    //        get { return memorySize; }
    //        set
    //        {
    //            memorySize = value;
    //            R.memorySize = value;
    //        }
    //    }

    //    Random rnd = new Random(1);
    //    public System.IO.StreamWriter writer;

    //    List<actionType> availableActions;

    //    Dictionary<stateType, Dictionary<stateType, List<actionType>>> predecessors;
    //    Dictionary<stateType, double> priority;


    //    public WorldMemoryModel(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, params int[] parameters)
    //        : base(StateComparer, ActionComparer, AvailableActions, parameters)
    //    {
    //        stateComparer = StateComparer;
    //        actionComparer = ActionComparer;
    //        availableActions = AvailableActions;
    //        R = new TransitionMemory<stateType, actionType>(stateComparer, actionComparer, availableActions, defaultQ, 1);
    //        Qtable = new Dictionary<stateType, Dictionary<actionType, double>>(stateComparer);

    //        predecessors = new Dictionary<stateType, Dictionary<stateType, List<actionType>>>(stateComparer);
    //        priority = new Dictionary<stateType, double>(stateComparer);
    //    }

    //    //public void ForgetBoundaries()
    //    //{
    //    //    //R = new TransitionMemory<stateType, actionType>(stateComparer, actionComparer, availableActions, defaultQ, memorySize);
    //    //    foreach (stateType s in R.GetKnownStates())
    //    //    {
    //    //        foreach (actionType a in availableActions)
    //    //        {
    //    //            if (stateComparer.Equals(s, PredictNextState(s, a)))
    //    //            {
    //    //                R.Reset(s, a, s, 1);
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    public override stateType PredictNextState(stateType state, actionType action)
    //    {
    //        modelAccesses++;
    //        return R.Get(state, action).mostCommonState();
    //    }

    //    public override double PredictReward(stateType state, actionType action, stateType newState)
    //    {
    //        modelAccesses++;
    //        return R.Get(state, action).AverageForState(newState);
    //    }

    //    public override double[] value(stateType state, List<actionType> actions)
    //    {
    //        double[] response = new double[actions.Count()];


    //        for (int i = 0; i < response.Length; i++)
    //        {
    //            response[i] = value(state, actions.ElementAt(i));
    //        }
    //        return response;
    //    }

    //    public double value(stateType state, actionType action)
    //    {
    //        // retrieve the current estimate from the Q table
    //        Dictionary<actionType, double> stateTable = new Dictionary<actionType, double>();

    //        if (Qtable.ContainsKey(state))
    //            stateTable = Qtable[state];
    //        else
    //        {
    //            return defaultQ;
    //        }

    //        // retrieve the q value for this action
    //        if (stateTable.ContainsKey(action))
    //            return stateTable[action];
    //        else
    //            return defaultQ;
    //    }

    //    public override void update(StateTransition<stateType, actionType> transition)
    //    {
    //        // update the model values for the given transition
    //        R.Add(transition.oldState, transition.action, transition.newState, transition.reward); //thisReward + (transition.reward - thisReward) / thisCount);

    //        //// update predecessors list
    //        //if (!predecessors.ContainsKey(transition.newState))
    //        //    predecessors.Add(transition.newState, new Dictionary<stateType, List<actionType>>(stateComparer));
    //        //if (!predecessors[transition.newState].ContainsKey(transition.oldState))
    //        //    predecessors[transition.newState].Add(transition.oldState, new List<actionType>());
    //        //predecessors[transition.newState][transition.oldState].Add(transition.action);

    //        //// set this transition to a high priority
    //        //if (!priority.ContainsKey(transition.oldState))
    //        //    priority.Add(transition.oldState, double.PositiveInfinity);
    //        //else
    //        //    priority[transition.oldState] = double.PositiveInfinity;

    //        //// perform prioritized sweeping
    //        //int modelUpdates = 0;
    //        //for (int i = 0; i < 1000; i++)//while (true)
    //        //{
    //        //    // find the highest priority state
    //        //    double max = double.NegativeInfinity;
    //        //    stateType priorityS = default(stateType);
    //        //    foreach (stateType s in priority.Keys)
    //        //    {
    //        //        if (priority[s] > max)
    //        //        {
    //        //            max = priority[s];
    //        //            priorityS = s;
    //        //        }
    //        //    }
    //        //    if (max <= double.Epsilon)
    //        //        break;

    //        //    // perform update
    //        //    double oldValue = value(priorityS, availableActions).Max();
    //        //    foreach (actionType a in availableActions)
    //        //    {
    //        //        updateQ(priorityS, a);
    //        //        modelUpdates++;
    //        //    }
    //        //    double newValue = value(priorityS, availableActions).Max();
    //        //    double valueChange = Math.Abs(oldValue - newValue);

    //        //    // update priorities
    //        //    priority[priorityS] = 0;
    //        //    if (!predecessors.ContainsKey(priorityS))
    //        //        continue;
    //        //    foreach (stateType predState in predecessors[priorityS].Keys)
    //        //    {
    //        //        foreach (actionType predAct in predecessors[priorityS][predState])
    //        //        {
    //        //            priority[predState] = Math.Max(priority[predState], valueChange * R.Get(predState, predAct).Count(priorityS));
    //        //        }
    //        //    }
    //        //}

    //        // update the current Q value
    //        //updateQ(transition.oldState, transition.action);

    //        // update several of the Q values
    //        //stateType[] allStates = R.GetKnownStates();
    //        //for (int i = 0; i < numUpdates; i++)
    //        //{
    //        //    stateType randState = allStates[rnd.Next(allStates.Length)];
    //        //    actionType randAct = availableActions[rnd.Next(availableActions.Count)];
    //        //    updateQ(randState, randAct);
    //        //}

    //        IEnumerable<stateType> statesToUpdate = stateUpdateSelector == null ? R.GetKnownStates() : stateUpdateSelector(transition.oldState);
    //        double maxDif = double.PositiveInfinity;
    //        int modelUpdates = 0;
    //        while (maxDif > double.Epsilon)
    //        {
    //            maxDif = double.NegativeInfinity;
    //            foreach (stateType s in statesToUpdate)
    //            {
    //                foreach (actionType a in availableActions)
    //                {
    //                    double oldValue = value(s, a);
    //                    updateQ(s, a);
    //                    modelUpdates++;
    //                    double thisDiff = Math.Abs(oldValue - value(s, a));
    //                    if (thisDiff > maxDif)
    //                        maxDif = thisDiff;
    //                }
    //            }
    //        }
    //        writer.WriteLine("model accessed: " + modelAccesses);
    //        writer.WriteLine("model updates: " + modelUpdates);
    //        writer.Flush();
    //        modelAccesses = 0;
    //    }

    //    private void updateQ(stateType state, actionType action)
    //    {
    //        //double P = T.GetStateValueTable(state, action).Values.Sum();
    //        RewardMemory<stateType> temp = R.Get(state, action);
    //        if (temp.Count() == 1)
    //            return;
    //        double P = temp.Count();

    //        double newQ = 0, maxQ = 0;
    //        //double T_s_a_s2;

    //        foreach (stateType s2 in temp.distinctStates())
    //        {
    //            if (!Qtable.ContainsKey(s2))
    //            {
    //                Qtable.Add(s2, new Dictionary<actionType, double>(actionComparer));
    //                foreach (actionType act in availableActions)
    //                {
    //                    Qtable[s2].Add(act, defaultQ);
    //                }
    //            }
    //            maxQ = Qtable[s2].Values.Max();

    //            newQ += temp.Count(s2) / P * (temp.AverageForState(s2) + gamma * maxQ);
    //            try
    //            {
    //                if (newQ > Qtable[state][action])
    //                {
    //                    double thiscount = temp.Count(s2);
    //                    double avgreward = temp.AverageForState(s2);
    //                }
    //            }
    //            catch { }

    //            //T_s_a_s2 = R.Get(state, action, s2).Count;
    //            //double temp = R.Get(state, action, s2).Average();
    //            //if (newQ > 1)
    //            // updateQ(state, action);
    //        }

    //        if (!Qtable.ContainsKey(state))
    //        {
    //            Qtable.Add(state, new Dictionary<actionType, double>(actionComparer));
    //            foreach (actionType act in availableActions)
    //            {
    //                Qtable[state].Add(act, defaultQ);
    //            }
    //        }

    //        Qtable[state][action] = newQ;
    //    }

    //}

    //class TransitionMemory<stateType, actionType>
    //{
    //    Dictionary<stateType, Dictionary<actionType, RewardMemory<stateType>>> table;
    //    List<actionType> availableActions;
    //    IEqualityComparer<stateType> stateComparer;
    //    IEqualityComparer<actionType> actionComparer;
    //    double defaultValue;
    //    public int memorySize;

    //    public TransitionMemory(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, double DefaultValue, int MemorySize)
    //    {
    //        availableActions = AvailableActions;
    //        stateComparer = StateComparer;
    //        actionComparer = ActionComparer;
    //        table = new Dictionary<stateType, Dictionary<actionType, RewardMemory<stateType>>>(stateComparer);
    //        defaultValue = DefaultValue;
    //        memorySize = MemorySize;
    //    }

    //    public RewardMemory<stateType> Get(stateType oldState, actionType action)
    //    {
    //        ensureExistance(oldState, action);
    //        return table[oldState][action];
    //    }


    //    public stateType[] GetKnownStates()
    //    {
    //        return table.Keys.ToArray();
    //    }
        
    //    public void Add(stateType oldState, actionType action, stateType newState, double reward)
    //    {
    //        ensureExistance(oldState, action);
    //        table[oldState][action].Add(newState, reward);
    //    }

    //    public void Reset(stateType oldState, actionType action, stateType newState, double reward)
    //    {
    //        table[oldState][action] = new RewardMemory<stateType>(memorySize, stateComparer);
    //        table[oldState][action].Add(newState, reward);
    //    }
        
    //    private void ensureExistance(stateType oldState, actionType action)
    //    {
    //        if (!table.ContainsKey(oldState))
    //        {
    //            table.Add(oldState, new Dictionary<actionType, RewardMemory<stateType>>(actionComparer));
    //        }
    //        if (!table[oldState].ContainsKey(action))
    //        {
    //            table[oldState].Add(action, new RewardMemory<stateType>(memorySize, stateComparer));
    //        }
    //    }
    //}


    public class IntArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x == null || y == null)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }
            return true;
        }

        public int GetHashCode(int[] obj)
        {
            return obj.Sum();
        }
    }
}
