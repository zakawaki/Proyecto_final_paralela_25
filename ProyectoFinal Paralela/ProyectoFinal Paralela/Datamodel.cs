using System;

namespace TSPProject
{
    // Esta clase representa una ciudad con un ID y dos coordenadas que indican su posicion en el plano
    public class Ciudades
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        // Este constructor permite crear una ciudad asignando su numero y sus coordenadas
        public Ciudades(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        // Este metodo devuelve un texto legible para mostrar los datos de una ciudad en consola
        public override string ToString()
        {
            return $"Ciudades {{ Id = {Id}, X = {X:F3}, Y = {Y:F3} }}";
        }
    }

    // Esta clase contiene metodos para generar ciudades y calcular distancias para pruebas del TSP
    public static class TspDataGenerator
    {
        // Esta semilla fija asegura que los datos generados sean siempre los mismos para pruebas repetibles
        private static readonly Random _rnd = new Random(1234);


        // Este metodo genera un arreglo de ciudades con posiciones aleatorias dentro de un tamano dado
        // Sirve para crear datos de prueba sin depender de archivos externos

        public static Ciudades[] GenerateCities(int count, double canvasSize = 1000.0)
        {
            if (count < 1) throw new ArgumentException("count debe ser >= 1", nameof(count));

            // Aqui se crea el arreglo y se generan las coordenadas aleatorias para cada ciudad
            Ciudades[] cities = new Ciudades[count];
            for (int i = 0; i < count; i++)
            {
                double x = _rnd.NextDouble() * canvasSize;   // Coordenada X aleatoria
                double y = _rnd.NextDouble() * canvasSize;   // Coordenada Y aleatoria
                cities[i] = new Ciudades(i, x, y);
            }
            return cities;
        }


        // Este metodo crea una matriz donde cada espacio [i j] contiene la distancia entre dos ciudades

        public static double[,] CalculateDistanceMatrix(Ciudades[] cities)
        {
            if (cities == null) throw new ArgumentNullException(nameof(cities));

            int n = cities.Length;
            double[,] matrix = new double[n, n];

            // Aqui se recorren todas las parejas de ciudades para calcular sus distancias
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 0.0; // Una ciudad consigo misma siempre tiene distancia cero
                    }
                    else
                    {
                        double dx = cities[i].X - cities[j].X;
                        double dy = cities[i].Y - cities[j].Y;
                        matrix[i, j] = Math.Sqrt(dx * dx + dy * dy); // Distancia euclidiana
                    }
                }
            }

            return matrix;
        }


        // Este metodo imprime algunas ciudades para mostrar un resumen sin saturar la consola

        public static void PrintCities(Ciudades[] cities, int maxToShow = 10)
        {
            if (cities == null) return;

            int toShow = Math.Min(maxToShow, cities.Length);

            // Aqui se muestran solo las primeras ciudades o la cantidad especificada
            for (int i = 0; i < toShow; i++)
            {
                Console.WriteLine(cities[i]);
            }

            // Este mensaje aparece si hay mas ciudades de las que se mostraron
            if (cities.Length > toShow)
            {
                Console.WriteLine($"... +{cities.Length - toShow} ciudades mas");
            }
        }
    }
}
