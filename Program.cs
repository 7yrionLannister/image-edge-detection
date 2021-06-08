using System;
using System.Collections.Generic;
using System.IO;

namespace image_filter
{
    class Program
    {
        static int offset = 1078;
        static sbyte[][] kernel;

        static int Main(string[] args)
        {
            long start = DateTime.Now.Ticks;
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
            String dimension = args[0];
            try
            {
                int.Parse(dimension);
            }
            catch 
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

            //https://stackoverflow.com/questions/7296534/how-to-read-an-image-file-to-a-byte
            // Load file meta data with FileInfo
            String path = "../../../imgs/in/" + dimension + "x" + dimension + "x8.bmp";
            FileInfo fileInfo = new FileInfo(path);
            // The byte[] to save the data in
            byte[] data = new byte[fileInfo.Length];

            // Load a filestream and put its content into the byte[]
            using (FileStream fs = fileInfo.OpenRead())
            {
                fs.Read(data, 0, data.Length);
            }
            Console.WriteLine("File size is " + data.Length + " bytes and has an offset of 1078 bytes");

            // Pass the image through the filter
            data = filter(data, algorithm);

            // Write the output image to a BMP file
            FileInfo output = new FileInfo("../../../imgs/out/" + dimension + "x" + dimension + "x8-" + algorithm + ".bmp");
            using (FileStream fs = output.OpenWrite())
            {
                fs.Write(data, 0, data.Length);
            }
            return 0; // success return code
        }

        static byte[] filter(byte[] data, String algorithm)
        {
            switch (algorithm)
            {
                case "1":
                    return filteringAlgorithmXYIJ(data);
                case "2":
                    return filteringAlgorithmXYJI(data);
                case "3":
                    return filteringAlgorithmXYUnrolling(data);
                case "4":
                    return filteringAlgorithmYXIJ(data);
                case "5":
                    return filteringAlgorithmYXJI(data);
                case "6":
                    return filteringAlgorithmYXUnrolling(data);
            }
            return null;
        }

