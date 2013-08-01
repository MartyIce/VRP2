using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPOptimizer
{
    public class RandomSolver : AbstractSolver
    {
        public RandomSolver(VRPInputProvider provider) : base(provider)
        {
        }

        public override VRPSolution Solve(VRPSolution solution)
        {
            var r = new Route();
            solution.Routes.Add(r);
            int locationsPerVehicle = _provider.Locations.Count/_provider.NumberVehicles;
            int locationsPerCurrentVehicle = 0;
            int currentVehicleIndex = 0;
            r.LocationIndices.Add(0);
            for (int i = 1; i < _provider.Locations.Count; i++) 
            {
                double potentialDemand = CalculatePotentialDemand(r, i, _provider.Locations);
                bool safeDemand = potentialDemand <= _provider.VehicleCapacity;

                if (safeDemand)
                {
                    r.LocationIndices.Add(i);
                    CalculateRouteCost(r, _provider.Locations);
                    locationsPerCurrentVehicle++;
                }

                if (!safeDemand || locationsPerCurrentVehicle >= locationsPerVehicle)
                {
                    r.LocationIndices.Add(0);
                    locationsPerCurrentVehicle = 0;
                    currentVehicleIndex++;
                    r = new Route();
                    solution.Routes.Add(r);
                    r.LocationIndices.Add(0);
                    r.LocationIndices.Add(i);
                }
            }
            r.LocationIndices.Add(0);

            OnNewSolutionFound(solution);
            return base.Solve(solution);

        }


    }
}
