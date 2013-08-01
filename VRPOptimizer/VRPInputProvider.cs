using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRPOptimizer
{
    public class VRPInputProvider
    {
        private bool _runFromPython = false;
        private string _inputName = "";
        private double _tempDiff = 1000;
        private int _numberVehicles = 0;
        private double? _vehicleCapacity = null;
        private List<Location> _locations = null;

        public double TempDiff
        {
            get { return _tempDiff; }
        }

        public int NumberVehicles
        {
            get { return _numberVehicles; }
        }

        public double? VehicleCapacity
        {
            get { return _vehicleCapacity; }
        }

        public  List<Location> Locations
        {
            get { return _locations; }
        }

        public VRPInputProvider(string inputFileName)
        {
            var input = new StringReader(File.ReadAllText(inputFileName));
            CollectInput(input);
        }

        public VRPInputProvider(string[] args)
        {
            _runFromPython = args.Length >= 1 && args[0] == "python";
            if (args.Length >= 2)
                _tempDiff = int.Parse(args[1]);

            if (_runFromPython)
            {
                //if (args.Length >= 3)
                ////    _target = double.Parse(args[2]);
                CollectInputFromArgs(Console.In.ReadToEnd());
            }
            else
            {
                CollectInputFromFile(args);
            }
        }

        private void LogValue(string format, params object[] parameters)
        {
            using (
                StreamWriter sw =
                    new StreamWriter(@"C:\Temp\output" + _inputName + ".txt", true))
            {
                sw.WriteLine(format, parameters);
            }
            if (!_runFromPython)
                Console.Out.WriteLine(format, parameters);
        }

        private void CollectInputFromArgs(string args)
        {
            var input = new StringReader(args);
            CollectInput(input);

            //Console.WriteLine(input.ReadToEnd());

        }

        private void CollectInputFromFile(string[] args)
        {
            var input = new StringReader(File.ReadAllText(args[0]));
            CollectInput(input);
        }

        private void CollectInput(StringReader input)
        {
            _locations = new List<Location>();
            bool countsFound = false;
            int numberCustomers = 0;
            while (input.Peek() >= 0)
            {
                var line = input.ReadLine();
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    if (!countsFound)
                    {
                        countsFound = true;
                        numberCustomers = int.Parse(line.Split(' ')[0]);
                        _numberVehicles = int.Parse(line.Split(' ')[1]);
                        _vehicleCapacity = double.Parse(line.Split(' ')[2]);
                    }
                    else
                    {
                        var location = new Location(double.Parse(line.Split(' ')[0]),
                                                    double.Parse(line.Split(' ')[1]),
                                                    double.Parse(line.Split(' ')[2]));
                        location.Index = _locations.Count;
                        _locations.Add(location);
                    }
                }
            }
            LogValue("Locations: {0}", _locations.Count);
        }
    }
}
