using System;
using System.Text.RegularExpressions;

namespace MCE_API_SERVER.Uuid
{
    public class Uuid
    {
        private readonly byte[] _uuid;

        private static readonly Regex _regex = new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsUuid(string text)
        {
            if (text.Length == 36) {
                return _regex.IsMatch(text);
            }

            return false;
        }

        public Uuid(string text)
        {
            if (!IsUuid(text)) {
                throw new ArgumentException("text is not a valid Uuid. Got: " + text);
            }

            _uuid = Transcoder.HexToBin(text.Replace("-", string.Empty));
        }

        public Uuid(byte[] bytes)
        {
            if (bytes.Length != 16) {
                throw new ArgumentException("Length of bytes for new Uuid is not 16. Got: " + bytes.Length);
            }

            _uuid = bytes;
        }

        public override string ToString()
        {
            string text = Transcoder.BinToHex(_uuid);
            return $"{text.Substring(0, 8)}-{text.Substring(8, 4)}-{text.Substring(12, 4)}-{text.Substring(16, 4)}-{text.Substring(20, 12)}";
        }

        public byte[] ToByteArray()
        {
            return _uuid;
        }

        public Guid ToGuid()
        {
            return new Guid(ToString());
        }
    }
}
