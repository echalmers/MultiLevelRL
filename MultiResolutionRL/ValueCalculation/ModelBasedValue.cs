using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    [Serializable]
    public class ModelBasedValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        public double defaultQ = 10, gamma = 0.9;
        int c = 1;
        public int maxUpdates = 120;//1000;
        public SAStable<stateType, actionType, int> T;
        public SAStable<stateType, actionType, double> R;
        public Dictionary<stateType, Dictionary<actionType, double>> Qtable;
        IEqualityComparer<actionType> actionComparer;
        IEqualityComparer<stateType> stateComparer;
        public Func<stateType, IEnumerable<stateType>> stateUpdateSelector = null;

        Dictionary<stateType, Dictionary<stateType, HashSet<actionType>>> predecessors;
        Dictionary<stateType, double> priority;

        PerformanceStats stats = new PerformanceStats();
        
        Random rnd = new Random(1);

        List<actionType> availableActions;

        public ModelBasedValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;
            T = new SAStable<stateType, actionType, int>(stateComparer, actionComparer, availableActions, c);
            R = new SAStable<stateType, actionType, double>(stateComparer, actionComparer, availableActions, defaultQ);
            Qtable = new Dictionary<stateType, Dictionary<actionType, double>>(stateComparer);

            predecessors = new Dictionary<stateType, Dictionary<stateType, HashSet<actionType>>>(stateComparer);
            priority = new Dictionary<stateType, double>(stateComparer);
        }


        public override Dictionary<stateType, double> PredictNextStates(stateType state, actionType action)
        {
            stats.modelAccesses++;

            Dictionary<stateType, double> response = new Dictionary<stateType, double>(stateComparer);
            Dictionary<stateType, int> transitionCounts = T.GetStateValueTable(state, action);
            double total = transitionCounts.Values.Sum();
            foreach (stateType s in transitionCounts.Keys)
            {
                response.Add(s, ((double)transitionCounts[s]) / total);
            }
            return response;
        }

        public void printT()
        {
            T.print();
        }

        public override stateType PredictNextState(stateType state, actionType action)
        {
            stats.modelAccesses++;

            stateType next = default(stateType); int counts = -1;
            foreach (stateType s in T.GetStateValueTable(state, action).Keys)
            {
                if (T.Get(state, action, s) > counts)
                {
                    counts = T.Get(state, action, s);
                    next = s;
                }
            }
            return next;
        }

        public stateType PredictNextStateOptimistic(stateType state, actionType action)
        {
            stats.modelAccesses++;

            stateType next = default(stateType); double reward = double.NegativeInfinity;

            foreach (stateType s in R.GetStateValueTable(state, action).Keys)
            {
                if (R.Get(state, action, s) > reward)
                {
                    reward = R.Get(state, action, s);
                    next = s;
                }
            }
            return next;
        }
        
        public override double PredictReward(stateType state, actionType action, stateType newState)
        {
            stats.modelAccesses++;
            return R.Get(state, action, newState);
        }
        
        public double value(stateType state, actionType action)
        {
            // retrieve the current estimate from the Q table
            Dictionary<actionType, double> stateTable = new Dictionary<actionType, double>();

            if (Qtable.ContainsKey(state))
                stateTable = Qtable[state];
            else
            {
                return defaultQ;
            }

            // retrieve the q value for this action
            if (stateTable.ContainsKey(action))
                return stateTable[action];
            else
                return defaultQ;
        }

        public override double[] value(stateType state, List<actionType> actions)
        {
            double[] response = new double[actions.Count()];

            for (int i = 0; i < response.Length; i++)
            {
                response[i] = value(state, actions.ElementAt(i));
            }
            return response;
        }

        public override double update(StateTransition<stateType, actionType> transition)
        {
            double maxChange = double.NegativeInfinity;

            stats.cumulativeReward += transition.reward;

            // retrieve current count and reward values
            int thisCount = T.Get(transition.oldState, transition.action, transition.newState);
            double thisReward = R.Get(transition.oldState, transition.action, transition.newState);

            // update the model values for the given transition
            T.Set(transition.oldState, transition.action, transition.newState, thisCount + 1);
            R.Set(transition.oldState, transition.action, transition.newState, thisReward + (transition.reward - thisReward) / thisCount);

            // update predecessors list
            if (!predecessors.ContainsKey(transition.newState))
                predecessors.Add(transition.newState, new Dictionary<stateType, HashSet<actionType>>(stateComparer));
            if (!predecessors[transition.newState].ContainsKey(transition.oldState))
                predecessors[transition.newState].Add(transition.oldState, new HashSet<actionType>(actionComparer));
            predecessors[transition.newState][transition.oldState].Add(transition.action);

            
                // set this transition to a high priority
                if (!priority.ContainsKey(transition.oldState))
                    priority.Add(transition.oldState, double.PositiveInfinity);
                else
                    priority[transition.oldState] = double.PositiveInfinity;

            int i = 0;
                // perform prioritized sweeping
                for (i = 0; i < maxUpdates; i++)//while (true)
                {
                    // find the highest priority state
                    double max = double.NegativeInfinity;
                    stateType priorityS = default(stateType);
                    foreach (stateType s in priority.Keys)
                    {
                        if (priority[s] > max)
                        {
                            max = priority[s];
                            priorityS = s;
                        }
                    }
                    if (max <= double.Epsilon)
                        break;

                    // perform update
                    double oldValue = value(priorityS, availableActions).Max();
                    foreach (actionType a in availableActions) 
                    {
                        updateQ(priorityS, a);
                        stats.modelUpdates++;
                    }
                    double newValue = value(priorityS, availableActions).Max();
                    double valueChange = Math.Abs(oldValue - newValue);
                    maxChange = Math.Max(maxChange, valueChange);

                    // update priorities
                    priority[priorityS] = 0;
                    if (!predecessors.ContainsKey(priorityS))
                        continue;
                    foreach (stateType predState in predecessors[priorityS].Keys)
                    {
                        foreach (actionType predAct in predecessors[priorityS][predState])
                        {
                            priority[predState] = Math.Max(priority[predState], valueChange * T.Get(predState, predAct, priorityS));
                        }
                    }
            }
            return i;
        }

        private void updateQ(stateType state, actionType action)
        {
            double P = T.GetStateValueTable(state, action).Values.Sum();
            if (P == 0)
                return;

            double newQ = 0, maxQ = 0;
            double T_s_a_s2;

            foreach (stateType s2 in T.GetStateValueTable(state, action).Keys)
            {
                if (!Qtable.ContainsKey(s2))
                {
                    Qtable.Add(s2, new Dictionary<actionType, double>(actionComparer));
                    foreach (actionType act in availableActions)
                    {
                        Qtable[s2].Add(act, defaultQ);
                    }
                }
                maxQ = Qtable[s2].Values.Max();

                
                double thisT = T.Get(state, action, s2);
                double thisR = R.Get(state, action, s2);
                double thisProb = thisT / P;
                newQ += thisProb * (thisR + gamma * maxQ);

            }

            if (!Qtable.ContainsKey(state))
            {
                Qtable.Add(state, new Dictionary<actionType, double>(actionComparer));
                foreach (actionType act in availableActions)
                {
                    Qtable[state].Add(act, defaultQ);
                }
            }

            Qtable[state][action] = newQ;
        }

        public override PerformanceStats getStats()
        {
            return stats;
        }

    }

    [Serializable]
    public class SAStable<stateType, actionType, entryType>
    {
        Dictionary<stateType, Dictionary<actionType, Dictionary<stateType, entryType>>> table;
        List<actionType> availableActions;
        IEqualityComparer<stateType> stateComparer;
        IEqualityComparer<actionType> actionComparer;
        entryType defaultValue;

        public SAStable(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, entryType DefaultValue)
        {
            availableActions = AvailableActions;
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            table = new Dictionary<stateType, Dictionary<actionType, Dictionary<stateType, entryType>>>(stateComparer);
            defaultValue = DefaultValue;
        }

        public entryType Get(stateType oldState, actionType action, stateType newState)
        {
            if (!table.ContainsKey(oldState))
            {
                return defaultValue;
            }
            if (!table[oldState].ContainsKey(action))
            {
                return defaultValue;
            }
            if (!table[oldState][action].ContainsKey(newState))
            {
                return defaultValue;
            }
            return table[oldState][action][newState];
        }

        public stateType[] GetKnownStates()
        {
            return table.Keys.ToArray();
        }

        public Dictionary<stateType, entryType> GetStateValueTable(stateType oldState, actionType action)
        {
            if (!table.ContainsKey(oldState))
            {
                table.Add(oldState, new Dictionary<actionType, Dictionary<stateType, entryType>>(actionComparer));
            }
            if (!table[oldState].ContainsKey(action))
            {
                table[oldState].Add(action, new Dictionary<stateType, entryType>(stateComparer));
            }
                //foreach (actionType act in availableActions)
                //{
                //    table[oldState].Add(act, new Dictionary<stateType, entryType>(stateComparer));
                //}
            
            return table[oldState][action];
        }

        public void Set(stateType oldState, actionType action, stateType newState, entryType value)
        {
            if(!table.ContainsKey(oldState))
            {
                table.Add(oldState, new Dictionary<actionType, Dictionary<stateType, entryType>>(actionComparer));
            }
            if(!table[oldState].ContainsKey(action))
            {
                table[oldState].Add(action, new Dictionary<stateType, entryType>(stateComparer));
            }
            if(!table[oldState][action].ContainsKey(newState))
            {
                table[oldState][action].Add(newState, default(entryType));
            }
            table[oldState][action][newState] = value;
        }

        public void print()
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\T.csv");
            foreach (stateType s1 in table.Keys)
            {
                foreach (actionType a in table[s1].Keys)
                {
                    foreach (stateType s2 in table[s1][a].Keys)
                    {
                        int[] s1int = (int[])(object)s1;
                        int[] aint = (int[])(object)a;
                        int[] s2int = (int[])(object)s2;
                        writer.WriteLine(string.Join(",", s1int) + "," + string.Join(",", aint) + "," + string.Join(",", s2int) + "," + table[s1][a][s2].ToString());
                    }
                }
            }
            writer.Flush();
            writer.Close();

        }
    }
    
}
