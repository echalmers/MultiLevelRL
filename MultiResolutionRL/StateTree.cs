using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    public class StateTree<stateType>
    {
        int arity = 3;
        int activeLevel1State = 0;
        int activeLevel1StateChildren = 0;
        Dictionary<stateType, int> firstLevelParents;
        Dictionary<int, List<stateType>> firstLevelChildren;
        IEqualityComparer<stateType> stateComparer;

        public StateTree(IEqualityComparer<stateType> StateComparer)
        {
            stateComparer = StateComparer;
            firstLevelParents = new Dictionary<stateType, int>(stateComparer);
            firstLevelChildren = new Dictionary<int, List<stateType>>();
            
        }

        public void AddState(stateType state)
        {
            // if this state already exists in the tree at level 0...
            if (firstLevelParents.ContainsKey(state))
            {
                // but is not in the currently active level 1 state
                if (firstLevelParents[state] != activeLevel1State)
                {
                    if (activeLevel1StateChildren > 0)
                    {
                        activeLevel1State++;
                        activeLevel1StateChildren = 0;
                    }
                }
            }
            else // if this is a new state, add it to the tree. Create a new level 1 state as necessary
            {
                firstLevelParents.Add(state, activeLevel1State);
                activeLevel1StateChildren = (activeLevel1StateChildren+1) % arity;
                if (activeLevel1StateChildren == 0)
                    activeLevel1State++;

                if (!firstLevelChildren.ContainsKey(activeLevel1State))
                    firstLevelChildren.Add(activeLevel1State, new List<stateType>());
                firstLevelChildren[activeLevel1State].Add(state);
            }
        }

        public stateType GetParentState(stateType state, int level)
        {
            if (level < 1)
                return state;

            int level1State = firstLevelParents[state];
            return firstLevelChildren[level1State][0];
        }

        public List<stateType> GetChildren(stateType parentState, int parentLevel)
        {
            int level1Parent = firstLevelParents[parentState];
            List<stateType> children = new List<stateType>();
            while (stateComparer.Equals(parentState, GetParentState(firstLevelChildren[level1Parent][0], parentLevel)))
            {
                children.AddRange(firstLevelChildren[level1Parent]);
                level1Parent++;
            }
            return children;
        }
    }
    

    
}
