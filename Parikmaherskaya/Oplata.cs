using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parikmaherskaya
{
    public class OplataView
    {
        public int IdOplaty { get; set; }
        public decimal Summa { get; set; }
        public DateTime Data { get; set; }
        public string NomerCheka { get; set; } = string.Empty;
        public string KlientFIO { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Sposob { get; set; } = string.Empty;

        public string SummaFormat => $"{Summa:N0} ₽";
        public string DataFormat => Data.ToString("dd.MM.yyyy");
        public decimal SummaSort => Summa;

    }
}