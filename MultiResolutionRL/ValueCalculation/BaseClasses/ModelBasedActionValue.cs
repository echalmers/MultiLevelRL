using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    abstract public class ModelBasedActionValue<stateType,actionType> : ActionValue<stateType,actionType>
    {
        protected ModelBasedActionValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        { }




        abstract public SAStable<stateType, actionType, int> getTTable();
        abstract public SAStable<stateType, actionType, Histogram> getRTable();


        //**************Inheritted from the ActionValue*********************************************//
        abstract override public double[] value(stateType state, List<actionType> actions);

        abstract override public double update(StateTransition<stateType, actionType> transition);
        abstract override public PerformanceStats getStats();

        abstract override public stateType PredictNextState(stateType state, actionType action);
        abstract override public Dictionary<stateType, double> PredictNextStates(stateType state, actionType action);
        abstract override public double PredictReward(stateType state, actionType action, stateType newState);



    }

}
