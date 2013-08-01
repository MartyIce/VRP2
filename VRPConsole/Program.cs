using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRPOptimizer;

namespace VRPConsole
{
    internal class Program
    {
        private static VRPInputProvider _provider;
        private static void Main(string[] args)
        {
            _provider = new VRPInputProvider(args);

            VRPSolution solution = new VRPSolution();
            var s = new VRPOptimizer.VRPOptimizer(_provider);
            solution = s.Solve(solution);


            //List<int> solution = CalculateSolution();

    //outputData = str(obj) + ' ' + str(0) + '\n'
    //for v in range(0, vehicleCount):
    //    outputData += str(depotIndex) + ' ' + ' '.join(map(str,vehicleTours[v])) + ' ' + str(depotIndex) + '\n'

            var optimized = "0";
            Console.Out.WriteLine("{0} {1}", solution.TotalCost, optimized);
            for(int i = 0; i < _provider.NumberVehicles; i++)
                Console.Out.WriteLine("{0}", string.Join(" ", solution.Routes[i].LocationIndices));

        }

        static void s_NewSolutionFound(object sender, NewSolutionEventArgs e)
        {
        }




    }
}
