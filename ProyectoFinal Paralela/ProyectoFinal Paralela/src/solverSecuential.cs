using System;
using System.Collections.Generic;

// REALIZADO POR IMANOL RODRIGUEZ
// SOLUCIÓN SECUENCIAL DE LA PODA
// PROCEDO A EXPLICAR EL CODIGO POR BLOQUE 
// METIENDO MANO

namespace TSPProject
{
    public class TSPSolverSequential
    {
        // --- PROPIEDADES PÚBLICAS ACCESIBLES DESDE EL MAIN ---
        public double BestCost { get; private set; }
        public List<int> BestRoute { get; private set; }

        // --- VARIABLES INTERNAS DEL SOLVER ---
        private double[,] _distances;
        private int _numCities;

        // --------------------------------------------------------
        // MÉTODO PRINCIPAL DE ENTRADA
        // --------------------------------------------------------
        public void Solve(double[,] distances)
        {
            _distances = distances;
            _numCities = distances.GetLength(0);

            BestCost = double.MaxValue;
            BestRoute = new List<int>();

            bool[] visited = new bool[_numCities];
            visited[0] = true;

            List<int> currentRoute = new List<int>() { 0 };

            Search(0, 0.0, visited, currentRoute);
        }

        // --------------------------------------------------------
        // BACKTRACKING + BRANCH & BOUND
        // --------------------------------------------------------
        private void Search(int currentCity, double currentCost, bool[] visited, List<int> currentRoute)
        {
            // 1. PODA: si ya estamos peor que el récord global, descartamos
            if (currentCost >= BestCost)
                return;

            // 2. CASO BASE: ¿visitamos todas?
            if (currentRoute.Count == _numCities)
            {
                double totalCost = currentCost + _distances[currentCity, 0];

                if (totalCost < BestCost)
                {
                    BestCost = totalCost;
                    BestRoute = new List<int>(currentRoute); // copia profunda
                }
                return;
            }

            // 3. INTENTAR TODAS LAS CIUDADES NO VISITADAS
            for (int nextCity = 0; nextCity < _numCities; nextCity++)
            {
                if (!visited[nextCity])
                {
                    visited[nextCity] = true;
                    currentRoute.Add(nextCity);

                    double newCost = currentCost + _distances[currentCity, nextCity];

                    Search(nextCity, newCost, visited, currentRoute);

                    // BACKTRACK
                    visited[nextCity] = false;
                    currentRoute.RemoveAt(currentRoute.Count - 1);
                }
            }
        }
    }
}
