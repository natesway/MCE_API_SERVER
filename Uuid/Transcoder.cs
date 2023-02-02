using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Uuid
{
    public static class Transcoder
    {
        private const byte ASCII_0 = 48;

        private const byte ASCII_9 = 57;

        private const byte ASCII_A = 65;

        private const byte ASCII_F = 70;

        private const byte ASCII_a = 97;

        private const byte ASCII_f = 102;

        public static string BinToHex(byte[] input)
        {
            StringBuilder stringBuilder = new StringBuilder(2 * input.Length);
            foreach (byte num in input) {
                byte b = (byte)((int)num / 16);
                byte b2 = (byte)((int)num % 16);
                stringBuilder.Append(HexEncode(b));
                stringBuilder.Append(HexEncode(b2));
            }

            return stringBuilder.ToString();
        }

        public static byte[] HexToBin(string input)
        {
            if (input.Length % 2 != 0) {
                throw new ArgumentException("dafuq is this");
            }

            byte[] array = new byte[input.Length / 2];
            for (int i = 0; i < array.Length; i++) {
                array[i] = (byte)(16 * HexDecode(input[2 * i]) + HexDecode(input[2 * i + 1]));
            }

            return array;
        }

        private static byte HexDecode(char hb)
        {
            byte b = (byte)hb;
            if (48 <= b && b <= 57) {
                return (byte)(b - 48);
            }

            if (65 <= b && b <= 70) {
                return (byte)(b - 65 + 10);
            }

            if (97 <= b && b <= 102) {
                return (byte)(b - 97 + 10);
            }

            throw new ArgumentException("fuck this input");
        }

        private static char HexEncode(byte b)
        {
            if (b <= 9) {
                return (char)(b + 48);
            }

            return (char)(b + 97 - 10);
        }
    }
}
