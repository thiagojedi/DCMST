using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMSC_Exact
{
    class Helpers
    {
        /// <summary>
        /// Inicializa uma nova matriz quadrada de ordem <paramref name="o"/>.
        /// </summary>
        /// <param name="o">Ordem da matriz quadrada</param>
        /// <returns>Uma nova matriz quadrada</returns>
        public static int?[][] InicializaMatrix(int o)
        {
            int?[][] matrix = new int?[o][];
            for (int i = 0; i < o; i++)
                matrix[i] = new int?[o];
            return matrix;
        }

        /// <summary>
        /// Imprime uma matriz simétrica <paramref name="m"/> na tela ou na mensagem de Debug
        /// </summary>
        /// <param name="m">Matriz a simétrica a ser imprimida</param>
        /// <param name="to_console">Flag para mandar ou nao pro console</param>
        public static void PrintMatrix(int?[][] m, bool to_console = true)
        {
            StringBuilder sb_message = new StringBuilder();
            foreach (var i in m)
                foreach (var j in i)
                {
                    if (null != j)
                        sb_message.AppendFormat("{0,4}", j);
                    else
                        sb_message.Append("   -");
                    sb_message.Append(" ");
                }
            sb_message.AppendLine();
            if (to_console)
                Console.WriteLine(sb_message);
            else
                Debug.Print(sb_message.ToString());
        }

        /// <summary>
        /// Imprime na tela, de maneira ordenada a lista de arestas passada.
        /// <example><paramref name="name"/> = {0-1, 1-2, }</example>
        /// </summary>
        /// <param name="name">Nome da lista de arestas</param>
        /// <param name="l">A lista de arestas</param>
        /// <param name="to_console">Flag para dizer se deve imprimir na saída principal ou não</param>
        public static void PrintEdges(string name, List<Tuple<int, int>> l, bool to_console = false)
        {
            StringBuilder sb_message = new StringBuilder();
            sb_message.AppendFormat("{0} = ", name);
            sb_message.Append("{");

            foreach (var edge in l)
                sb_message.AppendFormat("{0}-{1}, ", edge.Item1 + 1, edge.Item2 + 1);

            sb_message.AppendLine("}");

            if (to_console)
                Console.WriteLine(sb_message);

            Debug.Print(sb_message.ToString());
        }

        /// <summary>
        /// Verifica se a aresta com os vértices <paramref name="a"/> e <paramref name="b"/>
        /// estão na lista <paramref name="l"/>
        /// </summary>
        /// <param name="l">Lista de arestas que deve cobrir ou não tais arestas</param>
        /// <param name="a">Um dos vértices da aresta a ser verificada</param>
        /// <param name="b">O outro vértice da aresta a ser verificada</param>
        /// <returns></returns>
        public static bool CoveredEdge(List<Tuple<int, int>> l, int a, int b)
        {
            return l.Exists(i => (i.Item1 == a && i.Item2 == b) || (i.Item1 == b && i.Item2 == a));
        }

        /// <summary>
        /// Calcula o custo da lista de arestas <paramref name="l"/> dentro do grafo
        /// representado pela matrix <paramref name="matrix"/>
        /// </summary>
        /// <param name="l">Lista de arestas</param>
        /// <param name="matrix">Representação matricial do grafo</param>
        /// <returns></returns>
        public static int Cost(List<Tuple<int, int>> l, int?[][] matrix)
        {
            int cost = 0;
            foreach (var edge in l)
                cost += (int)matrix[edge.Item1][edge.Item2];

            Debug.Print("Custo da árvore = {0}", cost);
            return cost;
        }
    }
}
