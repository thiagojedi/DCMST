using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ants
{
    class Helpers
    {
        /// <summary>
        /// Seleciona uma aresta partindo do vertice <paramref name="i"/>.
        /// Utiliza-se do método da roleta
        /// </summary>
        /// <param name="i">Vértice inicial</param>
        /// <param name="phero">Matriz de Feromonios</param>
        /// <returns></returns>
        public static Tuple<int, int> SelectEdge(int i, double?[,] phero)
        {
            //TODO Rever essa roleta
            double?[] prob = new double?[phero.GetLength(0)];

            double total = 0.0;
            for (int x = 0; x < phero.GetLength(0); x++)
            {
                prob[x] = phero[i, x];
                total += phero[i, x] == null ? 0 : (double)phero[i, x];
            }
            int last = 0;
            for (int x = 0; x < prob.Length; x++)
                if (i != x)
                {
                    double y = prob[last] == null ? 0 : (double)prob[last];
                    prob[x] = y + (prob[x] / total);
                    last = x;
                }

            double roleta = new Random().NextDouble() * (double)prob[last];

            int j = 0;
            for (int x = 0; x < prob.Length; x++)
                if (prob[x] != null && roleta > (double)prob[x])
                    j = prob[x + 1] != null ? x + 1 : x + 2;

            return new Tuple<int, int>(i, j);
        }
    }
}