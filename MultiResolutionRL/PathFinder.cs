using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL.ValueCalculation;

namespace MultiResolutionRL
{
    public class PathFinder<stateType, actionType>
    {
        IEqualityComparer<stateType> stateComparer;

        public PathFinder(IEqualityComparer<stateType> StateComparer)
        {
            stateComparer = StateComparer;
        }

        // Assumes deterministic transitions
        public List<Tuple<stateType, actionType, double>> AStar(stateType lowerLevelState, List<stateType> goals, ActionValue<stateType, actionType> lowerLevelModel, List<actionType> availableActions, bool stochasticity)
        {
            // from the higher level goal state, generate the list of goal states at this level
            //List<stateType> goals = getLowerLevelStatesMethod(higherLevelGoal);
            //goals.Add(new int[2] { higherLevelGoal[0] << 1, higherLevelGoal[1] << 1 });
            //goals.Add(new int[2] { (higherLevelGoal[0] << 1) + 1, higherLevelGoal[1] << 1 });
            //goals.Add(new int[2] { higherLevelGoal[0] << 1, (higherLevelGoal[1] << 1) + 1 });
            //goals.Add(new int[2] { (higherLevelGoal[0] << 1) + 1, (higherLevelGoal[1] << 1) + 1 });

            //// generate the list of allowed states the path can pass through
            //int[] higherLevelState = new int[2] {lowerLevelState[0] >> 1, lowerLevelState[1] >> 1};
            //List<int[]> allowedStates = new List<int[]>();
            //allowedStates.Add(new int[2] { higherLevelState[0] << 1, higherLevelState[1] << 1 });
            //allowedStates.Add(new int[2] { (higherLevelState[0] << 1) + 1, higherLevelState[1] << 1 });
            //allowedStates.Add(new int[2] { higherLevelState[0] << 1, (higherLevelState[1] << 1) + 1 });
            //allowedStates.Add(new int[2] { (higherLevelState[0] << 1) + 1, (higherLevelState[1] << 1) + 1 });
            //allowedStates.AddRange(goals);
            
            // search for a route to a goal state
            Dictionary<stateType, Tuple<stateType, actionType, double>> cameFrom = new Dictionary<stateType, Tuple<stateType, actionType, double>>(stateComparer);
            List<stateType> closed = new List<stateType>();
            PriorityQueue<double, stateType> open = new PriorityQueue<double, stateType>();

            Dictionary<stateType, double> gScore = new Dictionary<stateType, double>(stateComparer); 
            gScore.Add(lowerLevelState, 0);

            Dictionary<stateType, double> fScore = new Dictionary<stateType, double>(stateComparer); 
            fScore.Add(lowerLevelState, gScore[lowerLevelState] + heuristic(lowerLevelState, lowerLevelModel, availableActions));
            
            open.Enqueue(0, lowerLevelState);

            while(!open.IsEmpty)
            {
                stateType candidate = open.Dequeue();

                if (goals.Contains(candidate, stateComparer))
                {
                    List<Tuple<stateType, actionType, double>> path = reconstructPath(candidate, cameFrom);
                    //expectedReward = -(getGorF(candidate, gScore) * 2 - 1 * path.Count);
                    return path;
                }

                closed.Add(candidate);

                // identify all possible neighbors 
                List<stateType> neighbors = new List<stateType>();
                foreach (actionType act in availableActions)
                {
                    if (stochasticity)
                        neighbors.AddRange(lowerLevelModel.PredictNextStates(candidate, act).Keys);
                    else
                        neighbors.Add(lowerLevelModel.PredictNextState(candidate, act));

                    // evaluate each neighbor
                    foreach (stateType neighbor in neighbors)
                    {
                        //stateType neighbor = lowerLevelModel.PredictNextState(candidate, act);
                        if (neighbor == null) // if there is no knowledge of what this action does
                        {
                            continue;
                        }
                        //if (!allowedStates.Contains(neighbor, comparer))
                        //{
                        //    continue;
                        //}

                        if (closed.Contains(neighbor, stateComparer) || open.Contains(neighbor, stateComparer))
                            continue;

                        double reward = lowerLevelModel.PredictReward(candidate, act, neighbor);
                        double cost = -reward / 2 + 0.5; // ****** assumes the reward is between -1 and 1
                        double tentativeG = getGorF(candidate, gScore) + cost;

                        if (!open.Contains(neighbor, stateComparer) || (tentativeG < getGorF(neighbor, gScore)))
                        {
                            cameFrom[neighbor] = new Tuple<stateType, actionType, double>(candidate, act, reward);
                            gScore[neighbor] = tentativeG;
                            fScore[neighbor] = gScore[neighbor] + heuristic(neighbor, lowerLevelModel, availableActions);
                            if (!open.Contains(neighbor, stateComparer))
                            {
                                open.Enqueue(getGorF(neighbor, fScore), neighbor);
                            }
                        }
                    }
                }
            }
            return new List<Tuple<stateType, actionType, double>>();
        }

        private static double heuristic(stateType state, ActionValue<stateType, actionType> model, List<actionType> availableActions)
        {
            return (-model.value(state, availableActions).Max() / 2 + 0.5);
            //double bestDist = double.PositiveInfinity;
            //foreach (int[] goal in goals)
            //{
            //    int[] diff = arraySubtract(goal, state);
            //    double taxiDist = 0;
            //    foreach (int i in diff)
            //    {
            //        taxiDist += Math.Abs(i);
            //    }
            //    if (taxiDist < bestDist)
            //        bestDist = taxiDist;
            //}
            //return bestDist*0.01;
        }

        private static double getGorF(stateType state, Dictionary<stateType, double> scoreTable)
        {
            if (scoreTable.ContainsKey(state))
                return scoreTable[state];
            return double.PositiveInfinity;
        }


        private static List<Tuple<stateType, actionType, double>> reconstructPath(stateType endState, Dictionary<stateType, Tuple<stateType, actionType, double>> cameFrom)
        {
            List<Tuple<stateType, actionType, double>> path = new List<Tuple<stateType, actionType, double>>();
            path.Add(new Tuple<stateType, actionType, double>(endState, default(actionType), double.NaN));
            while(true)
            {
                if (cameFrom.ContainsKey(path[0].Item1))
                {
                    path.Insert(0, cameFrom[path[0].Item1]);
                }
                else
                    break;
            }
            return path;
        }

    }


    class PriorityQueue<P, V>
    {
        private SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();
        
        public bool Contains(V item, IEqualityComparer<V> comparer)
        {
            foreach(Queue<V> q in list.Values)
            {
                if (q.Contains(item, comparer))
                    return true;
            }
            return false;
        }

        public void Enqueue(P priority, V value)
        {
            Queue<V> q;
            if (!list.TryGetValue(priority, out q))
            {
                q = new Queue<V>();
                list.Add(priority, q);
            }
            q.Enqueue(value);
        }
        public V Dequeue()
        {
            // will throw if there isn’t any first element!
            var pair = list.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0) // nothing left of the top priority.
                list.Remove(pair.Key);
            return v;
        }
        public bool IsEmpty
        {
            get { return !list.Any(); }
        }
    }
}
