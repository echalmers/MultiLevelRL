using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class ModelFreeValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        double alpha = 0.5;
        public double gamma = 0.9;
        double defaultQ = 5;
        Dictionary<stateType, Dictionary<actionType, double>> table;
        IEqualityComparer<actionType> actionComparer;
        List<actionType> availableActions;
        PerformanceStats stats = new PerformanceStats();

        public ModelFreeValue(IEqualityComparer<stateType> stateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(stateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            table = new Dictionary<stateType, Dictionary<actionType, double>>(stateComparer);
            actionComparer = ActionComparer;
            availableActions = AvailableActions;
        }

        public override double[] value(stateType state, List<actionType> actions)
        {
            double[] response = new double[actions.Count()];
            for (int i = 0; i < response.Length; i++)
            {
                // retrieve the table of q values for this state
                Dictionary<actionType, double> stateTable = new Dictionary<actionType, double>();

                if (table.ContainsKey(state))
                    stateTable = table[state];
                else
                {
                    response[i] = defaultQ;
                }

                // retrieve the q value for this action
                if (stateTable.ContainsKey(actions.ElementAt(i)))
                    response[i] = stateTable[actions.ElementAt(i)];
                else
                    response[i] = defaultQ;
            }
            return response;
        }

        private double value(stateType state, actionType action)
        {
            List<actionType> dummy = new List<actionType>();
            dummy.Add(action);
            return value(state, dummy)[0];
        }

        public override double update(StateTransition<stateType, actionType> transition)
        {
            double oldVal = defaultQ;
            if (table.ContainsKey(transition.oldState) && table[transition.oldState].ContainsKey(transition.action))
                oldVal = table[transition.oldState][transition.action];

            stats.cumulativeReward += transition.reward;

            double q_s_a = value(transition.oldState, transition.action);

            if (!table.ContainsKey(transition.newState))
            {
                table.Add(transition.newState, new Dictionary<actionType, double>(actionComparer));
                foreach (actionType act in availableActions)
                {
                    table[transition.newState].Add(act, defaultQ);
                }
            }
            double maxNewQ = table[transition.newState].Values.Max();

            // add a new (blank) table of action values if appropriate
            if (!table.ContainsKey(transition.oldState))
                table.Add(transition.oldState, new Dictionary<actionType, double>(actionComparer));

            table[transition.oldState][transition.action] = q_s_a + alpha * (transition.reward + gamma * maxNewQ - q_s_a);
            double newVal = table[transition.oldState][transition.action];
            return Math.Abs(newVal - oldVal);
        }

        public override stateType PredictNextState(stateType state, actionType action)
        {
            throw new NotImplementedException();
        }
        
        public override double PredictReward(stateType state, actionType action, stateType newState)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<stateType, double> PredictNextStates(stateType state, actionType action)
        {
            throw new NotImplementedException();
        }

        public override PerformanceStats getStats()
        {
            return stats;
        }
    }

}
