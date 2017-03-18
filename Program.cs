/**
 * PNG Size Parser
 * Copyright (c) 2017 ukiuni
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * PNGデータからサイズを取得します。 フォーマットは http://www.setsuki.com/hsp/ext/png.htm を参照しました。
 * 
 * @author ukiuni
 *
 */
namespace PNGParser
{
    class SizeParser
    {
        public static readonly byte[] DATA_PNG_FILE_SIGNATURE = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        public const int LENGTH_PNG_FILE_SIGNATURE = 8;
        public const int LENGTH_IHDR = 25;
        public static readonly byte[] DATA_IHDR_CUNK_TYPE = new byte[] { (byte)'I', (byte)'H', (byte)'D', (byte)'R' };
        public const int LENGTH_INTEGER_PER_BYTE = 4;
        static void Main(string[] args)
        {
            Size size = SizeParser.Parse(args[0]);
            System.Console.WriteLine("PNG size [" + size.width + " ," + size.height + "]");
        }
        public static Size Parse(String filePath)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(
            filePath,
            System.IO.FileMode.Open,
            System.IO.FileAccess.Read))
            {
                byte[] data = new byte[LENGTH_PNG_FILE_SIGNATURE + LENGTH_IHDR];
                fs.Read(data, 0, data.Length);
                return Parse(data);
            }
        }
        public static Size Parse(byte[] data)
        {
            byte[] actualFirstBytes = new byte[LENGTH_PNG_FILE_SIGNATURE];
            Array.Copy(data, 0, actualFirstBytes, 0, actualFirstBytes.Length);
            if (!DATA_PNG_FILE_SIGNATURE.SequenceEqual(actualFirstBytes))
            {
                throw new NotPNGException("First bytes are wrong");
            }
            byte[] actualIHDRSizeBytes = pullBytes(data, LENGTH_PNG_FILE_SIGNATURE, LENGTH_INTEGER_PER_BYTE);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(actualIHDRSizeBytes);
            }
            int actualIHDRSize = BitConverter.ToInt32(actualIHDRSizeBytes, 0);
            if (13 != actualIHDRSize)
            {
                throw new NotPNGException("IHDR size must be 13");
            }
            byte[] actualIHDRChunkType = pullBytes(data, LENGTH_PNG_FILE_SIGNATURE + 4, DATA_IHDR_CUNK_TYPE.Length);
            if (!DATA_IHDR_CUNK_TYPE.SequenceEqual(actualIHDRChunkType))
            {
                throw new NotPNGException("IHDR chunk type must be \"IHDR\"");
            }
            byte[] widthBytes = pullBytes(data, LENGTH_PNG_FILE_SIGNATURE + actualIHDRSizeBytes.Length + DATA_IHDR_CUNK_TYPE.Length, LENGTH_INTEGER_PER_BYTE);

            byte[] heightBytes = pullBytes(data, LENGTH_PNG_FILE_SIGNATURE + actualIHDRSizeBytes.Length + DATA_IHDR_CUNK_TYPE.Length + widthBytes.Length, LENGTH_INTEGER_PER_BYTE);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(widthBytes);
                Array.Reverse(heightBytes);
            }
            return new Size(BitConverter.ToInt32(widthBytes, 0), BitConverter.ToInt32(heightBytes, 0));
        }
        private static byte[] pullBytes(byte[] src, int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(src, offset, result, 0, result.Length);
            return result;
        }

    }
    public class Size
    {
        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        public int width;
        public int height;
    }
    public class NotPNGException : Exception
    {
        public NotPNGException()
        {
        }

        public NotPNGException(string message)
            : base(message)
        {
        }

        public NotPNGException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

