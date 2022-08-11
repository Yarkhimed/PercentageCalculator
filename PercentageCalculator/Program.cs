using IronXL;

namespace PercentageCalculator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var acc = new Account();
            foreach (var maxDebt in acc.MaxDebts)
                Console.WriteLine($"{maxDebt.Key.ToString("dd.MM.yyyy")} {maxDebt.Value}");
        }
    }
}