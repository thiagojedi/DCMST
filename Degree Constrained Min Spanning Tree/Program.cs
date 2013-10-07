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

        int ordem;
        int degree;

        int?[][] initial_matrix;
        List<Tuple<int, int>> nxy;

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

            Helpers.PrintMatrix(initial_matrix, true);
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
                int?[][] matrix = Helpers.InicializaMatrix(ordem);
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
        /// Calcula uma Árvore de Cobertura com Grau Restrito utilizando o algoritmo de Prim. Não há garantia que é de custo mínimo.
        /// </summary>
        private List<Tuple<int, int>> PrimDCST()
        {
            int s = 0;

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
                Helpers.AddEdge(start_node, final_node, ref edges);
            }
            Helpers.PrintEdges("Prim's MST", edges);
            return edges;
        }

        void Branch()
        {
            nxy = this.PrimDCST();
            Helpers.PrintEdges("NXY", nxy);
            InsertionSortByPenalty(ref nxy);

            List<Tuple<int, int>> x = new List<Tuple<int, int>>();
            List<Tuple<int, int>> y = new List<Tuple<int, int>>();

            Dictionary<int, int> vertex_with_degrees = new Dictionary<int, int>();

            int aux_y = 0;

            foreach (var edge in nxy)
            {
                x.Add(edge);
                if (vertex_with_degrees.ContainsKey(edge.Item1))
                    vertex_with_degrees[edge.Item1] += 1;
                else
                    vertex_with_degrees.Add(edge.Item1, 1);

                if (vertex_with_degrees.ContainsKey(edge.Item2))
                    vertex_with_degrees[edge.Item2] += 1;
                else
                    vertex_with_degrees.Add(edge.Item2, 1);

                if (((vertex_with_degrees.ContainsKey(edge.Item1) && vertex_with_degrees[edge.Item1] >= this.degree)))
                {
                    aux_y = edge.Item1;
                    break;
                }
                if (((vertex_with_degrees.ContainsKey(edge.Item2) && vertex_with_degrees[edge.Item2] >= this.degree)))
                {
                    aux_y = edge.Item2;
                    break;
                }
            }

            Helpers.PrintEdges("X", x);

            for (int i = 0; i < ordem; i++)
            {
                bool ja_coberto = Helpers.CoveredEdge(x, i, aux_y);
                if (!ja_coberto && i != aux_y)
                    if (i < aux_y)
                        y.Add(Tuple.Create(i, aux_y));
                    else
                        y.Add(Tuple.Create(aux_y, i));

            }

            Helpers.PrintEdges("Y", y);

            CompleteMST(x, y);
        }

        List<Tuple<int,int>> CompleteMST(List<Tuple<int, int>> include, List<Tuple<int, int>> exclude)
        {
            Dictionary<int, int> vertex_with_degree = new Dictionary<int, int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>(include);

            foreach (var edge in edges)
            {
                if (!vertex_with_degree.ContainsKey(edge.Item1))
                    vertex_with_degree.Add(edge.Item1, 0);
                vertex_with_degree[edge.Item1] += 1;

                if (!vertex_with_degree.ContainsKey(edge.Item2))
                    vertex_with_degree.Add(edge.Item2, 0);
                vertex_with_degree[edge.Item2] += 1;
            }

            while (vertex_with_degree.Count < ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                foreach (var item in vertex_with_degree.OrderBy(x => x.Key))
                {
                    int vertex = item.Key;
                    for (int i = 0; i < ordem; i++)
                        if (initial_matrix[vertex][i] < min && !Helpers.CoveredEdge(exclude, vertex, i) && !vertex_with_degree.ContainsKey(i))
                        {
                            min = (int)initial_matrix[vertex][i];
                            start_node = vertex;
                            final_node = i;
                        }
                }
                vertex_with_degree[start_node] += 1;
                vertex_with_degree.Add(final_node, 1);

                Helpers.AddEdge(start_node, final_node, ref edges);
            }
            Helpers.PrintEdges("Nova MST", edges);

            return edges;
        }

        /// <summary>
        /// Função de ordenação da lista de arestas <paramref name="l"/> 
        /// com base na penalidade das arestas
        /// </summary>
        /// <param name="l">Lista a ser ordenada</param>
        void InsertionSortByPenalty(ref List<Tuple<int, int>> l)
        {
            Dictionary<Tuple<int, int>, int> dic = new Dictionary<Tuple<int, int>, int>();
            foreach (var edge in l)
            {
                dic.Add(edge, PenalityOfEdge(edge));
            }
            List<Tuple<int, int>> nl = new List<Tuple<int, int>>();
            foreach (var edge in l)
            {
                if (nl.Count == 0)
                    nl.Add(edge);
                else
                {
                    int i = 0;
                    while (dic[edge] > dic[nl[i]])
                        i++;
                    nl.Insert(i, edge);
                }
            }
            l = nl;
        }

        int PenalityOfEdge(Tuple<int, int> t)
        {
            int min = int.MaxValue;
            int min_i = 0;
            int min_j = 0;

            List<Tuple<int, int>> t0 = new List<Tuple<int, int>>(nxy);

            t0.Remove(t);

            List<int> vs_in_t1 = new List<int>();
            List<int> vs_in_t2 = new List<int>();

            vs_in_t1.Add(t.Item1);
            vs_in_t2.Add(t.Item2);

            while (t0.Count > 0)
            {
                var e = t0.First();
                if (vs_in_t1.Contains(e.Item1) && !vs_in_t1.Contains(e.Item2))
                    vs_in_t1.Add(e.Item2);
                else if (vs_in_t1.Contains(e.Item2) && !vs_in_t1.Contains(e.Item1))
                    vs_in_t1.Add(e.Item1);
                else if (vs_in_t2.Contains(e.Item1) && !vs_in_t2.Contains(e.Item2))
                    vs_in_t2.Add(e.Item2);
                else if (vs_in_t2.Contains(e.Item2) && !vs_in_t2.Contains(e.Item1))
                    vs_in_t2.Add(e.Item1);

                t0.Remove(e);
            }

            foreach (var x in vs_in_t1)
                foreach (var y in vs_in_t2)
                {
                    bool existe = Helpers.CoveredEdge(nxy, x, y);
                    if (!existe && min > (int)initial_matrix[x][y])
                    {
                        min = (int)initial_matrix[x][y];
                        min_i = x;
                        min_j = y;
                    }
                }

            int penality = (int)initial_matrix[t.Item1][t.Item2] - min;
            Debug.Print("Tuple {0}-{1}({2}) - Min {3}-{4}({5}) = {6}", t.Item1 + 1, t.Item2 + 1, (int)initial_matrix[t.Item1][t.Item2], min_i + 1, min_j + 1, min, penality);
            return penality;
        }



        static void Main(string[] args)
        {
            Program p = new Program(args[0], 3);

            p.Branch();

            Console.Read();
        }


    }
}
