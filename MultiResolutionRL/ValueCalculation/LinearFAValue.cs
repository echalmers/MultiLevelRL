using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Accord.Statistics.Models.Regression.Linear;

namespace MultiResolutionRL.ValueCalculation
{
    public class LinearFAValue<stateType, actionType> : ActionValue<int[], int[]>
    {

        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        List<int[]> availableActions;
        DoubleArrayComparer doubleArrayComparer = new DoubleArrayComparer();
        PerformanceStats stats = new PerformanceStats();

        public double defaultQ = 10;
        double gamma = 0.9;
        OnlineLinearRegression[] models;

        OptimalPolicy<int[], int[]> optimalPolicy = new OptimalPolicy<int[], int[]>();

        int[] worldSize = new int[2] { 16, 48 };


        public LinearFAValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            models = new OnlineLinearRegression[availableActions.Count];
            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new OnlineLinearRegression(worldSize[0] * worldSize[1], 0.5, false, defaultQ);
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
           
            models[actionIndex].Train(tileState, transition.reward + gamma * newVals.Max());

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

        public double value(int[] state, int[] action)
        {
            int actionIndex = availableActions.FindIndex((a) => actionComparer.Equals(a, action));

            double[] tileState = new double[worldSize[0] * worldSize[1]];
            tileState[state[0] * worldSize[0] + state[1]] = 1;

            double val = models[actionIndex].Compute(tileState);
            return val;
        }
    }

    class OnlineLinearRegression
    {
        double[] weights;
        public double alpha = 0.1;
        bool constantTerm = true;

        public OnlineLinearRegression(int inputs, double Alpha, bool ConstantTerm, double defaultWeight = 0)
        {
            alpha = Alpha;
            constantTerm = ConstantTerm;
            weights = new double[inputs + 1];

            if (defaultWeight != 0)
            {
                for (int i=0; i<inputs; i++)
                {
                    weights[i + 1] = defaultWeight;
                }
            }
        }

        public void Train(double[] input, double output)
        {
            double delta = output - Compute(input);

            // tune constant
            if (constantTerm)
                weights[0] += alpha * delta;

            // tune weights
            for (int i = 0; i < input.Length; i++)
            {
                weights[i + 1] += alpha * delta * input[i];
            }
        }

        public double Compute(double[] input)
        {
            double result = constantTerm ? weights[0] : 0;
            for (int i = 0; i < input.Length; i++)
            {
                result += weights[i + 1] * input[i];
            }
            return result;
        }
    }
}
