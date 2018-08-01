using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lectureTXIDAttempt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
            {
                string path = args[0];
                byte[] buffer = File.ReadAllBytes(path);
                lecture(path, buffer);
            }
        }

        public static void lecture(string fileName, byte[] buffer)
        {
            if (Encoding.UTF8.GetString(buffer).Contains("TXID"))
            {
                string modelName = Path.GetFileName(fileName);
                string[] array = modelName.Split('.');
                string subName = array[0];

                Console.WriteLine("- " + subName);

                List<long> positionOffset = new List<long>();

                if (Encoding.UTF8.GetString(buffer).Contains("TXID"))
                {
                    long offset = SearchPattern(buffer, Encoding.UTF8.GetBytes("TXID"));
                    positionOffset.Add(offset);
                }

                long firstChunkPos = positionOffset.Any() ? positionOffset.Min() : 0;

                Console.WriteLine("Offset TXID : " + firstChunkPos);

                List<UInt32> TextureFiledataID = new List<UInt32>();

                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                using (var br = new BinaryReader(fs))
                {

                    br.BaseStream.Seek(firstChunkPos+4, SeekOrigin.Current);
                    UInt32 TXIDSize = br.ReadUInt32()/4;
                    Console.WriteLine("TXID Size : " + TXIDSize);

                    long EndPose = firstChunkPos + 8 + TXIDSize*4;

                    long positionStream = br.BaseStream.Position; 

                    do
                    {
                        if (br.BaseStream.Position <= EndPose - 4)
                        {
                            UInt32 texture = br.ReadUInt32();
                            br.BaseStream.Position = positionStream + 4;
                            positionStream = br.BaseStream.Position;
                            TextureFiledataID.Add(texture);
                        }

                    } while (positionStream <= EndPose - 4);
                }

                for(UInt16 i = 0; i < TextureFiledataID.Count; i++)
                {
                    Console.WriteLine("TextureFileDataID " + i + " : " + TextureFiledataID.ElementAt(i));
                }

                    Console.ReadLine();
            }
        }

        private unsafe static long SearchPattern(byte[] haystack, byte[] needle)
        {
            fixed (byte* h = haystack) fixed (byte* n = needle)
            {
                for (byte* hNext = h, hEnd = h + haystack.Length + 1 - needle.Length, nEnd = n + needle.Length; hNext < hEnd; hNext++)
                    for (byte* hInc = hNext, nInc = n; *nInc == *hInc; hInc++)
                        if (++nInc == nEnd)
                            return hNext - h;
                return -1;
            }
        }
    }
}
