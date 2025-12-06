using System;
using System.Diagnostics;

namespace TSPProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== PROYECTO TSP ===");
            Console.Write("Ingrese el numero de ciudades (maximo 20): ");

            if (!int.TryParse(Console.ReadLine(), out int numCities) || numCities < 2)
            {
                Console.WriteLine("Numero invalido. Se usara 10 por defecto.");
                numCities = 10;
            }

            Console.WriteLine($"\nGenerando {numCities} ciudades con semilla fija...");
            var cities = TspDataGenerator.GenerateCities(numCities);
            var distances = TspDataGenerator.CalculateDistanceMatrix(cities);

            Console.WriteLine("Datos generados y matriz de distancias calculada con exito.\n");

            // Muestra un resumen de las primeras ciudades
            TspDataGenerator.PrintCities(cities, maxToShow: 10);
            Console.WriteLine();

            // Menu de opciones (hagan su parte)
            Console.WriteLine("Seleccione el modo de ejecucion:");
            Console.WriteLine("1. Secuencial (Integrante 2)");
            Console.WriteLine("2. Paralelismo (Integrante 3)");
            Console.WriteLine("3. Pruebas y Metricas (Integrante 4)");
            Console.Write("Opcion: ");

            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Console.WriteLine("Ejecutando algoritmo Secuencial...");
                    // TODO: Llamar a TSPSolverSequential
                    break;

                case "2":
                    Console.WriteLine("Ejecutando algoritmo Paralelo...");
                    // TODO: Llamar a TSPSolverParallel
                    break;

                case "3":
                    Console.WriteLine("Ejecutando prueas y metricas...");
                    // integrante 4
                    break;

                default:
                    Console.WriteLine("Opcion no valida, terminando ejecucion.");
                    break;
            }

            Console.WriteLine("\n presiona any tecla para salir...");
            Console.ReadKey();
        }
    }
}