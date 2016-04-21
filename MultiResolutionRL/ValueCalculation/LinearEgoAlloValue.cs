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
        

        ActionValue<int[], int[]> alloModel;
        ModelFreeValue<int[], int[]> egoModel;
        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        List<int[]> availableActions;
        DoubleArrayComparer doubleArrayComparer = new DoubleArrayComparer();
        PerformanceStats stats = new PerformanceStats();

        OnlineLinearRegression[][] linearModels;
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
                    alloModel = new ModelBasedValue<int[], int[]>(StateComparer, ActionComparer, availableActions, null, true);
                else
                    alloModel = new ModelFreeValue<int[], int[]>(StateComparer, ActionComparer, availableActions, null, true);
            }
                

            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;

            alloModel = new ModelFreeValue<int[], int[]>(StateComparer, ActionComparer, availableActions, null, true);
            //{
            //    defaultQ = 10.3
            //};
            egoModel = new ModelFreeValue<int[], int[]>(StateComparer, actionComparer, availableActions, StartState)
            {
                alpha = 0.9
            };

            linearModels = new OnlineLinearRegression[availableActions.Count][];
            for (int i = 0; i < linearModels.Length; i++)
            {
                linearModels[i] = new OnlineLinearRegression[3];
                for (int j = 0; j < 3; j++)
                {
                    linearModels[i][j] = new OnlineLinearRegression(12, 0.5, true);
                }
            }

            visitedStates = new Dictionary<int[],int>[4];
            for (int i=0; i<availableActions.Count; i++)
            {
                visitedStates[i] = new Dictionary<int[], int>(StateComparer);
            }
        }

        public void ResetAllocentric(bool useModelBased)
        {
            if (useModelBased)
                alloModel = new ModelBasedValue<int[], int[]>(stateComparer, actionComparer, availableActions, null, true);
            else
                alloModel = new ModelFreeValue<int[], int[]>(stateComparer, actionComparer, availableActions, null, true);

            stats.cumulativeReward = 0;
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            return alloModel.value(new int[2] { state[0], state[1] }, actions);
        }

        private void handCodedPrediction(int[] oldEgoState, int[] action, out double reward, int[] oldAlloState, out int[] newAlloState, double noise)
        {
            if (rnd.Next() < noise)
            {
                newAlloState = oldAlloState;
                reward = rnd.Next() * 0.1 - 0.1;
            }
            else
            {
                if ((action[0] == -1 && oldEgoState[0] == 1) || (action[0] == 1 && oldEgoState[2] == 1) || (action[1] == -1 && oldEgoState[1] == 1) || (action[1] == 1 && oldEgoState[3] == 1))
                {
                    reward = -0.1;
                    newAlloState = oldAlloState;
                }
                else if ((action[0] == -1 && oldEgoState[4] == 1) || (action[0] == 1 && oldEgoState[6] == 1) || (action[1] == -1 && oldEgoState[5] == 1) || (action[1] == 1 && oldEgoState[7] == 1))
                {
                    reward = 10;
                    newAlloState = oldAlloState;
                }
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
            double[] egoOldState = new double[12];
            Array.Copy(transition.oldState, 2, egoOldState, 0, 12);
            double[] egoNewState = new double[12];
            Array.Copy(transition.newState, 2, egoNewState, 0, 12);


            //double[] sa = new double[10];
            //Array.Copy(egoOldState, sa, 8);
            //sa[8] = transition.action[0];
            //sa[9] = transition.action[1];
            //Console.WriteLine("sa: " + string.Join(",", sa));
            //Console.WriteLine("sprime: " + alloNewState[0] + "," + alloNewState[1]);


            // run regression
            int actionIndex = availableActions.FindIndex((a) => actionComparer.Equals(a, transition.action));
            linearModels[actionIndex][0].Train(egoOldState, transition.reward);
            if (transition.absorbingStateReached == false)
            {
                linearModels[actionIndex][1].Train(egoOldState, transition.newState[0] - transition.oldState[0]);
                linearModels[actionIndex][2].Train(egoOldState, transition.newState[1] - transition.oldState[1]);
            }


            // update models with the current transition
            alloModel.update(new StateTransition<int[], int[]>(alloOldState, transition.action, transition.reward, alloNewState));
            egoModel.update(new StateTransition<int[], int[]>(Array.ConvertAll(egoOldState, x => (int)x), transition.action, transition.reward, Array.ConvertAll(egoNewState, x => (int)x)));

            // transfer info from ego to allo models
            //Console.WriteLine("current state: " + alloNewState[0] + "," + alloNewState[1]);
            //Console.WriteLine("ego. state: " + string.Join(",", egoNewState));
            

            for (int i = 0; i < availableActions.Count; i++)
            {
                if (!visitedStates[i].ContainsKey(alloNewState))
                    visitedStates[i].Add(alloNewState, 1);
                else
                    visitedStates[i][alloNewState]++;

                if (steps >= 10 && visitedStates[i][alloNewState] <= updateTerminationStepCount)
                {
                    //sa = new double[10];
                    //Array.Copy(egoNewState, sa, 8);
                    //sa[8] = availableActions[i][0];
                    //sa[9] = availableActions[i][1];
                    //double[] predicted = network.Compute(sa);// linearModel.Compute(sa);
                    double reward = linearModels[i][0].Compute(egoNewState);
                    double d0 = linearModels[i][1].Compute(egoNewState);
                    double d1 = linearModels[i][2].Compute(egoNewState);
                    int[] predictedAlo = { (int)Math.Round(d0 + alloNewState[0]), (int)Math.Round(d1 + alloNewState[1]) };

                    handCodedPrediction(Array.ConvertAll(egoNewState, x => (int)x), availableActions[i], out reward, alloNewState, out predictedAlo, 0.05);

                    //Console.WriteLine("action " + availableActions[i][0] + "," + availableActions[i][1] + " -> " + predictedAlo[0] + "," + predictedAlo[1] + " reward: " + reward);

                    //double[] matchingSample;
                    //if (inSample(sa, out matchingSample))
                    //{
                    //if (alloModel.value(alloNewState, availableActions[i]) == alloModel.defaultQ)
                    //{
                    if (fullPredictionMode)
                        alloModel.update(new StateTransition<int[], int[]>(alloNewState, availableActions[i], reward, predictedAlo));
                    else
                    {
                        double setQvalue = egoModel.value(Array.ConvertAll(egoNewState, x => (int)x), availableActions[i]);
                        alloModel.Qtable[alloNewState][availableActions[i]] = setQvalue;
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
