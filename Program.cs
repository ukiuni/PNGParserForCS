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
        public static readonly int LENGTH_PNG_FILE_SIGNATURE = DATA_PNG_FILE_SIGNATURE.Length;
        public static readonly int LENGTH_IHDR = 25;
        public static readonly byte[] DATA_IHDR_CUNK_TYPE = new byte[] { (byte)'I', (byte)'H', (byte)'D', (byte)'R', };
        static void Main(string[] args)
        {
            Size size = Parse(args[0]);
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
            byte[] actualIHDRSizeBytes = new byte[4];
            Array.Copy(data, LENGTH_PNG_FILE_SIGNATURE, actualIHDRSizeBytes, 0, actualIHDRSizeBytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(actualIHDRSizeBytes);
            }
            int actualIHDRSize = BitConverter.ToInt32(actualIHDRSizeBytes, 0);
            if (13 != actualIHDRSize)
            {
                throw new NotPNGException("IHDR size must be 13");
            }
            byte[] actualIHDRChunkType = new byte[DATA_IHDR_CUNK_TYPE.Length];
            Array.Copy(data, LENGTH_PNG_FILE_SIGNATURE + 4, actualIHDRChunkType, 0, actualIHDRChunkType.Length);
            if (!DATA_IHDR_CUNK_TYPE.SequenceEqual(actualIHDRChunkType))
            {
                throw new NotPNGException("IHDR chunk type must be \"IHDR\"");
            }
            byte[] widthBytes = new byte[4];
            Array.Copy(data, LENGTH_PNG_FILE_SIGNATURE + actualIHDRSizeBytes.Length + DATA_IHDR_CUNK_TYPE.Length, widthBytes, 0, widthBytes.Length);

            byte[] heightBytes = new byte[4];
            Array.Copy(data, LENGTH_PNG_FILE_SIGNATURE + actualIHDRSizeBytes.Length + DATA_IHDR_CUNK_TYPE.Length + widthBytes.Length, heightBytes, 0, heightBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(widthBytes);
                Array.Reverse(heightBytes);
            }
            return new Size(BitConverter.ToInt32(widthBytes, 0), BitConverter.ToInt32(heightBytes, 0));

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

