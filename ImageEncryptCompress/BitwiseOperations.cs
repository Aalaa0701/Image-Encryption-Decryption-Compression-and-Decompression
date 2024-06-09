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
        public static int GeneratePassword(ref Int64 key, int tapPosition, int numberOfBits)
        {
            long desiredBit;
            long result = (1 << 8) - 1;
            for (int i = 0; i < 8; i++)
            {
                key = XORBits(key, numberOfBits, tapPosition);
                desiredBit = key & 1;
                if (desiredBit == 1)
                    continue;
                desiredBit = ~(1 << 7 - i);
                result = result & desiredBit;
            }
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
