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
        List<List<Tuple<int, int>>> nxy;

        int upper_bound;

        List<int> left_bound;
        List<int> right_bound;

        int index;

        /// <summary>
        /// Construtor padrão. Armazena a matriz do arquivo passado.
        /// </summary>
        /// <param name="s">Caminho do arquivo de entrada</param>
        public Program(string s, int _degree)
        {
            #region Inicialização de Parametros Globais
            this.ordem = 0;
            this.index = 0;
            this.degree = _degree;
            this.nxy = new List<List<Tuple<int, int>>>();
            this.right_bound = new List<int>();
            this.left_bound = new List<int>();
            #endregion

            Console.WriteLine("Árvore Geradora Mínima de Grau Restrito");
            Console.WriteLine("---------------------------------------");

            Console.WriteLine("Grau Máximo: {0}", degree);

            initial_matrix = Helpers.CriaMatrix(s, ref ordem);

            Console.WriteLine("Ordem da Matriz: {0}", ordem);
            Console.WriteLine("Matriz:");

            Helpers.PrintMatrix(initial_matrix, true);
        }

        /// <summary>
        /// Calcula uma Árvore de Cobertura com Grau Restrito utilizando o algoritmo de Prim. Não há garantia que é de custo mínimo.
        /// </summary>
        /// <returns>Uma Árvore de Cobertura com Grau Restrito</returns>
        private List<Tuple<int, int>> PrimDegreeConstrained()
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
                        if (!vertexes_with_degrees.ContainsKey(i) && initial_matrix[vertex][i] < min && vertexes_with_degrees[vertex] < this.degree)
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
            Helpers.PrintEdges("Prim's DCST", edges);
            return edges;
        }

        List<Tuple<int, int>> Prim()
        {
            int s = 0;

            List<int> vertexes = new List<int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();

            vertexes.Add(s);

            while (vertexes.Count < ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                foreach (var vertex in vertexes)
                    for (int i = 0; i < ordem; i++)
                        if (!vertexes.Contains(i) && initial_matrix[vertex][i] < min)
                        {
                            min = (int)initial_matrix[vertex][i];
                            start_node = vertex;
                            final_node = i;
                        }
                vertexes.Add(final_node);
                Helpers.AddEdge(start_node, final_node, ref edges);
            }
            Helpers.PrintEdges("Prim's DCST", edges);
            return edges;
        }

        void BranchAndBound()
        {
            //Step 1
            upper_bound = Helpers.Cost(this.PrimDegreeConstrained(), initial_matrix);
            index = 0;

            //Step 2
            List<Tuple<int, int>> aux_nxy = Prim();
            right_bound.Insert(index, Helpers.Cost(aux_nxy, initial_matrix));
            left_bound.Insert(index, int.MaxValue);

            index += 1;

            List<Tuple<int, int>> x = new List<Tuple<int, int>>();
            List<Tuple<int, int>> y = new List<Tuple<int, int>>();
            Dictionary<int, int> vertex_with_degrees = new Dictionary<int, int>();

        Step3:
            InsertionSortByPenalty(ref aux_nxy);
            nxy.Add(new List<Tuple<int,int>>(aux_nxy));
            aux_nxy.RemoveAll(f => Helpers.CoveredEdge(x, f.Item1, f.Item2));

            int aux_y = 0;

            foreach (var edge in aux_nxy)
            {
                if (!Helpers.CoveredEdge(x, edge.Item1, edge.Item2))
                    x.Add(edge);

                if (!vertex_with_degrees.ContainsKey(edge.Item1))
                    vertex_with_degrees.Add(edge.Item1, 0);
                vertex_with_degrees[edge.Item1] += 1;

                if (!vertex_with_degrees.ContainsKey(edge.Item2))
                    vertex_with_degrees.Add(edge.Item2, 0);
                vertex_with_degrees[edge.Item2] += 1;

                if (vertex_with_degrees[edge.Item1] == this.degree)
                {
                    aux_y = edge.Item1;
                    break;
                }
                if (vertex_with_degrees[edge.Item2] == this.degree)
                {
                    aux_y = edge.Item2;
                    break;
                }
            }

            for (int i = 0; i < ordem; i++)
            {
                bool ja_coberto = Helpers.CoveredEdge(x, i, aux_y);
                if (!ja_coberto && i != aux_y)
                    if (i < aux_y)
                        y.Add(Tuple.Create(i, aux_y));
                    else
                        y.Add(Tuple.Create(aux_y, i));

            }

            aux_nxy = CompleteMST(x, y);

            Helpers.PrintEdges("Complete", aux_nxy, true);

            right_bound.Insert(index, Helpers.Cost(aux_nxy, initial_matrix));
            left_bound.Insert(index, right_bound[index - 1] - PenalityOfEdge(x.Last(), nxy[index - 1]));

            Console.WriteLine("R {0}", right_bound[index]);
            Console.WriteLine("L {0}", left_bound[index]);

        Step4:
            if (right_bound[index] < upper_bound)
                //Step 5
                if (Helpers.Possivel(aux_nxy, degree))
                    upper_bound = right_bound[index];
                else
                {
                    index += 1;
                    goto Step3;
                }

        Step6:
            if (left_bound[index] < upper_bound)
            {
                right_bound[index] = left_bound[index];

                //Step 7
                goto Step4;
            }
            else
            {
                //Step 8
                index -= 1;
                if (index > 0)
                    goto Step6;

            }

            Helpers.PrintEdges("Solução", aux_nxy, true);
            Console.WriteLine("Custo da Solução: {0}", Helpers.Cost(aux_nxy, initial_matrix));
        }

        /// <summary>
        /// Gera uma Árvore Geradora Mínima a partir de uma lista de arestas
        /// a serem incluídas e uma lista de arestas a serem excluídas
        /// </summary>
        /// <param name="include">Lista de arestas a serem incluídas</param>
        /// <param name="exclude">Lista de arestas a serem excluídas</param>
        /// <returns>Uma ãrvore geradora mínima</returns>
        List<Tuple<int, int>> CompleteMST(List<Tuple<int, int>> include, List<Tuple<int, int>> exclude)
        {
            List<int> vertexes = new List<int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>(include);

            foreach (var edge in edges)
            {
                if (!vertexes.Contains(edge.Item1))
                    vertexes.Add(edge.Item1);
                if (!vertexes.Contains(edge.Item2))
                    vertexes.Add(edge.Item2);
            }

            while (vertexes.Count < ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                vertexes.Sort();
                foreach (var vertex in vertexes)
                    for (int i = 0; i < ordem; i++)
                        if (initial_matrix[vertex][i] < min && !Helpers.CoveredEdge(exclude, vertex, i) && !vertexes.Contains(i))
                        {
                            min = (int)initial_matrix[vertex][i];
                            start_node = vertex;
                            final_node = i;
                        }
                vertexes.Add(final_node);

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
                dic.Add(edge, PenalityOfEdge(edge, l));
            List<Tuple<int, int>> nl = new List<Tuple<int, int>>();
            foreach (var edge in l)
                if (nl.Count == 0)
                    nl.Add(edge);
                else
                {
                    int i = 0;
                    while (i < nl.Count && dic[edge] > dic[nl[i]])
                        i++;
                    nl.Insert(i, edge);
                }
            l = nl;
        }

        /// <summary>
        /// Calcula a penalidade de remoção de uma aresta <paramref name="edge"/>
        /// </summary>
        /// <param name="edge">Aresta a ser calculada a penalidade</param>
        /// <returns>A penalidade da aresta</returns>
        int PenalityOfEdge(Tuple<int, int> edge, List<Tuple<int, int>> lista)
        {
            int min = int.MaxValue;
            int min_i = 0;
            int min_j = 0;

            List<Tuple<int, int>> t0 = new List<Tuple<int, int>>(lista);

            t0.Remove(edge);

            List<int> vs_in_t1 = new List<int>();
            List<int> vs_in_t2 = new List<int>();

            vs_in_t1.Add(edge.Item1);
            vs_in_t2.Add(edge.Item2);

            int i = 0;
            while (t0.Count > 0)
            {
                var e = t0[i];
                if (vs_in_t1.Contains(e.Item1) && !vs_in_t1.Contains(e.Item2))
                {
                    vs_in_t1.Add(e.Item2);
                    t0.Remove(e);
                    i = 0;
                }
                else if (vs_in_t1.Contains(e.Item2) && !vs_in_t1.Contains(e.Item1))
                {
                    vs_in_t1.Add(e.Item1);
                    t0.Remove(e);
                    i = 0;
                }
                else if (vs_in_t2.Contains(e.Item1) && !vs_in_t2.Contains(e.Item2))
                {
                    vs_in_t2.Add(e.Item2);
                    t0.Remove(e);
                    i = 0;
                }
                else if (vs_in_t2.Contains(e.Item2) && !vs_in_t2.Contains(e.Item1))
                {
                    vs_in_t2.Add(e.Item1);
                    t0.Remove(e);
                    i = 0;
                }
                else
                    i++;

            }

            foreach (var x in vs_in_t1)
                foreach (var y in vs_in_t2)
                {
                    bool existe = (edge.Item1 == x && edge.Item2 == y);
                    if (!existe && min > (int)initial_matrix[x][y])
                    {
                        min = (int)initial_matrix[x][y];
                        min_i = x;
                        min_j = y;
                    }
                }

            int penality = (int)initial_matrix[edge.Item1][edge.Item2] - min;
            Debug.Print("Tuple {0}-{1}({2}) - Min {3}-{4}({5}) = {6}", edge.Item1 + 1, edge.Item2 + 1, (int)initial_matrix[edge.Item1][edge.Item2], min_i + 1, min_j + 1, min, penality);
            return penality;
        }



        static void Main(string[] args)
        {
            Program p = new Program(args[0], 3);

            p.BranchAndBound();

            Console.Read();
        }


    }
}
