using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class ModelFreeValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        public double alpha = 0.2;
        public double gamma = 0.9;
        public double defaultQ = 10;
        public Dictionary<stateType, Dictionary<actionType, double>> Qtable;
        IEqualityComparer<actionType> actionComparer;
        public List<actionType> availableActions;
        PerformanceStats stats = new PerformanceStats();

        public ModelFreeValue(IEqualityComparer<stateType> stateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(stateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            Qtable = new Dictionary<stateType, Dictionary<actionType, double>>(stateComparer);
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

                if (Qtable.ContainsKey(state))
                    stateTable = Qtable[state];
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

        public double value(stateType state, actionType action)
        {
            List<actionType> dummy = new List<actionType>();
            dummy.Add(action);
            return value(state, dummy)[0];
        }

        public override double update(StateTransition<stateType, actionType> transition)
        {
            stats.cumulativeReward += transition.reward;

            double q_s_a = value(transition.oldState, transition.action);

            if (!Qtable.ContainsKey(transition.newState))
            {
                Qtable.Add(transition.newState, new Dictionary<actionType, double>(actionComparer));
                foreach (actionType act in availableActions)
                {
                    Qtable[transition.newState].Add(act, defaultQ);
                }
            }
            if (!Qtable.ContainsKey(transition.oldState))
            {
                Qtable.Add(transition.oldState, new Dictionary<actionType, double>(actionComparer));
                foreach (actionType act in availableActions)
                {
                    Qtable[transition.oldState].Add(act, defaultQ);
                }
            }
            double maxNewQ = Qtable[transition.newState].Values.Max();
            
            Qtable[transition.oldState][transition.action] = q_s_a + alpha * (transition.reward + gamma * maxNewQ - q_s_a);
            double newVal = Qtable[transition.oldState][transition.action];
            return Math.Abs(newVal - q_s_a);
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

        public override explorationMode getRecommendedExplorationMode()
        {
            return explorationMode.normal;
        }
    }

}
