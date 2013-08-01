using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPOptimizer
{
    public abstract class AbstractSolver 
    {
        public event EventHandler<NewSolutionEventArgs> NewSolutionFound;
        public event EventHandler<InfoEventArgs> InfoGenerated;
        protected VRPInputProvider _provider;

        protected AbstractSolver(VRPInputProvider provider)
        {
            _provider = provider;
        }

        public virtual VRPSolution Solve(VRPSolution solution)
        {
            GenerateTotals(solution);

            return solution;
        }

        protected void GenerateTotals(VRPSolution solution)
        {
            double totalCost = 0.0;
            foreach (var route in solution.Routes)
            {
                var routeCost = 0.0;
                var routeDemand = 0.0;
                for (int i = 0; i < route.LocationIndices.Count - 1; i++)
                {
                    routeCost += CalculateLength(_provider.Locations, route.LocationIndices[i], route.LocationIndices[i + 1]);
                    routeDemand += _provider.Locations[route.LocationIndices[i]].Demand;
                }
                routeCost += CalculateLength(_provider.Locations, route.LocationIndices[route.LocationIndices.Count - 1],
                                             route.LocationIndices[0]);
                routeDemand += _provider.Locations[route.LocationIndices[route.LocationIndices.Count - 1]].Demand;

                route.TotalCost = routeCost;
                route.TotalDemand = routeDemand;
                totalCost += routeCost;
            }
            solution.TotalCost = totalCost;

            foreach (var route in solution.Routes)
            {
                if (route.TotalDemand > _provider.VehicleCapacity)
                {
                    OnInfoGenerated("Vehicle {0} is over capacity {1}", route.TotalDemand, _provider.VehicleCapacity);
                }
            }
        }

        protected void OnNewSolutionFound(VRPSolution solution)
        {
            if (NewSolutionFound != null)
                NewSolutionFound(this, new NewSolutionEventArgs(solution));
        }
        protected void OnInfoGenerated(string info, params object[] args)
        {
            if (InfoGenerated != null)
                InfoGenerated(this, new InfoEventArgs(string.Format(info, args)));
        }
        protected double CalculateLength(List<Location> locations, int p0, int p1)
        {
            return Math.Sqrt(Math.Pow(locations[p0].X - locations[p1].X, 2) + Math.Pow(locations[p0].Y - locations[p1].Y, 2));

        }
        protected void CalculateRouteCost(Route route, List<Location> locations)
        {
            double total = CalculateIndicesCost(route.LocationIndices, locations);
            route.TotalCost = total;
        }

        protected double CalculateIndicesCost(List<int> indices, List<Location> locations)
        {
            double total = 0;

            if (indices == null)
            {
                Console.Write("No indices?");
            }

            for (int i = 0; i < indices.Count - 1; i++)
            {
                total += CalculateLength(locations, indices[i], indices[i + 1]);
            }
            return total;
        }

        protected double CalculatePotentialRouteCost(Route route, int index, List<Location> locations)
        {
            CalculateRouteCost(route, locations);
            return route.TotalCost + CalculateLength(locations, route.LocationIndices[route.LocationIndices.Count-1], index);
        }
        protected double CalculatePotentialDemand(Route route, int index, List<Location> locations)
        {
            route.TotalDemand = CalculateRouteDemand(route.LocationIndices, locations);
            return route.TotalDemand + locations[index].Demand;
        }

        protected double CalculateRouteDemand(List<int> indices , List<Location> locations)
        {
            double total = 0;
            for (int i = 0; i < indices.Count - 1; i++)
            {
                total += locations[indices[i]].Demand;
            }
            return total;
            
        }
        protected List<int> InsertLocationAtCheapestPoint(List<int> indices, Location location, List<Location> locations, bool respectCapacities)
        {
            double minCost = double.MaxValue;
            List<int> returnValue = null;
            for (int i = 1; i < indices.Count; i++)
            {
                List<int> newList = new List<int>();
                newList.AddRange(indices.GetRange(0, i));
                newList.Add(location.Index);
                newList.AddRange(indices.GetRange(i, indices.Count - i));

                double cost = CalculateIndicesCost(newList, locations);
                if (cost < minCost)
                {
                    bool shouldUse = true;
                    if (respectCapacities)
                    {
                        var routeDemand = CalculateRouteDemand(newList, locations);
                        shouldUse = (routeDemand <= _provider.VehicleCapacity);
                    }
                    if (shouldUse)
                    {
                        minCost = cost;
                        returnValue = newList;
                    }
                }
            }
            return returnValue;
        }
        protected Location FindMostExpensiveLocation(List<int> locationIndices, List<Location> locations)
        {
            int? returnValue = null;
            double maxCost = Double.MinValue;
            for (int i = 0; i < locationIndices.Count - 2; i++)
            {
                var cost = CalculateLength(locations, locationIndices[i], locationIndices[i + 1]) +
                           CalculateLength(locations, locationIndices[i + 1], locationIndices[i + 2]);

                if (cost > maxCost)
                {
                    returnValue = i + 1;
                    maxCost = cost;
                }
            }
            if (returnValue.HasValue)
                return locations[locationIndices[returnValue.Value]];
            else
                return null;
        }
        protected Location FindMostExpensiveLocationByCapacity(List<int> locationIndices, List<Location> locations)
        {
            int? returnValue = null;
            double maxDemand = Double.MinValue;
            for (int i = 0; i < locationIndices.Count; i++)
            {
                var demand = locations[locationIndices[i]].Demand;

                if (demand > maxDemand)
                {
                    returnValue = i;
                    maxDemand = demand;
                }
            }
            if (returnValue.HasValue)
                return locations[locationIndices[returnValue.Value]];
            else
                return null;
        }

        protected void FindCheapestRouteForLocation(List<Route> potentialRoutes, Location location, Route tooExpensiveRoute)
        {
            List<List<int>> val = new List<List<int>>();
            potentialRoutes.ForEach(x => val.Add(x.LocationIndices));
            int newRouteIndex = -1;
            FindCheapestRouteForLocation(val, location, ref newRouteIndex, true, tooExpensiveRoute.Index);
            if (newRouteIndex < 0)
                FindCheapestRouteForLocation(val, location, ref newRouteIndex, true, -1);
            if (newRouteIndex < 0)
                FindCheapestRouteForLocation(val, location, ref newRouteIndex, false, -1);

            if (newRouteIndex >= 0)
                potentialRoutes[newRouteIndex].LocationIndices = val[newRouteIndex];
            else
                OnInfoGenerated("Couldn't find new index?");
        }

        protected bool FindCheapestRouteForLocation(List<List<int>> potentialRoutes, Location location, ref int newRouteIndex, bool respectCapacities, int tooExpensiveIndex)
        {
            double lowestCost = double.MaxValue;
            List<int> lowestRoute = null;
            int routeIndex = 0;
            foreach (var potRoute in potentialRoutes)
            {
                if (routeIndex != tooExpensiveIndex)
                {
                    var newRoute = InsertLocationAtCheapestPoint(potRoute, location, _provider.Locations, respectCapacities);
                    if (newRoute != null)
                    {
                        var cost = CalculateIndicesCost(newRoute, _provider.Locations);
                        if (cost < lowestCost)
                        {
                            lowestCost = cost;
                            lowestRoute = newRoute;
                            newRouteIndex = routeIndex;
                        }
                    }
                }

                routeIndex++;
            }
            if (newRouteIndex < 0)
            {
                OnInfoGenerated("negative index");
                return false;
            }
            else
            {
                potentialRoutes[newRouteIndex] = lowestRoute;
                return true;
            }
        }
        protected void MakeSolutionFit(VRPSolution newSolution)
        {
            bool keepGoing = true;
            int counter = 0;
            while (keepGoing)
            {
                foreach (var route in newSolution.Routes)
                {
                    if (route.TotalDemand > _provider.VehicleCapacity)
                    {
                        int? cheapestIndex = FindCheapestDemandLocationInRoute(route);
                        if (cheapestIndex.HasValue)
                        {
                            route.LocationIndices.Remove(cheapestIndex.Value);
                            var otherRoute = newSolution.Routes.Find(x => x.TotalDemand == newSolution.Routes.FindAll(z => z != route).Min(y => y.TotalDemand));
                            otherRoute.LocationIndices = InsertLocationAtCheapestPoint(otherRoute.LocationIndices, _provider.Locations[cheapestIndex.Value],
                                                          _provider.Locations, false);
                            GenerateTotals(newSolution);
                        }
                    }
                }


                keepGoing = newSolution.Routes.Any(x => x.TotalDemand > _provider.VehicleCapacity);

                if(counter++ > 9999)
                    throw new Exception("Infinite loop");
            }
        }

        private int? FindCheapestDemandLocationInRoute(Route route)
        {
            int? returnValue = null;
            double cost = double.MaxValue;

            for (int i = 0; i < route.LocationIndices.Count; i++)
            {
                var l = _provider.Locations[route.LocationIndices[i]];
                if (l.Demand > 0 && l.Demand < cost)
                {
                    cost = l.Demand;
                    returnValue = route.LocationIndices[i];
                }
            }

            return returnValue;
        }
    }
}
