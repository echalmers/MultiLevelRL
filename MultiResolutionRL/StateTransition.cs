using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
    [Serializable]
    public class StateTransition<stateType, actionType>
    {
        public stateType oldState;
        public actionType action;
        public double reward;
        public stateType newState;
        public bool absorbingStateReached;

        public StateTransition(stateType OldState, actionType Action, double Reward, stateType NewState, bool SessionHasEnded=false)
        {
            oldState = OldState;
            action = Action;
            reward = Reward;
            newState = NewState;
            absorbingStateReached = SessionHasEnded;
        }
    }
}
