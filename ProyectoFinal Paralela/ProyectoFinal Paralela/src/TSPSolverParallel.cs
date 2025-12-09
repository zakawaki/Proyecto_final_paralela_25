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
        // Arreglo de arreglos: para cada ciudad (primera dimension) tenemos un arreglo con las ciudades vecinas ordenadas (segunda dimension)
        private int[][] _sortedNeighbors;

        public void Solve(double[,] distance)
        {
            _distance = distance; // Matriz de distancias entre ciudades
            _numCities = distance.GetLength(0); // Numero de ciudades

            // Calculamos una primera solucion rapida para tener una idea del costo minimo
            BestCost = GetGreedyInitialCost();
            BestRoute = new List<int>(); // Inicialmente vacia

            // Preparamos una lista de vecinos ordenados por cercania para cada ciudad
            // Esto ayuda a explorar primero las opciones mas prometedoras

            _sortedNeighbors = new int[_numCities][]; // Inicializamos el arreglo
            for (int i = 0; i < _numCities; i++) 
            {
                var neighbors = new List<int>();
                for (int j = 0; j < _numCities; j++)
                {
                    if (i != j) neighbors.Add(j);
                }
                // Ordenamos los vecinos desde el mas cercano al mas lejano
                neighbors.Sort((a, b) => _distance[i, a].CompareTo(_distance[i, b])); // Ordena las ciudades por distancia
                _sortedNeighbors[i] = neighbors.ToArray();
            }

            // Ejecutamos varios caminos posibles al mismo tiempo para ir mas rapido
            // Cada hilo empieza desde la ciudad 0 y elige una ciudad diferente como segunda ciudad

            Parallel.For(1, _numCities, nextCity =>
            {
                // Cada hilo usa su propia lista de ciudades visitadas
                bool[] localVisited = new bool[_numCities];
                localVisited[0] = true;
                localVisited[nextCity] = true;

                // Ruta local que va construyendo cada hilo
                List<int> localRoute = new List<int>(_numCities + 1);
                localRoute.Add(0); // Ciudad inicial
                localRoute.Add(nextCity); // Segunda ciudad 

                double currentCost = _distance[0, nextCity]; // Costo inicial

                Search(nextCity, currentCost, localVisited, localRoute);
            });
        }


        private void Search(int currentCity, double currentCost, bool[] visited, List<int> currentRoute)
        {
            // Si el costo ya es mayor que el mejor encontrado no vale la pena continuar
            if (currentCost >= BestCost) return;

            // Si ya visitamos todas las ciudades solo falta volver al inicio
            if (currentRoute.Count == _numCities) // 
            {
                double returnCost = _distance[currentCity, 0];
                double totalCost = currentCost + returnCost;

                // Si la ruta completa es mejor solcion la guardamos (
                if (totalCost < BestCost)
                {
                    lock (_lockObj)
                    {
                        // Si aun es mejor, se actualiza y copia la ruta actual
                        if (totalCost < BestCost)
                        {
                            BestCost = totalCost;
                            BestRoute = new List<int>(currentRoute);
                        }
                    }
                }
                return;
            }

            // Recorremos los vecinos en el orden mas corto primero
            foreach (int nextCity in _sortedNeighbors[currentCity])
            {
                // Solo exploramos ciudades NO visitadas
                if (!visited[nextCity])
                {
                    visited[nextCity] = true; 
                    currentRoute.Add(nextCity); 

                    // Avanzamos por el siguiente camino
                    Search(nextCity,
                           currentCost + _distance[currentCity, nextCity], // El costo va aumentando sumando la distancia
                           visited,
                           currentRoute);

                    // Retrocedemos para probar otra opcion
                    // Asi se podra probar otra ciudad despues
                    visited[nextCity] = false;
                    currentRoute.RemoveAt(currentRoute.Count - 1); // Quitamos la ruta actual
                }
            }
        }

        // Buscamos una ruta inicial simple eligiendo siempre la ciudad mas cercana
        private double GetGreedyInitialCost()
        {
            bool[] visited = new bool[_numCities];
            visited[0] = true; // comienza en la ciudad 0
            int currentCity = 0;
            double cost = 0;
            int citiesVisited = 1; // pq ya estamos en la ciudad 0

            // Elegimos siempre la ciudad NO visitada mas cercana
            while (citiesVisited < _numCities)
            {
                int nextCity = -1;
                double minDist = double.MaxValue;

                for (int i = 1; i < _numCities; i++)
                {
                    if (!visited[i] && _distance[currentCity, i] < minDist)
                    {
                        minDist = _distance[currentCity, i]; // Guardamos el nuevo record de distancia mas corta
                        nextCity = i; // Guardamos la ciudad mas cercana encontrada que por ahora es la mejor
                    }
                }

                // Avanzamos hacia la ciudad mas cercana
                if (nextCity != -1) // Vereficamos que se encontro una ciudad valida
                {
                    visited[nextCity] = true; // Marcamos como visitada para no volver a ella
                    cost += minDist; // Agregamos la distancia al costo total acomulado
                    currentCity = nextCity; // Actualizamos la ciudad actual
                    citiesVisited++; // Aumentamos el contador de ciudades visitadas
                }
            }

            // Volvemos a la ciudad inicial para cerrar la ruta
            cost += _distance[currentCity, 0]; // Regreso al inicio
            return cost;
        }
    }
}
