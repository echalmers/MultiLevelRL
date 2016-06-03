using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL.ValueCalculation;

namespace MultiResolutionRL
{
    public class Agent<stateType, actionType>
    {
        public stateType state;
        public Policy<stateType, actionType> _policy;
        public ActionValue<stateType, actionType> _actionValue;
        public List<actionType> _possibleActions;

        OptimalPolicy<stateType, actionType> explorationFreePolicy = new OptimalPolicy<stateType, actionType>();
                

        public Agent(stateType State, Policy<stateType, actionType> policy, ActionValue<stateType, actionType> actionValue, List<actionType> possibleActions)
        {
            state = State;
            _policy = policy;
            _actionValue = actionValue;
            _possibleActions = possibleActions;
        }

        public actionType selectAction()
        {
            // get value for every action;
            double[] values = _actionValue.value(state, _possibleActions);
            //Console.WriteLine(String.Join(",", values));

            if (_actionValue.getRecommendedExplorationMode() == explorationMode.suspendExploration)
                return explorationFreePolicy.selectAction(_possibleActions, values.ToList());
            else
                return _policy.selectAction(_possibleActions, values.ToList());
        }

        public void logEvent(StateTransition<stateType, actionType> transition)
        {
            state = transition.newState;
            _actionValue.update(transition);
        }
        public void logAltEvent(StateTransition<stateType, actionType> transition)
        {
            _actionValue.update(transition);
        }

        public PerformanceStats getStats()
        {
            return _actionValue.getStats();
        }

       
    }
}
