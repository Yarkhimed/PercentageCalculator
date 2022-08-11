using IronXL;
using System.Text;

namespace PercentageCalculator
{
    internal class Transaction
    {
        public Transaction(Cell[] row)
        {
            TransactionTime = DateTime.Parse(row[0].StringValue);
            Category = row[1].StringValue;
            Desctiption = row[2].StringValue;
            MCC = row[3].StringValue;
            Amount = row[4].DecimalValue;
            Currency = row[5].StringValue;
            OriginalAmount = row[6].DecimalValue;
            OriginalCurrency = row[7].StringValue;
            Fee = row[8].DecimalValue;
            Cashback = row[9].DecimalValue;
            Balance = row[10].DecimalValue;
        }
        public DateTime? TransactionTime { get; set; }
        public string Category { get; set; }
        public string Desctiption { get; set; }
        public string MCC { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal OriginalAmount { get; set; }
        public string OriginalCurrency { get; set; }
        public decimal Fee { get; set; }
        public decimal Cashback { get; set; }
        public decimal Balance { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetValue(this) != null)
                {
                    sb.Append($"{prop.Name}: {prop.GetValue(this)} ");
                }
            }
            return sb.ToString();
        }

        private List<string> headers = new()
        {
            /*["Дата і час"] = "TransactionTime",
            ["Категорія"] = "Category",
            ["Опис"] = "Desctiption",
            ["Код МСС"] = "MCC",
            ["Сума у валютi карти"] = "Amount",
            ["Валюта карти"] = "Currency",
            ["Сума у валютi операцiї"] = "OriginalAmount",
            ["Валюта операцiї"] = "OriginalCurrency",
            ["Комісія"] = "Fee",
            ["Кешбек"] = "Cashback",
            ["Залишок"] = "Balance"*/
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
