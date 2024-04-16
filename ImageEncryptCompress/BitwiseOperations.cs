using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageEncryptCompress
{
    internal class BitwiseOperations
    {
        public static int XORBits(int key, int numberOfBits, int tapPosition)
        {
            int keyMask = 1 << tapPosition;
            int lastPosMask = 1 << numberOfBits - 1;
            int capMask = (1 << numberOfBits) - 1;
            int tapBit = (key & keyMask);
            int lastBit = (key & lastPosMask) >> numberOfBits - tapPosition - 1;
            int xorResult = (tapBit ^ lastBit) >> tapPosition;
            int shiftedKey = key << 1;
            int result = (shiftedKey | xorResult) & capMask;
            return result;
        }
        public static int GeneratePassword(ref string key, int tapPosition, int numberOfBitsToGenerate = 8)
        {
            int convertedKey = Convert.ToInt32(key, 2);
            int numberOfBits = key.Length;
            for (int i = 0; i < numberOfBitsToGenerate; i++)
            {
                convertedKey = XORBits(convertedKey, numberOfBits, tapPosition);
            }
            key = Convert.ToString(convertedKey, 2).PadLeft(numberOfBits, '0');
            int result = convertedKey & ((1 << numberOfBitsToGenerate) - 1);
            return result;
        }
    }
}
