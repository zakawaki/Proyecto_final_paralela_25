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
        // Objeto Random usado para generar numeros aleatorios pero se pone fija con el 1234
        
        private static Random _rnd = new Random(1234);

        
        public static void UseFixedSeed()
        {
            _rnd = new Random(1234);
        }

        // Activa una semilla aleatoria basada en el reloj d tu maquina
        // Esto provoca que en cada ejecucion se generen ciudades diferentes
        public static void UseRandomSeed()
        {
            _rnd = new Random(); // semilla tomada del tiempo del sistema
        }

        // Metodo que genera un arreglo de ciudades con coordenadas aleatorias
        // count cantidad de ciudades a generar
        // canvasSize: tamano del plano 1000x1000 por defecto
        public static Ciudades[] GenerateCities(int count, double canvasSize = 1000.0)
        {
            // Valida que se solicite al menos 1 ciudad
            if (count < 1) throw new ArgumentException("count debe ser >= 1", nameof(count));

            // Arreglo que almacenara todas las ciudades generadas
            Ciudades[] cities = new Ciudades[count];

            // Ciclo para crear cada ciudad y asignarle coordenadas aleatorias
            for (int i = 0; i < count; i++)
            {
                // Coordenada X aleatoria entre 0 y canvasSize
                double x = _rnd.NextDouble() * canvasSize;

                // Coordenada Y aleatoria entre 0 y canvasSize
                double y = _rnd.NextDouble() * canvasSize;

                // Crea una ciudad con ID = i y sus coordenadas generadas
                cities[i] = new Ciudades(i, x, y);
            }

            return cities;
        }


        // Metodo que calcula la matriz de distancias entre todas las ciudades
        // matrix[i, j] representa la distancia desde la ciudad i hasta la ciudad j

        public static double[,] CalculateDistanceMatrix(Ciudades[] cities)
        {
            if (cities == null) throw new ArgumentNullException(nameof(cities));

            int n = cities.Length;
            // Matriz que almacenara todas las distancias.
            double[,] matrix = new double[n, n];
            // Recorre todas las combinaciones de ciudades.
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // Distancia de una ciudad a si misma es 0.
                    if (i == j)
                    {
                        matrix[i, j] = 0.0;
                    }
                    else
                    {
                        // Diferencias en coordenadas
                        double dx = cities[i].X - cities[j].X;
                        double dy = cities[i].Y - cities[j].Y;
                        // Distancia entre dos puntos; sqrt(dx al cuadrado + dy al cuadrado)
                        matrix[i, j] = Math.Sqrt(dx * dx + dy * dy);
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
