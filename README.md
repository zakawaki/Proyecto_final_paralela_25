# Optimización del Problema del Viajante (TSP) - Programación Paralela

Este proyecto implementa y compara soluciones para el **Problema del Viajante (Traveling Salesman Problem - TSP)** utilizando el algoritmo **Branch & Bound** en C\# (.NET 8.0).

El objetivo principal es demostrar la eficiencia de la computación paralela frente a la ejecución secuencial en problemas de optimización combinatoria de alta complejidad ($NP-Hard$).

## 📋 Información del Proyecto

  * **Institución:** Instituto Tecnológico de las Américas (ITLA)
  * **Materia:** Programación Paralela

## 🚀 Funcionalidades

El sistema permite resolver el TSP mediante dos enfoques:

1.  **Modo Secuencial:** Implementación recursiva clásica de *Branch & Bound*.
2.  **Modo Paralelo:** Implementación optimizada utilizando `Parallel.For` (TPL), memoria local por hilo y poda cooperativa mediante memoria compartida (`BestCost`).

**Características adicionales:**

  * **Generador de Escenarios:** Semillas fijas (para determinismo) o aleatorias.
  * **Heurísticas:** Inicialización *Greedy* (Vecino más cercano) y ordenamiento de vecinos para acelerar la poda.
  * **Benchmark Automático:** Módulo de pruebas que compara tiempos y calcula el *Speedup* entre ambas versiones.

## 🛠 Requisitos Técnicos

  * **SDK:** .NET 8.0
  * **IDE Recomendado:** Visual Studio 2022 o VS Code.

## 💻 Ejecución y Uso

1.  **Clonar el repositorio:**
    ```bash
    git clone https://github.com/zakawaki/Proyecto_final_paralela_25.git
    ```
2.  **Navegar al directorio del proyecto:**
    ```bash
    cd "ProyectoFinal Paralela/ProyectoFinal Paralela"
    ```
3.  **Ejecutar la aplicación:**
    ```bash
    dotnet run
    ```

### Menú Interactivo

Al iniciar, el programa solicitará:

1.  **Número de ciudades:** (Recomendado máx. 16 para secuencial).
      * *Atajo:* Presiona `p` para ejecutar el Benchmark automático directamente.
2.  **Tipo de Semilla:** Fija (1) o Aleatoria (2).
3.  **Modo de Ejecución:**
      * `1`: Algoritmo Secuencial.
      * `2`: Algoritmo Paralelo.
      * `3`: Pruebas y Métricas.

## 📊 Resultados de Rendimiento

Según las pruebas realizadas (Promedio de 3 corridas en 4 núcleos lógicos):

| Ciudades ($N$) | Tiempo Secuencial | Tiempo Paralelo | Speedup ($S$) |
| :---: | :---: | :---: | :---: |
| **8** | \~0.67 ms | \~18.33 ms | 0.04X (Overhead) |
| **12** | \~87.00 ms | \~26.67 ms | **3.26X** |
| **16** | \~47,584 ms | \~15,001 ms | **3.17X** |

> **Conclusión:** El paralelismo ofrece una mejora significativa (Speedup \> 3X) en escenarios complejos ($N \ge 12$), permitiendo resolver instancias que serían inviables secuencialmente en tiempos razonables.

## 📂 Estructura del Proyecto

  * `src/Datamodel.cs`: Definición de ciudades y generador de matrices de distancia.
  * `src/solverSecuential.cs`: Lógica Branch & Bound secuencial.
  * `src/TSPSolverParallel.cs`: Lógica paralela con manejo de hilos y sincronización (`lock`).
  * `metrics/BenchmarkRunner.cs`: Orquestador de pruebas de rendimiento.
  * `docs/`: Documentación técnica y académica del proyecto.
