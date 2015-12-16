using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiResolutionRL.ValueCalculation
{
    public class FeudalValue<stateType, actionType> : ActionValue<int[], int[]>
    {
        PerformanceStats stats = new PerformanceStats();
        IEqualityComparer<int[]> stateComparer;
        IEqualityComparer<int[]> actionComparer;
        List<int[]> standardActions;
        List<int[]> allActions;

        StateManagement.intStateTree stateTree = new StateManagement.intStateTree();
        Policy<int[], int[]> optimalPolicy = new OptimalPolicy<int[], int[]>();

        int numManagers;
        ActionValue<int[], int[]>[] managers;
        int[][] commands;
        int[][] destinations;
        int[] startTimes;

        int timerCnt = 0;
        int timeout = 10;

        public FeudalValue(IEqualityComparer<int[]> StateComparer, IEqualityComparer<int[]> ActionComparer, List<int[]> AvailableActions, int[] StartState, params object[] parameters)
            : base(StateComparer, ActionComparer, AvailableActions, StartState, parameters)
        {
            actionComparer = ActionComparer;
            stateComparer = StateComparer;

            // setup three different versions of the available actions list
            standardActions = AvailableActions;
            allActions = new List<int[]>(AvailableActions); allActions.Add(new int[2] { 0, 0 });

            // initialize the array of managers
            numManagers = 4;
            managers = new ActionValue<int[], int[]>[numManagers];
            commands = new int[numManagers+1][]; commands[numManagers] = new int[2] { 10, 10 };
            destinations = new int[numManagers][];
            startTimes = new int[numManagers];

            for (int i = 0; i < numManagers; i++)
            {
                managers[i] = new ModelFreeValue<int[], int[]>(stateComparer, actionComparer, i==0? standardActions:allActions, null);
                //managers[i] = new ModelBasedValue<int[], int[]>(stateComparer, actionComparer, i==0? standardActions:allActions, null);
            }

        }


        public override double[] value(int[] state, List<int[]> actions)
        {
            for (int i=numManagers-1; i>=0; i--)
            {
                if (commands[i]==null)
                    chooseCommand(state, i);
            }

            int[] stateCommand = new int[4] { state[0], state[1], commands[1][0], commands[1][1] };
            double[] values = managers[0].value(stateCommand, standardActions);

            Console.WriteLine(string.Join(",", stateCommand) + "____" + string.Join(",",commands[2]) + "____" + string.Join(",", commands[3]));
            return values;
        }

        private void chooseCommand(int[] state, int level)
        {
            //if (level == 0)
            //{
            //    throw new ArgumentException();
            //}
            //else
            //{
                int[] thisState = stateTree.GetParentState(state, level);
                int[] stateCommand = new int[4] { thisState[0], thisState[1], commands[level + 1][0], commands[level + 1][1] };

                double[] values = managers[level].value(stateCommand, level==0 ? standardActions : allActions);
                commands[level] = optimalPolicy.selectAction(allActions, values.ToList());
                startTimes[level] = timerCnt;
            //}
        }
        
        public override double update(StateTransition<int[], int[]> transition)
        {
            timerCnt++;
            stats.cumulativeReward += transition.reward;

            // store a temporary copy of all the issued commands
            int[][] oldCommands = new int[numManagers+1][];
            for (int i=0; i<numManagers+1; i++)
            {
                oldCommands[i] = new int[2];
                Array.Copy(commands[i], oldCommands[i], 2);
            }
            

            for (int i=numManagers-1; i>0; i--)
            {

                int[] thisLevelOldState = stateTree.GetParentState(transition.oldState, i);
                int[] thisLevelNewState = stateTree.GetParentState(transition.newState, i);
                
                // if transition has occurred at this level
                if (!stateComparer.Equals(thisLevelNewState, thisLevelOldState))
                {
                    // choose new command
                    chooseCommand(transition.newState, i);

                    // reward/punish sub-manager
                    int[] goalState = new int[2] { thisLevelOldState[0] + oldCommands[i][0], thisLevelOldState[1] + oldCommands[i][1] }; // ****Issue: state structure is assumed
                    double rewardForSubManager;
                    //if (stateComparer.Equals(thisLevelNewState, goalState)) //if the desired transition was made ***Issue: two wrongs make a right - submanagers can be rewarded for choosing the wrong thing, if their submanagers disobey them and choose the right thing
                    if (actionComparer.Equals(i == 1 ? transition.action : oldCommands[i - 1], oldCommands[i])) // if the submanager chose the right command ***Issue: breaks the information hiding rule
                    {
                        rewardForSubManager = 10;
                    }
                    else
                    {
                        rewardForSubManager = -10;
                    }
                    int[] lowLevelOldState = stateTree.GetParentState(transition.oldState, i - 1);
                    int[] lowLevelOldStateCmd = new int[4] { lowLevelOldState[0], lowLevelOldState[1], oldCommands[i][0], oldCommands[i][1] };
                    int[] lowLevelNewState = stateTree.GetParentState(transition.newState, i - 1);
                    int[] lowLevelNewStateCmd = new int[4] { lowLevelNewState[0], lowLevelNewState[1], commands[i][0], commands[i][1] };
                    int[] action = i == 1 ? transition.action : oldCommands[i - 1]; //***Issue: reward for commanded action or new state?
                    StateTransition<int[], int[]> subManagerTransition = new StateTransition<int[], int[]>(lowLevelOldStateCmd, action, rewardForSubManager, lowLevelNewStateCmd);
                    Console.WriteLine("manager reward " + (i - 1) + " " + subManagerTransition.reward);
                    managers[i - 1].update(subManagerTransition);

                    // issue a small negative reward for each step
                    int[] thisLevelOldStateCmd = new int[4] { thisLevelOldState[0], thisLevelOldState[1], oldCommands[i][0], oldCommands[i][1] };
                    int[] thisLevelNewStateCmd = new int[4] { thisLevelNewState[0], thisLevelNewState[1], commands[i][0], commands[i][1] };
                    StateTransition<int[], int[]> thisTransition = new StateTransition<int[], int[]>(thisLevelOldStateCmd, oldCommands[i], -0.1, thisLevelNewStateCmd);
                    Console.WriteLine("environmental reward " + i + " " + thisTransition.reward);
                    managers[i].update(thisTransition);

                    // see environmental reward if goal reached
                    if (transition.reward > 1)
                    {
                        if (stateComparer.Equals(oldCommands[i], new int[2] { 0, 0 })) //***Issue: how to tell which commands led to the goal? for now reward everyone who called *
                        {
                            thisLevelOldStateCmd = new int[4] { thisLevelOldState[0], thisLevelOldState[1], oldCommands[i][0], oldCommands[i][1] };
                            thisLevelNewStateCmd = new int[4] { thisLevelNewState[0], thisLevelNewState[1], commands[i][0], commands[i][1] };
                            thisTransition = new StateTransition<int[], int[]>(thisLevelOldStateCmd, oldCommands[i], transition.reward, thisLevelNewStateCmd);
                            Console.WriteLine("environmental reward " + i + " " + thisTransition.reward);
                            managers[i].update(thisTransition);
                        }
                    }
                }
                else // if no transition
                {
                }
            }


            // lowest level always sees environmental reward
            int[] lowestLevOldStateCmd = new int[4] { transition.oldState[0], transition.oldState[1], oldCommands[1][0], oldCommands[1][1] };
            int[] lowestLevNewStateCmd = new int[4] { transition.newState[0], transition.newState[1], commands[1][0], commands[1][1] };
            StateTransition<int[], int[]> thisLevel0Transition = new StateTransition<int[], int[]>(lowestLevOldStateCmd, transition.action, transition.reward, lowestLevNewStateCmd);
            managers[0].update(thisLevel0Transition);


            // check for timeout
            for (int i = numManagers - 1; i > 0; i--)
            {
                if ((timerCnt - startTimes[i]) > timeout * Math.Pow(2, i)) // timeout
                {
                    Console.WriteLine("timeout at level " + i);
                    int[] thisLevelOldState = stateTree.GetParentState(transition.oldState, i);
                    int[] thisLevelNewState = stateTree.GetParentState(transition.newState, i);
                    int[] thisLevelOldStateCmd = new int[4] { thisLevelOldState[0], thisLevelOldState[1], oldCommands[i + 1][0], oldCommands[i + 1][1] };
                    int[] thisLevelNewStateCmd = new int[4] { thisLevelNewState[0], thisLevelNewState[1], commands[i + 1][0], commands[i + 1][1] };
                    StateTransition<int[], int[]> timeoutTransition = new StateTransition<int[], int[]>(thisLevelOldStateCmd, oldCommands[i], -10, thisLevelNewStateCmd);
                    managers[i].update(timeoutTransition);

                    for (int l = i; l >= 0; l--)
                    {
                        commands[l] = null;
                    }
                    break;
                }
            }
            



            
            return double.NaN;
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
        public override PerformanceStats getStats()
        {
            return stats;
        }
    }
}
