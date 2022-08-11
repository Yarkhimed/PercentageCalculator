using IronXL;

namespace PercentageCalculator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var transactions = new List<Transaction>();
            var book = WorkBook.Load(@"E:\Download\Качанов_Ярослав_Олександрович_виписка_з_2022-01-01T000000.000_по_2022-08-08T235959.999.xlsx");
            string[] header = Array.ConvertAll(book.WorkSheets[0].Rows[0].ToArray(), x => x.ToString());
            var data = book.WorkSheets.First().Rows.Skip(1);
            foreach (var row in data)
                transactions.Add(new Transaction(row.ToArray()));

            transactions.Where(t => t.Fee > 0).ToList().ForEach(t => Console.WriteLine(t.ToString()));

        }

        private static string[] headers =
        {
            "Дата і час",
            "Категорія",
            "Опис",
            "Код МСС",
            "Сума у валютi карти",
            "Валюта карти",
            "Сума у валютi операцiї",
            "Валюта операцiї",
            "Комісія",
            "Кешбек",
            "Залишок"
        };
    }
}