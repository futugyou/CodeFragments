using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Tools
{
    public static class ChineseIDCardCheck
    {
        private readonly static int[] fixnums = new int[17] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
        private readonly static char[] checknums = new char[11] { '1', '0', 'x', '9', '8', '7', '6', '5', '4', '3', '2' };
        public static bool IsChineseIDCard(this string card)
        {
            if (string.IsNullOrEmpty(card) || card.Length != 18)
            {
                return false;
            }
            var cards = card.ToCharArray();
            var sum = 0;
            for (int i = 0; i < fixnums.Length; i++)
            {
                sum += fixnums[i] * (int)Char.GetNumericValue(cards[i]);
            }
            var mod = sum % 11;

            return checknums[mod] == card[17];
        }
    }
}
