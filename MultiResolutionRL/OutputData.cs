using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//
namespace MultiResolutionRL
{
    public class OutputData
    {
       public OutputData() { }
        //Members
       
        //Directory path to be created
        string outputData = "..\\OutputData";

        //BELONG TO WORLD ***********************************************************************/
        //"C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\gradientsVal.csv"
        string gradientsVal = "\\gradientsVal.csv";

        //"C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\gradientsX.csv"
        string gradientsX = "\\gradientsX.csv";

        //"C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\gradientsY.csv"
        string gradientsY = "\\gradientsY.csv";

        //"C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Fuzzy Place Field Test\\Adjacencies.csv"
        string Adjacencies = "\\Adjacencies.csv";

        //BELONG TO RLTEST ******************************************************************************/
        // "C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Presentation Sept 28\\cumulativeReward.txt"
        string cumulativeReward = "\\cumulativeReward.txt";

        //"C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\trajectory" + of.SafeFileName + ".csv"
        string trajectory = "\\trajectory";

        //"C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Images\\"
        string Images = "\\Images";

        //"C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\Images\\chart_" + DateTime.Now.ToString("ddMMM-hh-mm") + ".Tiff", System.Drawing.Imaging.ImageFormat.Tiff;
        string chart_ = "chart_";

        public string getAddress(string desired)
        {
            if (desired == "gradientsVal")
                return outputData + gradientsVal;

            else if (desired == "gradientsX")
                return gradientsX;

            else if (desired == "gradientsY")
                return outputData + gradientsY;

            else if (desired == "Adjacencies")
                return Adjacencies;

            else if (desired == "outputData")
                return outputData;

            else if (desired == "cumulativeReward")
                return outputData + cumulativeReward;

            else if (desired == "trajectory")
                return outputData + trajectory;

            else if (desired == "Images")
                return outputData + Images;

            else if (desired == "chart_")
                return outputData + Images + chart_;

            return null;
        }


    }
}
