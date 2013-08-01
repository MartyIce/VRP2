using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPOptimizer
{
    public class SimulatedAnnealingSolver : AbstractSolver
    {
        Random r = new Random();
        private double _tempDiff;


        public SimulatedAnnealingSolver(VRPInputProvider provider, double tempDiff) : base(provider)
        {
            _tempDiff = tempDiff;
        }
        public override VRPSolution Solve(VRPSolution solution)
        {
            VRPSolution bestOverall = solution.Clone();
            double temp = 10000000;
            int lastRouteIndex = -1;

            while (temp > 0)
            {
                //LogWarehousess(warehousesClone);

                VRPSolution newSolution = solution.Clone();

                // Look for improvements
                var overCapacityRoute = newSolution.Routes.FirstOrDefault(x => x.TotalDemand > _provider.VehicleCapacity);

                if (overCapacityRoute != null)
                {
                    var location = FindMostExpensiveLocationByCapacity(overCapacityRoute.LocationIndices, _provider.Locations);
                    if (location != null)
                    {
                        int priorCount = overCapacityRoute.LocationIndices.Count;
                        overCapacityRoute.LocationIndices.Remove(location.Index);
                        FindCheapestRouteForLocation(newSolution.Routes, location, overCapacityRoute);
                        if (priorCount == overCapacityRoute.LocationIndices.Count)
                        {
                            // We need to be more aggressive in getting this out of the route
                            MakeSolutionFit(newSolution);
                        }
                    }
                    else
                    {
                        OnInfoGenerated("Couldn't find expensive by capacity location?");
                    }
                }
                else
                {
                    var tooExpensiveRoute = newSolution.Routes.Find(x => x.TotalCost == newSolution.Routes.Max(y => y.TotalCost));
                    if (newSolution.Routes.IndexOf(tooExpensiveRoute) == lastRouteIndex)
                        tooExpensiveRoute = newSolution.Routes[r.Next(newSolution.Routes.Count)];
                    lastRouteIndex = newSolution.Routes.IndexOf(tooExpensiveRoute);
                    if (tooExpensiveRoute != null)
                    {
                        var location = FindMostExpensiveLocation(tooExpensiveRoute.LocationIndices, _provider.Locations);
                        if (location != null)
                        {
                            tooExpensiveRoute.LocationIndices.Remove(location.Index);
                            FindCheapestRouteForLocation(newSolution.Routes, location, tooExpensiveRoute);
                        }
                        else
                        {
                            OnInfoGenerated("Couldn't find expensive location?");
                        }
                    }
                }

                base.Solve(newSolution);

                bool acceptNew = false;
                if (newSolution != null)
                {
                    if (newSolution.TotalCost < solution.TotalCost)
                        acceptNew = true;
                    else if (SimulateAnnealing(solution.TotalCost, newSolution.TotalCost, temp))
                        acceptNew = true;
                }
                if (acceptNew)
                {
                    solution = newSolution;
                    OnInfoGenerated("Temp: {1}, Accepting : {0}", newSolution.TotalCost, temp);
                    OnNewSolutionFound(solution);
                    if (bestOverall == null || bestOverall.TotalCost > newSolution.TotalCost)
                    {
                        bestOverall = newSolution;
                        OnInfoGenerated("-------------------------------------");
                        OnInfoGenerated("New Best : {0}", bestOverall.TotalCost);
                        OnInfoGenerated("-------------------------------------");
                    }
                }

                temp -= _tempDiff;
            }
            return bestOverall;
        }


        private bool SimulateAnnealing(double originalCost, double newCost, double temperature)
        {
            double prob = Math.Exp(-(newCost - originalCost) / temperature);

            return r.NextDouble() < prob;
        }
    }

}
