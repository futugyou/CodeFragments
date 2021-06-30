using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CodeFragments
{
    class RandomNumber
    {
        public void UseRandomNumberGenerator()
        {
            byte[] randomBytes = new byte[4];
            var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomBytes);
            var result = BitConverter.ToInt32(randomBytes, 0);
            Console.WriteLine(result);
            result = RandomNumberGenerator.GetInt32(10, 100);
            Console.WriteLine(result);
        }
    }
}
