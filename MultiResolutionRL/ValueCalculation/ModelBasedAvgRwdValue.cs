/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL.ValueCalculation;

namespace MultiResolutionRL.ValueCalculation
{
    public class ModelBasedAvgRwdValue<stateType, actionType> : ModelBasedActionValue<stateType, actionType>
    {
        int c = 1;
        double defaultVal = 0;
        double defaultR = 10;
        double beta = 0.9, alpha = 0.1;
        double rho = 0;

        IEqualityComparer<actionType> actionComparer;
        IEqualityComparer<stateType> stateComparer;
        List<actionType> availableActions;

        Dictionary<stateType, Dictionary<actionType, double>> R;
        Dictionary<stateType, Dictionary<actionType, Dictionary<stateType, int>>> T;
        Dictionary<stateType, double> V;

        Dictionary<stateType, Dictionary<stateType, List<actionType>>> predecessors;
        Dictionary<stateType, double> priority;

        PerformanceStats stats = new PerformanceStats();

        public ModelBasedAvgRwdValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            R = new Dictionary<stateType, Dictionary<actionType, double>>(StateComparer);
            T = new Dictionary<stateType, Dictionary<actionType, Dictionary<stateType, double>>>(StateComparer);
            V = new Dictionary<stateType, double>(stateComparer);

            predecessors = new Dictionary<stateType, Dictionary<stateType, List<actionType>>>(stateComparer);
            priority = new Dictionary<stateType, double>(stateComparer);
        }

        public override PerformanceStats getStats()
        {
            return stats;
        }

        public override stateType PredictNextState(stateType state, actionType action)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<stateType, double> PredictNextStates(stateType state, actionType action)
        {
            throw new NotImplementedException();
        }

        public override double PredictReward(stateType state, actionType action, stateType newState)
        {
            throw new NotImplementedException();
        }

        public override double update(StateTransition<stateType, actionType> transition)
        {
            stats.cumulativeReward += transition.reward;

            if (!T.ContainsKey(transition.oldState))
            {
                T.Add(transition.oldState, new Dictionary<actionType, Dictionary<stateType, double>>(actionComparer));
                foreach (actionType act in availableActions)
                {
                    T[transition.oldState].Add(act, new Dictionary<stateType, double>(stateComparer));
                }
            }
            if (!T[transition.oldState][transition.action].ContainsKey(transition.newState))
                T[transition.oldState][transition.action][transition.newState] = 0;
            T[transition.oldState][transition.action][transition.newState] += 1;

            if (!R.ContainsKey(transition.oldState))
                R.Add(transition.oldState, new Dictionary<actionType, double>(actionComparer));
            if (!R[transition.oldState].ContainsKey(transition.action))
                R[transition.oldState].Add(transition.action, defaultR);
            R[transition.oldState][transition.action] += (transition.reward - R[transition.oldState][transition.action]) / T[transition.oldState][transition.action][transition.newState];

            double T_V_x = value(transition.oldState, availableActions).Max();
            V[transition.oldState] = T_V_x - rho;

            // update rho
            rho = (1 - alpha) * rho + alpha * (transition.reward);
            //rho = (1 - alpha) * rho + alpha * (transition.reward + bestNewStateR - bestOldStateR);

            return 0; 
        }

        // V(s) + rho = maxa[r(x,a) + sumy(Pxy(a)*V(y)]
        // T(V)(x) = maxa[r(x,a) + sumy(Pxy(a)V(y)]
        

        public override double[] value(stateType state, List<actionType> actions)
        {
            double[] vals = new double[actions.Count()];

            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = Math.Round(value(state, actions.ElementAt(i)),2);
            }

            Console.WriteLine(string.Join(",", vals));
            return vals;
        }

        public double value(stateType state, actionType action)
        {
            double v = defaultR;
            if (R.ContainsKey(state) && R[state].ContainsKey(action))
            {
                v = R[state][action];
            }
            else
                return v;
            
            double totalCnt = T[state][action].Values.Sum();
            foreach (stateType y in T[state][action].Keys)
            {
                if (!V.ContainsKey(y))
                    V.Add(y, defaultVal);

                v += T[state][action][y] / totalCnt * V[y];
            }
            return v;
        }

        public override SAStable<stateType, actionType, int> getTTable()
        {
            throw new NotImplementedException();
        }

        public override SAStable<stateType, actionType, Histogram> getRTable()
        {
            throw new NotImplementedException();
        }
    }
}
*/