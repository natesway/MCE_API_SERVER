using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Uuid
{
    public class Version4Generator
    {
        private readonly Random _random = new Random();

        public Uuid NewUuid(string name = null)
        {
            byte[] array = new byte[16];
            _random.NextBytes(array);
            array[6] &= 79;
            array[6] |= 64;
            array[8] &= 191;
            array[8] |= 128;
            return new Uuid(array);
        }
    }
}
