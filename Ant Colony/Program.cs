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
            public const int StopCycle = 2500;
            // Quantos nós uma formiga vizita por ciclo
            public const int AntSteps = 75;

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

        int GraphSize;
        int?[,] WeightMatrix;
        double?[,] PheromoneMatrix;
        int[,] UpdateMatrix;

        List<Tuple<int, int>> MelhorSolução;
        int MelhorCusto;
        
        Ant[] AntFarm;


        /// <summary>
        /// Construtor padrão
        /// </summary>
        /// <param name="file">Nome do Arquivo de Testes</param>
        /// <param name="graph_size">Número de Vértices no Grafo</param>
        public Program(string file, int graph_size)
        {
            this.MaxWeight = int.MinValue;
            this.MinWeight = int.MaxValue;
            this.GraphSize = graph_size;
            this.WeightMatrix = new int?[graph_size, graph_size];
            this.PheromoneMatrix = new double?[graph_size, graph_size];
            this.UpdateMatrix = Helpers.MatrizZero(graph_size);

            MelhorCusto = int.MaxValue;
            this.AntFarm = new Ant[graph_size];


            InitializeWeights(file);
            InitializePheromones();

            this.MaxPhero = 1000 * ((MaxWeight - MinWeight) + (MaxWeight - MinWeight) / 3);
            this.MinPhero = (MaxWeight - MinWeight) / 3;
        }

        private void InitializeWeights(string filepath)
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

        private void InitializePheromones()
        {
            for (int i = 0; i < GraphSize; i++)
                for (int j = i + 1; j < GraphSize; j++)
                {
                    PheromoneMatrix[i, j] = InitialPher(i, j);
                    PheromoneMatrix[j, i] = InitialPher(i, j);
                }
        }

        /// <summary>
        /// Calcula a quantidade inicial de Feromônio numa aresta
        /// </summary>
        /// <param name="i">Vertice inicial</param>
        /// <param name="j">Vertice final</param>
        /// <returns>Quantidade de Feromônio</returns>
        double InitialPher(int i, int j)
        {
            int a = i, b = j;
            if (a > b)
            {
                // XOR Swap
                a = a ^ b;
                b = a ^ b;
                a = a ^ b;
            }
            return (MaxWeight - (int)WeightMatrix[a, b]) + (MaxWeight - MinWeight) / 3;
        }

        public void ReleaseTheAnts()
        {
            // Coloca uma formiga em cada vertice
            for (int i = 0; i < GraphSize; i++)
                AntFarm[i].VerticeAtual = i;

            bool parada = false;
            while (!parada)
            {
                // Fase de Exploração
                for (int s = 0; s < Parameters.AntSteps; s++)
                {
                    foreach (var ant in AntFarm)
                    {
                        
                    }
                }
            }
        }

        private void MoveAnt(Ant a)
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
                    a.ArestasPercorridas.Add(nextEdge);
                }

            }
        }

        private void MarkForUpdate(Tuple<int, int> edge)
        {
            if (edge.Item1 < edge.Item2)
                UpdateMatrix[edge.Item1, edge.Item2] += 1;
            else
                UpdateMatrix[edge.Item2, edge.Item1] += 1;
        }

        [STAThread]
        static void Main(string[] args)
        {
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

            //TODO Implement Main Method

            Console.WriteLine();

            Console.WriteLine("Pressione ENTER para sair");

            Console.Read();

        }
    }

    struct Ant
    {
        private int atual;
        public int VerticeAtual
        {
            get { return atual; }
            set
            {
                atual = value;
                VerticesVisitados.Add(atual);
            }
        }

        public List<Tuple<int, int>> ArestasPercorridas { get; set; }
        public List<int> VerticesVisitados { get; set;}

        public Ant()
        {
            atual = 0;
            ArestasPercorridas = new List<Tuple<int, int>>();
            VerticesVisitados = new List<int>();
        }
    }
}
