using IronXL;

namespace PercentageCalculator
{
    internal class Account
    {
        public Account(string path, uint startingCL)
        {
            StartingCreditLimit = startingCL;
            Transactions = GetAllTransactions(path);
            BankingDays = GetBankingDays();
        }
        public uint StartingCreditLimit { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<BankingDay> BankingDays { get; set; }
        public List<BankingMonth> BankingMonths { get; set; }
        public GracePeriod GracePeriod { get; set; }

        private List<Transaction> GetAllTransactions(string path)
        {

            var transactions = new List<Transaction>();
            var book = WorkBook.Load(path);
            string[] header = Array.ConvertAll(book.WorkSheets[0].Rows[0].ToArray(), x => x.ToString());
            var data = book.WorkSheets.First().Rows.Skip(1);

            if (!Enumerable.SequenceEqual(headers, header))
                throw new Exception("Your headers is so lame");

            foreach (var row in data)
                transactions.Add(new Transaction(row.ToArray()));
            transactions = transactions.OrderBy(t => t.TransactionTime).ToList();

            transactions[0].CreditLimit = StartingCreditLimit;
            for (int i = 1; i < transactions.Count; i++)
            {
                int difference = (int)(transactions[i - 1].Balance + transactions[i].Amount - transactions[i].Balance);
                transactions[i].CreditLimit = (uint)(transactions[i - 1].CreditLimit - difference);
            }

            return transactions.OrderBy(t => t.TransactionTime).ToList();
        }

        private List<BankingDay> GetBankingDays()
        {
            var days = new List<BankingDay>();
            DateTime first = Transactions.First().TransactionTime.Value.Date;
            DateTime last = Transactions.Last().TransactionTime.Value.Date;

            for(DateTime day = first; day <= last; day = day.AddDays(1))
            {
                var bankDay = new BankingDay();
                bankDay.Day = day;
                bankDay.Credit = Transactions
                    .Where(t => t.TransactionTime.Value.Date == day && t.Amount > 0)
                    .Sum(t => t.Amount);
                bankDay.Debit = Transactions
                    .Where(t => t.TransactionTime.Value.Date == day && t.Amount < 0)
                    .Sum(t => t.Amount);
                if (Transactions.Where(t => t.TransactionTime.Value.Date == day).Any())
                {
                    decimal minbal = Transactions
                        .Where(t => t.TransactionTime.Value.Date == day)
                        .MinBy(t => t.Balance).Balance;
                    if (days.Any())
                    {
                        bankDay.MinimumBalance = minbal < days.Last().EndDayBalance ? minbal : days.Last().EndDayBalance;
                    }
                    else
                    {
                        bankDay.MinimumBalance = minbal;
                    }

                    bankDay.EndDayBalance = Transactions
                        .Where(t => t.TransactionTime.Value.Date == day)
                        .MaxBy(t => t.TransactionTime).Balance;
                }
                else
                {
                    bankDay.MinimumBalance = days.Last().EndDayBalance;
                    bankDay.EndDayBalance = days.Last().EndDayBalance;
                }
                days.Add(bankDay);
            }

            return days;
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
