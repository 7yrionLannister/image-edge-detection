﻿using System;
using System.Collections.Generic;
using System.IO;

namespace image_filter
{
    class Program
    {
        static sbyte[][] kernel;
        static int Main(string[] args)
        {
            // Kernel for group 2
            kernel = new sbyte[][] { new sbyte[] { -1, -1, -1 },
                                     new sbyte[] { -1,  8, -1 },
                                     new sbyte[] { -1, -1, -1 }
            };

            if (args.Length < 2)
            {
                printUsage();
                return 1; // failure return code
            }
            List<String> dims = new List<string>{"80", "150", "200", "400", "800", "1200", "1400", "1600", "2000", "2500", "3200", "3500"};
            String dimension = args[0];
            if (!dims.Contains(dimension))
            {
                printUsage();
                return 1;
            }
            String algorithm = args[1];
            if (algorithm.CompareTo("1") < 0 || algorithm.CompareTo("6") > 0)
            {
                printUsage();
                return 1;
            }

            // Load file meta data with FileInfo
            String path = "../../../imgs/" + dimension + "x" + dimension + "x8.bmp";
            FileInfo fileInfo = new FileInfo(path);
            // The byte[] to save the data in
            byte[] data = new byte[fileInfo.Length];

            // Load a filestream and put its content into the byte[]
            using (FileStream fs = fileInfo.OpenRead())
            {
                fs.Read(data, 0, data.Length);
            }
            Console.WriteLine(data.Length);
            
            // Post byte[] to database
            Console.WriteLine("Hello World! " + data.Length);

            for (int i = 1077; i < data.Length; i++) {
                data[i] /= 2;
            }
            
            data = filter(data, algorithm);

            FileInfo output = new FileInfo("../../../imgs/out/" + dimension + "x" + dimension + "x8.bmp");
            using (FileStream fs = output.OpenWrite())
            {
                fs.Write(data, 0, data.Length);
            }

            return 0; // success return code
        }

        static byte[] filter(byte[] data, String algorithm) {
            long start = DateTime.Now.Ticks;
            switch (algorithm)
            {
                case "1":
                    return filteringAlgorithmXYIJ(data);
            }
            long end = DateTime.Now.Ticks;
            Console.WriteLine((end - start)/100); // print time in nanoseconds
            // according to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=net-5.0#remarks, Ticks are a 10^-7 seconds, so we divide by 100 to convert to 10^-9, nanoseconds
            return null;
        }

        // There is a 1078 offset in the BMP file format, so the data of the image starts at byte 1078 (position 1078)
        static byte[] filteringAlgorithmXYIJ(byte[] data) {
            byte[] C = (byte[]) data.Clone();
            int offset = 1078;
            int n = (int) Math.Sqrt(data.Length - offset);
            int xlim = offset + n*(n-1) + 1 - n; // subtract n becasue image lower limit is ignored
            for (int x = offset + n; x < xlim; x += n) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int y = 1; y < n; y++) // starts at 1 because
                {
                    C[x + y] = 0; // resets target matrix on the go
                    for (int i = 0; i < kernel.Length; i++)
                    {
                        for (int j = 0; j < kernel[i].Length; j++)
                        {
                            int row = x + (i * n) - n - 1;
                            int col = y + j - 1;
                            
                            C[x + y] = (byte) (C[x + y] + data[row + col] * kernel[i][j]);
                        }
                    }
                }
            }
            return C;
        }

        static byte[] filteringAlgorithmXYIJ2(byte[] data)
        {
            int offset = 1078;
            int n = (int)Math.Sqrt(data.Length - offset);
            byte[,] C = new byte[n,n];
            for (int x = 1; x < n - 1; x++) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int y = 1; y < n - 1; y++) // starts at 1 because
                {
                    for (int i = 0; i < kernel.Length; i++)
                    {
                        for (int j = 0; j < kernel[i].Length; j++)
                        {
                            int row = x + i - 1;
                            int col = y + j - 1;

                            C[x,y] = (byte)(C[x,y] + data[offset + row*n + col] * kernel[i][j]);
                        }
                    }
                }
            }
            /*
            for (int x = 1; x < n - 1; x++) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int y = 1; y < n - 1; y++) // starts at 1 because
                {
                    data[offset + x * n + y] = C[x, y];
                }
            }*/
            int co = 0;
            for (int x = 0; x < n; x++) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int y = 0; y < n; y++) // starts at 1 because
                {
                    data[offset + co] = C[x, y];
                    co++;
                }
            }

            return data;
        }

        static void printUsage() {
            Console.Error.WriteLine("Usage: ");
            Console.Error.WriteLine("You must specify the dimension of the input image and the algorithm that performs the filter through positional arguments");
            Console.Error.WriteLine("$ ./Program.exe n a");
            Console.Error.WriteLine("# Where n is any of");
            Console.Error.WriteLine("                   * 80");
            Console.Error.WriteLine("                   * 150");
            Console.Error.WriteLine("                   * 200");
            Console.Error.WriteLine("                   * 400");
            Console.Error.WriteLine("                   * 800");
            Console.Error.WriteLine("                   * 1200");
            Console.Error.WriteLine("                   * 1400");
            Console.Error.WriteLine("                   * 1600");
            Console.Error.WriteLine("                   * 2000");
            Console.Error.WriteLine("                   * 2500");
            Console.Error.WriteLine("                   * 3200");
            Console.Error.WriteLine("                   * 3500");
            Console.Error.WriteLine("# Where a is any of");
            Console.Error.WriteLine("                   * 1 for x-y-i-j");
            Console.Error.WriteLine("                   * 2 for x-y-j-i");
            Console.Error.WriteLine("                   * 3 for x-y-unrolling");
            Console.Error.WriteLine("                   * 4 for y-x-i-j");
            Console.Error.WriteLine("                   * 5 for y-x-j-i");
            Console.Error.WriteLine("                   * 6 for y-x-unrolling");
        }
    }
}