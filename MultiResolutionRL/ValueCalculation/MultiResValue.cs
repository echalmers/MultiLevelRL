using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL.StateManagement;

namespace MultiResolutionRL.ValueCalculation
{
    public class MultiResValue<stateType,actionType> : ActionValue<int[], actionType>
    {
        public Goal<int[], actionType> currentGoal;

        public ModelBasedValue<int[], actionType>[] models;
        StateTransition<int[], actionType>[] transitions;
        public List<Goal<int[], actionType>>[] subgoals;
        List<actionType> availableActions;
        PathFinder<int[], actionType> pathFinder;
        StateTree<int[]> stateTree;
        //taxiStateTree stateTree;
        PerformanceStats combinedStats = new PerformanceStats();

        Random rnd = new Random(1);

        IEqualityComparer<int[]> stateComparer;
        IEqualityComparer<actionType> actionComparer;
        
        int minLevel = 0;
        
        
        
        public MultiResValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, int[] StartState, params object[] numLevels_minLevel)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, numLevels_minLevel)
        {
            minLevel = numLevels_minLevel.Length >= 2 ? (int)numLevels_minLevel[1] : 0;
            models = new ActionValue<int[], actionType>[(int)numLevels_minLevel[0]];
            transitions = new StateTransition<int[], actionType>[(int)numLevels_minLevel[0]];
            subgoals = new List<Goal<int[], actionType>>[(int)numLevels_minLevel[0]];

            availableActions = AvailableActions;
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            stateTree = new intStateTree();
            //stateTree = new taxiStateTree();
            //stateTree = new learnedStateTree();
            
            pathFinder = new PathFinder<int[], actionType>(stateComparer);

            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new ModelBasedValue<int[], actionType>(stateComparer, actionComparer, availableActions, StartState) 
                { 
                    maxUpdates = i==0 ? (minLevel>0 ? 20 : 20) : 20, 
                    defaultQ = i==0 ? 15 : 0,
                    gamma = i==0 ? 0.9 : 0.6
                };
                
                transitions[i] = new StateTransition<int[], actionType>(null, default(actionType), 0, null);
                subgoals[i] = new List<Goal<int[], actionType>>();
            }

            currentGoal = new Goal<int[], actionType>(0, null, default(actionType), null, 0, stateComparer, actionComparer);

        }


        private Goal<int[], actionType> selectGoal(int[] state, int maxLevel, List<actionType> availableActions)
        {
            Goal<int[], actionType> newGoal = new Goal<int[], actionType>(-1, null, default(actionType), null, double.NegativeInfinity, null, null);

            double[][] values = new double[models.Length][];
            for (int i = 0; i <= maxLevel; i++)
            {
                int[] thisState = stateTree.GetParentState(state, i);
                values[i] = models[i].value(thisState, availableActions);
            }

            // the default new goal will be the best lowest-level goal
            for (int i = 0; i < availableActions.Count(); i++)
            {
                if (values[0][i] > newGoal.value || newGoal == null)
                {
                    int[] goalState = models[0].PredictNextState(state, availableActions.ElementAt(i)); ///***************************** might need to find best possible next state instead
                    newGoal = new Goal<int[], actionType>(0, state, availableActions.ElementAt(i), goalState, values[0][i], stateComparer, actionComparer);

                }
            }

            // find the best action at any allowed level
            for (int l = Math.Max(minLevel,1); l <= maxLevel; l++)
            {
                int[] thisState = stateTree.GetParentState(state, l);
                for (int i = 0; i < availableActions.Count(); i++)
                {
                    int[] thisGoal = models[l].PredictNextState(thisState, availableActions.ElementAt(i));
                    if (thisGoal == null)
                        continue;
                    Goal<int[], actionType> candidateGoal = new Goal<int[], actionType>(l, thisState, availableActions.ElementAt(i), thisGoal, values[l][i], stateComparer, actionComparer);

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

                    if (candidateGoal.value > newGoal.value)
                    {
                        if (l > 0 && values[l][i] <= 0)//***************************
                            continue;
                        newGoal = candidateGoal;
                    }
                }
            }

            return newGoal;
        }

        public override double[] value(int[] state, List<actionType> actions)
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
                    List<Tuple<int[], actionType, double>> path = pathFinder.AStar(startState, goalStates, models[l], actions, false);//l != 0);
                    subgoals[l] = path2subgoals(path, l, models[l]);

                    if (subgoals[l].Count == 0) // if no path is known, pass control to lowest level
                    {
                        goalStates = stateTree.GetChildren(subgoals[l + 1][0].goalState, l + 1);
                        int[] currentGoalLevelState = stateTree.GetParentState(state, currentGoal.level);
                        Console.WriteLine("couldn't find a path to level " + currentGoal.level + ": " + String.Join(",",currentGoal.goalState) + " at level " + l);
                        
                        currentGoal.goalState = null;
                        for (int i = 0; i < subgoals.Length; i++)
                        {
                            subgoals[i].Clear();
                        }
                        currentGoal = selectGoal(state, 0, actions);
                        subgoals[0].Clear();
                        subgoals[0].Add(currentGoal);
                        break;


                    }

                    // no local path should be more than 2 steps
                    if (subgoals[l].Count > 3)
                    {
                        int[] thisOldState = subgoals[l+1][0].startState;
                        actionType thisAction = subgoals[l + 1][0].action; 
                        int[] thisNewState = subgoals[l + 1][0].goalState;
                        StateTransition<int[], actionType> t = new StateTransition<int[], actionType>(thisOldState, thisAction, -10, thisNewState); //*********************** size of negative reward?
                        models[l + 1].update(t);
                    }
                }

                System.IO.StreamWriter w = new System.IO.StreamWriter("log.txt", false);
                //w.WriteLine("Goal: Level " + currentGoal.level + ", at " + String.Join(",", currentGoal.goalState));
                for (int k = 0; k <= currentGoal.level; k++)
                {
                    foreach (Goal<int[], actionType> step in subgoals[k])
                    {
                        if (step.goalState != null)
                        {
                            foreach (int[] s in stateTree.GetLevel0Children(step.goalState, k))
                            {
                                w.WriteLine(string.Join(",", s) + "," + (k == currentGoal.level ? "g" : "p"));
                            }
                            //w.WriteLine("Goal: Level " + Math.Max(0, l) + ", at " + String.Join(",", step.goalState));
                        }
                    }
                }
                w.Flush(); w.Close();
            }

            if (minLevel > 0) // for simulating dH lesion
            {
                double[] vals = new double[availableActions.Count];
                if (currentGoal.level >= minLevel)
                {
                    vals[availableActions.IndexOf(currentGoal.action)] = 1;
                }
                else
                {
                    return models[0].value(state, availableActions);
                }
                return vals;
            }

            double[] response = new double[actions.Count()];
            for (int i = 0; i < availableActions.Count; i++)
            {
                if (actionComparer.Equals(subgoals[0][0].action, availableActions.ElementAt(i)))
                    response[i] = 1;
            }
            
            return response;
        }

        private List<Goal<int[], actionType>> path2subgoals(List<Tuple<int[], actionType, double>> path, int level, ActionValue<int[], actionType> model)
        {
            List<Goal<int[], actionType>> subgoals = new List<Goal<int[], actionType>>();
            for (int i = 0; i < path.Count-1; i++)
            {
                subgoals.Add(new Goal<int[], actionType>(level, path[i].Item1, path[i].Item2, path[i + 1].Item1, path[i].Item3, stateComparer, actionComparer));
            }
            return subgoals;
        }

        public override int[] PredictNextState(int[] state, actionType action)
        {
            throw new NotImplementedException();
        }
        
        public override Dictionary<int[], double> PredictNextStates(int[] state, actionType action)
        {
            throw new NotImplementedException();
        }

        public override double PredictReward(int[] state, actionType action, int[] newState)
        {
            throw new NotImplementedException();
        }

        public override double update(StateTransition<int[], actionType> transition)
        {            
            if (currentGoal.goalState == null)
            {
                Console.WriteLine("Goal: null");
            }
            else
            {
                Console.WriteLine("Goal: Level " + currentGoal.level + ", at " + String.Join(",", currentGoal.goalState) + ", value: " + currentGoal.value);
            }
            Console.WriteLine("   current state: " + String.Join(",", transition.newState) + " / " + String.Join(",", stateTree.GetParentState(transition.newState, currentGoal.level)));

            
            // update the stateTree
            stateTree.AddState(transition.newState);

            // perform model updates
            double returnValue = models[0].update(transition);


            if (transition.absorbingStateReached) // if this was the end of the simulation // ****** do we need this?
            {
                for (int i = 1; i < models.Length; i++)
                {
                    if (transitions[i].oldState != null)
                    {
                        transitions[i].reward += transition.reward;
                        models[i].update(transitions[i]);
                        transitions[i] = new StateTransition<int[], actionType>(null, default(actionType), 0, null);
                    }
                }

                currentGoal.goalState = null;
                for (int i = 0; i < subgoals.Length; i++)
                {
                    subgoals[i].Clear();
                }
                return returnValue;
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
                        if (transitions[i].oldState != null)
                        {
                            models[i].update(transitions[i]);

                        }
                        transitions[i] = new StateTransition<int[], actionType>(thisOldState, transition.action, transition.reward, thisNewState); // this line might only apply to grid-worlds

                    }
                    else
                    {
                        // accumulate the reward
                        transitions[i].reward += transition.reward;
                    }
                }
            }


            if (currentGoal.goalState == null)
                return returnValue;

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
            else if (minLevel>0 && !stateComparer.Equals(stateTree.GetParentState(transition.newState, currentGoal.level), currentGoal.startState)) // if we're no longer in the start state at the goal level (for dH lesions)
            {
                currentGoal.goalState = null;
                for (int i = 0; i < subgoals.Length; i++)
                {
                    subgoals[i].Clear();
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
                    subgoals[0].Clear();
                    //for (int i = 0; i <= currentGoal.level; i++)
                    //{
                    //    subgoals[i].Clear();
                    //}
                    //subgoals[currentGoal.level].Add(currentGoal);
                }
            }
            return returnValue;
        }

        public override PerformanceStats getStats()
        {
            combinedStats.modelAccesses = 0;
            combinedStats.modelUpdates = 0;
            combinedStats.cumulativeReward = models[0].getStats().cumulativeReward;
            foreach (ActionValue<int[], actionType> model in models)
            {
                PerformanceStats thisStats = model.getStats();
                combinedStats.modelAccesses += thisStats.modelAccesses;
                combinedStats.modelUpdates += thisStats.modelUpdates;
            }
            return combinedStats;
        }
        


    }

    


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
