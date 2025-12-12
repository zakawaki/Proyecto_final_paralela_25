using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPProject
{
    public class BenchmarkRunner
    {
        //cantidad de veces que repetimos cada prueba para sacar el tiempo promedio
        private const int _repetitions = 5;

        //si N > 16 la secuencial tarda muchisimo
        //esta regla evita que la PC se trabe haciendo brute-force
        private const int _seqLimit = 16;

        //tamanos de mapas que vamos a probar (cantidad de ciudades)
        private readonly int[] _testSizes = { 10, 15, 20, 22 };

        public void RunBenchmarks()
        {
            //mensaje recordando si estamos en DEBUG (mas lento que RELEASE)
#if DEBUG
            Console.WriteLine("ADVERTENCIA: Estas ejecutando en modo DEBUG.");
            Console.WriteLine("Para metricas reales, cambia configuracion RELEASE.\n");
#endif
            Console.WriteLine("===Iniciando Benchmark TSP (Avg de 5 corridas)===");
            Console.WriteLine($"Limite Secuencial: {_seqLimit} ciudades.");

            //impresion del encabezado de la tabla
            Console.WriteLine("{0,-10} | {1,-15} | {2,-15} | {3,-10}", "Ciudades", "Secuencial(ms)", "Paralelo(ms)", "Speedup");
            Console.WriteLine(new string('-', 65));


            //recorrer todos los tamanos de prueba definidos arriba
            foreach (int n in _testSizes)
            {
                RunSingleScenario(n); // ejecuta una prueba para ese numero de ciudades
            }

            Console.WriteLine(new string('-', 65));
            Console.WriteLine("Pruebas finalizadas.");
        }

        private void RunSingleScenario(int numCities)

        {   
            //listas donde guardaremos los tiempos de cada repeticion
            List<long> timesSeq = new List<long>();
            List<long> timesPar = new List<long>();

            //si el numero de ciudades sobrepasa el limite
            //saltamos la version secuencial para no congelar la PC
            bool skipSequential = numCities > _seqLimit;

            //repetimos varias veces para evitar ruido del sistema operativo
            for (int i = 0; i < _repetitions; i++)
            { 
                //1. generacion de mapa
                //tickcount + i para que cada repeticion sea un mapa distinto
                int seed = Environment.TickCount + i;

                var cities = TspDataGenerator.GenerateCities(numCities);
                var map = TspDataGenerator.CalculateDistanceMatrix(cities);


                //2. limpieza de memoria 
                //se forza el garbage collector para que no interrumpa la medicion 
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //3. medicion del algoritmo secuencial
                if (!skipSequential)
                {
                    var seqSolver = new TSPSolverSequential();
                    Stopwatch sw = Stopwatch.StartNew();
                    seqSolver.Solve(map); //ejecutamos el TSP secuencial
                    sw.Stop();
                    timesSeq.Add(sw.ElapsedMilliseconds); // guardamos tiempo
                }

                //4. limpieza de memoria de nuevo antes del paralelo
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //5. medicion paralela
                var parSolver = new TSPSolverParallel();
                Stopwatch swPar = Stopwatch.StartNew();
                parSolver.Solve(map); // ejecutamos el TSP paralelo
                swPar.Stop();
                timesPar.Add(swPar.ElapsedMilliseconds); // guardamos tiempo
            }

            //calculos de resultados

            //el promedio paralelo
            double avgPar = GetAverage(timesPar);

            //variables de texto para imprimir resultados
            string strSeq = "N/A";
            string strSpeedup = "INF"; //si no hay secuencial, el speedup es infinito

            if (!skipSequential)

            {
                //promedio del secuencial
                double avgSeq = GetAverage(timesSeq);
                strSeq = avgSeq.ToString("F2");

                //speedup = tiempo secuencial / tiempo paralelo
                if (avgSeq > 0)
                {
                    double speedup = avgSeq / avgPar;
                    strSpeedup = speedup.ToString("F2") + "X";
                }
            }

        //imprime fila de la tabla
        Console.WriteLine("{0,-10} | {1,-15} | {2,-15} | {3,-10}", numCities, strSeq, avgPar.ToString("F2"), strSpeedup);

            }

        //metodo auxiliar que calcula el promedio de una lista de tiempos
        private double GetAverage(List<long> times)
        {
            if (times.Count == 0) return 0;
            double sum = 0;
            foreach (long t in times) sum += t;
            return sum / times.Count;
        }
    }
}
