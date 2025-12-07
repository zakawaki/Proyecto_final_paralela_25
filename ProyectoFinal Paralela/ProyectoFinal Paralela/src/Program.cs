using System.Diagnostics;
using TSPProject;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PROYECTO TSP ===");

        // ======= LIMITE DE CIUDADES =======
        Console.Write("Ingrese el numero de ciudades (maximo 16): ");

        // Intenta convertir el texto ingresado a entero y validar el rango
        // numCitiesalmacena el numero de ciudades ingresado por el usuario
        // Si el usuario escribe algo no valido, menor que 2 o mayor que 16, se asignan 16 por defecto
        if (!int.TryParse(Console.ReadLine(), out int numCities) || numCities < 2 || numCities > 16)
        {
            Console.WriteLine("Numero invalido. Se usaran 16 ciudades por defecto.");
            numCities = 16;
        }

        // ======= MENU DE SEMILLAS ======
        Console.WriteLine("\nSeleccione el tipo de semilla:");
        Console.WriteLine("1. Semilla Fija (siempre genera los mismos datos)");
        Console.WriteLine("2. Semilla Aleatoria / Hibrida");
        Console.Write("Opcion: ");

        // seedOption: guarda la opcion que el usuario escriba
        string seedOption = Console.ReadLine();

        
        switch (seedOption)
        {
            case "1":
                // Semilla fija: la generacion de ciudades siempre sera igual en cada ejecucion
                Console.WriteLine("\nUsando semilla fija...");
                TspDataGenerator.UseFixedSeed();
                break;

            case "2":
                // Semilla aleatoria: genera datos distintos en cada ejecucion del programa
                Console.WriteLine("\nUsando semilla aleatoria...");
                TspDataGenerator.UseRandomSeed();
                break;

            default:
                // Si el usuario elige algo incorrecto, se usa semilla fija por defecto
                Console.WriteLine("\nOpcion invalida. Usando semilla fija por defecto...");
                TspDataGenerator.UseFixedSeed();
                break;
        }


        // ======= GENERAR CIUDADES =======
        Console.WriteLine($"\nGenerando {numCities} ciudades...");

        // Genera una lista oarreglo de ciudades con posiciones aleatorias en el plano
        // Cada ciudad tiene un ID, coordenada X y coordenada Y
        var cities = TspDataGenerator.GenerateCities(numCities);

        // Calcula la matriz de distancias entre todas las ciudades generadas
        // distances[i][j] representa la distancia desde la ciudad i hasta la ciudad j
        var distances = TspDataGenerator.CalculateDistanceMatrix(cities);


        Console.WriteLine("Datos generados y matriz de distancias calculada con exito.\n");

        // Mostrar resumen
        TspDataGenerator.PrintCities(cities, maxToShow: 10);
        Console.WriteLine();

        // ======= MENU PRINCIPAL =======
        Console.WriteLine("Seleccione el modo de ejecucion:");
        Console.WriteLine("1. Secuencial");
        Console.WriteLine("2. Paralelo ");
        Console.WriteLine("3. Pruebas y Metricas ");
        Console.Write("Opcion: ");

        string option = Console.ReadLine();

        switch (option)
        {
            case "1":
                Console.WriteLine("Ejecutando algoritmo Secuencial...\n");

                var solver = new TSPSolverSequential();

                Stopwatch sw = Stopwatch.StartNew();
                solver.Solve(distances);
                sw.Stop();

                Console.WriteLine("=== RESULTADOS SECUENCIALES ===");
                Console.WriteLine($"Mejor Costo Encontrado: {solver.BestCost:F3}");
                Console.WriteLine("Mejor Ruta: " + string.Join(" -> ", solver.BestRoute) + " -> 0");

                Console.WriteLine($"Tiempo de Ejecucion: {sw.ElapsedMilliseconds} ms");
                break;

            case "2":
                Console.WriteLine("Ejecutando algoritmo Paralelo...");
                break;

            case "3":
                Console.WriteLine("Ejecutando Pruebas y Metricas...");
                break;

            default:
                Console.WriteLine("Opcion no valida. Terminando ejecucion.");
                break;
        }

        Console.WriteLine("\nPresiona cualquier tecla para salir...");
        Console.ReadKey();
    }
}
