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
            double[] prob = new double[phero.GetLength(0)];

            double total = 0.0;
            for (int x = 0; x < phero.GetLength(0); x++)
            {
                prob[x] = phero[i, x] == null ? 0 : (double)phero[i, x];
                total += prob[x];
            }

            for (int x = 0; x < prob.Length; x++)
                if (i != x)
                    prob[x] = (prob[x] / total);

            int last = 0;
            for (int x = 1; x < prob.Length; x++)
                if (i != x)
                {
                    if (prob[last] != 0)
                        prob[x] += prob[last];
                    last = x;
                }
            double roleta = new Random().NextDouble() * (double)prob[last];

            int j = 0;
            for (int x = 0; x < prob.Length; x++)
                if (prob[x] != 0 && roleta <= (double)prob[x])
                {
                    j = x;
                    break;
                }

            return new Tuple<int, int>(i, j);
        }

        public static int[,] MatrizZero(int graph_size)
        {
            int[,] matriz = new int[graph_size, graph_size];
            for (int i = 0; i < graph_size; i++)
                for (int j = 0; j < graph_size; j++)
                {
                    matriz[i, j] = 0;
                }
            return matriz;
        }

        public static bool ContainsEdge(List<Tuple<int, int>> l, int i, int j)
        {
            if (i < j)
                return l.Contains(new Tuple<int, int>(i, j));
            else
                return l.Contains(new Tuple<int, int>(j, i));
        }

        public static bool ContainsEdge(List<Tuple<int, int>> l, Tuple<int, int> e)
        {
            return ContainsEdge(l, e.Item1, e.Item2);
        }

        public static int Cost(List<Tuple<int, int>> Tree, int?[,] WeightMatrix)
        {
            int cost = 0;
            foreach (var edge in Tree)
            {
                cost += (int)WeightMatrix[edge.Item1, edge.Item2];
            }
            return cost;
        }
    }


    #region Testes Unitários
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass()]
    public class HelperTest
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod()]
        public void TestContains()
        {
            List<Tuple<int, int>> lista = new List<Tuple<int, int>>();
            Tuple<int, int> t = new Tuple<int, int>(1, 2);
            lista.Add(t);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(Helpers.ContainsEdge(lista, 1, 2));
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod()]
        public void TestContainsInverted()
        {
            List<Tuple<int, int>> lista = new List<Tuple<int, int>>();
            Tuple<int, int> t = new Tuple<int, int>(1, 2);
            lista.Add(t);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(Helpers.ContainsEdge(lista, 2, 1));
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestMatrizSize()
        {
            int[,] matriz = Helpers.MatrizZero(5);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(5, matriz.GetLength(0));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(5, matriz.GetLength(1));
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestMatrizZeros()
        {
            int[,] matriz = Helpers.MatrizZero(5);
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(0, matriz[i, j]);
                }
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestTreeCost()
        {
            int?[,] matriz = { { null, 2, 3 }, { 2, null, 1 }, { 3, 1, null } };
            List<Tuple<int, int>> tree = new List<Tuple<int, int>>();
            tree.Add(new Tuple<int, int>(0, 1));
            tree.Add(new Tuple<int, int>(0, 2));
            tree.Add(new Tuple<int, int>(1, 2));

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(6, Helpers.Cost(tree, matriz));

        }
    }
    #endregion
}