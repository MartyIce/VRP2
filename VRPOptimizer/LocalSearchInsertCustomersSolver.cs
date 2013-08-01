using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPOptimizer
{
    public class LocalSearchInsertCustomersSolver : AbstractSolver
    {
        public LocalSearchInsertCustomersSolver(VRPInputProvider provider) : base(provider)
        {
        }

        public override VRPSolution Solve(VRPSolution solution)
        {
            try
            {
                List<List<int>> potentialRoutes = new List<List<int>>();
                for (int i = 0; i < _provider.NumberVehicles; i++)
                {
                    potentialRoutes.Add(new List<int>() {0, 0});
                }
                for (int i = 1; i < _provider.Locations.Count; i++)
                {
                    var location = _provider.Locations[i];
                    int newRouteIndex = -1;
                    if (!FindCheapestRouteForLocation(potentialRoutes, location, ref newRouteIndex, true, -1))
                        FindCheapestRouteForLocation(potentialRoutes, location, ref newRouteIndex, false, -1);
                }

                solution.Routes.Clear();
                for (int i = 0; i < potentialRoutes.Count; i++)
                {
                    var route = new Route();
                    route.Index = solution.Routes.Count;
                    route.LocationIndices = potentialRoutes[i];
                    solution.Routes.Add(route);
                }

                VRPSolution returnValue = null;
                try
                {
                    returnValue = base.Solve(solution);
                }
                catch (Exception exc)
                {
                    Console.Write(exc.Message);
                    throw exc;
                }
                OnNewSolutionFound(returnValue);

                return returnValue;
            }
            catch (Exception exc)
            {
                Console.Write(exc.Message);
                throw;
            }
        }


    }

}
