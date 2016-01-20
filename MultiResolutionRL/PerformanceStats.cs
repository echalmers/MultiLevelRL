using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    [Serializable]
    public class PerformanceStats
    {
        public double cumulativeReward = 0;
        public int modelAccesses = 0;
        public int modelUpdates = 0;

        public List<double> stepsToGoal = new List<double>();

        public PerformanceStats()
        {
            stepsToGoal.Add(0);
        }

        public void TallyStepsToGoal(bool goalReached)
        {
            if (goalReached)
            {
                stepsToGoal.Add(0);
            }
            else
            {
                stepsToGoal[stepsToGoal.Count - 1]++;
            }
        }
    }
}
