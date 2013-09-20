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

        /// <summary>
        /// Cria uma matriz quadrada a partir do arquivo de teste passado.
        /// </summary>
        /// <param name="filepath">Endereço do arquivo de teste. Pode ser relativo ou absoluto.</param>
        /// <returns>Matriz quadrada preenchida de acordo com o arquivo de teste.</returns>
        public int[][] CriaMatrix(string filepath)
        {
            using (StreamReader testcase = new StreamReader(filepath))
            {
                string linha = testcase.ReadLine();
                int ordem = int.Parse(linha);
                Console.WriteLine("Ordem: " + ordem.ToString());
                Console.WriteLine("Criando array");
                int[][] matrix = InicializaMatrixNula(ordem);
                int x = 0;
                for (int i = 0; i < ordem; i++)
                {
                    string[] row = testcase.ReadLine().Split(' ');
                    for (int j = 0; j < ordem; j++)
                    {
                        matrix[i][j] = int.Parse(row[j]);
                        matrix[j][i] = matrix[i][j];
                        Console.Write(matrix[i][j]);
                        Console.Write(" ");
                        x++;
                    }
                    Console.WriteLine();
                }
                return matrix;
            }
        }

        /// <summary>
        /// Inicializa uma nova matriz quadrada de ordem <paramref name="o"/>.
        /// </summary>
        /// <param name="o">Ordem da matriz quadrada</param>
        /// <returns>Uma nova matriz quadrada</returns>
        private static int[][] InicializaMatrixNula(int o)
        {
            int[][] matrix = new int[o][];
            for (int i = 0; i < o; i++)
            {
                matrix[i] = new int[o];
            }
            return matrix;
        }



        static void Main(string[] args)
        {
            Program p = new Program();

            p.CriaMatrix(args[0]);
            Console.Read();
        }

        
    }
}
