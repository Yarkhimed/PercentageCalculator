using IronXL;

namespace PercentageCalculator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "";
            uint startingCreditLimit;

            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            do
            {
                try
                {
                    Console.WriteLine("Enter path of the statement:");
                    path = Console.ReadLine()[1..^1];
                }
                catch (Exception ex)
                {
                    Console.Clear();
                    Console.WriteLine(ex.Message);
                }
            } while (path == "");

            Console.WriteLine("Enter starting credit limit:");
            while (true)
            {
                try
                {
                    startingCreditLimit = Convert.ToUInt32(Console.ReadLine());
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            var acc = new Account(path, startingCreditLimit);
            foreach (var tr in acc.BankingDays)
                Console.WriteLine(tr);
        }
    }
}