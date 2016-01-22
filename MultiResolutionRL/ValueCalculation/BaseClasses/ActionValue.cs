using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    namespace ValueCalculation
    {
        [Serializable]
        public abstract class ActionValue<stateType, actionType>
        {
            public List<actionType> availableActions;

            protected ActionValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters) { }
            abstract public double[] value(stateType state, List<actionType> actions);
            abstract public double update(StateTransition<stateType, actionType> transition);
            abstract public PerformanceStats getStats();

            abstract public stateType PredictNextState(stateType state, actionType action);
            //abstract public stateType PredictBestNextState(stateType state, actionType action);
            abstract public Dictionary<stateType, double> PredictNextStates(stateType state, actionType action);
            abstract public double PredictReward(stateType state, actionType action, stateType newState);
        }


    }
}
