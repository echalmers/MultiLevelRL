using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Regression.Linear;

namespace MultiResolutionRL.ValueCalculation
{
    public class TileCodingTest<stateType, actionType> : ActionValue<int[], int[]>
    {
        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        List<int[]> availableActions;
        DoubleArrayComparer doubleArrayComparer = new DoubleArrayComparer();
        PerformanceStats stats = new PerformanceStats();

        double[][] weights;
        double gamma = 0.9, alpha = 0.5;
        
        OptimalPolicy<int[], int[]> optimalPolicy = new OptimalPolicy<int[], int[]>();

        int[] worldSize = new int[2] { 21, 21 };


        public TileCodingTest(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            weights = new double[AvailableActions.Count][];
            for (int i=0; i<weights.Length;i++)
            {
                weights[i] = new double[worldSize[0] * worldSize[1] + 1];
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
            
            double[] tileState = new double[worldSize[0] * worldSize[1]];
            tileState[transition.oldState[0] * worldSize[0] + transition.oldState[1]] = 1;

            double[] newVals = value(transition.newState, availableActions);
            double delta = transition.reward + gamma*newVals.Max() - value(transition.oldState, transition.action);

            // tune constant
            weights[actionIndex][0] += alpha * delta;

            // tune weights
            for (int i = 0; i < tileState.Length; i++)
            {
                weights[actionIndex][i + 1] += alpha * delta * tileState[i];
            }

            return 0;
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            double[] vals = new double[actions.Count];
            for (int i=0;i<actions.Count; i++)
            {
                vals[i] = value(state, actions[i]);
            }
            return vals;
        }

        private double value(int[] state, int[] action)
        {
            int actionIndex = availableActions.FindIndex((a) => actionComparer.Equals(a, action));

            double[] tileState = new double[worldSize[0] * worldSize[1]];
            tileState[state[0] * worldSize[0] + state[1]] = 1;

            double val = 0;
            for (int i=0; i<weights[actionIndex].Length-1; i++)
            {
                val += weights[actionIndex][i+1] * tileState[i];
            }
            return val;
        }
    }
}
