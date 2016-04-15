using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Neuro.Networks;

using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.Learning;
using AForge;
//using AForge.Controls;
using AForge.Neuro;

namespace MultiResolutionRL.ValueCalculation
{
    public class EgoAlloValue<stateType, actionType> : ActionValue<int[], int[]>
    {
        int errors = 0, correct = 0;
        bool fullPredictionMode = false;
        
        ModelFreeValue<int[], int[]> alloModel;
        ModelFreeValue<int[], int[]> egoModel;
        IEqualityComparer<int[]> actionComparer;
        IEqualityComparer<int[]> stateComparer;
        List<int[]> availableActions;
        DoubleArrayComparer doubleArrayComparer = new DoubleArrayComparer();
        PerformanceStats stats = new PerformanceStats();

        Queue<double[]> saHistory = new Queue<double[]>();
        Queue<double[]> sPrimeHistory = new Queue<double[]>();
        
        ActivationNetwork network;
        ParallelResilientBackpropagationLearning teacher;


        public EgoAlloValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
           : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            if (parameters.Length > 0)
                fullPredictionMode = (bool)parameters[0];

            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;
            
            alloModel = new ModelFreeValue<int[], int[]>(StateComparer, ActionComparer, availableActions, StartState, true)
            {
                defaultQ = 10.3
            };
            egoModel = new ModelFreeValue<int[], int[]>(StateComparer, actionComparer, availableActions, StartState)
            {
                alpha = 0.9
            };

            network = new ActivationNetwork(new BipolarSigmoidFunction(2), 10, 10, 3);
            teacher = new ParallelResilientBackpropagationLearning(network);
        }

        public override double[] value(int[] state, List<int[]> actions)
        {
            return alloModel.value(new int[2] { state[0], state[1] }, actions);
        }

        private void handCodedPrediction(int[] oldEgoState, int[] action, out double reward, int[] oldAlloState, out int[] newAlloState)
        {
            if ((action[0] == -1 && oldEgoState[1] == 1) || (action[0] == 1 && oldEgoState[6] == 1) || (action[1] == -1 && oldEgoState[3] == 1) || (action[1] == 1 && oldEgoState[4] == 1))
            {
                reward = -0.1;
                newAlloState = oldAlloState;
            }
            else
            {
                reward = -0.01;
                newAlloState = new int[2] { oldAlloState[0] + action[0], oldAlloState[1] + action[1] };
            }
        }

        private bool inSample(double[] sa, out double[] matchingSample)
        {
            matchingSample = null;

            if (saHistory.Count == 0)
                return false;
            
            foreach (double[] sample in saHistory)
            {
                bool thisSampleMatches = true;
                for (int i=0; i<10; i++)
                {
                    if (sa[i] != sample[i])
                    {
                        thisSampleMatches = false;
                        break;
                    }
                }
                if (thisSampleMatches)
                {
                    matchingSample = sample;
                    return true;
                }
            }
            return false;
        }

        public override double update(StateTransition<int[], int[]> transition)
        {
            stats.cumulativeReward += transition.reward;

            int[] alloOldState = new int[2] { transition.oldState[0], transition.oldState[1] };
            int[] alloNewState = new int[2] { transition.newState[0], transition.newState[1] };
            int[] egoOldState = new int[8];
            Array.Copy(transition.oldState, 2, egoOldState, 0, 8);
            int[] egoNewState = new int[8];
            Array.Copy(transition.newState, 2, egoNewState, 0, 8);

            // load the transition into the history
            if (saHistory.Count > 500)
            {
                saHistory.Dequeue();
                sPrimeHistory.Dequeue();
            }
            double[] sa = new double[10];
            Array.Copy(egoOldState, sa, 8);
            sa[8] = transition.action[0];
            sa[9] = transition.action[1];
            Console.WriteLine("sa: " + string.Join(",", sa));
            Console.WriteLine("sprime: " + alloNewState[0] + "," + alloNewState[1]);

            //double[] dummy;
            //if (!inSample(sa, out dummy))
            //{
                saHistory.Enqueue(sa);
                sPrimeHistory.Enqueue(new double[3] { alloNewState[0] - alloOldState[0], alloNewState[1] - alloOldState[1], transition.reward});
            //}

            // run regression
            if (saHistory.Count > 50 && fullPredictionMode)
            {
                double error;
                for (int epoch = 1; epoch < 2; epoch++)
                {
                    error = teacher.RunEpoch(saHistory.ToArray(), sPrimeHistory.ToArray()) / saHistory.Count;
                }
            }
            
            // update models with the current transition
            alloModel.update(new StateTransition<int[], int[]>(alloOldState, transition.action, transition.reward, alloNewState));
            egoModel.update(new StateTransition<int[], int[]>(egoOldState, transition.action, transition.reward, egoNewState));

            // transfer info from ego to allo models
            Console.WriteLine("current state: " + alloNewState[0] + "," + alloNewState[1]);
            Console.WriteLine("ego. state: " + string.Join(",", egoNewState));

            foreach (int[] a in availableActions)
            {
                sa = new double[10];
                Array.Copy(egoNewState, sa, 8);
                sa[8] = a[0];
                sa[9] = a[1];
                double[] predicted = network.Compute(sa);// linearModel.Compute(sa);
                int[] predictedAlo = { (int)Math.Round(predicted[0]) + alloNewState[0], (int)Math.Round(predicted[1]) + alloNewState[1] };
                double reward = predicted[2];
                
                double handCodedReward; int[] handCodedPredictedAlo;
                handCodedPrediction(egoNewState, a, out handCodedReward, alloNewState, out handCodedPredictedAlo);

                Console.WriteLine("action " + a[0] + "," + a[1] + " -> " + predictedAlo[0] + "," + predictedAlo[1] + " reward: " + reward);

                if (saHistory.Count >= 50)
                {
                    double[] matchingSample;
                    //if (inSample(sa, out matchingSample))
                    //{
                    if (alloModel.value(alloNewState, a) == alloModel.defaultQ)
                    {
                        if (fullPredictionMode)
                            alloModel.update(new StateTransition<int[], int[]>(alloNewState, a, reward, predictedAlo));
                        else
                            alloModel.Qtable[alloNewState][a] = egoModel.value(egoNewState, a);
                    }
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
