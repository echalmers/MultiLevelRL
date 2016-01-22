using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    [Serializable]
    public class ModelBasedValue<stateType, actionType> : ModelBasedActionValue<stateType, actionType>
    {
        public double defaultQ = 10, gamma = 0.9;
        int c = 1;
        public int maxUpdates = 120;//1000;
        public SAStable<stateType, actionType, int> T;
        public SAStable<stateType, actionType, Histogram> R;
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
            T = new SAStable<stateType, actionType, int>(stateComparer, actionComparer, availableActions,
                (dummy) => { return c; },
                null
                );

            R = new SAStable<stateType, actionType, Histogram>(stateComparer, actionComparer, availableActions,
                (x) =>
                {
                    Histogram defaultHistogram = new Histogram((double)x[0]);
                    return defaultHistogram;
                },
                new object[1] { defaultQ }
            );
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

        //public stateType PredictNextStateOptimistic(stateType state, actionType action)
        //{
        //    stats.modelAccesses++;

        //    stateType next = default(stateType); double reward = double.NegativeInfinity;

        //    foreach (stateType s in R.GetStateValueTable(state, action).Keys)
        //    {
        //        if (R.Get(state, action, s) > reward)
        //        {
        //            reward = R.Get(state, action, s);
        //            next = s;
        //        }
        //    }
        //    return next;
        //}
        
        public override double PredictReward(stateType state, actionType action, stateType newState)
        {
            stats.modelAccesses++;
            return R.Get(state, action, newState).Average();
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
            Histogram thisReward = R.Get(transition.oldState, transition.action, transition.newState);

            // update the model values for the given transition
            T.Set(transition.oldState, transition.action, transition.newState, thisCount + 1);
            //R.Set(transition.oldState, transition.action, transition.newState, thisReward + (transition.reward - thisReward) / thisCount);
            thisReward.Add(transition.reward);
            R.Set(transition.oldState, transition.action, transition.newState, thisReward);

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
                double thisR = R.Get(state, action, s2).Average();
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

        public override SAStable<stateType, actionType, int> getTTable()
        {
            return T;
        }

        public override SAStable<stateType, actionType, Histogram> getRTable()
        {
            return R;
        }


    }

    

    public class Histogram
    {
        Dictionary<double, double> counts = new Dictionary<double, double>();
        double defaultAverage;
        double totalCounts = 0;
        double recentAverage;

        //public Histogram()
        //{ }

        //public Histogram(Histogram toCopy)
        //{
        //    counts = new Dictionary<double, double>(toCopy.counts);
        //}

        public Histogram(double DefaultAverage)
        {
            defaultAverage = DefaultAverage;
        }

        public void Add(double value)
        {
            if (!counts.ContainsKey(value))
                counts.Add(value, 1);
            else
                counts[value]++;

            totalCounts++;

            // decide whether to recalculate average
            if (Math.Abs(value - recentAverage) > 0.01)
                recentAverage = CalcAverage();
        }
                
        public double P(double value, int priorCnt)
        {
            double thisCnts = priorCnt;
            if (counts.ContainsKey(value))
                thisCnts += counts[value];
            return thisCnts / (totalCounts+priorCnt);
        }

        public double Average()
        {
            return recentAverage;
        }

        private double CalcAverage()
        {
            if (counts.Count == 0)
                return defaultAverage;
            else if (counts.Count == 1)
                return counts.Keys.ElementAt(0);

            double avg = 0, sum = 0;
            foreach (double value in counts.Keys)
            {
                avg += value * counts[value];
                sum += counts[value];
            }
            avg = (double)avg / (double)sum;
            return avg;
        }
    }
    
}
