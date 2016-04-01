using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    class Approximator<stateType, actionType>
    {

        Dictionary<actionType, double[,]> weightDict = new Dictionary<actionType, double[,]>();// 0 in the double array is the x, 1 refers to the Y
        Dictionary<actionType, int[]> prediction = new Dictionary<actionType, int[]>();
        int epochs;
        Dictionary<actionType, List<double[]>> historyOfStates = new Dictionary<actionType, List<double[]>>();
        int maxParameter = 0;
        int trials;
        double[] oldTotalState = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public Dictionary<actionType, bool> batchDone = new Dictionary<actionType, bool>();

        Dictionary<actionType, double[]> weightPermX = new Dictionary<actionType, double[]>();
        Dictionary<actionType, double[]> weightPermY = new Dictionary<actionType, double[]>();



        bool first = true;
        actionType firstAC;

        public Approximator(List<actionType> availActs, int eps = 1000, int trls = 100, int maxParam = 0)
        {
            foreach (actionType ac in availActs)
            {
                batchDone.Add(ac, false);
                prediction.Add(ac, new int[2] { 0, 0 });

                double[,] weights = new double[2, 11] {
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
                weightDict.Add(ac, weights);
            }
            epochs = eps;
            trials = trls;
            maxParameter = maxParam;


        }
        //**** EXPERIMENTAL ****//
        public int[] LinReg(double[] newTotalState, actionType act, actionType lastAct)
        {


            if (!historyOfStates.ContainsKey(lastAct))
                historyOfStates.Add(lastAct, new List<double[]>());

            if (!historyOfStates[lastAct].Contains(newTotalState) && historyOfStates[lastAct].Count < trials)
                historyOfStates[lastAct].Add(newTotalState);

            if (first) //TO TEST FAST, REMOVE WHEN WORKING
                firstAC = lastAct; //TO TEST FAST, REMOVE WHEN WORKING
            first = false; //TO TEST FAST, REMOVE WHEN WORKING

            if (historyOfStates[lastAct].Count >= trials && Comparer.OC.Equals(firstAC, lastAct)) //TO TEST FAST, REMOVE WHEN WORKING
                                                                                                  //for (int i = 0; i < epochs; i++)
                funcAproxUpdate(weightDict[lastAct], newTotalState, lastAct);

            for (int i = 0; i < 11; i++)
                oldTotalState[i] = newTotalState[i];

            prediction[act] = funcAprox(weightDict[act], newTotalState);
            return prediction[act];
        }

        //**** EXPERIMENTAL ****//
        public int[] funcAprox(double[,] weights, double[] state)
        {
            int x = 0;
            int y = 1;
            double outputx = -1 / maxParameter;
            double outputy = -1 / maxParameter;

            for (int i = 0; i < 11; i++)
            {
                outputx += weights[x, i] * state[i];
                outputy += weights[y, i] * state[i];
            }

            if (state[3] == 1)
                Console.WriteLine("outputx: {0}", outputx);
            int[] ret = new int[] { (int)Math.Round(outputx), (int)Math.Round(outputy) };
            return ret;
        }
        //**** EXPERIMENTAL ****//
        void funcAproxUpdate(double[,] weights, double[] state, actionType AcPrediction)
        {
            int x = 0;
            int y = 1;
            double alpha = 0.1;
            double[] error = { (prediction[AcPrediction][0] - state[11]), (prediction[AcPrediction][1] - state[12]) };
            for (int i = 0; i < 11; i++)
            {
                weights[x, i] = weights[x, i] - (alpha * oldTotalState[i] * error[x]);
                weights[y, i] = weights[y, i] - (alpha * oldTotalState[i] * error[y]);
            }

        }
        public void UpdateMaxParam(int n)
        {
            maxParameter = n;
        }



        public int[] LinRegBatch(double[] newTotalState, actionType act)
        {
            double[] pred = { 0, 0 };

            for (int i = 0; i < newTotalState.Count(); i++)
            {
                if (i == 0 || i == 1)
                {
                    pred[0] += weightPermX[act][i] * (newTotalState[i]/maxParameter);
                    pred[1] += weightPermY[act][i] * (newTotalState[i]/maxParameter);
                    
                }
                else
                {
                    pred[0] += weightPermX[act][i] * newTotalState[i];
                    pred[1] += weightPermY[act][i] * newTotalState[i];
                }
            }

            pred[0] *= maxParameter;
            pred[1] *= maxParameter;
            return new int[] { (int)Math.Round(pred[0]), (int)Math.Round(pred[1]) };
        }

        public bool TrainLinRegBatch(double[] TrainingTotalState, actionType act)
        {
            //Create the history of States,
            if (!historyOfStates.ContainsKey(act))
            {
                historyOfStates.Add(act, new List<double[]>());
                /*
                    X(old),Y(old),Ego0,Ego1,Ego2,Ego3,Ego4,Ego5,Ego6,Ego7,Ego8,Constant
                    0     ,1     ,2   ,3   ,4   ,5   ,6   ,7   ,8   ,9   ,10  ,11
                */
                //WeightPerm only requires 11, as X(new) and Y(new) will be from the prediction
                weightPermX.Add(act, new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, });
                weightPermY.Add(act, new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            }

            if (historyOfStates[act].Count < trials)
            {
                /*
                    X(old),Y(old),Ego0,Ego1,Ego2,Ego3,Ego4,Ego5,Ego6,Ego7,Ego8,X(new),Y(new),Constant
                    0     ,1     ,2   ,3   ,4   ,5   ,6   ,7   ,8   ,9   ,10  ,11    ,12    ,13
                */
                if (!historyOfStates[act].Contains(TrainingTotalState, Comparer.DAC))
                    historyOfStates[act].Add(TrainingTotalState);

                return false;//We were not at the requested capacity need to begin the training
            }

            else
                 if (!batchDone[act])//The training has not been completed for this act, but it is the correct Size
            {
                CalcBatchWeights(weightPermX[act], weightPermY[act], act);
                batchDone[act] = true;
                return true; //This training is now complete for the current action
            }
            else
                return true; //The Training had been previously completed for this action
        }


        void CalcBatchWeights(double[] weightPermX, double[] weightPermY, actionType act)
        {
            double[] error = { 0, 0 };
            double[] pred = { 0, 0 };
            double[] outcome = { 0, 0 };
            double alpha = 0.1;
            int x = 0;
            int y = 1;
            int xresult = 11;
            int yresult = 12;

            //double[] weights = new double[x[1].Count()];
            double[] weightListX = new double[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            double[] weightListY = new double[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            //  X(old),Y(old),Ego0,Ego1,Ego2,Ego3,Ego4,Ego5,Ego6,Ego7,Ego8,Constant
            //  0     ,1     ,2   ,3   ,4   ,5   ,6   ,7   ,8   ,9   ,10  ,11


            //Need To normalize each of the states that are given.
            foreach (double[] state in historyOfStates[act])
            {
                for (int j = 0; j < state.Length; j++)
                {
                    if (j == 0 || j == 1 || j == xresult || j == yresult)
                        state[j] /= maxParameter;
                }
            }

            for (int e = 0; e < epochs; e++)
            {
                int xCount = 0;
                foreach (double[] xState in historyOfStates[act]) //This provides with each row of the states
                {
                    outcome[x] = xState[xresult];
                    outcome[y] = xState[yresult];
                    pred[x] = pred[y] = 0;

                    for (int c = 0; c < xState.Count(); c++)//Last two are the outcome
                    {
                        if (c >= xresult) // To skip the results held by the xState
                        {
                            c = xresult + 2;
                            pred[x] += weightListX[c - 3] * xState[c];
                            pred[y] += weightListY[c - 3] * xState[c];
                        }
                        else
                        {
                            pred[x] += weightListX[c] * xState[c];
                            pred[y] += weightListY[c] * xState[c];
                        }
                    }


                    error[x] = pred[x] - outcome[x];
                    error[y] = pred[y] - outcome[y];

                    xCount++;
                    for (int d = 0; d < xState.Count(); d++) //weights[j] = weights[j] - (alpha * x[i][j] * err);
                    {
                        if (d >= xresult) // To skip the results held by the xState
                        {
                            d = xresult + 2;
                            weightListX[d - 3] = weightListX[d - 3] - alpha * error[x] * xState[d];
                            weightListY[d - 3] = weightListY[d - 3] - alpha * error[y] * xState[d];

                        }
                        else
                        {
                            weightListX[d] = weightListX[d] - alpha * error[x] * xState[d];
                            weightListY[d] = weightListY[d] - alpha * error[y] * xState[d];
                        }
                    }
                }
            }
            for (int i = 0; i < weightListX.Count(); i++)
            {
                weightPermX[i] = weightListX[i];
                weightPermY[i] = weightListY[i];
            }
        }
    }
}



///*
//    maxI = maxI;
//            for(int r = 0; r< x.Count;r++)
//            {
//                for (int i = 0; i < 2; i++)
//                    if (i == 0 || i == 1)
//                        x[r][i] /= maxI;
//                y[r] /= maxI;
//            }


//            //*** MODIFIABLES ***/
//double alpha = 0.1;
//int epochs = 1000;
//double pred = 0;
//double[] weights = new double[x[1].Count()]; 
//int index = 0;
//double err = 0;
//            //*** MODIFIABLES ***/



//            for (int e = 0; e<epochs; e++)
//            {
//                for (int i = 0; i<x.Count() - 1; i++)
//                {
//                    pred = 0;
//                    for (int c = 0; c<x[0].Count(); c++)
//                        pred += x[i][c] * weights[c]; 

//                         err = pred - y[i];              
//                    for (int j = 0; j<x[0].Count(); j++) 
//                    {
//                        weights[j] = weights[j] - (alpha* x[i][j] * err); 
//                    } 
//                    index++;  
//                } 
//            } 
//            double[] finalWeights = new double[x[0].Count()];
//    */