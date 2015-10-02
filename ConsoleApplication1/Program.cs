using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Type t = typeof(Dictionary<,>);
            t = t.MakeGenericType(typeof(int), typeof(int));
            Dictionary<int, int> test = (Dictionary<int, int>)Activator.CreateInstance(t);
            IEqualityComparer<int> comp = test.Comparer;
            

        }
    }
}
