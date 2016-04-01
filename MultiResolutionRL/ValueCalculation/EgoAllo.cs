using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    class EgoAlloValue : ActionValue<int[], int[]>
    {
        ModelBasedValue<int[], int[]> egoModel;
        ModelBasedValue<int[], int[]> alloModel;
        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        List<int[]> availableActions;

        public EgoAlloValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            egoModel = new ModelBasedValue<int[], int[]>(StateComparer, ActionComparer, availableActions, StartState, parameters);
            alloModel = new ModelBasedValue<int[], int[]>(StateComparer, ActionComparer, availableActions, StartState, parameters);
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            throw new NotImplementedException();
        }

        public override double update(StateTransition<int[], int[]> transition)
        {
            throw new NotImplementedException();
        }

        public override PerformanceStats getStats()
        {
            throw new NotImplementedException();
        }

        public override explorationMode getRecommendedExplorationMode()
        {
            throw new NotImplementedException();
        }

        public override int[] PredictNextState(int[] state, int[] action)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<int[], double> PredictNextStates(int[] state, int[] action)
        {
            throw new NotImplementedException();
        }

        public override double PredictReward(int[] state, int[] action, int[] newState)
        {
            throw new NotImplementedException();
        }
    }
}
