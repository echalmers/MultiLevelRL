using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MultiResolutionRL.ValueCalculation
{
    [Serializable]
    public class ContextSwitchValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        Random rnd = new Random();

        IEqualityComparer<actionType> actionComparer;
        IEqualityComparer<stateType> stateComparer;
        List<actionType> availableActions;
        stateType startState;
        
        List<MultiResValue<stateType, actionType>> models = new List<MultiResValue<stateType, actionType>>();
        MultiResValue<stateType, actionType> currentModel;
        MultiResValue<stateType, actionType> candidateModel;

        Queue<StateTransition<stateType, actionType>> transitionHistory = new Queue<StateTransition<stateType, actionType>>();

        enum machineState {useCurrent, tryAdapt};
        machineState currentMachineState;

        PerformanceStats combinedStats = new PerformanceStats();

        double pThreshold = 0.05;
        double vThreshold = 0.0001;

        int layers = 8;
        int maxUpdates = 120;
        int priorCnts = 1;

        public ContextSwitchValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            if (parameters.Length > 0)
                layers = (int)parameters[0];
            if (parameters.Length > 1)
                maxUpdates = (int)parameters[1];

            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            availableActions = AvailableActions;
            startState = StartState;

            currentModel = newModel(15);
            models.Add(currentModel);

            currentMachineState = machineState.useCurrent;
        }

        public override PerformanceStats getStats()
        {
            combinedStats.cumulativeReward = 0;
            combinedStats.modelAccesses = 0;
            combinedStats.modelUpdates = 0;

            foreach (MultiResValue<stateType, actionType> m in models)
            {
                PerformanceStats thisStats = m.getStats();
                combinedStats.cumulativeReward += thisStats.cumulativeReward;
                combinedStats.modelAccesses += thisStats.modelAccesses;
                combinedStats.modelUpdates += thisStats.modelUpdates;
            }

            if (!models.Contains(currentModel))
            {
                PerformanceStats thisStats = currentModel.getStats();
                combinedStats.cumulativeReward += thisStats.cumulativeReward;
                combinedStats.modelAccesses += thisStats.modelAccesses;
                combinedStats.modelUpdates += thisStats.modelUpdates;
            }

            return combinedStats;
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
        
        public void resetHistory()
        {
            transitionHistory.Clear();
            currentModel = null;
        }

        public override double update(StateTransition<stateType, actionType> transition)
        {
            transitionHistory.Enqueue(transition);
            while (transitionHistory.Count() > 20)
                transitionHistory.Dequeue();
            

            switch (currentMachineState)
            {
                case machineState.useCurrent:

                    // switch to the model which best explains the recent transition history
                    double bestP = EventProbability(transitionHistory, currentModel, priorCnts);
                    MultiResValue < stateType, actionType > bestModel = currentModel;
                    foreach (MultiResValue<stateType, actionType> m in models)
                    {
                        if (m == currentModel)
                            continue;

                        double thisP = EventProbability(transitionHistory, m, priorCnts);
                        
                        if (thisP > (bestP + 0.05))
                        {
                            if (thisP >= pThreshold)
                            {
                                Console.WriteLine("Switching to previously learned model: " + models.IndexOf(m) + "(p = " + Math.Round(thisP, 2) + " vs " + Math.Round(bestP,2) + ")");
                                currentModel = m;
                            }
                            bestP = thisP;
                            bestModel = m;
                        }
                    }
                    //Console.WriteLine(bestP);

                    if (bestP < pThreshold) // if none explain it well
                    {
                        //// find the model with the best value from the current state
                        //double bestVal = currentModel.models[0].value((int[])((object)transition.newState), availableActions).Max();
                        //bestModel = currentModel;

                        //foreach (MultiResValue<stateType, actionType> m in models)
                        //{
                        //    double thisVal = m.models[0].value((int[])((object)transition.newState), availableActions).Max();
                        //    if (thisVal > bestVal)
                        //    {
                        //        bestVal = thisVal;
                        //        bestModel = m;
                        //    }
                        //}

                        if (layers > 1)
                        {
                            // copy the best model for adaptation
                            Console.WriteLine("Adapting model " + models.IndexOf(bestModel) + " (p = " + bestP);// + ", bestVal = " + bestVal + ")");
                            currentModel = copyModel(bestModel);
                            //models.Add(currentModel); //??????????????????? if not here then move to adaptation successful
                            candidateModel = newModel(0.001);
                            currentMachineState = machineState.tryAdapt;
                        }
                        else
                        {
                            currentModel = newModel(15);
                            models.Add(currentModel);
                            Console.WriteLine("Starting new model (p = " + bestP + ")");
                        }
                    }

                    break;

                case machineState.tryAdapt:
                    
                    // let the candidate model see the state transition
                    candidateModel.update((StateTransition<int[], actionType>)((object)transition));

                    // if goal has been found, assume model is adapted successfully
                    
                    if (transition.reward > 0)
                    {
                        currentMachineState = machineState.useCurrent;
                        currentModel = candidateModel;
                        models.Add(candidateModel);
                        //models.Add(currentModel);
                        Console.WriteLine("Adaptation successful");
                        break;
                    }

                    //// switch to the model which best explains the recent transition history
                    //bestP = EventProbability(transitionHistory, currentModel, 1);
                    //bestModel = currentModel;
                    //foreach (MultiResValue<stateType, actionType> m in models)
                    //{
                    //    if (m == currentModel)
                    //        continue;

                    //    double thisP = EventProbability(transitionHistory, m, 1);
                    //    if (thisP > (bestP + 0.05))
                    //    {
                    //        bestP = thisP;
                    //        bestModel = m;

                    //        if (thisP >= pThreshold)
                    //        {
                    //            Console.WriteLine("Adaptation aborted. Switching to previously learned model: " + models.IndexOf(m) + "(p = " + Math.Round(thisP, 2) + ")");
                    //            currentModel = m;
                    //            candidateModel = null;
                    //            currentMachineState = machineState.useCurrent;
                    //        }
                    //    }
                    //}

                    // if value gradient flattens, assume model cannot be adapted
                    double currentValue = currentModel.models[0].value((int[])((object)transition.newState), availableActions).Max();
                    if (currentValue < vThreshold)
                    {
                        //currentModel = newModel(15);
                        //models.Add(currentModel);
                        //candidateModel = null;

                        currentModel = candidateModel;
                        candidateModel = null;
                        models.Add(currentModel);
                        currentModel.models[0].defaultQ = 15;

                        currentMachineState = machineState.useCurrent;
                        Console.WriteLine("Adaptation failed. Starting new model");

                        //// switch to the model which best explains the recent transition history
                        //bestP = 0;
                        //bestModel = currentModel;
                        //foreach (MultiResValue<stateType, actionType> m in models)
                        //{
                        //    if (m == currentModel)
                        //        continue;

                        //    double thisP = EventProbability(transitionHistory, m, priorCnts);
                        //    Console.WriteLine(thisP);
                        //    if (thisP > (bestP + 0.05))
                        //    {
                        //        bestP = thisP;
                        //        bestModel = m;

                        //        if (thisP >= pThreshold)
                        //        {
                        //            Console.WriteLine("Switching to previously learned model: " + models.IndexOf(m) + "(p = " + Math.Round(thisP, 2) + ")");
                        //            currentModel = m;
                        //        }
                        //    }
                        //}

                        //if (bestP < pThreshold) // if none explain it well
                        //{
                        //    currentModel = newModel(15);
                        //    models.Add(currentModel);
                        //    Console.WriteLine("Starting new model (p = " + bestP + ")");
                        //}
                        //currentMachineState = machineState.useCurrent;
                    }
                    break;
            }

            
            currentModel.update((StateTransition<int[], actionType>)((object)transition));
            
            return 0;
        }

        public override explorationMode getRecommendedExplorationMode()
        {
            if (currentMachineState == machineState.tryAdapt)
                return explorationMode.suspendExploration;
            else
                return explorationMode.normal;
        }

        public override double[] value(stateType state, List<actionType> actions)
        {
            if (currentModel == null)
            {
                double bestValue = double.NegativeInfinity;
                foreach(MultiResValue<stateType, actionType> m in models)
                {
                    double thisValue = m.models[0].value((int[])(object)state, actions).Max();
                    if (thisValue > bestValue)
                    {
                        bestValue = thisValue;
                        currentModel = m;
                    }
                }
                Console.WriteLine("starting with model " + models.IndexOf(currentModel));
            }

            if (currentMachineState == machineState.tryAdapt)
            { 
                try
                {
                    return currentModel.value((int[])((object)state), actions);
                }
                catch (ApplicationException ex)
                {
                    //candidateModel = null;
                    //currentModel = newModel(15);
                    //models.Add(currentModel);

                    currentModel = candidateModel;
                    candidateModel = null;
                    models.Add(currentModel);
                    currentModel.models[0].defaultQ = 15;

                    currentMachineState = machineState.useCurrent;
                    Console.WriteLine("Starting new model");
                    return currentModel.value((int[])((object)state), actions);
                }
            }
            else if (currentMachineState == machineState.useCurrent)
            {
                return currentModel.models[0].value((int[])((object)state), actions);
            }

            return null;

                //    // switch to the model which best explains the recent transition history
                //    double bestP = 0;
                //    MultiResValue<stateType, actionType> bestModel = currentModel;
                //    foreach (MultiResValue<stateType, actionType> m in models)
                //    {
                //        if (m == currentModel)
                //            continue;

                //        double thisP = EventProbability(transitionHistory, m, priorCnts);
                //Console.WriteLine(thisP);
                //if (thisP > (bestP + 0.05))
                //        {
                //            bestP = thisP;
                //            bestModel = m;

                //            if (thisP >= pThreshold)
                //            {
                //                Console.WriteLine("Switching to previously learned model: " + models.IndexOf(m) + "(p = " + Math.Round(thisP, 2) + ")");
                //                currentModel = m;
                //            }
                //        }
                //    }

                //    if (bestP < pThreshold) // if none explain it well
                //    {
                //        currentModel = newModel(15);
                //        models.Add(currentModel);
                //        Console.WriteLine("Starting new model (p = " + bestP + ")");
                //    }
                //    currentMachineState = machineState.useCurrent;
                //    return currentModel.value((int[])((object)state), actions);
            //}
            //        break;
            //}

            //return null;
        }

        MultiResValue<stateType, actionType> newModel(double defaultQ)
        {
            MultiResValue<stateType, actionType> newmodel = new MultiResValue<stateType, actionType>((IEqualityComparer<int[]>)stateComparer, actionComparer, availableActions, (int[])((object)startState), layers);
            foreach (ModelBasedValue<int[], actionType> m in newmodel.models)
            {
                m.maxUpdates = maxUpdates;
            }
            newmodel.models[0].defaultQ = defaultQ;

            //foreach(StateTransition<stateType, actionType> t in transitionHistory)
            //{
            //    newmodel.update((StateTransition<int[], actionType>)(object)t);
            //}

            return newmodel;
        }

        double EventProbability(IEnumerable<StateTransition<stateType, actionType>> transitions, MultiResValue<stateType, actionType> model, int priorCnt)
        {
            double p = 1;
            foreach (StateTransition<stateType, actionType> transition in transitions)
            {
                Dictionary<int[], int> s2Counts = model.models[0].T.GetStateValueTable((int[])((object)transition.oldState), transition.action);
                double thisS2Counts = priorCnt;
                if (s2Counts.ContainsKey((int[])((object)transition.newState)))
                    thisS2Counts = (double)s2Counts[(int[])((object)transition.newState)] + priorCnt;
                double total = (double)s2Counts.Values.Sum() + priorCnt;
                
                p *= (thisS2Counts / total);

                p *= model.models[0].R.Get((int[])((object)transition.oldState), transition.action, (int[])((object)transition.newState)).P(transition.reward, priorCnt);
            }
            return p;
        }

        MultiResValue<stateType, actionType> copyModel(MultiResValue<stateType, actionType> toCopy)
        {
            string filename = rnd.Next().ToString() + ".bin";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, toCopy);
            stream.Close();

            formatter = new BinaryFormatter();
            stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            MultiResValue < stateType, actionType > copied = (MultiResValue<stateType, actionType>)formatter.Deserialize(stream);
            stream.Close();

            copied.ResetStats();

            return copied;
        }
    }
}
