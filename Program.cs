namespace IndexGrayscaler
{
    internal class Program
    {
        static readonly int checkSum = -1598344453;
        static readonly int checkSum2 = 2103292130;

        static void Main(string[] args)
        {
            Console.WriteLine("This tool overwrites files, make sure to make backups before continuing.");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            if(args.Length < 1)
            {
                Console.WriteLine("No input path selected, press any key to close");
                goto End;
            }

            bool bw = false;
            if (args.Length >= 2)
                bw = args[1] == "-bw";

            if (File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
                readFolder(args[0], bw);
            else
                readFile(args[0], bw);

            Console.WriteLine("Completed, press any key to close");
            End:
            Console.ReadKey();
        }

        static void readFolder(string folder, bool bw)
        {
            Console.WriteLine("Processing folder \"" + folder + "\"");
            foreach(string path in Directory.EnumerateFileSystemEntries(folder))
            {
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    readFolder(path, bw);
                else
                    readFile(path, bw);
            }
        }

        static void readFile(string path, bool bw)
        {
            Console.Write("Processing file \"" + path + "\"");
            string extention = path.Substring(path.LastIndexOf('.'));
            if(extention != ".png")
            {
                Console.WriteLine("Not a valid png file, skipping");
                return;
            }

            updatePaletteChunk(path, bw);
        }

        static bool updatePaletteChunk(string path, bool bw)
        {
            return updatePaletteChunk(new BinaryReader(File.Open(path, FileMode.Open)), path, bw);
        }

        //really fucking ineficient, just hopes that PTLE is really close to header, which it usually is.
        //actually just found out gimp adds a shit ton of bytes as a string for no fucking reason
        //fuck gimp
        //also make sure that the palette has 256 colors. since idk how png chunk sizes are defined it just hopes that its 256 colors
        static bool updatePaletteChunk(BinaryReader file, string path, bool bw)
        {
        Restart:
            while (file.ReadByte() != 80) //P as a byte
                if(file.BaseStream.Position >= file.BaseStream.Length)
                {
                    Console.WriteLine("File Contains no palette, skipping");
                    file.Close();
                    return false;
                }

            file.BaseStream.Position -= 1;
            if(file.ReadInt32() != 1163152464) //PTLE as an int
            {
                file.BaseStream.Position -= 3;
                goto Restart;
            }

            long position = file.BaseStream.Position;
            file.Close();
            BinaryWriter file2 = new BinaryWriter(File.Open(path, FileMode.Open));
            file2.BaseStream.Position = position;

            for(int i = 0; i < 256; i++)
            {
                file2.Write((byte)i);
                if (bw)
                {
                    file2.Write((byte)i);
                    file2.Write((byte)i);
                }
                else
                {
                    file2.Write((byte)0);
                    file2.Write((byte)0);
                }
            }

            if (bw)
                file2.Write(checkSum2);
            else
                file2.Write(checkSum);

            file2.Close();
            return true;
        }
    }
}