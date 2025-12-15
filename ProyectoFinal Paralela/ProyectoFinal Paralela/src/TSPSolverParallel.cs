using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TSPProject
{
    public class TSPSolverParallel
    {
        
        private double _bestCost;

        /// <summary>
        /// Obtiene o establece el mejor costo actual. 
        /// Utiliza operaciones Volatile para asegurar la visibilidad inmediata 
        /// del valor entre multiples hilos y evitar lecturas de cache obsoletas
        /// </summary>
        public double BestCost
        {
            get { return Volatile.Read(ref _bestCost); }
            private set { Volatile.Write(ref _bestCost, value); }
        }
        
        private readonly object _lockObj = new object();

        public List<int> BestRoute { get; private set; }

        
        private double[,] _distance;
        private int _numCities;

        // Guardamos para cada ciudad una lista de sus vecinos ordenados desde el mas cercano
        // Arreglo de arreglos: para cada ciudad (primera dimension) tenemos un arreglo con las ciudades vecinas ordenadas (segunda dimension)
        private int[][] _sortedNeighbors;

        public void Solve(double[,] distance)
        {
            _distance = distance; // Matriz de distancias entre ciudades
            _numCities = distance.GetLength(0); // Contamos cuantas ciudades hay

            // Calculamos una primera solucion rapida para tener una idea del costo minimo
            BestCost = GetGreedyInitialCost();
            BestRoute = new List<int>(); // Inicialmente vacia

            // Preparamos una lista de vecinos ordenados por cercania para cada ciudad
            // Esto ayuda a explorar primero las opciones mas prometedoras

            _sortedNeighbors = new int[_numCities][]; // Inicializamos el arreglo
            for (int i = 0; i < _numCities; i++) // i representa la ciudad actual 
            {
                var neighbors = new List<int>(); // Lista temporal
                for (int j = 0; j < _numCities; j++)
                {
                    // Nos aseguramos dee que una ciudad no se agregue asi misma como vecina
                    if (i != j) neighbors.Add(j);
                }
                // Ordenamos los vecinos desde el mas cercano al mas lejano
                neighbors.Sort((a, b) => _distance[i, a].CompareTo(_distance[i, b])); // Ordena las ciudades por distancia
                _sortedNeighbors[i] = neighbors.ToArray(); // Lo convertimos en arreglo ya ordenado
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
                List<int> localRoute = new List<int>(_numCities + 1); // Se crea una lista, cada entero es un ID de la ciudad
                localRoute.Add(0); // Ciudad inicial
                localRoute.Add(nextCity); // Segunda ciudad 

                double currentCost = _distance[0, nextCity]; // Costo inicial

                Search(nextCity, currentCost, localVisited, localRoute); 
            });
        }

        /// <summary>
        /// ees una funcion que se llama asi misma una y ota vez para explorar caminos,
        /// pero tambien es lo suficiente inteligente para dejar de buscar si el camino va mal.
        /// Es como una mezcla entre Backtracking y Branch and Bound
        /// </summary>

        private void Search(int currentCity, double currentCost, bool[] visited, List<int> currentRoute)
        {
            // Si el costo ya es mayor que el mejor encontrado no vale la pena continuar
            if (currentCost >= BestCost) return;

            // Si ya visitamos todas las ciudades solo falta volver al inicio
            if (currentRoute.Count == _numCities) // Esto see ejecuta cuando la lista de ruta tenga tantas ciudades como el total
            {
                double returnCost = _distance[currentCity, 0]; // Calculamos la distancia de mi ciudad actual dee vuelta a la de origen
                double totalCost = currentCost + returnCost; // Precio final de la ruta

                // Si la ruta completada es mejor solucion la guardamos 
                if (totalCost < BestCost)
                {
                    lock (_lockObj) // Solo un hilo entra aqui a la vez
                    {
                        // Si aun es mejor, se actualiza y copia la ruta actual
                        if (totalCost < BestCost)
                        {
                            BestCost = totalCost; 
                            BestRoute = new List<int>(currentRoute); // Guardamos la ruta ganadora
                        }
                    }
                }
                return;
            }

            // Si no heemos terminado ni hemos pasado el costo, seguimos buscando
            // Recorremos los vecinos en el orden mas corto primero

            foreach (int nextCity in _sortedNeighbors[currentCity]) 
            {
                // Solo exploramos ciudades NO visitadas
                if (!visited[nextCity]) // Si no ha ido a esta ciudad
                {
                    visited[nextCity] = true; // La marcamos como verdadera
                    currentRoute.Add(nextCity); // La agregamos en el "mapa"

                    // Avanzamos por el siguiente camino
                    // "Saltamos" a la siguiente ciudad para explorar sus conexiones,
                    // arrastrando con nosotros el costo actualizado hasta este punto
                    Search(nextCity,
                           currentCost + _distance[currentCity, nextCity], // El costo va aumentando sumando la distancia
                           visited,
                           currentRoute);

                    //-----BACKTRACKING-----
                    // Marcamos como "no visitada" y la quitamos de la ruta actual
                    // para dejar el estado limpio y poder probar otro camino diferente en el bucl
                    visited[nextCity] = false;
                    currentRoute.RemoveAt(currentRoute.Count - 1); // Quitamos la ruta actual
                }
            }
        }

        // Buscamos una ruta inicial simple eligiendo siempre la ciudad mas cercana (el vecino mas cercano)
        private double GetGreedyInitialCost()
        {
            bool[] visited = new bool[_numCities]; // Para recordar quee ciudades ya visitamos y no volver a ellas
            visited[0] = true; // comienza en la ciudad 0
            int currentCity = 0;
            double cost = 0;
            int citiesVisited = 1; // pq ya estamos en la ciudad 0

            // Elegimos siempre la ciudad NO visitada mas cercana
            while (citiesVisited < _numCities)
            {
                int nextCity = -1;
                double minDist = double.MaxValue; // 

                for (int i = 1; i < _numCities; i++)
                {
                    // si la ciudad i NO ha sido visitada y la distancia es menor que el record actual
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
                    cost += minDist; // Sumamos la distancia al costo total acomulado
                    currentCity = nextCity; // Nos movemos a esta ciudad
                    citiesVisited++; // Aumentamos el contador de ciudades visitadas
                }
            }

            // Volvemos a la ciudad inicial para cerrar la ruta
            cost += _distance[currentCity, 0]; // Regreso al inicio
            return cost;
        }
    }
}
