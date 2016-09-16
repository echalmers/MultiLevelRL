using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class FreireValue<stateType, actionType> : ActionValue<int[], int[]>
    {
        int currentTaskNumber = 0;
        int stepNumber = 0;
        public int tau = 1000;
        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        PerformanceStats stats = new PerformanceStats();
        Random rnd = new Random(1);
        ActionValue<int[], int[]> baseLearner;

        Dictionary<int[], HashSet<int[]>> abstractToConcreteMap;
        OptimalPolicy<int[], int[]> localOptimalPolicy = new OptimalPolicy<int[], int[]>();
        EGreedyPolicy<int[], int[]> localEGreedyPolicy = new EGreedyPolicy<int[], int[]>();
        SoftmaxPolicy<int[], int[]> localSoftmaxPolicy = new SoftmaxPolicy<int[], int[]>();


        public FreireValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            baseLearner = new ModelFreeValue<int[], int[]>(StateComparer, ActionComparer, availableActions, StartState);
            ((ModelFreeValue<int[],int[]>)baseLearner).defaultQ = 0;
            abstractToConcreteMap = new Dictionary<int[], HashSet<int[]>>(stateComparer);
        }

        public void changeTask()
        {
            currentTaskNumber++;
            stepNumber = 0;
            stats.cumulativeReward = 0;
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            double rate = (double)(tau - stepNumber) / tau;
            stepNumber++;

            int[] aloState = new int[3] { currentTaskNumber, state[0], state[1] };
            int[] egoState = new int[state.Length - 2];
            Array.Copy(state, 2, egoState, 0, state.Length - 2);

            // load the abstractToConcreteMap element
            if (!abstractToConcreteMap.ContainsKey(egoState))
            {
                abstractToConcreteMap.Add(egoState, new HashSet<int[]>(stateComparer));
            }
            bool addded = abstractToConcreteMap[egoState].Add(aloState);


            int numberOfTransferrs = 0;
            if (rnd.NextDouble() < rate) // follow abstract policy
            {
                double[] values = new double[actions.Count];
                foreach (int[] concreteS in abstractToConcreteMap[egoState])
                {
                    // skip concrete states in the current task (only transfer from previous tasks)
                    if (concreteS[0] == currentTaskNumber)
                        continue;

                    numberOfTransferrs++;
                    double[] theseConcreteValues = baseLearner.value(concreteS, availableActions);

                    // Non-deterministic policy
                    foreach (int i in localOptimalPolicy.selectActionIndices(availableActions, theseConcreteValues.ToList()))
                    {
                        values[i] = 1;
                    }

                    // Probabilistic policy
                    //for (int i=0; i<theseConcreteValues.Length; i++)
                    //{
                    //    values[i] += theseConcreteValues[i];
                    //}
                }
                if (numberOfTransferrs==0)
                {
                    double[] theseValues = baseLearner.value(aloState, availableActions);
                    int[] selectedAction = localEGreedyPolicy.selectAction(availableActions, theseValues.ToList());
                    values = new double[actions.Count];
                    values[availableActions.IndexOf(selectedAction)] = 1;
                    return values;
                }

                // probabilisitic policy
                //int selectedIndex = localSoftmaxPolicy.selectActionIndex(availableActions, values.ToList());
                //values = new double[availableActions.Count];
                //values[selectedIndex] = 1;

                return values;
            }
            else // follow concrete policy
            {
                double[] theseValues = baseLearner.value(aloState, availableActions);
                int[] selectedAction = localEGreedyPolicy.selectAction(availableActions, theseValues.ToList());
                double[] values = new double[actions.Count];
                values[availableActions.IndexOf(selectedAction)] = 1;
                return values;
            }

        }

        public override double update(StateTransition<int[], int[]> transition)
        {
            stats.cumulativeReward += transition.reward;

            int[] oldAloState = new int[3] { currentTaskNumber, transition.oldState[0], transition.oldState[1] };
            int[] newAloState = new int[3] { currentTaskNumber, transition.newState[0], transition.newState[1] };
            StateTransition<int[], int[]> aloTransition = new StateTransition<int[], int[]>(oldAloState, transition.action, transition.reward, newAloState);
            return baseLearner.update(aloTransition);
        }


        public override PerformanceStats getStats()
        {
            return stats;
        }

        public override int[] PredictNextState(int[] state, int[] action)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<int[], double> PredictNextStates(int[] state, int[] action)
        {
            throw new NotImplementedException();
        }

        public override double PredictReward(int[] state, int[] action, int[] newState)
        {
            throw new NotImplementedException();
        }



        public override explorationMode getRecommendedExplorationMode()
        {
            return explorationMode.normal;
        }

    }
}
