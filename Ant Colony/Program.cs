using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ants
{
    class Program
    {
        // Algorithm Parameters
        struct Parameters
        {
            // Número máximo de iterações do algoritmo
            public const int MaxCycles = 10000;
            // Número de iterações sem melhoras antes de reduzir os feromonios da melhor solução
            public const int EscapeCycle = 100;
            // Número de iterações sem melhoras antes de sair o algoritmo
            public const int StopCycle = 250;
            // Quantos nós uma formiga visita por ciclo
            public const int AntSteps = 30;

            // Fator de evaporação de feromonio
            public static double EvapFactor = 0.5;
            // Fator de atualização da evaporação
            public const double EvapUpdate = 0.95;

            // Fator de acrécimo de ferormonio na melhor solução
            public static double EnchanceFactor = 1.5;
            // Fator de atualização do acrécimo
            public const double EnchanceUpdate = 1.05;

            // A cada quantos ciclos deve-se atualizar os fatores
            public const int UpdateCycle = 500;
        }

        // Parametros Auxiliares
        int MaxWeight;
        int MinWeight;

        double MaxPhero;
        double MinPhero;

        int MaxDegree;
        int GraphSize;
        int?[,] WeightMatrix;
        double?[,] PheromoneMatrix;
        int[,] UpdateMatrix;

        List<Tuple<int, int>> MelhorSolucao;
        int MelhorCusto;

        List<Ant> AntFarm;


        /// <summary>
        /// Construtor padrão
        /// </summary>
        /// <param name="file">Nome do Arquivo de Testes</param>
        /// <param name="graph_size">Número de Vértices no Grafo</param>
        public Program(string file, int graph_size, int degree)
        {
            this.MaxWeight = int.MinValue;
            this.MinWeight = int.MaxValue;
            this.GraphSize = graph_size;
            this.MaxDegree = degree;
            this.WeightMatrix = new int?[graph_size, graph_size];
            this.PheromoneMatrix = new double?[graph_size, graph_size];
            this.UpdateMatrix = Helpers.MatrizZero(graph_size);

            MelhorCusto = int.MaxValue;
            this.AntFarm = new List<Ant>();


            InitializeWeights(file);
            InitializePheromones();

            this.MaxPhero = 1000 * ((MaxWeight - MinWeight) + (MaxWeight - MinWeight) / 3);
            this.MinPhero = (MaxWeight - MinWeight) / 3;
        }

        void InitializeWeights(string filepath)
        {
            // Lê arquivo
            using (System.IO.StreamReader file = new System.IO.StreamReader(filepath))
            {
                string linha;
                string pattern = @"^(?<in>\d+)\s+(?<out>\d+)\s+(?<weight>\d+)";
                Regex rx_line = new Regex(pattern);

                while (!file.EndOfStream)
                {
                    linha = file.ReadLine();
                    Match match = Regex.Match(linha, pattern);

                    int a = int.Parse(match.Groups["in"].Value);
                    int b = int.Parse(match.Groups["out"].Value);
                    int w = int.Parse(match.Groups["weight"].Value);

                    // Salva pesos
                    this.WeightMatrix[a, b] = w;
                    this.WeightMatrix[b, a] = w;

                    // Atualiza Limites
                    if (MaxWeight < w)
                        MaxWeight = w;

                    if (MinWeight > w)
                        MinWeight = w;
                }
            }
        }

        void InitializePheromones()
        {
            for (int i = 0; i < GraphSize; i++)
                for (int j = i + 1; j < GraphSize; j++)
                {
                    PheromoneMatrix[i, j] = InitialPhero(i, j);
                    PheromoneMatrix[j, i] = InitialPhero(i, j);
                }
        }

        /// <summary>
        /// Calcula a quantidade inicial de Feromônio numa aresta
        /// </summary>
        /// <param name="i">Vertice inicial</param>
        /// <param name="j">Vertice final</param>
        /// <returns>Quantidade de Feromônio</returns>
        double InitialPhero(int i, int j)
        {
            return (MaxWeight - (int)WeightMatrix[i, j]) + (MaxWeight - MinWeight) / 3;
        }

        public void AntBasedAlgorithm()
        {

            // Coloca uma formiga em cada vertice
            AntFarm = new List<Ant>();
            for (int i = 0; i < GraphSize; i++)
                AntFarm.Add(new Ant(i));

            int NoImprov = 0;
            int Cycles = 0;
            bool StopCondition = false;
            while (!StopCondition)
            {
                Cycles++;

                // Coloca uma formiga em cada vertice
                foreach (Ant ant in AntFarm)
                    ant.ClearMarked();

                // Fase de Exploração
                ReleaseTheAnts();

                UpdatePheros();

                // Fase de Construção da Árvore
                var Tree = BuildTree();

                int NewCost = Helpers.Cost(Tree, WeightMatrix);
                if (NewCost < this.MelhorCusto)
                {                    
                    Console.Write(".");
                    MelhorSolucao = Tree;
                    MelhorCusto = NewCost;
                    NoImprov = 0;
                }
                else
                    NoImprov += 1;

                EnchanceBest();

                // Verificações
                if (NoImprov == Parameters.EscapeCycle)
                    NerfBest();

                if (Cycles == Parameters.UpdateCycle)
                    UpdateParameters();

                StopCondition = NoImprov == Parameters.StopCycle || Cycles == Parameters.MaxCycles;
                //Console.WriteLine("Iteração: {0}", Cycles);
            }
        }

        void ReleaseTheAnts()
        {
            for (int s = 0; s < Parameters.AntSteps; s++)
            {
                if (s == Parameters.AntSteps / 3 || s == 2 * Parameters.AntSteps / 3)
                    UpdatePheros();
                foreach (var ant in AntFarm)
                    MoveAnt(ant);
            }
        }

        void MoveAnt(Ant a)
        {
            Random r = new Random();
            int tentativas = 0;
            while (tentativas < 5)
            {
                Tuple<int, int> nextEdge = Helpers.SelectEdge(a.VerticeAtual, PheromoneMatrix);
                if (a.VerticesVisitados.Contains(nextEdge.Item2))
                    tentativas++;
                else
                {
                    a.VerticeAtual = nextEdge.Item2;
                    MarkForUpdate(nextEdge);
                    break;
                }
            }
        }

        void MarkForUpdate(Tuple<int, int> edge)
        {
            UpdateMatrix[edge.Item1, edge.Item2] += 1;
            UpdateMatrix[edge.Item2, edge.Item1] += 1;
        }

        void UpdatePheros()
        {
            for (int i = 0; i < GraphSize; i++)
                for (int j = i + 1; j < GraphSize; j++)
                {
                    double updatedPhero = (1 - Parameters.EvapFactor) * (double)PheromoneMatrix[i, j] + UpdateMatrix[i, j] * InitialPhero(i, j);
                    if (updatedPhero > MaxPhero)
                        updatedPhero = MaxPhero - InitialPhero(i, j);
                    if (updatedPhero < MinPhero)
                        updatedPhero = MinPhero + InitialPhero(i, j);
                    PheromoneMatrix[i, j] = updatedPhero;
                }
        }

        List<Tuple<int, int>> AllEdgesByPhero()
        {
            List<Tuple<int, int>> covered = new List<Tuple<int, int>>();
            Dictionary<Tuple<int, int>, double> PheroOf = new Dictionary<Tuple<int, int>, double>();

            for (int i = 0; i < GraphSize; i++)
                for (int j = i + 1; j < GraphSize; j++)
                    covered.Add(new Tuple<int, int>(i, j));

            Helpers.QSort<double>(ref covered, PheromoneMatrix, 0, covered.Count - 1);

            covered.Reverse();
            return covered;
        }

        List<Tuple<int, int>> BuildTree()
        {
            List<Tuple<int, int>> Edges = AllEdgesByPhero();
            var BestN = Edges.Take(5 * GraphSize).ToList();
            Helpers.OrderByCost(ref BestN, WeightMatrix);

            Edges.RemoveAll(x => BestN.Contains(x));

            List<Tree> forest = new List<Tree>();
            Dictionary<int, int> Degrees = new Dictionary<int, int>();
            for (int i = 0; i < GraphSize; i++)
            {
                forest.Add(new Tree(i));
                Degrees.Add(i, 0);
            }

            while (forest.First().vertices.Count < GraphSize)
            {
                if (BestN.Count != 0)
                {
                    var e = BestN[0];
                    BestN.Remove(e);

                    Tree t1 = forest.Find(x => x.vertices.Contains(e.Item1));
                    Tree t2 = forest.Find(x => x.vertices.Contains(e.Item2));
                    if (t1.Root != t2.Root && Degrees[e.Item1] < MaxDegree && Degrees[e.Item2] < MaxDegree)
                    {
                        t1.AddTree(t2);
                        t1.edges.Add(e);
                        forest.Remove(t2);
                        Degrees[e.Item1]++;
                        Degrees[e.Item2]++;
                    }
                }
                else
                {
                    BestN = Edges.Take(5 * GraphSize).ToList();
                    if (BestN.Count == 0)
                        break;
                    Helpers.OrderByCost(ref BestN, WeightMatrix);
                    Edges.RemoveAll(x => BestN.Contains(x));
                }
            }

            return forest.First().edges;
        }

        void EnchanceBest()
        {
            foreach (var edge in MelhorSolucao)
                PheromoneMatrix[edge.Item1, edge.Item2] *= Parameters.EnchanceFactor;
        }

        void NerfBest()
        {
            foreach (var edge in MelhorSolucao)
                PheromoneMatrix[edge.Item1, edge.Item2] *= Parameters.EvapFactor;
        }

        void UpdateParameters()
        {
            Parameters.EvapFactor *= Parameters.EvapUpdate;
            Parameters.EnchanceFactor *= Parameters.EnchanceUpdate;
        }

        [STAThread]
        static void Main(string[] args)
        {
        Start:
            Console.WriteLine("AGM Grau Restrito - Ataque das Formigas ");
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


            Program p = new Program(arquivo, ordem, grau);

            Console.WriteLine("---------------------------------------");
            Console.Write("Calculando");

            //Profiling
            DateTime time = DateTime.Now;

            p.AntBasedAlgorithm();

            TimeSpan decorrido = DateTime.Now.Subtract(time);

            Console.WriteLine();
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Custo da Melhor Solução: {0}", p.MelhorCusto);
            Console.WriteLine("Calculado em: {0}s", decorrido.TotalSeconds);

            Console.WriteLine();

            Helpers.ImprimeArvore(p.MelhorSolucao, p.WeightMatrix);

            Console.WriteLine();

            Console.WriteLine("Deseja realizar outro teste? (S/N)");

            string opcao = Console.ReadLine();
            if (opcao == "S" || opcao == "s")
                goto Start;

        }
    }

    class Ant
    {
        private int atual;
        public int VerticeAtual
        {
            get { return atual; }
            set
            {
                atual = value;
                VerticesVisitados.Add(value);
            }
        }

        public List<int> VerticesVisitados { get; set; }

        public Ant(int vertice)
        {
            VerticesVisitados = new List<int>();
            VerticeAtual = vertice;
        }

        public void ClearMarked()
        {
            VerticesVisitados = new List<int>();
            VerticesVisitados.Add(VerticeAtual);
        }
    }

    class Tree
    {
        public int Root { get; private set; }
        public List<int> vertices { get; private set; }
        public List<Tuple<int, int>> edges { get; private set; }

        public Tree(int root)
        {
            vertices = new List<int>();
            edges = new List<Tuple<int, int>>();

            this.Root = root;
            vertices.Add(root);
        }
        public void AddTree(Tree t)
        {
            this.edges.AddRange(t.edges);
            this.vertices.AddRange(t.vertices);
        }
    }
}
