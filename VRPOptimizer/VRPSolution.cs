using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPOptimizer
{
    public class Location
    {
        public Location(double demand, double x, double y)
        {
            Demand = demand;
            X = x;
            Y = y;
        }

        public double Demand { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Index { get; set; }
    }
    public class VRPSolution
    {
        public VRPSolution()
        {
            Routes = new List<Route>();
        }

        public double TotalCost { get; set; }
        public List<Route> Routes { get; set; }

        public VRPSolution Clone()
        {
            VRPSolution returnValue = new VRPSolution();
            foreach (var route in Routes)
            {
                var newRoute = new Route();
                newRoute.Index = route.Index;
                newRoute.TotalCost = route.TotalCost;
                newRoute.TotalDemand = route.TotalDemand;
                newRoute.LocationIndices.AddRange(route.LocationIndices);
                returnValue.Routes.Add(newRoute);
            }
            returnValue.TotalCost = TotalCost;
            return returnValue;
        }
    }
    [DebuggerDisplay("{LocationIndices.Count} Locations, TotalCost: {TotalCost}, TotalDemand: {TotalDemand}")]
    public class Route
    {
        public Route()
        {
            LocationIndices = new List<int>();
        }

        public List<int> LocationIndices { get; set; }
        public double TotalCost { get; set; }
        public double TotalDemand { get; set; }
        public int Index { get; set; }
    }

    public class NewSolutionEventArgs : EventArgs
    {
        public NewSolutionEventArgs(VRPSolution solution)
        {
            Solution = solution;
        }

        public VRPSolution Solution { get; set; }
    }
    public class InfoEventArgs : EventArgs
    {
        public InfoEventArgs(string info)
        {
            Info = info;
        }

        public string Info { get; set; }
    }
}
