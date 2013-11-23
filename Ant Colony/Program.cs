using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ants
{
    class Program
    {
        private int[,] WeightMatrix;
        private double[,] PheromoneMatrix;

        // Algorithm Parameters
        private int MaxWeith;
        private int MinWeith;

        private double EvapFactor;


        private double MaxPhero { get { return 1000 * ((MaxWeith - MinWeith) + (MaxWeith - MinWeith) / 3); } }
        private double MinPhero { get { return (MaxWeith - MinWeith) / 3; } }



        /// <summary>
        /// Construtor padrão
        /// </summary>
        /// <param name="file">Nome do Arquivo de Testes</param>
        /// <param name="graph_size">Número de Vértices no Grafo</param>
        public Program(string file, int graph_size)
        {
            #region Inicialização de Variáveis
            this.WeightMatrix = new int[graph_size, graph_size];
            #endregion
        }

        /// <summary>
        /// Calcula quanto deve ser adicionado de Feromônio por formiga
        /// </summary>
        /// <param name="edge">Aresta percorrida</param>
        /// <returns>Incremento de Feromônio</returns>
        double AddedPher(Tuple<int,int> edge)
        {
            int a = edge.Item1, b = edge.Item2;
            if (a > b)
            {
                // XOR Swap
                a = a ^ b;
                b = a ^ b;
                a = a ^ b;
            }
            return (MaxWeith - WeightMatrix[a, b]) + (MaxWeith - MinWeith) / 3;
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
