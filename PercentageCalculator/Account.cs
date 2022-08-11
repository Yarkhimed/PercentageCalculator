using IronXL;

namespace PercentageCalculator
{
    internal class Account
    {
        public Account()
        {
            Transactions = GetAllTransactions();
            FirstDate = Transactions.First().TransactionTime.Value.Date;
            LastDate = Transactions.Last().TransactionTime.Value.Date;
            MaxDebts = getMaxDebts();
        }
        public uint CreditLimit { get; set; } = 10_000;
        public Dictionary<DateTime, decimal> MaxDebts { get; set; }
        public List<Transaction> Transactions { get; set; }
        public DateTime FirstDate { get; set; }
        public DateTime LastDate { get; set; }
        private List<Transaction> GetAllTransactions()
        {

            var transactions = new List<Transaction>();
            var book = WorkBook.Load(@"E:\Download\Качанов_Ярослав_Олександрович_виписка_з_2022-01-01T000000.000_по_2022-08-08T235959.999.xlsx");
            string[] header = Array.ConvertAll(book.WorkSheets[0].Rows[0].ToArray(), x => x.ToString());
            var data = book.WorkSheets.First().Rows.Skip(1);

            if (!Enumerable.SequenceEqual(headers, header))
                throw new Exception("Your headers is so lame");

            foreach (var row in data)
                transactions.Add(new Transaction(row.ToArray()));

            return transactions.OrderBy(t => t.TransactionTime).ToList();
        }

        private Dictionary<DateTime, decimal> getMaxDebts()
        {
            Dictionary<DateTime, decimal> maxDebts = new Dictionary<DateTime, decimal>();

            DateTime date = FirstDate;
            decimal minimumBalance = Transactions
                    .Where(t => t.TransactionTime.Value.Date == date)
                    .MinBy(t => t.Balance).Balance;
            decimal dayBalance = Transactions
                        .Where(t => t.TransactionTime.Value.Date == date)
                        .Last().Balance;

            while (date <= LastDate)
            {
                if (Transactions.Any(Transaction => Transaction.TransactionTime.Value.Date == date))
                {
                    minimumBalance = Transactions
                        .Where(t => t.TransactionTime.Value.Date == date)
                        .MinBy(t => t.Balance).Balance;

                    if (minimumBalance > dayBalance)
                        minimumBalance = dayBalance;

                    dayBalance = Transactions
                        .Where(t => t.TransactionTime.Value.Date == date)
                        .Last().Balance;
                }
                else
                {
                    minimumBalance = dayBalance;
                }
                maxDebts.Add(date, minimumBalance);
                date = date.AddDays(1);
            }

            return maxDebts;
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
