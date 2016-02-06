using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation.BaseClasses
{
    //Is used for splitting up the multiple factors that can come from the  stateclass, then create a appropriate learner
    //that used each descriptor appropriately
    class ActionClass<stateType, actionType> : ActionValue<stateType,actionType>
    {
        //MEMBERS//
        ActionValue<stateType, actionType> egoModel = null;
        ActionValue<stateType, actionType> aloModel = null;
        Type egoType = null;
        Type aloType = null;
        bool hasEgo = false;
        bool isStateClass = false;

        //Needs to create a model for each form of data in stateClass
        ActionClass(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {

            if (typeof(stateType) == typeof(StateClass))
            {
                isStateClass = true;
                StateClass holder = (StateClass)parameters[0];
                aloType = (holder.GetStateFactor("GlobalLocation")).GetType();
                aloModel = (ActionValue<stateType, actionType>)Activator.CreateInstance(aloType, Comparer.OC, ActionComparer, AvailableActions, StartState, parameters);
                if (holder.FactorExist("Ego"))
                {
                    hasEgo = true;
                    egoType = (holder.GetStateFactor("Ego")).GetType();
                    egoModel = (ActionValue<stateType, actionType>)Activator.CreateInstance(egoType, Comparer.OC, ActionComparer, AvailableActions, StartState, parameters);
                }
            }
            else aloModel = new ModelBasedValue<stateType, actionType>(StateComparer, ActionComparer, AvailableActions, StartState, parameters);
        }


        //Have both ego get updated to its new state, and have the alo updated from the transition that occured in the world.
        public override double update(StateTransition<stateType, actionType> transition)
        {
            
            if (isStateClass)
            {
                StateClass newState = (StateClass)Convert.ChangeType(transition.newState, typeof(StateClass));
                StateClass oldState = (StateClass)Convert.ChangeType(transition.oldState, typeof(StateClass));
                stateType oldHolderState;
                stateType newHolderState;
                actionType act = transition.action;
                double reward = transition.reward;
                if (hasEgo)
                {
                    string e = "Ego";
                    oldHolderState = (stateType)oldState.GetStateFactor(e);
                    newHolderState = (stateType)newState.GetStateFactor(e);
                    StateTransition<stateType, actionType> egoTrans = new StateTransition<stateType, actionType>(oldHolderState, act, reward, newHolderState);
                    egoModel.update(egoTrans);
                }
                string a = "GlobalLocation";
                oldHolderState = (stateType)oldState.GetStateFactor(a);
                newHolderState = (stateType)newState.GetStateFactor(a);
                StateTransition<stateType, actionType> aloTrans = new StateTransition<stateType, actionType>(oldHolderState, act, reward, newHolderState);            
                return aloModel.update(aloTrans);
            }
            else
                return aloModel.update(transition);
        }

        //First update the Alo model with value from Ego 
        public override double[] value(stateType state, List<actionType> actions)
        {
            if(isStateClass)
            {
                string a = "GlobalLocation";
                StateClass stateUsable = (StateClass)Convert.ChangeType(state, typeof(StateClass));
                stateType aloState = (stateType)stateUsable.GetStateFactor(a);
                double[] returnVals = aloModel.value(aloState, actions);

                if (hasEgo)
                {
                    string e = "Ego";
                    stateType egoState = (stateType)stateUsable.GetStateFactor(e);
                    double[] returnEgo = egoModel.value(egoState, actions);
                    for (int i = 0; i < actions.Count;i++)
                    {
                        returnVals[i] = (returnVals[i]+returnEgo[i]) / 2;
                    }
                }
                return returnVals;
            }
            return aloModel.value(state, actions);
        }





        public override PerformanceStats getStats()
        {
            throw new NotImplementedException();
        }

        public override stateType PredictNextState(stateType state, actionType action)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<stateType, double> PredictNextStates(stateType state, actionType action)
        {
            throw new NotImplementedException();
        }

        public override double PredictReward(stateType state, actionType action, stateType newState)
        {
            throw new NotImplementedException();
        }
    }
}
