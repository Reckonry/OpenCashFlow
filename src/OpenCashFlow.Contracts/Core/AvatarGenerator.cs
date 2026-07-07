using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Contracts.Core
{
    public static class AvatarGenerator
    {
        public static (string BgColor, string TextColor, string Initials) GetAvatar(string name)
        {
            try
            {
                // Divide il nome in parole e rimuove eventuali spazi vuoti
                var nameParts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Calcola le iniziali: prima lettera del nome e prima lettera del cognome
                string initials = nameParts.Length == 0
                    ? "?"
                    : nameParts.Length == 1
                        ? nameParts[0][0].ToString().ToUpper()
                        : $"{nameParts[0][0]}{nameParts[^1][0]}".ToUpper();

                // Determina il colore di sfondo basato sulla prima lettera
                string bgColor = initials[0] switch
                {
                    >= 'A' and <= 'B' => "#F44336",
                    >= 'C' and <= 'D' => "#E91E63",
                    >= 'E' and <= 'F' => "#9C27B0",
                    >= 'G' and <= 'H' => "#673AB7",
                    >= 'I' and <= 'J' => "#3F51B5",
                    >= 'K' and <= 'L' => "#2196F3",
                    >= 'M' and <= 'N' => "#03A9F4",
                    >= 'O' and <= 'P' => "#00BCD4",
                    >= 'Q' and <= 'R' => "#009688",
                    >= 'S' and <= 'T' => "#4CAF50",
                    >= 'U' and <= 'V' => "#8BC34A",
                    >= 'W' and <= 'X' => "#CDDC39",
                    >= 'Y' and <= 'Z' => "#FFEB3B",
                    >= '0' and <= '3' => "#FFC107",
                    >= '4' and <= '6' => "#607D8B",
                    >= '7' and <= '9' => "#FF9800",
                    _ => "#795548"
                };

                // Colore del testo fisso (puoi cambiarlo se necessario)
                string textColor = "#fff";

                return (BgColor: bgColor, TextColor: textColor, Initials: initials);
            }
            catch
            {
                // Valore predefinito in caso di errore
                return ("#2962ff", "#fff", "??");
            }
        }


    }
}