using System.Globalization;
using System.IO;

namespace PercentageCalculator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            testAcc();
        }
        static void test()
        {
            var list = new List<int>();
            for (int i = 1; i < 10; i++)
                list.Add(i * 2);
            int? a;
            try
            {
                a = list.Where(i => i % 2 != 0).First();
            }
            catch (InvalidOperationException)
            {
                a = null;
            }
            Console.WriteLine(a.ToString());
        }
        static void testAcc()
        {
            var acc = new Account(@"E:\Download\Качанов_Ярослав_Олександрович_виписка_з_2017-10-01T000000.000_по_2018-11-30T000000.000.xls", 4000);
            foreach (var gp in acc.GracePeriods)
                Console.WriteLine(gp.ToString());
        }

        static void start()
        {
            string path = "";
            uint startingCreditLimit;

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