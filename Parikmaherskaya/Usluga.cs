using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parikmaherskaya
{
    public class Usluga
    {
        public int IdUslugi { get; set; }
        public string Naimenovanie { get; set; } = string.Empty;
        public string Opisanie { get; set; } = string.Empty;
        public int Cena { get; set; }
        public TimeSpan Dlitelnost { get; set; }

        public string CenaFormat => $"{Cena:N0} ₽";
        public string DlitelnostFormat => $"{Dlitelnost.Hours:D2}ч {Dlitelnost.Minutes:D2}мин";
        public int CenaSort => Cena;
        public double DlitelnostSort => Dlitelnost.TotalMinutes;
    }
}