using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    [Serializable]
    public class SAStable<stateType, actionType, entryType>
    {
        Dictionary<stateType, Dictionary<actionType, Dictionary<stateType, entryType>>> table;
        List<actionType> availableActions;
        IEqualityComparer<stateType> stateComparer;
        IEqualityComparer<actionType> actionComparer;

        DefaultEntryMaker<entryType> makeDefaultEntry;
        object[] defaultEntryConstructorParams;

        public SAStable(IEqualityComparer<stateType> StateComparer, IEqualityComparer<actionType> ActionComparer, List<actionType> AvailableActions, DefaultEntryMaker<entryType> MakeDefaultEntry, params object[] DefaultEntryConstructorParams)
        {
            availableActions = AvailableActions;
            stateComparer = StateComparer;
            actionComparer = ActionComparer;
            table = new Dictionary<stateType, Dictionary<actionType, Dictionary<stateType, entryType>>>(stateComparer);

            makeDefaultEntry = MakeDefaultEntry;
            defaultEntryConstructorParams = DefaultEntryConstructorParams;
        }

        public entryType Get(stateType oldState, actionType action, stateType newState)
        {
            if (!table.ContainsKey(oldState))
            {
                return makeDefaultEntry(defaultEntryConstructorParams);
                //return defaultEntry;
            }
            if (!table[oldState].ContainsKey(action))
            {
                return makeDefaultEntry(defaultEntryConstructorParams);
                //return defaultEntry;
            }
            if (!table[oldState][action].ContainsKey(newState))
            {
                return makeDefaultEntry(defaultEntryConstructorParams);
                //return defaultEntry;
            }
            return table[oldState][action][newState];
        }

        public stateType[] GetKnownStates()
        {
            return table.Keys.ToArray();
        }

        public Dictionary<stateType, entryType> GetStateValueTable(stateType oldState, actionType action)
        {
            if (!table.ContainsKey(oldState))
            {
                table.Add(oldState, new Dictionary<actionType, Dictionary<stateType, entryType>>(actionComparer));
            }
            if (!table[oldState].ContainsKey(action))
            {
                table[oldState].Add(action, new Dictionary<stateType, entryType>(stateComparer));
            }
            //foreach (actionType act in availableActions)
            //{
            //    table[oldState].Add(act, new Dictionary<stateType, entryType>(stateComparer));
            //}

            return table[oldState][action];
        }

        public void Set(stateType oldState, actionType action, stateType newState, entryType value)
        {
            if (!table.ContainsKey(oldState))
            {
                table.Add(oldState, new Dictionary<actionType, Dictionary<stateType, entryType>>(actionComparer));
            }
            if (!table[oldState].ContainsKey(action))
            {
                table[oldState].Add(action, new Dictionary<stateType, entryType>(stateComparer));
            }
            if (!table[oldState][action].ContainsKey(newState))
            {
                table[oldState][action].Add(newState, default(entryType));
            }
            table[oldState][action][newState] = value;
        }

        public void print()
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\T.csv");
            foreach (stateType s1 in table.Keys)
            {
                foreach (actionType a in table[s1].Keys)
                {
                    foreach (stateType s2 in table[s1][a].Keys)
                    {
                        int[] s1int = (int[])(object)s1;
                        int[] aint = (int[])(object)a;
                        int[] s2int = (int[])(object)s2;
                        writer.WriteLine(string.Join(",", s1int) + "," + string.Join(",", aint) + "," + string.Join(",", s2int) + "," + table[s1][a][s2].ToString());
                    }
                }
            }
            writer.Flush();
            writer.Close();

        }
    }

    public delegate entryType DefaultEntryMaker<entryType>(params object[] constructorParams);
}
