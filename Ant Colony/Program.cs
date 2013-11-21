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
