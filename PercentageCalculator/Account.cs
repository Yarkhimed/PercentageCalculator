using IronXL;
using System.Globalization;

namespace PercentageCalculator
{
    internal class Account
    {
        public Account(string path, uint startingCL)
        {
            StartingCreditLimit = startingCL;
            Transactions = GetAllTransactions(path);
            BankingDays = GetBankingDays();
            GetBankingMonths();
        }
        public uint StartingCreditLimit { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<BankingDay> BankingDays { get; set; }
        public List<BankingMonth> BankingMonths { get; set; } = new List<BankingMonth>();
        public List<GracePeriod> GracePeriods { get; set; } = new List<GracePeriod>();
        public decimal InterestRate { get; } = 0.031m;

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
                bankDay.Transactions = Transactions.Where(t => t.TransactionTime.Value.Date == day).ToList();

                if (bankDay.Transactions.Any())
                {
                    decimal minbal = bankDay.Transactions.MinBy(t => t.Balance).Balance;
                    if (days.Any())
                    {
                        bankDay.MinimumBalance = minbal < days.Last().EndDayBalance ? minbal : days.Last().EndDayBalance;
                    }
                    else
                    {
                        if (minbal <= minbal - bankDay.Credit + bankDay.Debit)
                        {
                            bankDay.MinimumBalance = minbal;
                        }
                        else
                        {
                            bankDay.MinimumBalance = minbal - bankDay.Credit + bankDay.Debit;
                        }
                    }

                    bankDay.EndDayBalance = bankDay.Transactions.MaxBy(t => t.TransactionTime).Balance;
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

        private void GetBankingMonths()
        {
            bool isInterestCharged = false;

            for (int year = Transactions.First().TransactionTime.Value.Year; year <= Transactions.Last().TransactionTime.Value.Year; year++)
            {
                int month = year == Transactions.First().TransactionTime.Value.Year ? Transactions.First().TransactionTime.Value.Month : 1;
                int lastMonth = year == Transactions.Last().TransactionTime.Value.Year ? Transactions.Last().TransactionTime.Value.Month : 12;

                for (; month <= lastMonth; month++)
                {
                    var bankingMonth = new BankingMonth
                    {
                        Year = year,
                        Month = month,
                        IsInterestCharged = isInterestCharged,
                        DailyInterest = InterestRate / DateTime.DaysInMonth(year, month)
                    };

                    if (isInterestCharged)
                    {
                        var daysInMonth = BankingDays
                            .Where(d => d.Day.Month == month && d.Day.Year == year)
                            .ToList();
                        foreach (var day in daysInMonth)
                        {
                            var transInDay = Transactions.Where(t => t.TransactionTime.Value.Date == day.Day).ToList();
                            foreach (var trans in day.Transactions)
                            {
                                if (!bankingMonth.IsDebtRepaid && trans.Balance > trans.CreditLimit)
                                {
                                    bankingMonth.IsDebtRepaid = true;
                                    bankingMonth.RepaymentDay = trans.TransactionTime.Value.Date;
                                }
                            }
                        }
                    }
                    else
                    {
                        GracePeriod? gracePeriod;
                        try
                        {
                            gracePeriod = GracePeriods
                                .Where(g => g.Deadline.Year == year && g.Deadline.Month == month)
                                .First();
                        }
                        catch (InvalidOperationException)
                        {
                            gracePeriod = null;
                        }
                        if (gracePeriod == null)
                        {
                            var reverseDaysInMonth = BankingDays
                                .Where(d => d.Day.Month == month && d.Day.Year == year)
                                .Reverse()
                                .ToList();

                            var lastTransInMonth = Transactions
                                .Where(t => t.TransactionTime.Value.Year == year && t.TransactionTime.Value.Month == month)
                                .Last();

                            if (lastTransInMonth.Balance < lastTransInMonth.CreditLimit)
                            {
                                DateTime? gracePeriodStartDay = null;
                                foreach (var day in reverseDaysInMonth)
                                {
                                    foreach (var trans in day.Transactions)
                                    {
                                        if (trans.Balance >= trans.CreditLimit)
                                        {
                                            gracePeriodStartDay = Transactions[Transactions.IndexOf(trans) + 1].TransactionTime.Value.Date;
                                            break;
                                        }
                                    }
                                }
                                GracePeriods.Add(new GracePeriod
                                {
                                    StartDate = gracePeriodStartDay ?? new DateTime(year, month, 1),
                                    Deadline = new DateTime(year, month + 1, DateTime.DaysInMonth(year, month + 1)),
                                    SumToRepay = lastTransInMonth.CreditLimit - lastTransInMonth.Balance
                                });
                            }
                        }
                        else
                        {
                            var daysInMonth = BankingDays
                                .Where(d => d.Day.Year == year && d.Day.Month == month)
                                .ToList();

                            var reverseDaysInMonth = BankingDays
                                .Where(d => d.Day.Month == month && d.Day.Year == year)
                                .Reverse()
                                .ToList();

                            var lastTransInMonth = Transactions
                                .Where(t => t.TransactionTime.Value.Year == year && t.TransactionTime.Value.Month == month)
                                .Last();
                            decimal monthCredit = BankingDays
                                .Where(d => d.Day.Year == year && d.Day.Month == month)
                                .Sum(d => d.Credit);

                            if (gracePeriod.SumToRepay <= monthCredit)
                            {
                                gracePeriod.Fulfilled = true;

                                decimal creditSum = 0;
                                foreach (var day in daysInMonth)
                                {
                                    creditSum += day.Credit;
                                    if (creditSum >= gracePeriod.SumToRepay)
                                    {
                                        gracePeriod.EndDate = day.Day;
                                        if (creditSum > gracePeriod.SumToRepay)
                                        {
                                            gracePeriod.Overpayment = creditSum - gracePeriod.SumToRepay;
                                        }
                                        break;
                                    }
                                }

                                if (lastTransInMonth.Balance < lastTransInMonth.CreditLimit)
                                {
                                    DateTime? gracePeriodStartDay = null;
                                    foreach (var day in reverseDaysInMonth)
                                    {
                                        foreach (var trans in day.Transactions)
                                        {
                                            if (trans.Balance >= trans.CreditLimit)
                                            {
                                                gracePeriodStartDay = Transactions[Transactions.IndexOf(trans) + 1].TransactionTime.Value.Date;
                                                break;
                                            }
                                        }
                                    }
                                    GracePeriods.Add(new GracePeriod
                                    {
                                        StartDate = gracePeriodStartDay ?? new DateTime(year, month, 1),
                                        Deadline = new DateTime(year, month + 1, DateTime.DaysInMonth(year, month + 1)),
                                        SumToRepay = lastTransInMonth.CreditLimit - lastTransInMonth.Balance
                                    });
                                }

                                decimal sumDebit = 0;
                                foreach (var day in daysInMonth)
                                {
                                    sumDebit += day.Debit * -1;
                                    if (sumDebit > gracePeriod.Overpayment)
                                    {
                                        if (GracePeriods.Last().StartDate < day.Day)
                                        {
                                            GracePeriods.Last().StartDate = day.Day;
                                        }
                                        break;
                                    }
                                }
                            }
                            else
                            {

                            }

                        }
                        BankingMonths.Add(bankingMonth);
                    }
                }
            }
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
