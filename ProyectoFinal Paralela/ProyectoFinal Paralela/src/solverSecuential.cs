using System;
using System.Collections.Generic;

namespace TSPProject
{

    public class TSPSolverSequential
    {

        // BLOQUE 1 — PROPIEDADES PÚBLICAS
        // Aqui van las variables publicas donde se guardan los resultados del algoritmo, en este caso BestCost almacena el costo mínimo encontrado.
        // y BestRoute la ruta que es la lista de ciudades correspondiente al mejor costo.
        public double BestCost { get; private set; }
        public List<int> BestRoute { get; private set; }


        // BLOQUE 2 — VARIABLES INTERNAS
        // _distances es la matriz de distancias entre ciudades.
        // _numCities  es el número total de ciudades, derivado de la matriz.
        // ¿Qué es? " [,] ", es un arreglo bidimensional , que guarda qué tan lejos está cada ciudad de la otra.
        // Estas variables solo se usan dentro de esta clase.

        private double[,] _distances;
        private int _numCities;


        // BLOQUE 3 — MÉTODO PRINCIPAL Solve()
        // Este método inicializa el solver y arranca la búsqueda.
        // Pasos principales:
        //   1) Guarda la matriz de distancias y calculas cuántas ciudades hay.
        //   2) Inicializa el mejor costo como infinito.
        //   3) Prepara estructuras de seguimiento (visited y currentRoute).
        //   4) Llama al método Search() que ejecuta el algoritmo recursivo.

        public void Solve(double[,] distances)
        {
            _distances = distances;                 // Guardamos la matriz de distancias
            _numCities = distances.GetLength(0);    // Total de ciudades (filas)

            BestCost = double.MaxValue;             // AL iniciar no tenemos un mejor costo ni
            BestRoute = new List<int>();            // tenemos ruta

            bool[] visited = new bool[_numCities];  // Arreglo que indica qué ciudades se han visitado
            visited[0] = true;                      // Empezamos en la ciudad 0

            List<int> currentRoute = new List<int>() { 0 };  // Lista ruta actual, ruta arranca en la ciudad 0

            // Iniciar la búsqueda desde la ciudad 0 con costo 0
            Search(0, 0.0, visited, currentRoute);
        }


        // BLOQUE 4— MÉTODO Search(): BACKTRACKING + BRANCH & BOUND
        // Este método recorre todas las rutas posibles usando recursión, pero
        // descarta rutas que ya son más costosas que la mejor encontrada, esta es la poda.

        // Parámetros:
        //   currentCity es la ciudad actual del recorrido.
        //   currentCost  es el costo acumulado hasta llegar a currentCity.
        //   visited es el arreglo que indica qué ciudades se han visitado.
        //   currentRoute es la lista que representa la ruta actual.

        private void Search(int currentCity, double currentCost, bool[] visited, List<int> currentRoute)
        {
            // BLOQUE 5 — PODA (BRANCH & BOUND)
            // Si el costo actual ya es mayor o igual que el mejor costo encontrado,
            // no vale la pena seguir explorando este camino.

            if (currentCost >= BestCost)
                return;

            // BLOQUE 6 — CASO BASE: TODAS LAS CIUDADES VISITADAS
            // Si la ruta actual contiene todas las ciudades, falta sumar el costo de volver a la ciudad inicial (0). Luego comparamos el costo total
            // contra el mejor registro y lo actualizamos si es necesario.

            if (currentRoute.Count == _numCities)
            {
                // Costo total volviendo al inicio
                double totalCost = currentCost + _distances[currentCity, 0];

                // Actualizar mejor ruta si encontramos una solución más barata
                if (totalCost < BestCost)
                {
                    BestCost = totalCost;
                    BestRoute = new List<int>(currentRoute); // Copia de la ruta
                }
                return;
            }

            // BLOQUE 8 — EXPLORAR TODAS LAS CIUDADES POSIBLES (BACKTRACKING)
            // Intentamos ir a cada ciudad no visitada:
            //   - La marcamos como visitada.
            //   - La agregamos a la ruta actual.
            //   - Recalculamos el costo y seguimos explorando recursivamente.
            //   - Luego deshacemos los cambios (backtracking).
==
            for (int nextCity = 0; nextCity < _numCities; nextCity++)
            {
                if (!visited[nextCity])  // Solo vale intentar las ciudades no visitadas
                {
                    visited[nextCity] = true;            // Marcamos como visitada
                    currentRoute.Add(nextCity);          // Añadimos a la ruta

                    double newCost = currentCost + _distances[currentCity, nextCity];  // Nuevo costo parcial

                    Search(nextCity, newCost, visited, currentRoute); // Exploración recursiva

                    // --- BACKTRACK: revertimos cambios ---
                    visited[nextCity] = false;           // Desmarcamos ciudad
                    currentRoute.RemoveAt(currentRoute.Count - 1);  // Quitamos la ciudad añadida. Esto le dice al sistema de rastreo
                                                                    // que la ciudad que acabamos de explorar está disponible para
                                                                    // ser visitada de nuevo por otra rama de la exploración.
                }
            }
        }
    }
}
