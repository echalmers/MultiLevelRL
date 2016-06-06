using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiResolutionRL;
using MultiResolutionRL.ValueCalculation;
using System.Drawing;

namespace LargeProblemTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // create the map by scaling up the standard one
            double scale = 4;
            Bitmap map = new Bitmap("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10b.bmp");

            int w = (int)Math.Round(map.Width * scale);
            int h = (int)(Math.Round(map.Height * scale));
            Bitmap resizedMap = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(resizedMap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(map, 0, 0, w, h);
            }
            resizedMap.Save("C:\\Users\\Eric\\Google Drive\\Lethbridge Projects\\map10Resized.bmp");

            // instantiate the world
            GridWorld world = new GridWorld();
            world.Load(resizedMap);

            // add agent
            //world.addAgent(typeof(OptimalPolicy<,>), typeof(MultiResValue<,>), 10);
            world.addAgent(typeof(OptimalPolicy<,>), typeof(ModelBasedValue<,>));

            // run
            PerformanceStats stats = new PerformanceStats();
            for (int i=0; i<1000000; i++)
            {
                stats = world.stepAgent();
                int numTrials = stats.stepsToGoal.Count;
                if (stats.stepsToGoal.Last() % 100 == 0)
                    Console.WriteLine(stats.stepsToGoal.Last());
                if (stats.stepsToGoal.Last() >= 50000)
                {
                    stats.stepsToGoal[numTrials-1] = -1;
                    break;
                }
                if (numTrials >= (10 * scale + 1) && stats.stepsToGoal[numTrials-1]== stats.stepsToGoal[numTrials - 2])
                    break;
            }

            //process results
            Console.WriteLine(string.Join(",", stats.stepsToGoal));
            Console.ReadKey();
        }
    }
}
