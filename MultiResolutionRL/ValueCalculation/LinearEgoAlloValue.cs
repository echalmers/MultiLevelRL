using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class LinearEgoAlloValue<stateType, actionType> : ActionValue<int[], int[]>
    {
        int steps = 0;
        bool fullPredictionMode = false;
        

        ActionValue<int[], int[]> alloLearner;
        ModelFreeValue<int[], int[]> egoLearner;
        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        List<int[]> availableActions;
        DoubleArrayComparer doubleArrayComparer = new DoubleArrayComparer();
        PerformanceStats stats = new PerformanceStats();

        ModelBasedValue<int[],int[]>[] egoPredictionModels;
        Dictionary<int[],int>[] visitedStates;

        int updateTerminationStepCount = 1;

        Random rnd = new Random();


        public LinearEgoAlloValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            if (parameters.Length > 0)
                fullPredictionMode = (bool)parameters[0];
            if (parameters.Length > 1)
                updateTerminationStepCount = (int)parameters[1];
            if (parameters.Length > 2)
            {
                if ((bool)parameters[2])
                    alloLearner = new ModelBasedValue<int[], int[]>(StateComparer, ActionComparer, AvailableActions, null, true);
                else
                    alloLearner = new ModelFreeValue<int[], int[]>(StateComparer, ActionComparer, AvailableActions, null, true);
            }
                

            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            //alloModel = new ModelFreeValue<int[], int[]>(StateComparer, ActionComparer, availableActions, null, true);
            //{
            //    defaultQ = 10.3
            //};
            egoLearner = new ModelFreeValue<int[], int[]>(StateComparer, actionComparer, availableActions, StartState)
            {
                alpha = 0.9
            };

            egoPredictionModels = new ModelBasedValue<int[], int[]>[3];
            for (int i = 0; i < egoPredictionModels.Length; i++)
            {
                egoPredictionModels[i] = new ModelBasedValue<int[], int[]>(StateComparer, ActionComparer, availableActions, StartState)
                {
                    gamma = 0
                };
            }

            visitedStates = new Dictionary<int[],int>[4];
            for (int i=0; i<availableActions.Count; i++)
            {
                visitedStates[i] = new Dictionary<int[], int>(StateComparer);
            }
        }

        public void ResetAllocentric(bool useModelBased)
        {
            visitedStates = new Dictionary<int[], int>[4];
            for (int i = 0; i < availableActions.Count; i++)
            {
                visitedStates[i] = new Dictionary<int[], int>(stateComparer);
            }

            if (useModelBased)
                alloLearner = new ModelBasedValue<int[], int[]>(stateComparer, actionComparer, availableActions, null, true);
            else
                alloLearner = new ModelFreeValue<int[], int[]>(stateComparer, actionComparer, availableActions, null, true);

            stats.cumulativeReward = 0;
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            return alloLearner.value(new int[2] { state[0], state[1] }, actions);
        }

        private void handCodedPrediction(int[] oldEgoState, int[] action, out double reward, int[] oldAlloState, out int[] newAlloState, double noise)
        {
            Console.WriteLine("hand coded prediction with noise = " + noise);

            // generate a random prediction at the rate specified by the noise parameter
            if (rnd.NextDouble() < noise)
            {
                Console.WriteLine("false prediction");
                newAlloState = new int[2] { oldAlloState[0] + rnd.Next(-1, 1), oldAlloState[1] + rnd.Next(-1, 1) };
                reward = rnd.NextDouble() * 0.1 - 0.1;
            }
            else // otherwise generate a hand-coded (correct) prediction
            {
                // case for navigation into a wall
                if ((action[0] == -1 && oldEgoState[0] == 1) || (action[0] == 1 && oldEgoState[2] == 1) || (action[1] == -1 && oldEgoState[1] == 1) || (action[1] == 1 && oldEgoState[3] == 1))
                {
                    reward = -0.1;
                    newAlloState = oldAlloState;
                }
                // case for navigation into a goal state
                else if ((action[0] == -1 && oldEgoState[4] == 1) || (action[0] == 1 && oldEgoState[6] == 1) || (action[1] == -1 && oldEgoState[5] == 1) || (action[1] == 1 && oldEgoState[7] == 1))
                {
                    reward = 10;
                    newAlloState = oldAlloState;
                }
                // case for navigation into lava
                else if ((action[0] == -1 && oldEgoState[8] == 1) || (action[0] == 1 && oldEgoState[10] == 1) || (action[1] == -1 && oldEgoState[9] == 1) || (action[1] == 1 && oldEgoState[11] == 1))
                {
                    reward = -1;
                    newAlloState = new int[2] { oldAlloState[0] + action[0], oldAlloState[1] + action[1] };
                }
                // case for navigation through open areas
                else
                {
                    reward = -0.01;
                    newAlloState = new int[2] { oldAlloState[0] + action[0], oldAlloState[1] + action[1] };
                }
            }
        }

        //private bool inSample(double[] sa, out double[] matchingSample)
        //{
        //    matchingSample = null;

        //    if (saHistory.Count == 0)
        //        return false;

        //    foreach (double[] sample in saHistory)
        //    {
        //        bool thisSampleMatches = true;
        //        for (int i=0; i<10; i++)
        //        {
        //            if (sa[i] != sample[i])
        //            {
        //                thisSampleMatches = false;
        //                break;
        //            }
        //        }
        //        if (thisSampleMatches)
        //        {
        //            matchingSample = sample;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public override double update(StateTransition<int[], int[]> transition)
        {
            steps++;
            stats.cumulativeReward += transition.reward;

            int[] alloOldState = new int[2] { transition.oldState[0], transition.oldState[1] };
            int[] alloNewState = new int[2] { transition.newState[0], transition.newState[1] };
            int[] egoOldState = new int[12];
            Array.Copy(transition.oldState, 2, egoOldState, 0, 12);
            int[] egoNewState = new int[12];
            Array.Copy(transition.newState, 2, egoNewState, 0, 12);


            //double[] sa = new double[10];
            //Array.Copy(egoOldState, sa, 8);
            //sa[8] = transition.action[0];
            //sa[9] = transition.action[1];
            //Console.WriteLine("sa: " + string.Join(",", sa));
            //Console.WriteLine("sprime: " + alloNewState[0] + "," + alloNewState[1]);


            // train ego prediction models
            egoPredictionModels[0].update(new StateTransition<int[], int[]>(egoOldState, transition.action, transition.newState[0] - transition.oldState[0], new int[1] { -1 }));
            egoPredictionModels[1].update(new StateTransition<int[], int[]>(egoOldState, transition.action, transition.newState[1] - transition.oldState[1], new int[1] { -1 }));
            egoPredictionModels[2].update(new StateTransition<int[], int[]>(egoOldState, transition.action, transition.reward, new int[1] { -1 }));

            
            // update models with the current transition
            alloLearner.update(new StateTransition<int[], int[]>(alloOldState, transition.action, transition.reward, alloNewState));
            egoLearner.update(new StateTransition<int[], int[]>(Array.ConvertAll(egoOldState, x => (int)x), transition.action, transition.reward, Array.ConvertAll(egoNewState, x => (int)x)));

            // transfer info from ego to allo models
            Console.WriteLine("current state: " + alloNewState[0] + "," + alloNewState[1]);
            //Console.WriteLine("ego. state: " + string.Join(",", egoNewState));


            for (int i = 0; i < availableActions.Count; i++)
            {
                if (!visitedStates[i].ContainsKey(alloNewState))
                    visitedStates[i].Add(alloNewState, 1);
                else
                    visitedStates[i][alloNewState]++;

                if (steps >= 10 && visitedStates[i][alloNewState] <= updateTerminationStepCount)
                {
                    double predictedReward = egoPredictionModels[2].value(egoNewState, availableActions[i]);
                    double d0 = egoPredictionModels[0].value(egoNewState, availableActions[i]);
                    double d1 = egoPredictionModels[1].value(egoNewState, availableActions[i]);
                    int[] predictedAlo = { (int)Math.Round(d0 + alloNewState[0]), (int)Math.Round(d1 + alloNewState[1]) };
                    

                    //handCodedPrediction(Array.ConvertAll(egoNewState, x => (int)x), availableActions[i], out predictedReward, alloNewState, out predictedAlo, 0.01);

                    Console.WriteLine("action " + availableActions[i][0] + "," + availableActions[i][1] + " -> " + predictedAlo[0] + "," + predictedAlo[1] + " reward: " + predictedReward);

                    //double[] matchingSample;
                    //if (inSample(sa, out matchingSample))
                    //{
                    //if (alloModel.value(alloNewState, availableActions[i]) == alloModel.defaultQ)
                    //{
                    if (fullPredictionMode && egoPredictionModels[0].T.GetStateValueTable(egoNewState, availableActions[i]).Values.Sum()>1)
                    {
                        alloLearner.update(new StateTransition<int[], int[]>(alloNewState, availableActions[i], predictedReward, predictedAlo));
                    }
                    else if (!fullPredictionMode)
                    {
                        double setQvalue = egoLearner.value(Array.ConvertAll(egoNewState, x => (int)x), availableActions[i]);
                        alloLearner.Qtable[alloNewState][availableActions[i]] = setQvalue;
                    }
                    //}
                    //}
                }
            }


            return 0;
        }

        public override PerformanceStats getStats()
        {
            return stats;
        }

        public override explorationMode getRecommendedExplorationMode()
        {
            return explorationMode.normal;
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
    }
}
