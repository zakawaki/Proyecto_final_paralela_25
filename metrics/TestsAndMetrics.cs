using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TSPProject
{
    public static class TestsAndMetrics
    {
        // Llamado desde Program: TestsAndMetrics.Run(distances);
        public static void Run(double[,] distances)
        {
            Console.WriteLine(">> TestsAndMetrics arrancó.");
            if (distances == null)
            {
                Console.WriteLine("La matriz distances es null. Asegúrate de pasar la matriz desde Program.cs.");
                return;
            }

            // CONFIGURACION (no tocar si no hace falta)
            int n = distances.GetLength(0);
            int[] sizes = new int[] { n };          // usamos el n ya generado en Program
            int[] threadsToTest = new int[] { 1, 2, 4, 8 };
            int repeats = 5;
            int timeoutSeconds = 60;                // si algo tarda más, lo marcamos TIMEOUT
            string outCsv = "tabla_tiempos.csv";
            string commitSHA = "poner_aqui_el_commit_SHA";

            Console.WriteLine($"Tamaño usado: {n} | Reps: {repeats} | Hilos: {string.Join(",", threadsToTest)}");
            Console.WriteLine($"Timeout por ejecución: {timeoutSeconds}s");
            Console.WriteLine();

            // Escribo cabecera (sobrescribe)
            using (var w = new StreamWriter(outCsv, false))
            {
                w.WriteLine("n,T_seq(s),T_p_2(s),T_p_4(s),T_p_8(s),Speedup_2,Speedup_4,Speedup_8,Eff_2,Eff_4,Eff_8,BestCostSeq,BestCostP2,BestCostP4,BestCostP8,StatusSeq,StatusP2,StatusP4,StatusP8,CommitSHA");
            }

            foreach (var size in sizes)
            {
                Console.WriteLine($"\n--- Ejecutando pruebas para n = {size} ---");

                // Usamos la matriz que vino desde Program
                var dist = distances;

                var times = new Dictionary<int, List<double>>();
                var bestCosts = new Dictionary<int, double>();
                var statuses = new Dictionary<int, string>();
                foreach (var t in threadsToTest) times[t] = new List<double>();

                for (int r = 0; r < repeats; r++)
                {
                    Console.WriteLine($" Repetición {r + 1}/{repeats}");

                    // Secuencial (threads == 1)
                    if (threadsToTest.Contains(1))
                    {
                        var (ok, time, cost, status) = RunWithTimeout(() =>
                        {
                            var res = TSPSolver.SolveSequential(dist);
                            return (res.BestCost, res.BestTour);
                        }, TimeSpan.FromSeconds(timeoutSeconds));

                        times[1].Add(time);
                        bestCosts[1] = double.IsNaN(cost) ? (bestCosts.ContainsKey(1) ? bestCosts[1] : double.NaN) : cost;
                        statuses[1] = status;
                        Console.WriteLine($"  Secuencial: {status} {(double.IsNaN(time) ? "" : $"{time:F4}s")} {(double.IsNaN(cost) ? "" : $"(cost {cost:F4})")}");
                    }

                    // Paralelo (cada configuración)
                    foreach (var th in threadsToTest.Where(x => x > 1))
                    {
                        var (ok, time, cost, status) = RunWithTimeout(() =>
                        {
                            var res = TSPSolver.SolveParallel(dist, th);
                            return (res.BestCost, res.BestTour);
                        }, TimeSpan.FromSeconds(timeoutSeconds));

                        times[th].Add(time);
                        bestCosts[th] = double.IsNaN(cost) ? (bestCosts.ContainsKey(th) ? bestCosts[th] : double.NaN) : cost;
                        statuses[th] = status;
                        Console.WriteLine($"  Paralelo {th} hilos: {status} {(double.IsNaN(time) ? "" : $"{time:F4}s")} {(double.IsNaN(cost) ? "" : $"(cost {cost:F4})")}");
                    }
                }

                // Calculo medianas ignorando NaN
                double T_seq = times.ContainsKey(1) ? MedianIgnoreNaN(times[1]) : double.NaN;
                double T_p_2 = times.ContainsKey(2) ? MedianIgnoreNaN(times[2]) : double.NaN;
                double T_p_4 = times.ContainsKey(4) ? MedianIgnoreNaN(times[4]) : double.NaN;
                double T_p_8 = times.ContainsKey(8) ? MedianIgnoreNaN(times[8]) : double.NaN;

                double s2 = ValidForDivision(T_seq, T_p_2) ? T_seq / T_p_2 : double.NaN;
                double s4 = ValidForDivision(T_seq, T_p_4) ? T_seq / T_p_4 : double.NaN;
                double s8 = ValidForDivision(T_seq, T_p_8) ? T_seq / T_p_8 : double.NaN;
                double e2 = double.IsNaN(s2) ? double.NaN : s2 / 2.0;
                double e4 = double.IsNaN(s4) ? double.NaN : s4 / 4.0;
                double e8 = double.IsNaN(s8) ? double.NaN : s8 / 8.0;

                double bestSeq = bestCosts.ContainsKey(1) ? bestCosts[1] : double.NaN;
                double best2 = bestCosts.ContainsKey(2) ? bestCosts[2] : double.NaN;
                double best4 = bestCosts.ContainsKey(4) ? bestCosts[4] : double.NaN;
                double best8 = bestCosts.ContainsKey(8) ? bestCosts[8] : double.NaN;

                // Guardar línea en CSV
                using (var w = new StreamWriter(outCsv, true))
                {
                    w.WriteLine($"{size},{FormatDouble(T_seq)},{FormatDouble(T_p_2)},{FormatDouble(T_p_4)},{FormatDouble(T_p_8)},{FormatDouble(s2)},{FormatDouble(s4)},{FormatDouble(s8)},{FormatDouble(e2)},{FormatDouble(e4)},{FormatDouble(e8)},{FormatDouble(bestSeq)},{FormatDouble(best2)},{FormatDouble(best4)},{FormatDouble(best8)},{statuses.GetValueOrDefault(1, "N/A")},{statuses.GetValueOrDefault(2, "N/A")},{statuses.GetValueOrDefault(4, "N/A")},{statuses.GetValueOrDefault(8, "N/A")},{commitSHA}");
                }

                Console.WriteLine($"Resultados guardados para n={size} en {outCsv}.");
            }

            Console.WriteLine("\nPruebas y métricas finalizadas.");
        }

        // Ejecuta la función y espera timeout. Devuelve (ok, time, bestCost, status)
        static (bool ok, double time, double bestCost, string status) RunWithTimeout(Func<(double bestCost, int[] tour)> action, TimeSpan timeout)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var task = Task.Run(() =>
                {
                    try
                    {
                        var r = action();
                        return (success: true, cost: r.bestCost);
                    }
                    catch (NotImplementedException)
                    {
                        return (success: false, cost: double.NaN);
                    }
                });

                bool finished = task.Wait(timeout);
                sw.Stop();

                if (!finished) return (false, double.NaN, double.NaN, "TIMEOUT");
                var res = task.Result;
                if (!res.success) return (false, double.NaN, double.NaN, "N/I");
                return (true, sw.Elapsed.TotalSeconds, res.cost, "OK");
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("  [Exception] " + ae.Flatten().InnerException?.Message);
                return (false, double.NaN, double.NaN, "ERROR");
            }
            catch (Exception ex)
            {
                Console.WriteLine("  [Exception] " + ex.Message);
                return (false, double.NaN, double.NaN, "ERROR");
            }
        }

        static bool ValidForDivision(double a, double b) => !(double.IsNaN(a) || double.IsNaN(b) || b == 0.0);
        static string FormatDouble(double d) => double.IsNaN(d) ? "NaN" : d.ToString("F6");
        static double MedianIgnoreNaN(List<double> list)
        {
            var clean = list.Where(x => !double.IsNaN(x)).OrderBy(x => x).ToArray();
            if (clean.Length == 0) return double.NaN;
            int m = clean.Length;
            if (m % 2 == 1) return clean[m / 2];
            return (clean[m / 2 - 1] + clean[m / 2]) / 2.0;
        }
    }
}
