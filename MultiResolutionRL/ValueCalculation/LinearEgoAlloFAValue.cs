using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class LinearEgoAlloFAValue<stateType, actionType> : ActionValue<int[], int[]>
    {

        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        List<int[]> availableActions;
        DoubleArrayComparer doubleArrayComparer = new DoubleArrayComparer();
        PerformanceStats stats = new PerformanceStats();

        public double defaultQ = 10;
        double gamma = 0.9;
        //OnlineLinearRegression[] models;

        OptimalPolicy<int[], int[]> optimalPolicy = new OptimalPolicy<int[], int[]>();

        int[] worldSize = new int[2] { 16, 48 };


        public LinearEgoAlloFAValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            models = new OnlineLinearRegression[availableActions.Count];
            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new OnlineLinearRegression(worldSize[0] * worldSize[1] + 8, 0.1, false, defaultQ);
            }

        }

        public override explorationMode getRecommendedExplorationMode()
        {
            return explorationMode.normal;
        }

        public override PerformanceStats getStats()
        {
            return stats;
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

        public override double update(StateTransition<int[], int[]> transition)
        {
            stats.cumulativeReward += transition.reward;

            int actionIndex = availableActions.FindIndex((a) => actionComparer.Equals(a, transition.action));

            double[] tileState = new double[worldSize[0] * worldSize[1] + 12];
            Array.Copy(transition.oldState, 2, tileState, 0, 12);
            tileState[transition.oldState[0] * worldSize[0] + transition.oldState[1] + 12] = 1;

            double[] newVals = value(transition.newState, availableActions);

            models[actionIndex].Train(tileState, transition.reward + gamma * newVals.Max());

            return 0;
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            double[] vals = new double[actions.Count];
            for (int i = 0; i < actions.Count; i++)
            {
                vals[i] = value(state, actions[i]);
            }
            return vals;
        }

        public double value(int[] state, int[] action)
        {
            int actionIndex = availableActions.FindIndex((a) => actionComparer.Equals(a, action));

            double[] tileState = new double[worldSize[0] * worldSize[1] + 12];
            Array.Copy(state, 2, tileState, 0, 12);
            tileState[state[0] * worldSize[0] + state[1] + 12] = 1;

            double val = models[actionIndex].Compute(tileState);
            return val;
        }
    }

}