        // There is a 1078 offset in the BMP file format, so the data of the image starts at byte 1078 (position 1078)
        static byte[] filteringAlgorithmXYIJ(byte[] data)
        {
            byte[] C = (byte[])data.Clone();
            int n = (int)Math.Sqrt(data.Length - offset);
            int xlim = offset + n * (n - 1) + 1 - n; // subtract n becasue image lower limit is ignored
            long sum = 0;
            long count = 0;
            long start = DateTime.Now.Ticks;
            int k = kernel.Length;
            for (int x = offset + n; x < xlim; x += n) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int y = 1; y < n - 1; y++) // starts at 1 because
                {
                    C[x + y] = 0; // resets target matrix on the go
                    for (int i = 0; i < k; i++)
                    {
                        for (int j = 0; j < k; j++)
                        {
                            int row = x + (i * n) - n - 1;
                            int col = y + j - 1;

                            C[x + y] = (byte)(C[x + y] + data[row + col] * kernel[i][j]);
                        }
                    }
                    count++;
                    sum += C[x + y];
                }
            }
            long end = DateTime.Now.Ticks;
            // according to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=net-5.0#remarks, Ticks are 10^-7 seconds, so we multiply by 100 to convert to 10^-9 seconds, nanoseconds
            Console.WriteLine("Time executing(ns) is " + ((end - start) * 100)); // print time in nanoseconds
            Console.WriteLine("The sum of the processed pixels is " + sum);
            Console.WriteLine("The number of pixels processed is " + count);
            return C;
        }

        static byte[] filteringAlgorithmXYJI(byte[] data)
        {
            byte[] C = (byte[])data.Clone();
            int offset = 1078;
            int n = (int)Math.Sqrt(data.Length - offset);
            int xlim = offset + n * (n - 1) + 1 - n; // subtract n becasue image lower limit is ignored
            long sum = 0;
            long count = 0;
            int k = 3;
            long start = DateTime.Now.Ticks;
            for (int x = offset + n; x < xlim; x += n) // starts at offset
                                                       // + n because image upper limit of the image is ignored
            {
                for (int y = 1; y < n - 1; y++) // starts at 1 because
                {
                    C[x + y] = 0; // resets target matrix on the go
                    for (int j = 0; j < k; j++)
                    {
                        for (int i = 0; i < k; i++)
                        {

                            int row = x + (i * n) - n - 1;
                            int col = y + j - 1;

                            C[x + y] = (byte)(C[x + y] + data[row + col] * kernel[i][j]);
                        }
                    }
                    count++;
                    sum += C[x + y];
                }
            }
            long end = DateTime.Now.Ticks;
            // according to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=net-5.0#remarks, Ticks are 10^-7 seconds, so we multiply by 100 to convert to 10^-9 seconds, nanoseconds
            Console.WriteLine("Time executing(ns) is " + ((end - start) * 100)); // print time in nanoseconds
            Console.WriteLine("The sum of the processed pixels is " + sum);
            Console.WriteLine("The number of pixels processed is " + count);
            return C;
        }

        static byte[] filteringAlgorithmXYUnrolling(byte[] data)
        {
            byte[] C = (byte[])data.Clone();
            int n = (int)Math.Sqrt(data.Length - offset);
            int xlim = offset + n * (n - 1) + 1 - n; // subtract n becasue image lower limit is ignored
            long sum = 0;
            long count = 0;
            long start = DateTime.Now.Ticks;
            for (int x = offset + n; x < xlim; x += n) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int y = 1; y < n - 1; y++) // starts at 1 because
                {
                    // f(i,j) = X + (i-1)*n + y + j - 2
                    C[x + y] = (byte)
                        (data[x - n + y - 2] * kernel[0][0] // 0,0
                        + data[x - n - 1 + y] * kernel[0][1] // 0,1
                        + data[x - n + y] * kernel[0][2] // 0,2
                        + data[x + y - 2] * kernel[1][0] // 1,0
                        + data[x + y - 1] * kernel[1][1] // 1,1
                        + data[x + y] * kernel[1][2] // 1,2
                        + data[x + n + y - 2] * kernel[2][0] // 2,0
                        + data[x + n + y - 1] * kernel[2][1] // 2,1
                        + data[x + n + y] * kernel[2][2] // 2,2
                        );

                    count++;
                    sum += C[x + y];
                }
            }
            long end = DateTime.Now.Ticks;
            // according to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=net-5.0#remarks, Ticks are 10^-7 seconds, so we multiply by 100 to convert to 10^-9 seconds, nanoseconds
            Console.WriteLine("Time executing(ns) is " + ((end - start) * 100)); // print time in nanoseconds
            Console.WriteLine("The sum of the processed pixels is " + sum);
            Console.WriteLine("The number of pixels processed is " + count);
            return C;
        }

        static byte[] filteringAlgorithmYXIJ(byte[] data)
        {
            byte[] C = (byte[])data.Clone();
            int n = (int)Math.Sqrt(data.Length - offset);
            int xlim = offset + n * (n - 1) + 1 - n; 
            long sum = 0;
            long count = 0;
            long start = DateTime.Now.Ticks;
            int k = kernel.Length;
            for (int y = 1; y < n - 1; y++) 
            {
                for (int x = offset + n; x < xlim; x += n)
                {
                    C[y + x] = 0; 
                    for (int i = 0; i < k; i++)
                    {
                        for (int j = 0; j < k; j++)
                        {
                            int col = y + j - 1;
                            int row = x + (i * n) - n - 1;

                            C[y + x] = (byte)(C[y + x] + data[col + row] * kernel[i][j]);
                        }
                    }
                    count++;
                    sum += C[y + x];
                }
            }
            long end = DateTime.Now.Ticks;
            // according to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=net-5.0#remarks, Ticks are 10^-7 seconds, so we multiply by 100 to convert to 10^-9 seconds, nanoseconds
            Console.WriteLine("Time executing(ns) is " + ((end - start) * 100)); // print time in nanoseconds
            Console.WriteLine("The sum of the processed pixels is " + sum);
            Console.WriteLine("The number of pixels processed is " + count);
            return C;
        }

        static byte[] filteringAlgorithmYXJI(byte[] data)
        {
            byte[] C = (byte[])data.Clone();
            int offset = 1078;
            int n = (int)Math.Sqrt(data.Length - offset);
            int xlim = offset + n * (n - 1) + 1 - n; 
            long sum = 0;
            long count = 0;
            long start = DateTime.Now.Ticks;
            int k = kernel.Length;
            for (int y = 1; y < n - 1; y++) 
                                                       
            {
                for (int x = offset + n; x < xlim; x += n) 
                {
                    C[y + x] = 0; 
                    for (int j = 0; j < k; j++)
                    {
                        for (int i = 0; i < k; i++)
                        {
                            int col = y + j - 1;
                            int row = x + (i * n) - n - 1;

                            C[y + x] = (byte)(C[y + x] + data[col + row] * kernel[i][j]);
                        }
                    }
                    count++;
                    sum += C[y + x];
                }
            }
            long end = DateTime.Now.Ticks;
            // according to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=net-5.0#remarks, Ticks are 10^-7 seconds, so we multiply by 100 to convert to 10^-9 seconds, nanoseconds
            Console.WriteLine("Time executing(ns) is " + ((end - start) * 100)); // print time in nanoseconds
            Console.WriteLine("The sum of the processed pixels is " + sum);
            Console.WriteLine("The number of pixels processed is " + count);
            return C;
        }

        static byte[] filteringAlgorithmYXUnrolling(byte[] data)
        {
            byte[] C = (byte[])data.Clone();
            int n = (int)Math.Sqrt(data.Length - offset);
            int xlim = offset + n * (n - 1) + 1 - n; // subtract n becasue image lower limit is ignored
            long sum = 0;
            long count = 0;
            long start = DateTime.Now.Ticks;
            for (int y = 1; y < n - 1; y++) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int x = offset + n; x < xlim; x += n) // starts at 1 because 
                {
                    // f(i,j) = X + (i-1)*n + y + j - 2
                    C[x + y] = (byte)
                        (data[x - n + y - 2] * kernel[0][0] // 0,0
                        + data[x - n - 1 + y] * kernel[0][1] // 0,1
                        + data[x - n + y] * kernel[0][2] // 0,2
                        + data[x + y - 2] * kernel[1][0] // 1,0
                        + data[x + y - 1] * kernel[1][1] // 1,1
                        + data[x + y] * kernel[1][2] // 1,2
                        + data[x + n + y - 2] * kernel[2][0] // 2,0
                        + data[x + n + y - 1] * kernel[2][1] // 2,1
                        + data[x + n + y] * kernel[2][2] // 2,2
                        );

                    count++;
                    sum += C[x + y];
                }
            }
            long end = DateTime.Now.Ticks;
            // according to https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=net-5.0#remarks, Ticks are 10^-7 seconds, so we multiply by 100 to convert to 10^-9 seconds, nanoseconds
            Console.WriteLine("Time executing(ns) is " + ((end - start) * 100)); // print time in nanoseconds
            Console.WriteLine("The sum of the processed pixels is " + sum);
            Console.WriteLine("The number of pixels processed is " + count);
            return C;
        }

        static void printUsage() {
            Console.Error.WriteLine("Usage: ");
            Console.Error.WriteLine("You must specify the dimension of the input image and the algorithm that performs the filter through positional arguments");
            Console.Error.WriteLine("$ ./Program.exe n a");
            Console.Error.WriteLine("# Where n is the size of the image, which is assumed to be squared and be located in the imgs/in folder with a name of nxnx8.bmp");
            Console.Error.WriteLine("# Where a is any of");
            Console.Error.WriteLine("                   * 1 for x-y-i-j");
            Console.Error.WriteLine("                   * 2 for x-y-j-i");
            Console.Error.WriteLine("                   * 3 for x-y-unrolling");
            Console.Error.WriteLine("                   * 4 for y-x-i-j");
            Console.Error.WriteLine("                   * 5 for y-x-j-i");
            Console.Error.WriteLine("                   * 6 for y-x-unrolling");
        }

        /*static byte[] filteringAlgorithmXYIJ2(byte[] data)
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
            
            for (int x = 1; x < n - 1; x++) // starts at offset + n because image upper limit of the image is ignored
            {
                for (int y = 1; y < n - 1; y++) // starts at 1 because
                {
                    data[offset + x * n + y] = C[x, y];
                }
            }

            return data;
        }*/
    }
}
