using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{

    public class RewardMemory<stateType>
    {
        int size;
        IEqualityComparer<stateType> stateComparer;
        List<stateType> newStates = new List<stateType>();
        List<double> rewards = new List<double>();

        public RewardMemory(int Size, IEqualityComparer<stateType> StateComparer)
        {
            size = Size;
            stateComparer = StateComparer;
        }

        public void Add(stateType newState, double reward)
        {
            newStates.Add(newState);
            rewards.Add(reward);
            while (newStates.Count > size)
            {
                newStates.RemoveAt(0);
                rewards.RemoveAt(0);
            }
        }

        public int Count() 
        { return newStates.Count + 1; }

        public int Count(stateType state)
        {
            int count=1;
            foreach (stateType s in newStates)
            {
                if (stateComparer.Equals(s, state))
                    count++;
            }
            return count;
        }

        public IEnumerable<stateType> distinctStates()
        {
            return newStates.Distinct(stateComparer);
        }
        
        public double Average()
        {
            return rewards.Average();
        }

        public double AverageForState(stateType state)
        {
            double total = 0, count = 0;
            for (int i=0; i<newStates.Count; i++)
            {
                if (stateComparer.Equals(newStates[i], state))
                {
                    total += rewards[i];
                    count++;
                }
            }
            return total / count;
        }

        public stateType mostCommonState()
        {
            Dictionary<stateType, int> counts = new Dictionary<stateType, int>(stateComparer);
            foreach(stateType s in newStates)
            {
                if (!counts.ContainsKey(s))
                    counts[s] = 0;
                counts[s]++;
            }
            stateType mostCommon = default(stateType);
            int largestCount = -1;
            foreach(stateType s in counts.Keys)
            {
                if (counts[s] > largestCount)
                {
                    mostCommon = s;
                    largestCount = counts[s];
                }
            }
            return mostCommon;
        }

    }

}
