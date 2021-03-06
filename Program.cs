﻿/**
 * PNG Size Parser
 * Copyright (c) 2017 ukiuni
 * This software is released under public domain.
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * load size from PNG. implement with refering http://www.setsuki.com/hsp/ext/png.htm .
 * 
 * @author ukiuni
 *
 */
namespace PNGParser
{
    public class SizeParser
    {
        private static readonly byte[] DATA_PNG_FILE_SIGNATURE = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private const int LENGTH_PNG_FILE_SIGNATURE = 8;
        private static readonly byte[] DATA_IHDR_CUNK_TYPE = new byte[] { (byte)'I', (byte)'H', (byte)'D', (byte)'R' };
        private static readonly byte[] DATA_IEND_CUNK_TYPE = new byte[] { (byte)'I', (byte)'E', (byte)'N', (byte)'D' };
        private const int LENGTH_CHUNK_TYPE = 4;
        private const int LENGTH_INTEGER_PER_BYTE = 4;
        private const int LENGTH_CRC = 4;
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
                return Parse(fs);
            }
        }
        public static Size Parse(System.IO.Stream stream)
        {
            byte[] actualFirstBytes = pullBytes(stream, LENGTH_PNG_FILE_SIGNATURE);
            if (!DATA_PNG_FILE_SIGNATURE.SequenceEqual(actualFirstBytes))
            {
                throw new NotPNGException("First bytes are wrong");
            }
            while (true)
            {
                byte[] actualChunkSizeBytes = pullBytes(stream, LENGTH_INTEGER_PER_BYTE);
                int actualChunkSize = toInt(actualChunkSizeBytes);

                byte[] actualChunkType = pullBytes(stream, LENGTH_CHUNK_TYPE);
                if (!DATA_IHDR_CUNK_TYPE.SequenceEqual(actualChunkType))
                {
                    if (DATA_IEND_CUNK_TYPE.SequenceEqual(actualChunkType))
                    {
                        throw new NotPNGException("no IHDR");
                    }
                    pullBytes(stream, actualChunkSize + LENGTH_CRC);
                    continue;
                }
                byte[] widthBytes = pullBytes(stream, LENGTH_INTEGER_PER_BYTE);
                byte[] heightBytes = pullBytes(stream, LENGTH_INTEGER_PER_BYTE);

                return new Size(toInt(widthBytes), toInt(heightBytes));
            }
        }
        private static byte[] pullBytes(System.IO.Stream stream, int length)
        {
            byte[] result = new byte[length];
            int readed = stream.Read(result, 0, length);
            if (0 == readed)
            {
                throw new NotPNGException("File ended before found IHDR");
            }
            return result;
        }
        private static int toInt(byte[] src)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(src);
            }
            return BitConverter.ToInt32(src, 0);
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

