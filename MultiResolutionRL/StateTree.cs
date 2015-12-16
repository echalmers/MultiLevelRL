﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    namespace StateManagement
    {
        public interface StateTree<stateType>
        {
            void AddState(stateType state);
            stateType GetParentState(stateType state, int level);
            List<stateType> GetLevel0Children(stateType parentState, int parentLevel);
            List<stateType> GetChildren(stateType parentState, int parentLevel);
        }

        public class SelfAbstractingStateTree<stateType> : StateTree<stateType>
        {
            List<Dictionary<stateType, stateType>> parents = new List<Dictionary<stateType, stateType>>();
            List<Dictionary<stateType, List<stateType>>> children = new List<Dictionary<stateType, List<stateType>>>();

            public void AddState(stateType state)
            {
                //throw new NotImplementedException();
            }

            public List<stateType> GetChildren(stateType parentState, int parentLevel)
            {
                if ((children[parentLevel] != null) && (children[parentLevel].ContainsKey(parentState)))
                    return children[parentLevel][parentState];
                else
                    return new List<stateType>();
            }

            public List<stateType> GetLevel0Children(stateType parentState, int parentLevel)
            {
                List<stateType> highLevel = new List<stateType>();
                highLevel.Add(parentState);
                List<stateType> lowLevel = new List<stateType>();

                for (int i = parentLevel; i > 0; i--)
                {
                    foreach (stateType s in highLevel)
                    {
                        lowLevel.AddRange(GetChildren(s, i));
                    }
                    highLevel = new List<stateType>(lowLevel);
                    lowLevel.Clear();
                }
                return highLevel;
            }

            public stateType GetParentState(stateType state, int level)
            {
                if (level == 0)
                    return state;
                
                stateType parent = state;
                for (int i=0; i< level; i++)
                {
                    parent = parents[i][parent];
                }
                return parent;
            }

            public List<stateType> PerformAbstraction(double[,] distances, List<stateType> stateList, IEqualityComparer<stateType> stateComparer)
            {
                int numClus = 10;
                List<stateType> clusters = new List<stateType>(stateList);

                for (int i = 0; i < stateList.Count - numClus; i++)
                {
                    // find min distance
                    int[] coords = findMinDist(distances);

                    // create distance info for the new cluster
                    for (int e=0; e<distances.GetLength(0); e++)
                    {
                        distances[coords[0], e] = Math.Max(distances[coords[0], e], distances[coords[1], e]);
                        distances[e, coords[0]] = distances[coords[0], e];

                        distances[coords[1], e] = double.PositiveInfinity;
                        distances[e, coords[1]] = double.PositiveInfinity;

                        if (stateComparer.Equals(clusters[e], clusters[coords[1]]))
                            clusters[e] = clusters[coords[0]];
                    }
                    
                }

                return clusters;
            }

            private int[] findMinDist(double[,] distances)
            {
                double minVal = double.PositiveInfinity;
                int[] coords = new int[2];

                for (int i=1; i<distances.GetLength(0); i++)
                {
                    for (int j=0; j<i; j++)
                    {
                        if (distances[i,j] < minVal)
                        {
                            minVal = distances[i, j];
                            coords[0] = i;
                            coords[1] = j;
                        }
                    }
                }
                return coords;
            }

            private void assignParent(stateType child, int childLevel, stateType parent)
            {

            }
        }

        public class intStateTree : StateTree<int[]>
        {        
            public void AddState(int[] state)
            { }

            public int[] GetParentState(int[] state, int level)
            {
                int[] parent = new int[state.Length];
                for (int i = 0; i < parent.Length; i++)
                {
                    parent[i] = (int)(state[i] / Math.Pow(2, level));
                }
                return parent;
            }

            public List<int[]> GetLevel0Children(int[] parentState, int parentLevel)
            {
                List<int[]> highLevel = new List<int[]>();
                highLevel.Add(parentState);
                List<int[]> lowLevel = new List<int[]>();

                for (int i = parentLevel; i > 0; i--)
                {
                    foreach (int[] s in highLevel)
                    {
                        lowLevel.AddRange(GetChildren(s, i));
                    }
                    highLevel = new List<int[]>(lowLevel);
                    lowLevel.Clear();
                }
                return highLevel;
            }

            public List<int[]> GetChildren(int[] parentState, int parentLevel)
            {
                int totalChildren = 1 << parentState.Length;
                List<int[]> children = new List<int[]>();

                for (int childNum = 0; childNum < totalChildren; childNum++ )
                {
                    int[] thisChild = new int[parentState.Length];
                    for (int i=0; i<parentState.Length; i++)
                    {
                        thisChild[i] = (parentState[i] << 1) + ((childNum >> i) & 1) ;
                    }
                    children.Add(thisChild);
                }

                return children;
            }
        }

        public class taxiStateTree : StateTree<int[]>
        {
            public void AddState(int[] state)
            { }

            public int[] GetParentState(int[] state, int level)
            {
                if (level == 0)
                    return state;

                int[] parent = new int[state.Length];
                for (int i = 0; i < parent.Length-1; i++)
                {
                    parent[i] = (int)(state[i] / Math.Pow(2, level));
                }
                parent[parent.Length - 1] = Math.Abs(state[state.Length - 1]);
                return parent;
            }

            public List<int[]> GetLevel0Children(int[] parentState, int parentLevel)
            {
                List<int[]> highLevel = new List<int[]>();
                highLevel.Add(parentState);
                List<int[]> lowLevel = new List<int[]>();

                for (int i = parentLevel; i > 0; i--)
                {
                    foreach (int[] s in highLevel)
                    {
                        lowLevel.AddRange(GetChildren(s, i));
                    }
                    highLevel = new List<int[]>(lowLevel);
                    lowLevel.Clear();
                }
                return highLevel;
            }

            public List<int[]> GetChildren(int[] parentState, int parentLevel)
            {
                int numOuterLoops = 1 << (parentState.Length-1);
                List<int[]> children = new List<int[]>();
                if (parentLevel == 0)
                {
                    children.Add(parentState);
                    return children;
                }


                for (int childNum = 0; childNum < numOuterLoops; childNum++)
                {
                    int[] thisChildBase = new int[parentState.Length];
                    for (int i = 0; i < thisChildBase.Length-1; i++)
                    {
                        thisChildBase[i] = (parentState[i] << 1) + ((childNum >> i) & 1);
                    }

                    thisChildBase[thisChildBase.Length - 1] = parentState[parentState.Length - 1];
                    children.Add(thisChildBase);

                    if (parentLevel==1)
                    {
                        children.Add(new int[thisChildBase.Length]);
                        Array.Copy(thisChildBase, children.Last(), thisChildBase.Length);
                        children.Last()[thisChildBase.Length - 1] *= -1;
                    }
                }

                if (children.Count > 16)
                    children = children;
                return children;
            }
        }
    }
}
        

    //public class StateTree<stateType>
    //{
    //    int arity = 5;
    //    int activeLevel1State = 0;
    //    int activeLevel1StateChildren = 0;
    //    Dictionary<stateType, int> firstLevelParents;
    //    Dictionary<int, List<stateType>> firstLevelChildren;
    //    IEqualityComparer<stateType> stateComparer;

    //    public StateTree(IEqualityComparer<stateType> StateComparer)
    //    {
    //        stateComparer = StateComparer;
    //        firstLevelParents = new Dictionary<stateType, int>(stateComparer);
    //        firstLevelChildren = new Dictionary<int, List<stateType>>();

    //    }

    //    public void AddState(stateType state)
    //    {
    //        // if this state already exists in the tree at level 0...
    //        if (firstLevelParents.ContainsKey(state))
    //        {
    //            // but is not in the currently active level 1 state
    //            if (firstLevelParents[state] != activeLevel1State)
    //            {
    //                if (activeLevel1StateChildren > 0)
    //                {
    //                    activeLevel1State++;
    //                    activeLevel1StateChildren = 0;
    //                }
    //            }
    //        }
    //        else // if this is a new state, add it to the tree. Create a new level 1 state as necessary
    //        {
    //            firstLevelParents.Add(state, activeLevel1State);

    //            if (!firstLevelChildren.ContainsKey(activeLevel1State))
    //                firstLevelChildren.Add(activeLevel1State, new List<stateType>());
    //            firstLevelChildren[activeLevel1State].Add(state);

    //            activeLevel1StateChildren = (activeLevel1StateChildren + 1) % arity;
    //            if (activeLevel1StateChildren == 0)
    //                activeLevel1State++;
    //        }
    //    }

    //    public stateType GetParentState(stateType state, int level)
    //    {
    //        if (level < 1)
    //            return state;

    //        int highLevelState = (int)(firstLevelParents[state] / Math.Pow(arity, level - 1));
    //        int level1State = (int)(highLevelState * Math.Pow(arity, level - 1));
    //        return firstLevelChildren[level1State][0];
    //    }

    //    public List<stateType> GetLevel0Children(stateType parentState, int parentLevel)
    //    {
    //        int level1Parent = firstLevelParents[parentState];
    //        List<stateType> children = new List<stateType>();
    //        while (stateComparer.Equals(parentState, GetParentState(firstLevelChildren[level1Parent][0], parentLevel)))
    //        {
    //            children.AddRange(firstLevelChildren[level1Parent]);
    //            level1Parent++;

    //            if (!firstLevelChildren.ContainsKey(level1Parent))
    //                break;
    //        }
    //        return children;
    //    }

    //    public List<stateType> GetChildren(stateType parentState, int parentLevel)
    //    {
    //        if (parentLevel == 1)
    //            return firstLevelChildren[firstLevelParents[parentState]];

    //        int lowerLevel = (int)(firstLevelParents[parentState] / Math.Pow(arity, parentLevel - 2));
    //        List<stateType> children = new List<stateType>();

    //        for (int i = 0; i < arity; i++)
    //        {
    //            int thisLevel1State = (int)((lowerLevel + i) * Math.Pow(arity, parentLevel - 2));
    //            if (firstLevelChildren.ContainsKey(thisLevel1State))
    //                children.Add(firstLevelChildren[thisLevel1State][0]);
    //        }

    //        return children;
    //    }

    //}



    
    
//}
