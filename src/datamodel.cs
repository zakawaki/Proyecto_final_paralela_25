using System;

namespace TSPProject
{
    // Clase que representa una ciudad (ID + coordenadas)
    public class City
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public City(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"City {{ Id = {Id}, X = {X:F3}, Y = {Y:F3} }}";
        }
    }

    // Generador y utilidades para el TSP (semilla fija para reproducibilidad)
    public static class TspDataGenerator
    {
        // SEMILLA FIJA: obligatorio para que las pruebas sean repetibles.
        private static readonly Random _rnd = new Random(1234);

        
        /// Genera un arreglo de ciudades con coordenadas en [0, canvasSize].
        /// Determinista (misma semilla) para cada ejecucion.
        
        public static City[] GenerateCities(int count, double canvasSize = 1000.0)
        {
            if (count < 1) throw new ArgumentException("count debe ser >= 1", nameof(count));

            City[] cities = new City[count];
            for (int i = 0; i < count; i++)
            {
                double x = _rnd.NextDouble() * canvasSize;
                double y = _rnd.NextDouble() * canvasSize;
                cities[i] = new City(i, x, y);
            }
            return cities;
        }

       
        /// Calcula y devuelve la matriz de distancias Euclidianas (double[,]).
        /// matrix[i,j] = distancia desde ciudad i a ciudad j.
        
        public static double[,] CalculateDistanceMatrix(City[] cities)
        {
            if (cities == null) throw new ArgumentNullException(nameof(cities));

            int n = cities.Length;
            double[,] matrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 0.0;
                    }
                    else
                    {
                        double dx = cities[i].X - cities[j].X;
                        double dy = cities[i].Y - cities[j].Y;
                        matrix[i, j] = Math.Sqrt(dx * dx + dy * dy);
                    }
                }
            }

            return matrix;
        }

     
        /// (Opcional) Imprime un pequeno resumen de las ciudades en consola.
       
        public static void PrintCities(City[] cities, int maxToShow = 10)
        {
            if (cities == null) return;
            int toShow = Math.Min(maxToShow, cities.Length);
            for (int i = 0; i < toShow; i++)
            {
                Console.WriteLine(cities[i]);
            }
            if (cities.Length > toShow)
            {
                Console.WriteLine($"... +{cities.Length - toShow} ciudades mas");
            }
        }
    }

}
