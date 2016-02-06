using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL
{
   public class StateClass
    {
        /*
        Will create the Dictionary member for the statesFactors to be added to.
        If the number of descriptors is unequal to number of stateFactor, 
        or either is empty, an empty dictionary will be created, which can be added
        to later.
         */
        public StateClass(string descriptor, object stateFactor)
        {
            stateDict = new Dictionary<string, object>();
          //  if (descriptor.Length == stateFactor.Length)
               // for (int s = 0; s < descriptor.Length; s++)
                {
                    stateDict.Add(descriptor, stateFactor);
                    numFactors++;
                }
        }
        //Returns true if the descriptor was added else the key already exists and the stateFactor was not added
        public bool addState(string descriptor, object stateFactor)
        {
            if (stateDict.ContainsKey(descriptor))
                return false;
            

            stateDict.Add(descriptor, stateFactor);
            numFactors++;
            return true;

        }

        //Returns True if the state exists and was modified, else returns false
        //Will modify the given descriptor to hold the given stateFactor object
       public bool ModifyState(string descriptor, object stateFactor)
        {
            if (stateDict.ContainsKey(descriptor))
            {
                stateDict[descriptor] = stateFactor;
                return true;
            }
            return false;      
        }

        // returns the object associated with the descriptor
        //Be sure to check if the descriptor exists first using FactorExist
        public object GetStateFactor(string descriptor)
        {
            return stateDict[descriptor];
        }

        //Returns T/F if descriptor exists in the stateDictionary
        public bool FactorExist(string descriptor)
        {
            return stateDict.ContainsKey(descriptor);
        }

        //Number of stateFactors in the stateDict
        public int Count()
        {
            return numFactors;
        }

        //Returns a List<object> of all the stateFactors in the dictionary
        public List<object> GetFactors()
        {
            List<object> facts = new List<object>();
            foreach (string s in stateDict.Keys)
                facts.Add(stateDict[s]);

            return facts;
        }

        //Returns a List<string> of all the descriptors in the dictionary
        public List<string> GetDescriptors()
        {
        List<string> descriptors = new List<string>();
            foreach (string s in stateDict.Keys)
                descriptors.Add(s);
            return descriptors;
                
        }

        //MEMBERS//
        Dictionary<string, object> stateDict;
        int numFactors = 0;

    }
}
