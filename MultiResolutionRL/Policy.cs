using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    public interface Policy<stateType, actionType>
    {
        actionType selectAction(List<actionType> availableActions, List<double> values, params double[] parameters);
    }

    public class RandomPolicy<stateType, actionType> : Policy<stateType, actionType>
    {
        Random rnd = new Random();
        public actionType selectAction(List<actionType> availableActions, List<double> values, params double[] parameters)
        {
            return availableActions[rnd.Next(availableActions.Count)];
        }
    }

    public class SoftmaxPolicy<stateType, actionType> : Policy<stateType, actionType>
    {
        public double defaultT = 1;

        Random rnd = new Random();
        
        public int selectActionIndex(List<actionType> availableActions, List<double> values, params double[] parameters)
        {
            double T = defaultT;
            if (parameters.Length > 0)
                T = parameters[0];

            double[] p = new double[values.Count];
            p[0] = Math.Exp(values[0] / T);
            for (int i = 1; i < values.Count; i++)
            {
                p[i] = p[i - 1] + Math.Exp(values[i] / T);
            }

            double threshold = rnd.NextDouble() * p.Last();
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] > threshold)
                    return i;
            }
            return -1;
        }

        public actionType selectAction(List<actionType> availableActions, List<double> values, params double[] parameters)
        {
            double T = defaultT;
            if (parameters.Length > 0)
                T = parameters[0];

            double[] p = new double[values.Count];
            p[0] = Math.Exp(values[0]/T);
            for (int i=1; i<values.Count; i++)
            {
                p[i] = p[i-1] + Math.Exp(values[i]/T);
            }

            double threshold = rnd.NextDouble() * p.Last();
            for (int i=0; i<p.Length; i++)
            {
                if (p[i] > threshold)
                    return availableActions[i];
            }
            return availableActions.Last();
        }
    }

    public class EGreedyPolicy<stateType, actionType> : Policy<stateType, actionType>
    {
        double defaultE = 0.9;// double.PositiveInfinity;
        Random rnd = new Random();
        
        public actionType selectAction(List<actionType> availableActions, List<double> values, params double[] parameters)
        {
            double e = defaultE;
            if (parameters.Length > 0)
                e = parameters[0];

            // select at random or by value?
            if (rnd.NextDouble() < e) // by value
            {
                //actionType bestAction = availableActions[0];
                //double expectedReward = double.NegativeInfinity;
                //for (int i = 0; i < values.Count; i++)
                //{
                //    if (values[i] > expectedReward)
                //    {
                //        expectedReward = values[i];
                //        bestAction = availableActions[i];
                //    }
                //}
                //return bestAction;

                List<actionType> bestActions = new List<actionType>();
                double bestValue = values.Max();
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i] == bestValue)
                        bestActions.Add(availableActions[i]);
                }
                return bestActions[rnd.Next(bestActions.Count)];
            }
            else // randomly
            {
                int randIndex = rnd.Next(availableActions.Count);
                //double expectedReward = values[randIndex];
                return availableActions[randIndex];
            }
        }
    }

    public class OptimalPolicy<stateType, actionType> : Policy<stateType, actionType>
    {
        Random rnd = new Random();

        public List<int> selectActionIndices(List<actionType> availableActions, List<double> values, params double[] parameters)
        {
            double bestVal = double.NegativeInfinity;
            foreach (double val in values)
            {
                bestVal = Math.Max(bestVal, val);
            }

            List<int> bestActionIndices = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == bestVal)
                {
                    bestActionIndices.Add(i);
                }
            }

            return bestActionIndices;
        }

        public actionType selectAction(List<actionType> availableActions, List<double> values, params double[] parameters)
        {
            List<int> actionIndices = selectActionIndices(availableActions, values, parameters);
            return availableActions[actionIndices[rnd.Next(actionIndices.Count)]];
        }
    }
}
