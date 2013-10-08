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
            this.right_bound = new List<int>();
            this.left_bound = new List<int>();
            #endregion

            Console.WriteLine("Grau Máximo: {0}", degree);

            this.initial_matrix = Helpers.CriaMatrix(s, ref ordem);

            Console.WriteLine("Ordem da Matriz: {0}", ordem);
            //Console.WriteLine("Matriz:");

            Helpers.PrintMatrix(this.initial_matrix, false);
        }

        /// <summary>
        /// Calcula uma Árvore de Cobertura com Grau Restrito utilizando o algoritmo de Prim. Não há garantia que é de custo mínimo.
        /// </summary>
        /// <returns>Uma Árvore de Cobertura com Grau Restrito</returns>
        private List<Tuple<int, int>> PrimDegreeConstrained(int s)
        {
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
                        if (!vertexes_with_degrees.ContainsKey(i) && this.initial_matrix[vertex][i] < min && vertexes_with_degrees[vertex] < this.degree)
                        {
                            min = (int)this.initial_matrix[vertex][i];
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

        List<Tuple<int, int>> Prim(int s)
        {
            List<int> vertexes = new List<int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();

            vertexes.Add(s);

            while (vertexes.Count < this.ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                foreach (var vertex in vertexes)
                    for (int i = 0; i < this.ordem; i++)
                        if (!vertexes.Contains(i) && this.initial_matrix[vertex][i] < min)
                        {
                            min = (int)this.initial_matrix[vertex][i];
                            start_node = vertex;
                            final_node = i;
                        }
                vertexes.Add(final_node);
                Helpers.AddEdge(start_node, final_node, ref edges);
            }
            Helpers.PrintEdges("Prim's MST", edges);
            return edges;
        }

        void GenerateXY(ref List<Tuple<int, int>> x, ref List<Tuple<int, int>> y, List<Tuple<int, int>> nxy)
        {
            int node_to_y = 0;
            Dictionary<int, int> vertex_with_degrees = new Dictionary<int, int>();
            foreach (var edge in x)
            {
                if (!vertex_with_degrees.ContainsKey(edge.Item1))
                    vertex_with_degrees.Add(edge.Item1, 0);
                vertex_with_degrees[edge.Item1] += 1;

                if (!vertex_with_degrees.ContainsKey(edge.Item2))
                    vertex_with_degrees.Add(edge.Item2, 0);
                vertex_with_degrees[edge.Item2] += 1;
            }

            foreach (var edge in nxy)
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
                    node_to_y = edge.Item1;
                    break;
                }
                if (vertex_with_degrees[edge.Item2] == this.degree)
                {
                    node_to_y = edge.Item2;
                    break;
                }
            }
            for (int i = 0; i < ordem; i++)
            {
                bool ja_coberto = Helpers.CoveredEdge(x, i, node_to_y) || Helpers.CoveredEdge(y, i, node_to_y);
                if (!ja_coberto && i != node_to_y)
                    if (i < node_to_y)
                        y.Add(Tuple.Create(i, node_to_y));
                    else
                        y.Add(Tuple.Create(node_to_y, i));

            }
        }

        void BranchAndBound()
        {
            //int start = new Random().Next(ordem);
            int start = ordem-1;

            //Step 1
            upper_bound = Helpers.Cost(this.PrimDegreeConstrained(start), this.initial_matrix);
            Console.WriteLine("Upper bound inicial: {0}", upper_bound);
            index = 0;

            //Step 2
            List<Tuple<int, int>> not_in_xy = Prim(start);
            right_bound.Insert(index, Helpers.Cost(not_in_xy, this.initial_matrix));
            left_bound.Insert(index, int.MaxValue);

            index += 1;

            List<List<Tuple<int, int>>> spanning_trees = new List<List<Tuple<int, int>>>();
            List<Tuple<int, int>> x = new List<Tuple<int, int>>();
            List<Tuple<int, int>> y = new List<Tuple<int, int>>();

        Step3:
            InsertionSortByPenalty(ref not_in_xy);
            not_in_xy.RemoveAll(f => Helpers.CoveredEdge(x, f.Item1, f.Item2));

            List<Tuple<int, int>> old_y = new List<Tuple<int, int>>(y);
            GenerateXY(ref x, ref y, not_in_xy);

            Helpers.PrintEdges("X: ", x);
            Helpers.PrintEdges("Y: ", y);

            var complete = CompleteMST(x, y);
            spanning_trees.Add(complete);

            right_bound.Insert(index, Helpers.Cost(complete, this.initial_matrix));
            left_bound.Insert(index, right_bound[index - 1] + PenaltyOfEdge(x.Last(), spanning_trees[index - 1]));

            Debug.Print("Right Bound {1}: {0}", right_bound[index], index);
            Debug.Print("Left Bound {1}: {0}", left_bound[index], index);

        Step4:
            if (right_bound[index] < upper_bound)
                //Step 5
                if (Helpers.Possivel(complete, degree))
                    upper_bound = right_bound[index];
                else
                {
                    index += 1;
                    not_in_xy = complete;
                    goto Step3;
                }

        Step6:
            if (left_bound[index] < upper_bound)
            {
                right_bound[index] = left_bound[index];

                //Step 7
                Tuple<int, int> ult_x = x.Last();
                x.Remove(ult_x);
                if (x.Count > 0)
                {
                    List<Tuple<int, int>> outro_nxy = new List<Tuple<int, int>>(not_in_xy);
                    outro_nxy.Remove(ult_x);

                    GenerateXY(ref x, ref old_y, outro_nxy);
                    left_bound[index] = Helpers.Cost(CompleteMST(x, y), this.initial_matrix);
                }
                else
                    left_bound[index] = int.MaxValue;
                goto Step4;
            }
            else
            {
                //Step 8
                index -= 1;
                if (index > -1)
                    goto Step6;

            }

            Helpers.PrintEdges("Solução", spanning_trees.Last(), true);
            Console.WriteLine("Custo da Solução: {0}", Helpers.Cost(spanning_trees.Last(), this.initial_matrix));
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

            vertexes.Sort();

            while (vertexes.Count < ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                vertexes.Sort();
                foreach (var vertex in vertexes)
                    for (int i = 0; i < ordem; i++)
                        if (this.initial_matrix[vertex][i] < min && !Helpers.CoveredEdge(exclude, vertex, i) && !vertexes.Contains(i))
                        {
                            min = (int)this.initial_matrix[vertex][i];
                            start_node = vertex;
                            final_node = i;
                        }
                vertexes.Add(final_node);

                Helpers.AddEdge(start_node, final_node, ref edges);
            }
            //Helpers.PrintEdges("Nova MST", edges);

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
                dic.Add(edge, PenaltyOfEdge(edge, l));
            List<Tuple<int, int>> nl = new List<Tuple<int, int>>();
            foreach (var edge in l)
                if (nl.Count == 0)
                    nl.Add(edge);
                else
                {
                    int i = 0;
                    while (i < nl.Count && dic[edge] < dic[nl[i]])
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
        int PenaltyOfEdge(Tuple<int, int> edge, List<Tuple<int, int>> tree)
        {
            int min = Helpers.Cost(tree, this.initial_matrix);

            List<Tuple<int, int>> t1 = new List<Tuple<int, int>>();
            List<Tuple<int, int>> t2 = new List<Tuple<int, int>>(tree);
            t1.Add(edge);
            t2.Remove(edge);
            int max = Helpers.Cost(CompleteMST(t2, t1), this.initial_matrix);
            
            int penality = Math.Abs(max - min);
            Debug.Print("Penalidade de Tuple {0}-{1} = {2}", edge.Item1 + 1, edge.Item2 + 1, penality);
            return penality;
        }



        static void Main(string[] args)
        {
            Console.WriteLine("Árvore Geradora Mínima de Grau Restrito");
            Console.WriteLine("---------------------------------------");

            Console.WriteLine("Digite o nome do arquivo: ");
            string arquivo = Console.ReadLine();
            if (arquivo.Contains("40"))
                Console.WriteLine("Digite o grau máximo [3, 5 ou 10]: ");
            else
                Console.WriteLine("Digite o grau máximo [5, 10, 20]: ");
            int grau = int.Parse(Console.ReadLine());

            if (!arquivo.Contains(".txt"))
                arquivo = arquivo + ".txt";

            //for (int i = 0; i < 5; i++)
            //{
                DateTime start = DateTime.Now;
                Program p = new Program(arquivo, grau);

                p.BranchAndBound();

                Console.WriteLine("Calculado em {0}s", (DateTime.Now - start).TotalSeconds);

                Console.WriteLine();
            //}

            Console.Read();
        }


    }
}
