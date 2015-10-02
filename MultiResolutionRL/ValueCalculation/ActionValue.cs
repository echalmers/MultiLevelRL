using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    namespace ValueCalculation
    {
        public abstract class ActionValue<stateType, actionType>
        {
            protected ActionValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params int[] parameters) { }
            abstract public double[] value(stateType state, List<actionType> actions);
            abstract public void update(StateTransition<stateType, actionType> transition);
            abstract public stateType PredictNextState(stateType state, actionType action);
            abstract public Dictionary<stateType, double> PredictNextStates(stateType state, actionType action);
            abstract public double PredictReward(stateType state, actionType action, stateType newState);
        }


    }
}
