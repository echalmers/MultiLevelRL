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
        IEqualityComparer<actionType> actionComparer;
        IEqualityComparer<stateType> stateComparer;
        List<actionType> availableActions;
        stateType startState;
        
        List<MultiResValue<stateType, actionType>> models = new List<MultiResValue<stateType, actionType>>();
        MultiResValue<stateType, actionType> currentModel;

        Queue<StateTransition<stateType, actionType>> transitionHistory = new Queue<StateTransition<stateType, actionType>>();

        enum machineState {useCurrent, tryAdapt};
        machineState currentMachineState;

        PerformanceStats combinedStats = new PerformanceStats();

        double pThreshold = 0.2;
        double vThreshold = 0.0001;

        int layers = 8;
        int maxUpdates = 100;

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

            models.Add(new MultiResValue<stateType, actionType>((IEqualityComparer<int[]>)StateComparer, ActionComparer, availableActions, (int[])((object)StartState), layers));
            foreach (ModelBasedValue<int[], actionType> m in models.Last().models)
            {
                m.maxUpdates = maxUpdates;
            }
            //models.Add(new ModelBasedValue<stateType, actionType>(StateComparer, ActionComparer, availableActions, StartState, parameters));

            currentModel = models[0];

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

        public override double update(StateTransition<stateType, actionType> transition)
        {
            transitionHistory.Enqueue(transition);
            while (transitionHistory.Count() > 20)
                transitionHistory.Dequeue();
            

            switch (currentMachineState)
            {
                case machineState.useCurrent:
                    
                    // switch to the model which best explains the recent transition history
                    double bestP = EventProbability(transitionHistory, currentModel, 5);
                    MultiResValue < stateType, actionType > bestModel = currentModel;
                    foreach (MultiResValue<stateType, actionType> m in models)
                    {
                        if (m == currentModel)
                            continue;

                        double thisP = EventProbability(transitionHistory, m, 5);
                        if (thisP > (bestP + 0.05))
                        {
                            bestP = thisP;
                            bestModel = m;

                            if (thisP >= pThreshold)
                            {
                                Console.WriteLine("Switching to previously learned model: " + models.IndexOf(m) + "(p = " + Math.Round(thisP, 2) + ")");
                                currentModel = m;
                            }
                        }
                    }
                    //Console.WriteLine(bestP);

                    if (bestP < pThreshold) // if none explain it well
                    {
                        //// find the model with the best value from the current state
                        //double bestVal = currentModel.models[0].value((int[])((object)transition.newState), availableActions).Max();
                        //MultiResValue<stateType, actionType> bestModel = currentModel;

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
                            models.Add(currentModel); //??????????????????? if not here then move to adaptation successful
                            currentMachineState = machineState.tryAdapt;
                        }
                        else
                        {
                            currentModel = new MultiResValue<stateType, actionType>((IEqualityComparer<int[]>)stateComparer, actionComparer, availableActions, (int[])((object)startState), layers);
                            foreach (ModelBasedValue<int[], actionType> m in models.Last().models)
                            {
                                m.maxUpdates = maxUpdates;
                            }
                            models.Add(currentModel);
                            Console.WriteLine("Starting new model (p = " + bestP + ")");
                        }
                    }

                    break;

                case machineState.tryAdapt:
                    
                    // if goal has been found, assume model is adapted successfully
                    if (transition.reward > 0)
                    {
                        currentMachineState = machineState.useCurrent;
                        Console.WriteLine("Adaptation successful");
                    }

                    // switch to the model which best explains the recent transition history
                    bestP = EventProbability(transitionHistory, currentModel, 1);
                    bestModel = currentModel;
                    foreach (MultiResValue<stateType, actionType> m in models)
                    {
                        if (m == currentModel)
                            continue;

                        double thisP = EventProbability(transitionHistory, m, 1);
                        if (thisP > (bestP + 0.05))
                        {
                            bestP = thisP;
                            bestModel = m;

                            if (thisP >= pThreshold)
                            {
                                Console.WriteLine("Adaptation aborted. Switching to previously learned model: " + models.IndexOf(m) + "(p = " + Math.Round(thisP, 2) + ")");
                                currentModel = m;
                                currentMachineState = machineState.useCurrent;
                            }
                        }
                    }

                    // if value gradient flattens, assume model cannot be adapted
                    double currentValue = currentModel.models[0].value((int[])((object)transition.newState), availableActions).Max();
                    if (currentValue < vThreshold)
                    {
                        currentModel = new MultiResValue<stateType, actionType>((IEqualityComparer<int[]>)stateComparer, actionComparer, availableActions, (int[])((object)startState), layers);
                        foreach (ModelBasedValue<int[], actionType> m in models.Last().models)
                        {
                            m.maxUpdates = maxUpdates;
                        }
                        models.Add(currentModel);
                        currentMachineState = machineState.useCurrent;
                        Console.WriteLine("Adaptation failed. Starting new model");
                    }
                    break;
            }

            
            currentModel.update((StateTransition<int[], actionType>)((object)transition));
            
            return 0;
        }

        public override double[] value(stateType state, List<actionType> actions)
        {
            //switch (currentMachineState)
            //{
            //    case machineState.useCurrent:
            //        return currentModel.models[0].value((int[])((object)state), actions);
            //        break;

            //    case machineState.tryAdapt:
                    try
                    {
                        return currentModel.value((int[])((object)state), actions);
                    }
                    catch (ApplicationException ex)
                    {
                        currentModel = new MultiResValue<stateType, actionType>((IEqualityComparer<int[]>)stateComparer, actionComparer, availableActions, (int[])((object)startState), layers);
                        foreach (ModelBasedValue<int[], actionType> m in models.Last().models)
                        {
                            m.maxUpdates = maxUpdates;
                        }
                        currentMachineState = machineState.useCurrent;
                        Console.WriteLine("Adaptation failed. Starting new model");
                        return currentModel.value((int[])((object)state), actions);
                    }
            //        break;
            //}

            //return null;
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
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("temp.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, toCopy);
            stream.Close();

            formatter = new BinaryFormatter();
            stream = new FileStream("temp.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            MultiResValue < stateType, actionType > copied = (MultiResValue<stateType, actionType>)formatter.Deserialize(stream);
            stream.Close();

            copied.ResetStats();

            return copied;
        }
    }
}
