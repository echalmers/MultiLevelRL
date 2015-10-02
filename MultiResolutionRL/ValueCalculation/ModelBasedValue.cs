using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class ModelBasedValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        public double defaultQ = 10, gamma = 0.9;
        int c = 1;
        int accesses = 0;
        public int maxUpdates = 1000;
        SAStable<stateType, actionType, int> T;
        SAStable<stateType, actionType, double> R;
        Dictionary<stateType, Dictionary<actionType, double>> Qtable;
        IEqualityComparer<actionType> actionComparer;
        IEqualityComparer<stateType> stateComparer;
        public Func<stateType, IEnumerable<stateType>> stateUpdateSelector = null;

        Dictionary<stateType, Dictionary<stateType, List<actionType>>> predecessors;
        Dictionary<stateType, double> priority;

        public System.IO.StreamWriter writer;

        Random rnd = new Random(1);

        List<actionType> availableActions;

        public ModelBasedValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params int[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;
            T = new SAStable<stateType, actionType, int>(stateComparer, actionComparer, availableActions, c);
            R = new SAStable<stateType, actionType, double>(stateComparer, actionComparer, availableActions, defaultQ);
            Qtable = new Dictionary<stateType, Dictionary<actionType, double>>(stateComparer);

            predecessors = new Dictionary<stateType, Dictionary<stateType, List<actionType>>>(stateComparer);
            priority = new Dictionary<stateType, double>(stateComparer);
        }


        public override Dictionary<stateType, double> PredictNextStates(stateType state, actionType action)
        {
            accesses++;

            Dictionary<stateType, double> response = new Dictionary<stateType, double>(stateComparer);
            Dictionary<stateType, int> transitionCounts = T.GetStateValueTable(state, action);
            double total = transitionCounts.Values.Sum();
            foreach (stateType s in transitionCounts.Keys)
            {
                response.Add(s, ((double)transitionCounts[s]) / total);
            }
            return response;
        }

        public override stateType PredictNextState(stateType state, actionType action)
        {
            accesses++;

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

        public override double PredictReward(stateType state, actionType action, stateType newState)
        {
            accesses++;
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

        public override void update(StateTransition<stateType, actionType> transition)
        {
            if (writer==null)
                writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\MultiResolutionRL\\Presentation Sept 28\\modelBasedUpdates.txt");

            // retrieve current count and reward values
            int thisCount = T.Get(transition.oldState, transition.action, transition.newState);
            double thisReward = R.Get(transition.oldState, transition.action, transition.newState);

            // update the model values for the given transition
            T.Set(transition.oldState, transition.action, transition.newState, thisCount + 1);
            R.Set(transition.oldState, transition.action, transition.newState, thisReward + (transition.reward - thisReward) / thisCount);

            // update predecessors list
            if (!predecessors.ContainsKey(transition.newState))
                predecessors.Add(transition.newState, new Dictionary<stateType, List<actionType>>(stateComparer));
            if (!predecessors[transition.newState].ContainsKey(transition.oldState))
                predecessors[transition.newState].Add(transition.oldState, new List<actionType>());
            predecessors[transition.newState][transition.oldState].Add(transition.action);

            int totalUpdates = 0;
            if (maxUpdates < 0)
            {
                List<stateType> statesToUpdate = new List<stateType>();
                if (predecessors.ContainsKey(transition.oldState))
                    statesToUpdate.AddRange(predecessors[transition.oldState].Keys);
                if (!statesToUpdate.Contains(transition.oldState,stateComparer))
                    statesToUpdate.Add(transition.oldState);

                foreach (stateType s in statesToUpdate)
                {
                    foreach (actionType a in availableActions)
                    {
                        updateQ(s, a);
                        totalUpdates++;
                    }
                }
            }
            else
            {
                // set this transition to a high priority
                if (!priority.ContainsKey(transition.oldState))
                    priority.Add(transition.oldState, double.PositiveInfinity);
                else
                    priority[transition.oldState] = double.PositiveInfinity;

                // perform prioritized sweeping
                for (int i = 0; i < maxUpdates; i++)//while (true)
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
                        totalUpdates++;
                    }
                    double newValue = value(priorityS, availableActions).Max();
                    double valueChange = Math.Abs(oldValue - newValue);

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
            }


                // update the current Q value
                //updateQ(transition.oldState, transition.action);

                // update several of the Q values
                //stateType[] allStates = T.GetKnownStates();
                //for (int i = 0; i < numUpdates; i++)
                //{
                //    stateType randState = allStates[rnd.Next(allStates.Length)];
                //    actionType randAct = availableActions[rnd.Next(availableActions.Count)];
                //    updateQ(randState, randAct);
                //}
                //foreach (stateType s in T.GetKnownStates())
                //{
                //    foreach (actionType a in availableActions)
                //    {
                //        updateQ(s, a);
                //    }
                //}

            //IEnumerable<stateType> statesToUpdate = stateUpdateSelector == null ? R.GetKnownStates() : stateUpdateSelector(transition.oldState);
            //double maxDif = double.PositiveInfinity;
            //int totalUpdates = 0;
            //while (maxDif > 0.01)
            //{
            //    maxDif = double.NegativeInfinity;
            //    foreach (stateType s in statesToUpdate)
            //    {
            //        foreach (actionType a in availableActions)
            //        {
            //            double oldValue = value(s, a);
            //            updateQ(s, a);
            //            totalUpdates++;
            //            double thisDiff = Math.Abs(oldValue - value(s, a));
            //            if (thisDiff > maxDif)
            //                maxDif = thisDiff;
            //        }
            //    }
            //}
            writer.WriteLine("total updates:," + totalUpdates);
            writer.WriteLine("total accesses:," + accesses);
            writer.Flush();
            accesses = 0;
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

                newQ += T.Get(state, action, s2) / P * (R.Get(state, action, s2) + gamma * maxQ);

                T_s_a_s2 = T.Get(state, action, s2);
                double temp = R.Get(state, action, s2);
                //if (newQ > 1000)
                //{
                //    double thiscount = T.Get(state, action, s2);
                //    double avgreward = R.Get(state, action, s2);
                //}
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

    }

    class SAStable<stateType, actionType, entryType>
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
            ensureExistance(oldState, action, newState);
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
                foreach (actionType act in availableActions)
                {
                    table[oldState].Add(act, new Dictionary<stateType, entryType>(stateComparer));
                }
            }
            return table[oldState][action];
        }

        public void Set(stateType oldState, actionType action, stateType newState, entryType value)
        {
            ensureExistance(oldState, action, newState);
            table[oldState][action][newState] = value;
        }

        private void ensureExistance(stateType oldState, actionType action, stateType newState)
        {
            if (!table.ContainsKey(oldState))
            {
                table.Add(oldState, new Dictionary<actionType, Dictionary<stateType, entryType>>(actionComparer));
                foreach (actionType act in availableActions)
                {
                    table[oldState].Add(act, new Dictionary<stateType, entryType>(stateComparer));
                }
            }
            if (!table[oldState][action].ContainsKey(newState))
            {
                table[oldState][action].Add(newState, defaultValue);
            }
        }
    }
    
}
