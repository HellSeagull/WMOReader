using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMOReader
{
    class Reader
    {

        private readonly string[] ChunkNames = new[] { "REVM", "OMOM", "DHOM", "XTOM", "TMOM", "VOUM", "NGOM",
                                                        "IGOM", "BSOM", "VPOM", "TPOM", "RPOM", "VVOM", "BVOM",
                                                        "TLOM", "SDOM", "NDOM", "DDOM", "GOFM", "PVCM", "DIFG"};

        private readonly string[] SubChunkNames = new[] { "REVM", "PGOM", "YPOM", "IVOM", "TVOM", "RNOM", "VTOM",
                                                        "ABOM", "RLOM", "RDOM", "NBOM", "RBOM", "VBPM", "PBPM",
                                                        "IBPM", "GBPM", "QILM", "IROM", "BROM", "2VTOM", "2VCOM"};

        private Dictionary<string, long> Offsets = new Dictionary<string, long>();


        public void lectureWMO(string path, byte[] buffer)
        {

            string modelName = Path.GetFileName(path);
            string[] array = modelName.Split('.');
            string subName = array[0];

            Console.WriteLine("WMO : " + subName);
            Console.WriteLine();
            Console.WriteLine();

            foreach (var chunk in ChunkNames)
            {
                if (Encoding.UTF8.GetString(buffer).Contains(chunk))
                {
                    long offset = SearchPattern(buffer, Encoding.UTF8.GetBytes(chunk));
                    Offsets.Add(chunk, offset);
                }
            }

            Dictionary<string, long> DuplicatePositionOffset = Offsets;

            foreach (KeyValuePair<string, long> entry in Offsets)
            {
                Console.WriteLine("CHUNK : " + entry.Key + " at Offset " + entry.Value);
            }

            Console.WriteLine();
            Console.WriteLine();

            foreach (KeyValuePair<string, long> entry in Offsets)
            {
                if (entry.Key.Equals("DIFG"))
                {
                    List<UInt32> LODFileDataID = new List<UInt32>();

                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                    using (var br = new BinaryReader(fs))
                    {

                        br.BaseStream.Seek(entry.Value + 4, SeekOrigin.Current);
                        UInt32 CHUNKSize = br.ReadUInt32() / 4;
                        Console.WriteLine("DIFG Size : " + CHUNKSize);

                        long EndPose = entry.Value + 8 + CHUNKSize * 4;

                        long positionStream = br.BaseStream.Position;

                        do
                        {
                            if (br.BaseStream.Position <= EndPose - 4)
                            {
                                UInt32 LOD = br.ReadUInt32();
                                br.BaseStream.Position = positionStream + 4;
                                positionStream = br.BaseStream.Position;
                                LODFileDataID.Add(LOD);
                            }

                        } while (positionStream <= EndPose - 4);
                    }

                    for (UInt16 i = 0; i < LODFileDataID.Count; i++)
                    {
                        Console.WriteLine("LODFileDataID " + i + " : " + LODFileDataID.ElementAt(i));
                    }

                    Console.WriteLine();
                    Console.WriteLine();

                    List<string> found = new List<string>();

                    string line = string.Empty;

                    using (var file = new StreamReader(Directory.GetCurrentDirectory() + @"\WMO_Root"))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            foreach(var lod in LODFileDataID)
                            {
                                if (line.Contains(lod.ToString()))
                                {
                                    found.Add(line);
                                }
                            }     
                        }
                    }

                    List<string> LodFilesLinked = new List<string>();

                    foreach(var lodFile in found)
                    {
                        lodFile.Split(',').ToArray().ElementAt(0);
                        string file = lodFile;
                        LodFilesLinked.Add(file);
                    }

                    for(UInt16 i = 0; i < LodFilesLinked.Count; i++)
                    {
                        Console.WriteLine("Fichier Lod lié " + i + " : " + LodFilesLinked.ElementAt(i));
                    }

                }

            }

        }

        //public void lecture(string fileName, byte[] buffer)
        //{
        //    if (Encoding.UTF8.GetString(buffer).Contains("TXID"))
        //    {
        //        string modelName = Path.GetFileName(fileName);
        //        string[] array = modelName.Split('.');
        //        string subName = array[0];

        //        Console.WriteLine("- " + subName);

        //        List<long> positionOffset = new List<long>();

        //        if (Encoding.UTF8.GetString(buffer).Contains("TXID"))
        //        {
        //            long offset = SearchPattern(buffer, Encoding.UTF8.GetBytes("TXID"));
        //            positionOffset.Add(offset);
        //        }

        //        long firstChunkPos = positionOffset.Any() ? positionOffset.Min() : 0;

        //        Console.WriteLine("Offset TXID : " + firstChunkPos);

        //        List<UInt32> TextureFiledataID = new List<UInt32>();

        //        using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
        //        using (var br = new BinaryReader(fs))
        //        {

        //            br.BaseStream.Seek(firstChunkPos + 4, SeekOrigin.Current);
        //            UInt32 TXIDSize = br.ReadUInt32() / 4;
        //            Console.WriteLine("TXID Size : " + TXIDSize);

        //            long EndPose = firstChunkPos + 8 + TXIDSize * 4;

        //            long positionStream = br.BaseStream.Position;

        //            do
        //            {
        //                if (br.BaseStream.Position <= EndPose - 4)
        //                {
        //                    UInt32 texture = br.ReadUInt32();
        //                    br.BaseStream.Position = positionStream + 4;
        //                    positionStream = br.BaseStream.Position;
        //                    TextureFiledataID.Add(texture);
        //                }

        //            } while (positionStream <= EndPose - 4);
        //        }

        //        for (UInt16 i = 0; i < TextureFiledataID.Count; i++)
        //        {
        //            Console.WriteLine("TextureFileDataID " + i + " : " + TextureFiledataID.ElementAt(i));
        //        }

        //        Console.ReadLine();
        //    }
        //}

        private unsafe long SearchPattern(byte[] haystack, byte[] needle)
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
