// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2021 Xpl0itR, ZRA Contributors (https://github.com/zraorg)

using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;
using ZRA.NET.Streaming;

namespace zratoolnet
{
    public class Program
    {
        public static void Main(string[] cmdArgs)
        {
            string outDirectoryPath = null;
            bool   compress         = true;
            bool   showHelp         = false;
            byte   compressionLevel = 9;
            uint   frameSize        = 131072;

            OptionSet optionSet = new OptionSet
            {
                { "h|help",       "Show this message and exit",                                                             _ => showHelp         = true           },
                { "d|decompress", "Set the operation mode to decompress.\n(Compress by default)",                           _ => compress         = false          },
                { "l|level=",     "zStd compression level used to compress the file.",                                      s => compressionLevel = byte.Parse(s)  },
                { "f|framesize=", "Size of a frame used to split a file.",                                                  s => frameSize        = uint.Parse(s)  },
                { "o|output=",    "The directory to output the compressed file.\n(Defaults to the same dir as input file)", s => outDirectoryPath = s              }
            };

            List<string> args = optionSet.Parse(cmdArgs);

            if (showHelp)
            {
                Console.WriteLine("zratoolnet - Copyright (c) 2021 Xpl0itR, ZRA Contributors");
                Console.WriteLine("Usage: zratoolnet(.exe) [options] <path>");
                Console.WriteLine("Options:");
                optionSet.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (args.Count < 1 || !File.Exists(args[0]))
            {
                Console.WriteLine("Input file does not exist!");
                return;
            }

            outDirectoryPath ??= Path.GetDirectoryName(args[0]);

            if (!Directory.Exists(outDirectoryPath))
            {
                Console.WriteLine("Out directory doesn't exist! Attempting to create...");
                Directory.CreateDirectory(outDirectoryPath);
            }

            if (compressionLevel < 1 || compressionLevel > 22)
            {
                Console.WriteLine("You must enter a valid compression level!");
                return;
            }

            string inFileName  = args[0];
            string outFilePath = Path.Join(outDirectoryPath,
                                           compress
                                               ? $"{Path.GetFileName(inFileName)}.zra"
                                               : Path.GetFileNameWithoutExtension(inFileName));

            using FileStream inStream  = File.OpenRead(inFileName);
            using FileStream outStream = File.OpenWrite(outFilePath);

            if (compress)
            {
                using ZraCompressionStream compressionStream = new ZraCompressionStream(outStream, (ulong)inStream.Length, compressionLevel, frameSize);
                inStream.CopyTo(compressionStream, (int)frameSize);
            }
            else
            {
                using ZraFullDecompressionStream decompressionStream = new ZraFullDecompressionStream(inStream);
                decompressionStream.CopyTo(outStream);
            }
        }
    }
}