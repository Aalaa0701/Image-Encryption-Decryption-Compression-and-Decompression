using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageEncryptCompress
{
    internal class BitwiseOperations
    {
        public static long XORBits(long key, int numberOfBits, int tapPosition)
        {
            long keyMask = 1 << tapPosition;
            long lastPosMask = 1 << numberOfBits - 1;
            long capMask = (1 << numberOfBits) - 1;
            long tapBit = (key & keyMask);
            long lastBit = (key & lastPosMask) >> numberOfBits - tapPosition - 1;
            long xorResult = (tapBit ^ lastBit) >> tapPosition;
            long shiftedKey = key << 1;
            long result = (shiftedKey | xorResult) & capMask;
            return result;
        }
        public static int GeneratePassword(ref string key, int tapPosition, int numberOfBitsToGenerate = 8)
        {
            long convertedKey = Convert.ToInt64(key, 2);
            int numberOfBits = key.Length;
            long desiredBit;
            long result = (1 << 8) - 1;
            string bitADDER;
            for (int i = 0; i < numberOfBitsToGenerate; i++)
            {
                convertedKey = XORBits(convertedKey, numberOfBits, tapPosition);
                desiredBit = convertedKey & 1;
                if (desiredBit == 1)
                    continue;
                desiredBit = ~(1 << 7 - i);

                result = result & desiredBit;


            }
            key = Convert.ToString(convertedKey, 2).PadLeft(numberOfBits, '0');
            return (int)result;
        }
        public static byte GenerateAlphanumericPassword(ref StringBuilder seed, int tapPos, int seedLength)
        {
            byte lastChar = Convert.ToByte(seed[0]);
            byte charTapPos = Convert.ToByte(seed[seedLength - tapPos]);
            byte generatedChar = (byte)((int)lastChar ^ (int)charTapPos);
            seed.Remove(0, 1);
            seed.Append((char)generatedChar);
            return generatedChar;
        }
        public static int CountBits(int n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n >>= 1;
            }
            return count;
        }
    }
}
