using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Brandon Robertson robertsonb@uleth.ca
    ContextChange Value is intended to approach the issue of context change. It will use model based learning 
    to learn the navigation, however will have a higher level for decision.
    The bottom level will be navigating, while the upper level decides on what model to use.
    As the context changes, the upper level will have to decide to create new models, as the models are created they will be stored
    so they may be recalled later. In a sense the upper level decides where it wants to go, and the lower level decides how to get there
*/

namespace MultiResolutionRL.ValueCalculation
{
   public class ContextChangeValue<stateType, actionType> : ActionValue<stateType, actionType>
    {
        public ContextChangeValue(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, stateType StartState, params object[] parameters) : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            //Constructor
            ModelBasedValue<stateType, actionType> first = new ModelBasedValue<stateType, actionType>(StateComparer,ActionComparer,AvailableActions,StartState,parameters);
            models = new Dictionary<int, ActionValue<stateType, actionType>>();
            models.Add(0, first);
            activeModelKey = 0;
            

            SC = StateComparer;
            AC = ActionComparer;
            AA = AvailableActions;
            SS = StartState;
            parames = parameters;

        }

        //Needs to update the currently active model.
        public override double update(StateTransition<stateType, actionType> transition)
        {
            double holder=0;

            if (!didChange)
            holder = models[activeModelKey].update(transition);

            if (models[0] is ModelBasedValue<stateType, actionType>)
                t_tableProbs = t_TableValues(transition, models);
            return holder;
        }

        //Needs to get the values from the currently active Model.
        public override double[] value(stateType state, List<actionType> actions)
        {

            activeModelKey = FuncApprox();
            return models[activeModelKey].value(state, actions);           
        }

        //TODO:
        //Will be a function approximator, will hopefully take features from
        // T-Table values, Gaussian, R-Table values, ActionValues, possibly sequence of moves or states
        public int FuncApprox()
        {          
            int modelChoice = activeModelKey;
            didChange = false;
            if (t_tableProbs != null)
            {
                double maxProb = t_tableProbs[activeModelKey];
                int maxProbIndex = activeModelKey;

                for (int index = 0; index < t_tableProbs.Count; index++)
                {
                    Console.WriteLine("t_tableProbs: {0}, index = {2}, maxProb = {1}", t_tableProbs[index], maxProb, index);
                    Console.WriteLine("ActiveModel: {0}", activeModelKey);
                    if (t_tableProbs[index] > maxProb)
                    {
                        didChange = true;
                        maxProb = t_tableProbs[index];
                        modelChoice = index;
                    }
                }

                if (maxProb <= 0.5)
                {
                    didChange = true;                   
                    modelChoice = models.Count;
                    models.Add(modelChoice, newModel());
                    Console.WriteLine("Creating new model: {0}, the MaxProb was: {1}", modelChoice,maxProb);
                }
            }
            return modelChoice;
        }

        //Given the Current state, needs to view all of the existing model values for the state
        //Will return the probabilities from the t-Table values from all the models as a list
        public List<double> t_TableValues(StateTransition<stateType,actionType> StaTran,
                                          Dictionary<int,ActionValue<stateType,actionType>> models )
        {
            
            List<double> returnValues = new List<double>();
            int sumActionUsedAtState = -1; //how many times action has been called at state for the model
            double tValue;

            foreach (int key in models.Keys)
            {
                ModelBasedValue<stateType, actionType> modelsCopy = (ModelBasedValue < stateType, actionType> )models[key];
                sumActionUsedAtState = modelsCopy.T.GetStateValueTable(StaTran.oldState, StaTran.action).Values.Sum();

                tValue = modelsCopy.T.Get(StaTran.oldState, StaTran.action, StaTran.newState);

                if (sumActionUsedAtState > 0)
                    returnValues.Add(tValue / sumActionUsedAtState);
                else
                    returnValues.Add(1);
            }

            return returnValues;

        }

        //Will take the similarities between models and apply a gaussian distribution on what model should be attempted
        public List<int> gaussianDistribution()
        {
            List<int> returnValues = null;

            return returnValues;
        }

        //function will return a list of values associated with a all the models given the current state
        public List<int> r_TableValues()
        {
            List<int> returnValues = null;
            return returnValues;

        }


        //Return a list of values associated with the action values from each model for the given state
        public List<double[]> modelActionValues(StateTransition<stateType, actionType> StaTran,
                                                Dictionary<int, ActionValue<stateType, actionType>> models)
        {
            List<double[]> returnValues = null;
            foreach(int key in models.Keys)
            {
                returnValues.Add(models[key].value(StaTran.oldState, models[key].availableActions));
            }
            return returnValues;
        }

        //TODO:
        //Under set criteria, will create a new model, either a blank slate, or a copy of a previously existing
        // previously existing model is useful for models that are similar up to a point but may have a slight difference
        public ActionValue<stateType,actionType> newModel()
        {
            ActionValue<stateType, actionType> returnModel = null;
            Type AVtype = models[0].GetType();

          //  AVtype.MakeGenericType(typeof(stateType), typeof(actionType));
            returnModel = (ActionValue<stateType, actionType>)Activator.CreateInstance(AVtype, SC, AC, AA, SS, parames);

            return returnModel;
        }

        //*******************MEMBERS*******************//
        Dictionary<int, ActionValue<stateType, actionType>> models;
        int activeModelKey;
        bool didChange = false;

        //int numFactors=1;
        //List<double> FAweights= new List<double>(1);

        List<double> t_tableProbs = null;

        IEqualityComparer<stateType> SC;
        IEqualityComparer<actionType> AC;
        List<actionType> AA;
        stateType SS;
        object[] parames;

        //NOT IMPLEMENTED/USED
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
        public override PerformanceStats getStats()
        {         
            return new PerformanceStats();
           // throw new NotImplementedException();
        }

    }
}
