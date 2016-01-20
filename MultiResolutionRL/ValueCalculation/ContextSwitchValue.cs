using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    [Serializable]
    public class ContextSwitchValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        IEqualityComparer<actionType> actionComparer;
        IEqualityComparer<stateType> stateComparer;
        List<actionType> availableActions;
        stateType startState;
        
        List<ModelBasedValue<stateType, actionType>> models = new List<ModelBasedValue<stateType, actionType>>();
        ModelBasedValue<stateType, actionType> currentModel;

        public ContextSwitchValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;
            
            models.Add(new ModelBasedValue<stateType, actionType>(StateComparer, ActionComparer, availableActions, StartState, parameters));
            //models.Add(new ModelBasedValue<stateType, actionType>(StateComparer, ActionComparer, availableActions, StartState, parameters));

            currentModel = models[0];
        }

        public override PerformanceStats getStats()
        {
            return currentModel.getStats();
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

        public override double update(StateTransition<stateType, actionType> transition)
        {
            currentModel.update(transition);


            double[] ps = new double[models.Count];
            for (int i=0; i<ps.Length; i++)
            {
                ps[i] = Tprobability(transition, models[i]);
            }
            Console.WriteLine(string.Join(",", ps));


            double bestP = Tprobability(transition, currentModel);

            foreach (ModelBasedValue<stateType, actionType> m in models)
            {
                if (m == currentModel)
                    continue;

                double thisP = Tprobability(transition, m);

                if (thisP > bestP)
                {
                    Console.WriteLine("switching to previously learned model: " + models.IndexOf(m));
                    if (models.IndexOf(m)==2)
                    {
                        int a = 0;
                    }
                    currentModel = m;
                    bestP = thisP;
                }
            }

            if (bestP < 0.5)
            {
                Console.WriteLine("creating new model");
                currentModel = new ModelBasedValue<stateType, actionType>(stateComparer, actionComparer, availableActions, startState);
                models.Add(currentModel);
            }
            
            return 0;
        }

        public override double[] value(stateType state, List<actionType> actions)
        {
            return currentModel.value(state, actions);
        }
        
        double Tprobability(StateTransition<stateType,actionType> transition, ModelBasedValue<stateType, actionType> model)
        {
            Dictionary<stateType, int> s2Counts = model.T.GetStateValueTable(transition.oldState, transition.action);
            double thisS2Counts = 0;
            if (s2Counts.ContainsKey(transition.newState))
                thisS2Counts = (double)s2Counts[transition.newState];
            double total = (double)s2Counts.Values.Sum();

            if (total == 0)
                return 0;
            else
                return thisS2Counts / total;
        }
    }
}
