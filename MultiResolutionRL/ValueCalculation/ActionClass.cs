using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    //Is used for splitting up the multiple factors that can come from the  stateclass, then create a appropriate learner
    //that used each descriptor appropriately
    public class ActionClass<stateType, actionType> : ActionValue<stateType, actionType>
    {

        ////**** EXPERIMENTAL ****//
        int maxParameter = 1;
        int[] prediction = { 0, 0 };
        Approximator<double[], actionType> Approxim;
       
        //MEMBERS//
        ActionValue<int[], actionType> egoModel = null;
      public  ModelBasedValue<int[], actionType> aloModel = null;
        Type egoType = null;
        Type aloType = null;
        List<actionType> availActs;

        //Needs to create a model for each form of data in stateClass
        public ActionClass(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters)
             : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            availActs = AvailableActions;



            ////**** EXPERIMENTAL ****//
            Approxim = new Approximator<double[], actionType>(availActs,1000,100);


            if (typeof(stateType) == typeof(StateClass))
            {


                StateClass holder = (StateClass)parameters[0];
                parameters = null;
                aloType = (holder.GetStateFactor("GlobalLocation")).GetType();
                object aloStartState = holder.GetStateFactor("GlobalLocation");
                Type modelType;

                modelType = typeof(ModelBasedValue<int[], actionType>);
                int[] start = (int[])Convert.ChangeType(aloStartState, typeof(int[]));
                aloModel = (ModelBasedValue<int[], actionType>)Activator.CreateInstance(modelType, Comparer.IAC, ActionComparer, AvailableActions, start, parameters);


                object egoStartState = holder.GetStateFactor("Ego");
                egoType = (holder.GetStateFactor("Ego")).GetType();
                // modelType = typeof(ModelFreeValue<int[], int[]>);
                modelType = typeof(ModelFreeValue<int[], int[]>);
                start = (int[])Convert.ChangeType(egoStartState, typeof(int[]));
                egoModel = (ActionValue<int[], actionType>)Activator.CreateInstance(modelType, Comparer.IAC, ActionComparer, AvailableActions, start, parameters);



            }

        }


        //Have both ego get updated to its new state, and have the alo updated from the transition that occured in the world.
        public override double update(StateTransition<stateType, actionType> transition)
        {

            StateClass newState = (StateClass)Convert.ChangeType(transition.newState, typeof(StateClass));
            StateClass oldState = (StateClass)Convert.ChangeType(transition.oldState, typeof(StateClass));
            int[] oldHolderStateE;
            int[] newHolderStateE;
            int[] oldHolderStateA;
            int[] newHolderStateA;
            actionType act = transition.action;
            double reward = transition.reward;

            string a = "GlobalLocation";
            oldHolderStateA = (int[])oldState.GetStateFactor(a);
            newHolderStateA = (int[])newState.GetStateFactor(a);
            StateTransition<int[], actionType> aloTrans = new StateTransition<int[], actionType>(oldHolderStateA, act, reward, newHolderStateA);
            aloModel.update(aloTrans);


            string e = "Ego";
            oldHolderStateE = (int[])oldState.GetStateFactor(e);
            newHolderStateE = (int[])newState.GetStateFactor(e);
            StateTransition<int[], actionType> egoTrans = new StateTransition<int[], actionType>(oldHolderStateE, act, reward, newHolderStateE);
            egoModel.update(egoTrans);






            ////**** EXPERIMENTAL ****//    

            if (!Approxim.batchDone[act])
                {
                    ///* OldX, oldY, Old Ego[9], new X, new Y, constant*/
                    double[] TrainingTotalState = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

                    TrainingTotalState[0] = oldHolderStateA[0]; TrainingTotalState[1] = oldHolderStateA[1];
                    TrainingTotalState[11] = newHolderStateA[0]; TrainingTotalState[12] = newHolderStateA[1];

                    for (int i = 0; i < 9; i++)
                        TrainingTotalState[i + 2] = oldHolderStateE[i];

                    foreach (int i in TrainingTotalState)
                        if (i > maxParameter)
                        {
                            maxParameter = i;
                            Approxim.UpdateMaxParam(maxParameter);
                        }

                    Approxim.TrainLinRegBatch(TrainingTotalState, act);
                }

                double[] newTotalState = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                newTotalState[0] = newHolderStateA[0]; newTotalState[1] = newHolderStateA[1];
                for (int i = 0; i < 9; i++)
                    newTotalState[i + 2] = newHolderStateE[i];

                foreach (actionType ac in availActs)
                {
                    if (Approxim.batchDone[ac])
                    {                     
                        if (null == aloModel.PredictNextState(newHolderStateA, ac))  //IF the Known transition doesnt exist, then update with prediction
                        {
                        prediction = Approxim.LinRegBatch(newTotalState, ac);
                        //Predict the reward for the ego to perform each action,
                        int[] s1 = newHolderStateE;
                            int[] predEgo = egoModel.PredictNextState(s1, ac);
                            double egoRew = egoModel.PredictReward(s1, ac, predEgo);

                       if (egoRew == -0.1)
                       {

                                StateTransition<int[], actionType> predAloTrans = new StateTransition<int[], actionType>(newHolderStateA, ac, egoRew*100, prediction);
                               aloModel.update(predAloTrans);
                        }
                        }
                    }
                }


            return 0;
        }

        //
        public override double[] value(stateType state, List<actionType> actions)
        {


            string a = "GlobalLocation";
            StateClass stateUsable = (StateClass)Convert.ChangeType(state, typeof(StateClass));
            int[] aloState = (int[])stateUsable.GetStateFactor(a);
            double[] returnVals = aloModel.value(aloState, actions);

            
            Console.WriteLine();
            Console.WriteLine(string.Join(" , " ,  returnVals));
           // if(returnVals[0] == )

            return returnVals;
        }

        public override PerformanceStats getStats()
        {
            return aloModel.getStats();
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
