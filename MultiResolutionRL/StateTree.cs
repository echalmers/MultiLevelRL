using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    namespace StateManagement
    {
        public class StateTree<stateType>
        {
            int arity = 4;
            StateTreeNode<stateType> root;
            IEqualityComparer<stateType> stateComparer;

            public StateTree(IEqualityComparer<stateType> StateComparer, stateType startState)
            {
                stateComparer = StateComparer;
                root = new StateTreeNode<stateType>(arity, 1, startState, stateComparer);
                AddState(startState);
            }

            public void AddState(stateType level0State)
            {
                if (root.Contains(level0State, 0, true))
                    return;

                if (root.AddState(level0State)==false)
                {
                    StateTreeNode<stateType> newRoot = new StateTreeNode<stateType>(arity, root.level + 1, level0State, stateComparer);
                    newRoot.AddChildNode(root);
                    root = newRoot;
                    root.AddState(level0State);
                }
            }

            public stateType GetParentState(stateType level0State, int parentLevel)
            {
                if (parentLevel == 0)
                    return level0State;

                stateType parent;
                if (root.GetParent(level0State, parentLevel, out parent) == false)
                    throw new ArgumentException();
                return parent;
            }

            public List<stateType> GetLevel0Children(stateType parentState, int parentLevel)
            {
                return root.GetAllChildren();
            }

            public List<stateType> GetChildren(stateType parentState, int parentLevel)
            {
                return root.GetImmediateChildren(parentState, parentLevel);
            }
        }

        class StateTreeNode<stateType>
        {
            int arity;
            public int level;
            stateType name;
            List<StateTreeNode<stateType>> children = new List<StateTreeNode<stateType>>();
            IEqualityComparer<stateType> stateComparer;
            int activeChild = 0;

            public StateTreeNode(int Arity, int Level, stateType Name, IEqualityComparer<stateType> StateComparer)
            {
                arity = Arity;
                level = Level;
                name = Name;
                stateComparer = StateComparer;
            }

            public bool AddState(stateType level0State)
            {
                if (level == 1)
                {
                    if (children.Count >= arity)
                        return false;

                    else
                    {
                        children.Add(new StateTreeNode<stateType>(arity, 0, level0State, stateComparer));
                        activeChild = children.Count - 1;
                        return true;
                    }
                }

                else
                {
                    // must add if the children list is still empty
                    if (children.Count == 0)
                        children.Add(new StateTreeNode<stateType>(arity, level - 1, level0State, stateComparer));
                    //

                    if (children[activeChild].AddState(level0State) == false)
                    {
                        if (children.Count >= arity)
                            return false;

                        children.Add(new StateTreeNode<stateType>(arity, level - 1, level0State, stateComparer));
                        activeChild = children.Count - 1;
                        children[activeChild].AddState(level0State);
                        return true;
                    }
                    else
                        return true;
                }
            }

            public void AddChildNode(StateTreeNode<stateType> node)
            {
                children.Add(node);
                activeChild = children.Count - 1;
            }

            public bool Contains(stateType state, int atLevel, bool setActive)
            {
                if (level == atLevel)
                {
                    return stateComparer.Equals(name, state);
                }

                for (int i=0; i<children.Count; i++)
                {
                    if (children[i].Contains(state, atLevel, setActive))
                    {
                        if (setActive)
                            activeChild = i;
                        return true;
                    }
                }
                return false;
            }

            public bool GetParent(stateType level0State, int parentLevel, out stateType parentName)
            {
                if (level == parentLevel)
                {
                    if (this.Contains(level0State, 0, false))
                    {
                        parentName = name;
                        return true;
                    }
                    else
                    {
                        parentName = default(stateType);
                        return false;
                    }
                }

                if (level > parentLevel)
                {
                    foreach (StateTreeNode<stateType> child in children)
                    {
                        if (child.GetParent(level0State, parentLevel, out parentName))
                            return true;
                    }
                }

                parentName = default(stateType);
                return false;
            }

            public List<stateType> GetAllChildren()
            {
                List<stateType> allChildren = new List<stateType>();

                if (level==1)
                {
                    foreach(StateTreeNode<stateType> child in children)
                    {
                        allChildren.Add(child.name);
                    }
                }
                else
                {
                    foreach (StateTreeNode<stateType> child in children)
                    {
                        allChildren.AddRange(child.GetAllChildren());
                    }
                }
                return allChildren;
            }

            public List<stateType> GetImmediateChildren(stateType parentState, int parentLevel)
            {
                List<stateType> immediateChildren = new List<stateType>();
                
                if (level == parentLevel && stateComparer.Equals(name, parentState))
                {   
                    foreach(StateTreeNode<stateType> child in children)
                    {
                        immediateChildren.Add(child.name);
                    }
                }

                else
                {
                    foreach (StateTreeNode<stateType> child in children)
                    {
                        immediateChildren.AddRange(child.GetImmediateChildren(parentState, parentLevel));
                    }
                }
                return immediateChildren;
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

//    public class intStateTree : StateTree<int[]>
//    {
//        public intStateTree(IEqualityComparer<int[]> StateComparer)
//            : base(StateComparer)
//        { }

//        new public void AddState(int[] state)
//        { }

//        new public int[] GetParentState(int[] state, int level)
//        {
//            int[] parent = new int[2];
//            parent[0] = (int)(state[0] / Math.Pow(2, level));
//            parent[1] = (int)(state[1] / Math.Pow(2, level));
//            return parent;
//        }

//        new public List<int[]> GetLevel0Children(int[] parentState, int parentLevel)
//        {
//            List<int[]> highLevel = new List<int[]>();
//            highLevel.Add(parentState);
//            List<int[]> lowLevel = new List<int[]>();

//            for (int i = parentLevel; i > 0; i--)
//            {
//                foreach (int[] s in highLevel)
//                {
//                    lowLevel.AddRange(GetChildren(s, i));
//                }
//                highLevel = new List<int[]>(lowLevel);
//                lowLevel.Clear();
//            }
//            return highLevel;
//        }

//        new public List<int[]> GetChildren(int[] parentState, int parentLevel)
//        {
//            List<int[]> children = new List<int[]>();
//            children.Add(new int[2] { parentState[0] << 1, parentState[1] << 1 });
//            children.Add(new int[2] { (parentState[0] << 1) + 1, parentState[1] << 1 });
//            children.Add(new int[2] { parentState[0] << 1, (parentState[1] << 1) + 1 });
//            children.Add(new int[2] { (parentState[0] << 1) + 1, (parentState[1] << 1) + 1 });
//            return children;
//        }
//    }

    
    
//}
