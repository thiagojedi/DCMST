using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMSC_Exact
{
    class Program
    {

        int?[][] initial_matrix;
        int ordem;
        int degree;

        /// <summary>
        /// Construtor padrão. Armazena a matriz do arquivo passado.
        /// </summary>
        /// <param name="s">Caminho do arquivo de entrada</param>
        public Program(string s, int _degree)
        {
            this.ordem = 0;
            this.degree = _degree;

            Console.WriteLine("Árvore Geradora Mínima de Grau Restrito");
            Console.WriteLine("---------------------------------------");

            Console.WriteLine("Grau Máximo: {0}", degree);

            initial_matrix = this.CriaMatrix(s);

            Console.WriteLine("Ordem da Matriz: {0}", ordem);
            Console.WriteLine("Matriz:");

            PrintMatrix(initial_matrix, true);
        }

        /// <summary>
        /// Cria uma matriz quadrada a partir do arquivo de teste passado.
        /// </summary>
        /// <param name="filepath">Endereço do arquivo de teste. Pode ser relativo ou absoluto.</param>
        /// <returns>Matriz quadrada preenchida de acordo com o arquivo de teste.</returns>
        public int?[][] CriaMatrix(string filepath)
        {
            using (StreamReader testcase = new StreamReader(filepath))
            {
                string linha = testcase.ReadLine();
                ordem = int.Parse(linha);
                Debug.Print("Criando array");
                int?[][] matrix = InicializaMatrix(ordem);
                for (int i = 0; i < ordem; i++)
                {
                    string[] row = testcase.ReadLine().Split(' ');
                    for (int j = 0; j < i; j++)
                    {
                        matrix[i][j] = int.Parse(row[j]);
                        matrix[j][i] = matrix[i][j];
                    }
                }
                return matrix;
            }
        }

        /// <summary>
        /// Inicializa uma nova matriz quadrada de ordem <paramref name="o"/>.
        /// </summary>
        /// <param name="o">Ordem da matriz quadrada</param>
        /// <returns>Uma nova matriz quadrada</returns>
        private static int?[][] InicializaMatrix(int o)
        {
            int?[][] matrix = new int?[o][];
            for (int i = 0; i < o; i++)
            {
                matrix[i] = new int?[o];
            }
            return matrix;
        }


        /// <summary>
        /// Calcula uma Árvore de Cobertura com Grau Restrito utilizando o algoritmo de Prim. Não há garantia que é de custo mínimo.
        /// </summary>
        private List<Tuple<int,int>> PrimDCST()
        {
            int s = new Random().Next(0, ordem);

            Dictionary<int, int> vertexes_with_degrees = new Dictionary<int, int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();
            vertexes_with_degrees.Add(s, 1);

            while (vertexes_with_degrees.Count < ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                foreach (var item in vertexes_with_degrees)
                {
                    int vertex = item.Key;

                    for (int i = 0; i < ordem; i++)
                    {
                        if (initial_matrix[vertex][i] < min && !vertexes_with_degrees.ContainsKey(i) && vertexes_with_degrees[vertex] <= this.degree)
                        {
                            min = (int)initial_matrix[vertex][i];
                            start_node = vertex;
                            final_node = i;
                        }
                    }
                }
                vertexes_with_degrees[start_node] += 1;
                vertexes_with_degrees.Add(final_node, 1);
                if (start_node < final_node)
                    edges.Add(Tuple.Create(start_node, final_node));
                else
                    edges.Add(Tuple.Create(final_node, start_node));
            }
            PrintEdges("Prim's MST", edges);
            return edges;
        }

        void Branch()
        {
            var nxy = this.PrimDCST();
            List<Tuple<int, int>> x = new List<Tuple<int, int>>();
            List<Tuple<int, int>> y = new List<Tuple<int, int>>();

            Dictionary<int, int> vertex_with_degrees = new Dictionary<int, int>();

            foreach (var edge in nxy)
            {
                if (((vertex_with_degrees.ContainsKey(edge.Item1) && vertex_with_degrees[edge.Item1] > this.degree)) ||
                    ((vertex_with_degrees.ContainsKey(edge.Item2) && vertex_with_degrees[edge.Item2] > this.degree)))
                {
                    break;                    
                }
                x.Add(edge);
                if (vertex_with_degrees.ContainsKey(edge.Item1))
                    vertex_with_degrees[edge.Item1] += 1;
                else
                    vertex_with_degrees.Add(edge.Item1, 1);

                if (vertex_with_degrees.ContainsKey(edge.Item2))
                    vertex_with_degrees[edge.Item2] += 1;
                else
                    vertex_with_degrees.Add(edge.Item2, 1);
            }

        }




        static void PrintMatrix(int?[][] m, bool to_console = false)
        {
            StringBuilder sb_message = new StringBuilder();
            foreach (var i in m)
            {
                foreach (var j in i)
                {
                    if (null != j)
                        sb_message.AppendFormat("{0,4}", j);
                    else
                        sb_message.Append("   -");
                    sb_message.Append(" ");
                }
                sb_message.AppendLine();
            }
            if (to_console)
                Console.WriteLine(sb_message);
            //Debug.Print(sb_message.ToString());
        }

        static void PrintEdges(string name, List<Tuple<int, int>> l, bool to_console = false)
        {
            StringBuilder sb_message = new StringBuilder();
            sb_message.AppendFormat("{0} = ", name);
            sb_message.Append("{");
            foreach (var edge in l)
            {
                sb_message.AppendFormat("{0}-{1}, ", edge.Item1, edge.Item2);
            }
            sb_message.AppendLine("}");

            if(to_console)
                Console.WriteLine(sb_message);
            Debug.Print(sb_message.ToString());
        }

        int Cost(List<Tuple<int, int>> l)
        {
            int cost = 0;
            foreach (var edge in l)
            {
                cost += (int)initial_matrix[edge.Item1][edge.Item2];
            }
            Debug.Print("Custo da árvore = {0}", cost);
            return cost;
        }

        int NonCrescentCostSort(Tuple<int, int> t1, Tuple<int, int> t2)
        {
            int x = PenalityOfEdge(t1);
            int y = PenalityOfEdge(t2);
            return y.CompareTo(x);

        }

        int PenalityOfEdge(Tuple<int, int> t)
        {
            int min = int.MaxValue;
            int min_i = 0;
            int min_j = 0;
            for (int i = 0; i < ordem; i++)
            {
                if (i != t.Item1 && i != t.Item2)
                {
                    if ((int)initial_matrix[t.Item1][i] < min)
                    {
                        min = (int)initial_matrix[t.Item1][i];
                        min_i = t.Item1;
                        min_j = i;
                    }
                    if ((int)initial_matrix[t.Item2][i] < min)
                    {
                        min = (int)initial_matrix[t.Item2][i];
                        min_i = t.Item2;
                        min_j = i;
                    }
                }
            }
            int penality = (int)initial_matrix[t.Item1][t.Item2] - min;
            Debug.Print("Tuple {0}-{1}({2}) - Min {3}-{4}({5}) = {6}", t.Item1, t.Item2, (int)initial_matrix[t.Item1][t.Item2], min_i, min_j, min, penality);
            return penality;
        }


        static void Main(string[] args)
        {
            Program p = new Program(args[0], 3);

            p.PrimDCST();

            //Limite superior

            Console.Read();
        }

        
    }
}
