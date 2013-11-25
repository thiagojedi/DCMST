using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DCMSC_Exact
{
    class Program
    {

        int ordem;
        int degree;

        int?[,] initial_matrix;

        public int upper_bound;

        List<int> left_bound;
        List<int> right_bound;

        int index;

        List<Tuple<int, int>> all_edges_by_weight;

        public List<Tuple<int, int>> best_solution;

        /// <summary>
        /// Construtor padrão. Armazena a matriz do arquivo passado.
        /// </summary>
        /// <param name="s">Caminho do arquivo de entrada</param>
        /// <param name="_degree">Grau máximo</param>
        /// <param name="_ordem">Tamanho do grafo</param>
        public Program(string s, int _degree, int _ordem)
        {
            #region Inicialização de Parametros Globais
            this.ordem = _ordem;
            this.index = 0;
            this.degree = _degree;
            this.right_bound = new List<int>();
            this.left_bound = new List<int>();
            #endregion

            Console.WriteLine("Grau Máximo: {0}", degree);

            this.initial_matrix = Helpers.CriaMatrix(s, ref ordem);

            Console.WriteLine("Ordem da Matriz: {0}", ordem);
            //Console.WriteLine("Matriz:");

            //Helpers.PrintMatrix(this.initial_matrix, false);

            this.all_edges_by_weight = new List<Tuple<int, int>>();
            for (int i = 0; i < ordem; i++)
                for (int j = i + 1; j < ordem; j++)
                    all_edges_by_weight.Add(new Tuple<int, int>(i, j));
            Helpers.QSort<int>(all_edges_by_weight, initial_matrix, 0, all_edges_by_weight.Count - 1);
        }

        /// <summary>
        /// Calcula uma Árvore de Cobertura com Grau Restrito utilizando o algoritmo de Prim. Não há garantia que é de custo mínimo.
        /// </summary>
        /// <returns>Uma Árvore de Cobertura com Grau Restrito</returns>
        private List<Tuple<int, int>> PrimDegreeConstrained(int s)
        {
            Dictionary<int, int> vertexes_with_degrees = new Dictionary<int, int>();
            List<int> not_inserted = new List<int>();
            for (int i = 0; i < ordem; i++)
                not_inserted.Add(i);
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();

            vertexes_with_degrees.Add(s, 0);
            not_inserted.Remove(s);

            while (vertexes_with_degrees.Count < this.ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                foreach (var vertex in vertexes_with_degrees.OrderBy(x => x.Key))
                    foreach (int i in not_inserted)
                        if (this.initial_matrix[vertex.Key, i] < min && vertex.Value < this.degree)
                        {
                            min = (int)this.initial_matrix[vertex.Key, i];
                            start_node = vertex.Key;
                            final_node = i;
                        }
                vertexes_with_degrees[start_node] += 1;
                vertexes_with_degrees.Add(final_node, 1);
                not_inserted.Remove(final_node);
                Helpers.AddEdge(start_node, final_node, ref edges);
            }
            Helpers.PrintEdges("Prim's DCST", edges);
            return edges;
        }

        /// <summary>
        /// Algoritmo de Prim. 
        /// </summary>
        /// <param name="s">Nó inicial</param>
        /// <returns>Retorna uma Árvore de Cobertura Mínima sem restrição de grau.</returns>
        List<Tuple<int, int>> Prim(int s)
        {
            List<int> vertexes = new List<int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();
            List<int> not_inserted = new List<int>();
            for (int i = 0; i < ordem; i++)
                not_inserted.Add(i);

            vertexes.Add(s);
            not_inserted.Remove(s);

            while (vertexes.Count < this.ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                vertexes.Sort();
                foreach (var vertex in vertexes)
                    foreach (int i in not_inserted)
                        if (this.initial_matrix[vertex, i] < min)
                        {
                            min = (int)this.initial_matrix[vertex, i];
                            start_node = vertex;
                            final_node = i;
                        }
                vertexes.Add(final_node);
                not_inserted.Remove(final_node);
                Helpers.AddEdge(start_node, final_node, ref edges);
            }
            Helpers.PrintEdges("Prim's MST", edges);
            return edges;
        }

        void GenerateXY(List<Tuple<int, int>> x, List<Tuple<int, int>> y, List<Tuple<int, int>> nxy)
        {
            List<int> node_to_y = new List<int>();
            Dictionary<int, int> vertex_with_degrees = new Dictionary<int, int>();
            for (int i = 0; i < ordem; i++)
                vertex_with_degrees.Add(i, 0);
            foreach (var edge in x)
            {
                vertex_with_degrees[edge.Item1] += 1;
                vertex_with_degrees[edge.Item2] += 1;
            }

            while (nxy.Count > 0)
            {
                var edge = nxy.First();
                nxy.Remove(edge);
                if ((vertex_with_degrees[edge.Item1] < this.degree) &&
                    (vertex_with_degrees[edge.Item2] < this.degree))
                {
                    x.Add(edge);

                    vertex_with_degrees[edge.Item1] += 1;
                    vertex_with_degrees[edge.Item2] += 1;

                    if (vertex_with_degrees[edge.Item1] == this.degree)
                        node_to_y.Add(edge.Item1);
                    if (vertex_with_degrees[edge.Item2] == this.degree)
                        node_to_y.Add(edge.Item2);
                    if (node_to_y.Count > 0)
                        break;
                }
            }
            List<Tuple<int, int>> new_y = new List<Tuple<int, int>>();
            foreach (var i in node_to_y)
                for (int j = 0; j < ordem; j++)
                {
                    bool ja_coberto = Helpers.CoveredEdge(x, j, i) || Helpers.CoveredEdge(y, j, i);
                    if (!ja_coberto && j != i)
                        Helpers.AddEdge(j, i, ref new_y);

                }

            y.AddRange(new_y);
        }

        void Branch(List<Tuple<int, int>> nxy, List<Tuple<int, int>> x, List<Tuple<int, int>> y, int index)
        {
            List<Tuple<int, int>> neutral = new List<Tuple<int, int>>(nxy);
            List<Tuple<int, int>> new_x = new List<Tuple<int, int>>(x);
            List<Tuple<int, int>> new_y = new List<Tuple<int, int>>(y);

            SortByPenalty(neutral);
            foreach (var edge in x)
                neutral.Remove(edge);

            GenerateXY(new_x, new_y, neutral);
            //Helpers.PrintEdges("X: ", new_x);
            //Helpers.PrintEdges("Y: ", new_y);

            var right_tree = Bound(new_x, new_y);

            if (index < right_bound.Count)
                right_bound[index] = Helpers.Cost(right_tree, this.initial_matrix);
            else
                right_bound.Add(Helpers.Cost(right_tree, this.initial_matrix));

            var aux_x = new List<Tuple<int, int>>(new_x);
            aux_x.Remove(new_x.Last());
            var aux_y = new List<Tuple<int, int>>(y);
            aux_y.Add(new_x.Last());
            var left_tree = Bound(aux_x, aux_y);
            if (index < left_bound.Count)
                left_bound[index] = Helpers.Cost(left_tree, this.initial_matrix);
            else
                left_bound.Add(Helpers.Cost(left_tree, this.initial_matrix));

        Pruning:
            if (right_bound[index] < upper_bound)
                if (Helpers.Possivel(right_tree, degree, ordem))
                {
                    upper_bound = right_bound[index];
                    best_solution = right_tree;
                }
                else
                    Branch(right_tree, new_x, new_y, index + 1);
            if (left_bound[index] < upper_bound)
            {
                right_bound[index] = left_bound[index];
                right_tree = new List<Tuple<int, int>>(left_tree);

                var last = new_x.Last();
                new_x.Remove(new_x.Last());
                if (new_x.Count - x.Count > 0)
                {
                    new_y = new List<Tuple<int, int>>(y);
                    new_y.Add(new_x.Last());
                    var anyway = Bound(new_x, new_y);
                    left_bound[index] = Helpers.Cost(anyway, initial_matrix);
                }
                else
                    left_bound[index] = int.MaxValue;
                goto Pruning;
            }
        }

        void BranchAndBound()
        {
            int start = 0;

            Random r = new Random();

            //Step 1
            //start = r.Next(ordem);
            upper_bound = Helpers.Cost(this.PrimDegreeConstrained(start), this.initial_matrix);
            Console.WriteLine("Upper bound inicial: {0}", upper_bound);
            index = 0;

            //Step 2
            //start = r.Next(ordem);
            List<Tuple<int, int>> not_in_xy = Prim(start);
            right_bound.Insert(index, Helpers.Cost(not_in_xy, this.initial_matrix));
            left_bound.Insert(index, int.MaxValue);

            index += 1;

            List<Tuple<int, int>> x = new List<Tuple<int, int>>();
            List<Tuple<int, int>> y = new List<Tuple<int, int>>();

            Branch(not_in_xy, x, y, index);
            //Step3:
            //    SortByPenalty(not_in_xy);
            //    not_in_xy.RemoveAll(f => Helpers.CoveredEdge(x, f.Item1, f.Item2));

            //    List<Tuple<int, int>> old_x = new List<Tuple<int, int>>(x);
            //    List<Tuple<int, int>> old_y = new List<Tuple<int, int>>(y);

            //    GenerateXY(x, y, not_in_xy);

            //    Helpers.PrintEdges("X: ", x);
            //    Helpers.PrintEdges("Y: ", y);

            //    var complete = Bound(x, y);

            //    right_bound.Insert(index, Helpers.Cost(complete, this.initial_matrix));

            //    var new_x = new List<Tuple<int, int>>(x);
            //    left_bound.Insert(index, right_bound[index - 1] + PenaltyOfEdge(x.Last(), complete));

            //    Debug.Print("Right Bound {1}: {0}", right_bound[index], index);
            //    Debug.Print("Left Bound {1}: {0}", left_bound[index], index);

            //Step4:
            //    if (right_bound[index] < upper_bound)
            //        //Step 5
            //        if (Helpers.Possivel(complete, degree, ordem))
            //        {
            //            upper_bound = right_bound[index];
            //            best_solution = complete;
            //        }
            //        else
            //        {
            //            index += 1;
            //            not_in_xy = complete;
            //            goto Step3;
            //        }

            //Step6:
            //    if (left_bound[index] < upper_bound)
            //    {
            //        right_bound[index] = left_bound[index];

            //        //Step 7
            //        if (x.Count - old_x.Count > 0)
            //        {
            //            var ult_x = x.Last();
            //            x.Remove(ult_x);
            //            old_y.Add(ult_x);
            //            y = old_y;
            //            complete = Bound(x, y);
            //            left_bound[index] = Helpers.Cost(complete, this.initial_matrix);
            //        }
            //        else
            //            left_bound[index] = int.MaxValue;
            //        goto Step4;
            //    }
            //    else
            //    {
            //        //Step 8
            //        index -= 1;
            //        if (index > 0)
            //            goto Step6;

            //    }
        }

        /// <summary>
        /// Gera uma Árvore Geradora Mínima a partir de uma lista de arestas
        /// a serem incluídas e uma lista de arestas a serem excluídas
        /// </summary>
        /// <param name="include">Lista de arestas a serem incluídas</param>
        /// <param name="exclude">Lista de arestas a serem excluídas</param>
        /// <returns>Uma ãrvore geradora mínima</returns>
        List<Tuple<int, int>> Bound(List<Tuple<int, int>> include, List<Tuple<int, int>> exclude)
        {
            List<Tree> forest = new List<Tree>();
            for (int i = 0; i < ordem; i++)
                forest.Add(new Tree(i));

            foreach (var edge in include)
            {
                Tree t1 = forest.Find(x => x.vertices.Contains(edge.Item1));
                Tree t2 = forest.Find(x => x.vertices.Contains(edge.Item2));
                t1.AddTree(t2);
                t1.edges.Add(edge);
                forest.Remove(t2);
            }

            var all = new List<Tuple<int, int>>(all_edges_by_weight);

            while (forest.Count > 1)
            {
                var edge = all.First();
                all.Remove(edge);
                if (!Helpers.CoveredEdge(exclude, edge.Item1, edge.Item2) && !Helpers.CoveredEdge(include, edge.Item1, edge.Item2))
                {
                    var t1 = forest.Find(x => x.vertices.Contains(edge.Item1));
                    var t2 = forest.Find(x => x.vertices.Contains(edge.Item2));
                    if (t1.Root != t2.Root)
                    {
                        t1.AddTree(t2);
                        t1.edges.Add(edge);
                        forest.Remove(t2);
                    }
                }
            }

            return forest.First().edges;
        }

        /// <summary>
        /// Função de ordenação da lista de arestas <paramref name="l"/> 
        /// com base na penalidade das arestas
        /// </summary>
        /// <param name="l">Lista a ser ordenada</param>
        void SortByPenalty(List<Tuple<int, int>> l)
        {
            int?[,] penalty_matrix = new int?[ordem, ordem];
            foreach (var edge in l)
                penalty_matrix[edge.Item1, edge.Item2] = PenaltyOfEdge(new Tuple<int, int>(edge.Item1, edge.Item2), l);

            Helpers.QSort<int>(l, penalty_matrix, 0, l.Count - 1);
            l.Reverse();
        }

        /// <summary>
        /// Calcula a penalidade de remoção de uma aresta <paramref name="edge"/>
        /// </summary>
        /// <param name="edge">Aresta a ser calculada a penalidade</param>
        /// <returns>A penalidade da aresta</returns>
        int PenaltyOfEdge(Tuple<int, int> edge, List<Tuple<int, int>> tree)
        {
            List<Tree> forest = new List<Tree>();

            for (int i = 0; i < ordem; i++)
                forest.Add(new Tree(i));

            int min = int.MaxValue;
            int min_i = 0;
            int min_j = 0;

            List<Tuple<int, int>> t0 = new List<Tuple<int, int>>(tree);
            t0.Remove(edge);

            while (t0.Count > 0)
            {
                var e = t0.First();
                t0.Remove(e);
                Tree t1 = forest.Find(x => x.vertices.Contains(e.Item1));
                Tree t2 = forest.Find(x => x.vertices.Contains(e.Item2));
                t1.AddTree(t2);
                t1.edges.Add(e);
                forest.Remove(t2);
            }

            List<int> vs_in_t1 = forest[0].vertices;
            List<int> vs_in_t2 = forest[1].vertices;

            foreach (var x in vs_in_t1)
                foreach (var y in vs_in_t2)
                    if ((int)initial_matrix[x, y] < min && !Helpers.CoveredEdge(tree, x, y))
                    {
                        min = (int)initial_matrix[x, y];
                        min_i = x;
                        min_j = y;
                    }


            int penality = min - (int)initial_matrix[edge.Item1, edge.Item2];
            //Debug.Print("Tuple {0}-{1}({2}) - Min {3}-{4}({5}) = {6}", edge.Item1 + 1, edge.Item2 + 1, (int)initial_matrix[edge.Item1, edge.Item2], min_i + 1, min_j + 1, min, penality);
            return penality;
        }


        [STAThread]
        static void Main(string[] args)
        {
        Start:
            Console.WriteLine("Árvore Geradora Mínima de Grau Restrito ");
            Console.WriteLine("---------------------------------------");

            Console.WriteLine("1- Abrir arquivo                2- Sair ");

            int option = int.Parse(Console.ReadLine());

            if (2 == option)
                return;

            string arquivo;

            OpenFileDialog open_file = new OpenFileDialog();
            DialogResult result = open_file.ShowDialog();

            if (result == DialogResult.OK)
                arquivo = open_file.FileName;
            else
                return;

            Console.WriteLine("Digite quantos nós há no grafo do arquivo selecionado:");
            int ordem = int.Parse(Console.ReadLine());

            Console.WriteLine("Digite qual o grau máximo escolhido:");
            int grau = int.Parse(Console.ReadLine());


            Program p = new Program(arquivo, grau, ordem);

            DateTime start = DateTime.Now;
            var x = p.PrimDegreeConstrained(0);

            Console.WriteLine("Custo da Solução: {0}", Helpers.Cost(x, p.initial_matrix));

            Console.WriteLine("Calculado em {0}s", (DateTime.Now - start).TotalSeconds);

            Helpers.PrintEdges("Solução", x, true);

            Console.WriteLine();

            Console.WriteLine("Deseja realizar outro teste? (S/N)");

            string opcao = Console.ReadLine();
            if (opcao == "S" || opcao == "s")
                goto Start;
        }


    }
}
