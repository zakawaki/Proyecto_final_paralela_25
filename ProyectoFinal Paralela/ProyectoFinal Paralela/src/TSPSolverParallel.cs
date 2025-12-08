using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSPProject
{
    public class TSPSolverParallel
    {
        public double BestCost { get; private set; }
        public List<int> BestRoute { get; private set; }

        private readonly object _lockObj = new object();
        private double[,] _distance;
        private int _numCities;

        // Guardamos para cada ciudad una lista de sus vecinos ordenados desde el mas cercano
        private int[][] _sortedNeighbors;

        public void Solve(double[,] distance)
        {
            _distance = distance;
            _numCities = distance.GetLength(0);

            // Calculamos una primera solucion rapida para tener una idea del costo minimo
            BestCost = GetGreedyInitialCost();
            BestRoute = new List<int>();

            // Preparamos una lista de vecinos ordenados por cercania para cada ciudad
            // Esto ayuda a explorar primero las opciones mas prometedoras
            _sortedNeighbors = new int[_numCities][]; //
            for (int i = 0; i < _numCities; i++)
            {
                var neighbors = new List<int>();
                for (int j = 0; j < _numCities; j++)
                {
                    if (i != j) neighbors.Add(j);
                }
                // Ordenamos los vecinos desde el mas cercano al mas lejano
                neighbors.Sort((a, b) => _distance[i, a].CompareTo(_distance[i, b]));
                _sortedNeighbors[i] = neighbors.ToArray();
            }

            // Ejecutamos varios caminos posibles al mismo tiempo para ir mas rapido
            Parallel.For(1, _numCities, nextCity =>
            {
                // Cada hilo usa su propia lista de ciudades visitadas
                bool[] localVisited = new bool[_numCities];
                localVisited[0] = true;
                localVisited[nextCity] = true;

                // Ruta local que va construyendo cada hilo
                List<int> localRoute = new List<int>(_numCities + 1);
                localRoute.Add(0);
                localRoute.Add(nextCity);

                double currentCost = _distance[0, nextCity];

                Search(nextCity, currentCost, localVisited, localRoute);
            });
        }

        private void Search(int currentCity, double currentCost, bool[] visited, List<int> currentRoute)
        {
            // Si el costo ya es mayor que el mejor encontrado no vale la pena continuar
            if (currentCost >= BestCost) return;

            // Si ya visitamos todas las ciudades solo falta volver al inicio
            if (currentRoute.Count == _numCities)
            {
                double returnCost = _distance[currentCity, 0];
                double totalCost = currentCost + returnCost;

                // Si la ruta completa es mejor la guardamos
                if (totalCost < BestCost)
                {
                    lock (_lockObj)
                    {
                        if (totalCost < BestCost)
                        {
                            BestCost = totalCost;
                            BestRoute = new List<int>(currentRoute);
                        }
                    }
                }
                return;
            }

            // Recorremos los vecinos en el orden mas prometedor primero
            foreach (int nextCity in _sortedNeighbors[currentCity])
            {
                if (!visited[nextCity])
                {
                    visited[nextCity] = true;
                    currentRoute.Add(nextCity);

                    // Avanzamos por el siguiente camino
                    Search(nextCity,
                           currentCost + _distance[currentCity, nextCity],
                           visited,
                           currentRoute);

                    // Retrocedemos para probar otra opcion
                    visited[nextCity] = false;
                    currentRoute.RemoveAt(currentRoute.Count - 1);
                }
            }
        }

        // Buscamos una ruta inicial simple eligiendo siempre la ciudad mas cercana
        private double GetGreedyInitialCost()
        {
            bool[] visited = new bool[_numCities];
            visited[0] = true;
            int currentCity = 0;
            double cost = 0;
            int citiesVisited = 1;

            // Elegimos siempre la ciudad NO visitada mas cercana
            while (citiesVisited < _numCities)
            {
                int nextCity = -1;
                double minDist = double.MaxValue;

                for (int i = 1; i < _numCities; i++)
                {
                    if (!visited[i] && _distance[currentCity, i] < minDist)
                    {
                        minDist = _distance[currentCity, i];
                        nextCity = i;
                    }
                }

                // Avanzamos hacia la ciudad mas cercana
                if (nextCity != -1)
                {
                    visited[nextCity] = true;
                    cost += minDist;
                    currentCity = nextCity;
                    citiesVisited++;
                }
            }

            // Volvemos a la ciudad inicial para cerrar la ruta
            cost += _distance[currentCity, 0];
            return cost;
        }
    }
}
