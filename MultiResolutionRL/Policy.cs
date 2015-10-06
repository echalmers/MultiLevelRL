using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    public interface Policy<stateType, actionType>
    {
        actionType selectAction(List<actionType> availableActions, List<double> values);
    }

    public class SoftmaxPolicy<stateType, actionType> : Policy<stateType, actionType>
    {
        Random rnd = new Random();
        public actionType selectAction(List<actionType> availableActions, List<double> values)
        {
            double[] p = new double[values.Count];
            p[0] = Math.Exp(values[0]);
            for (int i=1; i<values.Count; i++)
            {
                p[i] = p[i-1] + Math.Exp(values[i]);
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
        double e = 0.8;//double.PositiveInfinity;
        Random rnd = new Random();
        
        public actionType selectAction(List<actionType> availableActions, List<double> values)
        {
            // select at random or by value?
            if (rnd.NextDouble() < e) // by value
            {
                actionType bestAction = availableActions[0];
                double expectedReward = double.NegativeInfinity;
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i] > expectedReward)
                    {
                        expectedReward = values[i];
                        bestAction = availableActions[i];
                    }
                }
                return bestAction;
            }
            else // randomly
            {
                int randIndex = rnd.Next(availableActions.Count - 1);
                double expectedReward = values[randIndex];
                return availableActions[randIndex];
            }
        }
    }
}
