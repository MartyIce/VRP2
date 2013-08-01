using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPOptimizer
{
    public class VRPOptimizer : AbstractSolver
    {
        public VRPOptimizer(VRPInputProvider provider) : base(provider)
        {
        }

        public override VRPSolution Solve(VRPSolution solution)
        {
            var r = new LocalSearchInsertCustomersSolver(_provider);
            r.NewSolutionFound += NewSolutionFound;
            r.InfoGenerated += r_InfoGenerated;
            var newSol = r.Solve(solution);

            var sa = new SimulatedAnnealingSolver(_provider, 100000);
            sa.NewSolutionFound += NewSolutionFound;
            sa.InfoGenerated += r_InfoGenerated;
            newSol = sa.Solve(newSol);

            foreach (var route in newSol.Routes)
            {
                if(route.TotalDemand > _provider.VehicleCapacity)
                    OnInfoGenerated("Route {0} has a demand {1} greater than capacity {2}", route.Index, route.TotalDemand, _provider.VehicleCapacity);
            }

            OnInfoGenerated("Complete!");

            return newSol;
        }

        void r_InfoGenerated(object sender, InfoEventArgs e)
        {
            OnInfoGenerated(e.Info);
        }

        void NewSolutionFound(object sender, NewSolutionEventArgs e)
        {
            OnNewSolutionFound(e.Solution);
        }
    }
}
