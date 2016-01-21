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

        Queue<StateTransition<stateType, actionType>> transitionHistory = new Queue<StateTransition<stateType, actionType>>();

        public ContextSwitchValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;
            startState = StartState;
            
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
            transitionHistory.Enqueue(transition);
            while (transitionHistory.Count() > 5)
                transitionHistory.Dequeue();

            //double[] ps = new double[models.Count];
            //for (int i = 0; i < ps.Length; i++)
            //{
            //    ps[i] = Math.Round(Tprobability(transitionHistory, models[i]),2);
            //}
            //Console.WriteLine(string.Join(",", ps));


            // select model
            double bestP = Tprobability(transitionHistory, currentModel);
            foreach (ModelBasedValue<stateType, actionType> m in models)
            {
                if (m == currentModel)
                    continue;

                double thisP = Tprobability(transitionHistory, m);

                if (thisP > bestP)
                {
                    Console.WriteLine("switching to previously learned model: " + models.IndexOf(m) + " (p=" + thisP + ")");
                    currentModel = m;
                    bestP = thisP;
                }
            }

            // create new model if necessary
            if (bestP < 0.5)
            {
                Console.WriteLine("creating new model");
                currentModel = new ModelBasedValue<stateType, actionType>(stateComparer, actionComparer, availableActions, startState);
                models.Add(currentModel);
            }

            currentModel.update(transition);
            
            return 0;
        }

        public override double[] value(stateType state, List<actionType> actions)
        {
            return currentModel.value(state, actions);
        }
        
        double Tprobability(IEnumerable<StateTransition<stateType, actionType>> transitions, ModelBasedValue<stateType, actionType> model)
        {
            double p = 1;
            foreach (StateTransition<stateType, actionType> transition in transitions)
            {
                Dictionary<stateType, int> s2Counts = model.T.GetStateValueTable(transition.oldState, transition.action);
                double thisS2Counts = 1;
                if (s2Counts.ContainsKey(transition.newState))
                    thisS2Counts = (double)s2Counts[transition.newState] + 1;
                double total = (double)s2Counts.Values.Sum() + 1;
                
                p *= (thisS2Counts / total);
            }
            return p;
        }
        
    }
}
