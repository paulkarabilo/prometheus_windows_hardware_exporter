using System;
using System.Runtime.CompilerServices;
using LibreHardwareMonitor.Hardware;
using PrometheusWindowsHardwareExporter;

namespace PrometheusWindowsHardwareExporter
{

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
    public static class Program
    {
        public static void Main(string[] args)
        {
            Config config = ArgsParser.ParseArgs(args);
            
            HashSet<string> collectors = new HashSet<string>(config.Collectors, StringComparer.OrdinalIgnoreCase);

            Computer computer = new Computer
            {
                IsCpuEnabled = collectors.Contains("cpu"),
                IsGpuEnabled = collectors.Contains("gpu"),
                IsMemoryEnabled = collectors.Contains("memory"),
                IsStorageEnabled = collectors.Contains("disk"),
                IsNetworkEnabled = collectors.Contains("network"),
                IsMotherboardEnabled = collectors.Contains("motherboard"),
                IsControllerEnabled = true
            };

            computer.Open();

            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware)
            {
                Console.WriteLine("Hardware: {0}", hardware.Name);
                
                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    Console.WriteLine("\tSubhardware: {0}", subhardware.Name);
                    
                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        Console.WriteLine("\t\tSensor: {0}, value: {1}, identifier: {2}, type: {3}", sensor.Name, sensor.Value, sensor.Identifier, sensor.SensorType);
                    }
                }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    Console.WriteLine("\tSensor: {0}, value: {1}, identifier: {2}, type: {3}", sensor.Name, sensor.Value, sensor.Identifier, sensor.SensorType);
                }
            }
            
            computer.Close();
        }
    }
}