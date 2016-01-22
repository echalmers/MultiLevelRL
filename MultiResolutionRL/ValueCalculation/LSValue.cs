using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class LSValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        IEqualityComparer<actionType> actionComparer;
        IEqualityComparer<stateType> stateComparer;
        List<actionType> availableActions;
        PerformanceStats stats = new PerformanceStats();

        ModelBasedValue<stateType, actionType> trueModel;
        //Dictionary<stateType, int> backupT;
        //Dictionary<stateType, double> backupR;
        //double backupQ;
        Histogram backupR;
        StateTransition<stateType, actionType> lossTransition;
        int LScounter = -1;

        public LSValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            trueModel = new ModelBasedValue<stateType, actionType>(StateComparer, ActionComparer, availableActions, StartState);
            trueModel.maxUpdates = 1000;
        }

        public override PerformanceStats getStats()
        {
            return stats;
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
            return trueModel.PredictReward(state, action, newState);
        }

        public override double update(StateTransition<stateType, actionType> transition)
        {
            stats.cumulativeReward += transition.reward;

            double RPE = 0;
            if (trueModel.T.GetStateValueTable(transition.oldState,transition.action).Count>0)
                RPE = transition.reward - trueModel.PredictReward(transition.oldState, transition.action, transition.newState);
            

            if (RPE < 0 && LScounter <= 0)
            {
                lossTransition = transition;

                //Dictionary<stateType, int> thisT = trueModel.T.GetStateValueTable(transition.oldState, transition.action);
                //Dictionary<stateType, double> thisR = trueModel.R.GetStateValueTable(transition.oldState, transition.action);

                //backupT = new Dictionary<stateType, int>(thisT, stateComparer);
                //backupR = new Dictionary<stateType, double>(thisR, stateComparer);
                //backupQ = trueModel.value(transition.oldState, transition.action);

                //thisT = new Dictionary<stateType, int>(stateComparer);
                //thisR = new Dictionary<stateType, double>(stateComparer);
                //trueModel.Qtable[transition.oldState][transition.action] = RPE;
                backupR = trueModel.R.Get(transition.oldState, transition.action, transition.newState);
                Histogram temp = new Histogram(0); temp.Add(RPE);
                trueModel.R.Set(transition.oldState, transition.action, transition.newState, temp);

                LScounter = 8;
            }

            if (LScounter == 0)
            {
                //Dictionary<stateType, int> thisT = trueModel.T.GetStateValueTable(lossTransition.oldState, lossTransition.action);
                //Dictionary<stateType, double> thisR = trueModel.R.GetStateValueTable(lossTransition.oldState, lossTransition.action);

                //thisT = new Dictionary<stateType, int>(backupT, stateComparer);
                //thisR = new Dictionary<stateType, double>(backupR, stateComparer);
                //trueModel.Qtable[lossTransition.oldState][lossTransition.action] = backupQ;

                //backupT = null;
                //backupR = null;

                trueModel.R.Set(lossTransition.oldState, lossTransition.action, lossTransition.newState, backupR);
                lossTransition = null;
            }

            trueModel.update(transition);
            LScounter--;

            //Console.WriteLine(RPE.ToString() + ", " + LScounter.ToString());
            return 0;
        }

        public override double[] value(stateType state, List<actionType> actions)
        {
            return trueModel.value(state, actions);
        }

        public double value(stateType state, actionType action)
        {
            return trueModel.value(state, action);
        }

        //Simple Getter function for the true models T table<stateType, actionType, int>
        public SAStable<stateType, actionType,int> getTTable()
            {
                 return trueModel.T;
            }
    }
}
