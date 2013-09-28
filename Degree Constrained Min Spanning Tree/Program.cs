using System;
using System.Collections.Generic;
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
            initial_matrix = this.CriaMatrix(s);

            PrintMatrix(initial_matrix);
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
                Console.WriteLine("Ordem: " + ordem.ToString());
                Console.WriteLine("Criando array");
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


        private void PrimDCST()
        {
            int?[][] dcst = InicializaMatrix(ordem);
            //int s = new Random().Next(0, ordem);
            int s = 3;

            Dictionary<int, int> degrees = new Dictionary<int, int>();
            degrees.Add(s, 1);

            while (degrees.Count < ordem)
            {
                int min = int.MaxValue;
                int start_node = 0;
                int final_node = 0;

                foreach (var item in degrees)
                {
                    int vertex = item.Key;

                    for (int i = 0; i < ordem; i++)
                    {
                        if (initial_matrix[vertex][i] < min && !degrees.ContainsKey(i) && degrees[vertex] <= this.degree)
                        {
                            min = (int)initial_matrix[vertex][i];
                            start_node = vertex;
                            final_node = i;
                        }
                    }
                }

                dcst[start_node][final_node] = min;
                dcst[final_node][start_node] = min;
                degrees[start_node] += 1;
                degrees.Add(final_node, 1);
            }

            PrintMatrix(dcst);
        }

        static void PrintMatrix(int?[][] m)
        {
            foreach (var i in m)
            {
                foreach (var j in i)
                {
                    if (null != j)
                        Console.Write(string.Format("{0,4}", j));
                    else
                        Console.Write("   -");
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }


        static void Main(string[] args)
        {
            Program p = new Program(args[0], 3);

            Console.WriteLine();
            Console.WriteLine();

            p.PrimDCST();
            

            Console.Read();
        }

        
    }
}
