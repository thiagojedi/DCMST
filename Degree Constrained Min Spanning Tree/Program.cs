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

        int upper_bound;

        List<int> left_bound;
        List<int> right_bound;

        int index;

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
                if (!Helpers.CoveredEdge(x, edge.Item1, edge.Item2) &&
                    (!vertex_with_degrees.ContainsKey(edge.Item1) || vertex_with_degrees[edge.Item1] < this.degree) &&
                    (!vertex_with_degrees.ContainsKey(edge.Item2) || vertex_with_degrees[edge.Item2] < this.degree))
                {
                    x.Add(edge);

                    if (!vertex_with_degrees.ContainsKey(edge.Item1))
                        vertex_with_degrees.Add(edge.Item1, 1);
                    else
                        vertex_with_degrees[edge.Item1] += 1;

                    if (!vertex_with_degrees.ContainsKey(edge.Item2))
                        vertex_with_degrees.Add(edge.Item2, 1);
                    else
                        vertex_with_degrees[edge.Item2] += 1;

                    if (vertex_with_degrees[edge.Item1] >= this.degree)
                    {
                        node_to_y = edge.Item1;
                        break;
                    }
                    if (vertex_with_degrees[edge.Item2] >= this.degree)
                    {
                        node_to_y = edge.Item2;
                        break;
                    }
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
            int start = 0;
            int solution_index = 0;

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

            right_bound.Insert(index, Helpers.Cost(complete, this.initial_matrix));

            var new_x = new List<Tuple<int, int>>(x);
            new_x.Remove(x.Last());

            GenerateXY(ref new_x, ref old_y, complete);
            int lb = Helpers.Cost(CompleteMST(new_x, old_y), this.initial_matrix);
            left_bound.Insert(index, lb);

            Debug.Print("Right Bound {1}: {0}", right_bound[index], index);
            Debug.Print("Left Bound {1}: {0}", left_bound[index], index);

        Step4:
            if (right_bound[index] < upper_bound)
                //Step 5
                if (Helpers.Possivel(complete, degree, ordem))
                {
                    upper_bound = right_bound[index];
                    solution_index = index;
                }
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
                    List<Tuple<int, int>> outro_old_y = new List<Tuple<int, int>>(old_y);
                    outro_old_y.Add(ult_x);
                    GenerateXY(ref x, ref outro_old_y, outro_nxy);
                    complete = CompleteMST(x, outro_old_y);
                    left_bound[index] = Helpers.Cost(complete, this.initial_matrix);
                }
                else
                    left_bound[index] = int.MaxValue;
                goto Step4;
            }
            else
            {
                //Step 8
                index -= 1;
                if (index > 0)
                    goto Step6;

            }

            Console.WriteLine("Custo da Solução: {0}", Helpers.Cost(complete, this.initial_matrix));
            Console.WriteLine("Possível? {0}", Helpers.Possivel(complete, degree, ordem) ? "Sim" : "Não");
            Helpers.PrintEdges("Solução", complete, false);
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
            List<int> not_inserted = new List<int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>(include);

            foreach (var edge in edges)
            {
                if (!vertexes.Contains(edge.Item1))
                    vertexes.Add(edge.Item1);
                if (!vertexes.Contains(edge.Item2))
                    vertexes.Add(edge.Item2);
            }

            for (int i = 0; i < ordem; i++)
                not_inserted.Add(i);

            vertexes.Sort();
            not_inserted.RemoveAll(f => vertexes.Contains(f));

            while (vertexes.Count < ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                vertexes.Sort();
                foreach (int vertex in vertexes)
                    foreach (int i in not_inserted)
                        if (this.initial_matrix[vertex, i] < min && !Helpers.CoveredEdge(exclude, vertex, i))
                        {
                            min = (int)this.initial_matrix[vertex, i];
                            start_node = vertex;
                            final_node = i;
                        }
                vertexes.Add(final_node);
                not_inserted.Remove(final_node);

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

            int min = int.MaxValue;
            int min_i = 0;
            int min_j = 0;

            List<Tuple<int, int>> t0 = new List<Tuple<int, int>>(tree);

            t0.Remove(edge);

            List<int> vs_in_t1 = new List<int>();
            List<int> vs_in_t2 = new List<int>();

            vs_in_t1.Add(edge.Item1);
            vs_in_t2.Add(edge.Item2);

            while (t0.Count > 0)
            {
                //TODO Review this for it's crashing in some instances
                var e = t0.Find(x => (vs_in_t1.Contains(x.Item1) || vs_in_t1.Contains(x.Item2) || vs_in_t2.Contains(x.Item1) || vs_in_t2.Contains(x.Item2)));
                if (vs_in_t1.Contains(e.Item1))
                    vs_in_t1.Add(e.Item2);
                else if (vs_in_t1.Contains(e.Item2))
                    vs_in_t1.Add(e.Item1);
                else if (vs_in_t2.Contains(e.Item1))
                    vs_in_t2.Add(e.Item2);
                else if (vs_in_t2.Contains(e.Item2))
                    vs_in_t2.Add(e.Item1);

                t0.Remove(e);
            }

            foreach (var x in vs_in_t1)
                foreach (var y in vs_in_t2)
                {
                    bool existe = Helpers.CoveredEdge(tree, x, y);
                    if (!existe && initial_matrix[x, y] != null && (int)initial_matrix[x, y] < min)
                    {
                        min = (int)initial_matrix[x, y];
                        min_i = x;
                        min_j = y;
                    }
                }


            int penality = min - (int)initial_matrix[edge.Item1, edge.Item2];
            //Debug.Print("Tuple {0}-{1}({2}) - Min {3}-{4}({5}) = {6}", edge.Item1 + 1, edge.Item2 + 1, (int)initial_matrix[edge.Item1,edge.Item2], min_i + 1, min_j + 1, min, penality);
            return penality;
        }


        [STAThread]
        static void Main(string[] args)
        {
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
            p.BranchAndBound();
            Console.WriteLine("Calculado em {0}s", (DateTime.Now - start).TotalSeconds);

            Console.WriteLine();

            Console.WriteLine("Pressione ENTER para sair");

            Console.Read();
        }


    }
}
