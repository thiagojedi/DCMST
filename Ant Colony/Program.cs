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
        private int GraphSize;
        private int?[,] WeightMatrix;
        private double?[,] PheromoneMatrix;

        // Algorithm Parameters
        private double EvapFactor;

        // Parametros Auxiliares
        private int MaxWeight;
        private int MinWeight;

        private double MaxPhero;
        private double MinPhero;



        /// <summary>
        /// Construtor padrão
        /// </summary>
        /// <param name="file">Nome do Arquivo de Testes</param>
        /// <param name="graph_size">Número de Vértices no Grafo</param>
        public Program(string file, int graph_size)
        {
            this.GraphSize = graph_size;
            this.WeightMatrix = new int?[graph_size, graph_size];
            this.PheromoneMatrix = new double?[graph_size, graph_size];
            this.MaxWeight = int.MinValue;
            this.MinWeight = int.MaxValue;

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
}
