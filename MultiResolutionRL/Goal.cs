using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    [Serializable]
    public class Goal<stateType, actionType>
    {
        public int level;
        public stateType startState;
        public actionType action;
        public stateType goalState;
        public double value;
        public IEqualityComparer<stateType> stateComparer;
        public IEqualityComparer<actionType> actionComparer;

        public Goal(int Level, stateType StartState, actionType Action, stateType Goal, double Value, IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer)
        {
            level = Level;
            startState = StartState;
            action = Action;
            goalState = Goal;
            value = Value;
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
        }

    }

    public class GoalComparer<stateType, actionType> : IEqualityComparer<Goal<stateType, actionType>>
    {
        public bool Equals(Goal<stateType, actionType> x, Goal<stateType, actionType> y)
        {
            if (!x.stateComparer.Equals(x.goalState, y.goalState))
                return false;
            if (!x.stateComparer.Equals(x.startState, y.startState))
                return false;
            if (x.level != y.level)
                return false;
            if (x.actionComparer.Equals(x.action, y.action))
                return false;
            return true;
        }

        public int GetHashCode(Goal<stateType, actionType> obj)
        {
            return obj.stateComparer.GetHashCode(obj.goalState) + obj.stateComparer.GetHashCode(obj.startState);
        }
    }


}
