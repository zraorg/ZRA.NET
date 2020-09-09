// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2020 ZRA Contributors (https://github.com/zraorg)

using System;
using System.Runtime.InteropServices;

namespace ZRA.NET
{
    public static class Zra
    {
        /**
         * <summary>
         * Compresses the supplied buffer with specified parameters in-memory.
         * </summary>
         * <param name="inBuffer"> A byte array containing the data to be compressed.</param>
         * <param name="compressionLevel"> The ZSTD compression level to compress the buffer with</param>
         * <param name="frameSize"> The size of a single frame which can be decompressed individually (This does not always equate to a single ZSTD frame)</param>
         * <param name="metaBuffer"> A buffer containing the metadata</param>
         * <returns>A byte array containing the newly compressed data.</returns>
         */
        public static byte[] Compress(byte[] inBuffer, byte compressionLevel = 9, uint frameSize = 131072, byte[] metaBuffer = null)
        {
            ulong  metaSize  = metaBuffer == null ? 0 : (ulong)metaBuffer.Length;
            byte[] outBuffer = new byte[LibZra.ZraGetCompressedOutputBufferSize((ulong)inBuffer.LongLength, frameSize)];
            LibZra.ZraCompressBuffer(inBuffer, (ulong)inBuffer.LongLength, outBuffer, out ulong outputSize, compressionLevel, frameSize, true, metaBuffer, metaSize).ThrowIfError();

            Array.Resize(ref outBuffer, (int)outputSize);

            return outBuffer;
        }

        /**
         * <summary>
         * Decompresses the entirety of the supplied compressed byte array in-memory.
         * </summary>
         * <param name="inBuffer"> A byte array containing the data to be decompressed.</param>
         * <returns>A byte array containing the newly decompressed data.</returns>
         */
        public static byte[] Decompress(byte[] inBuffer)
        {
            LibZra.ZraCreateHeader2(out IntPtr headerPtr, inBuffer, (ulong)inBuffer.LongLength).ThrowIfError();

            byte[] outBuffer = new byte[LibZra.ZraGetUncompressedSizeWithHeader(headerPtr)];
            LibZra.ZraDecompressBuffer(inBuffer, (ulong)inBuffer.LongLength, outBuffer).ThrowIfError();

            LibZra.ZraDeleteHeader(headerPtr);

            return outBuffer;
        }

        /**
         * <summary>
         * Decompresses a specific region of the supplied compressed buffer in-memory into the specified buffer
         * </summary>
         * <param name="inBuffer"> A byte array containing the compressed data</param>
         * <param name="offset"> The corresponding offset in the uncompressed buffer</param>
         * <param name="length"> The amount of bytes to decompress from the supplied offset</param>
         * <returns>A byte array containing the newly decompressed data.</returns>
         */
        public static byte[] DecompressSection(byte[] inBuffer, ulong offset, ulong length)
        {
            LibZra.ZraCreateHeader2(out IntPtr headerPtr, inBuffer, (ulong)inBuffer.LongLength).ThrowIfError();

            byte[] outBuffer = new byte[LibZra.ZraGetUncompressedSizeWithHeader(headerPtr)];
            LibZra.ZraDecompressRA(inBuffer, (ulong)inBuffer.LongLength, outBuffer, offset, length).ThrowIfError();

            LibZra.ZraDeleteHeader(headerPtr);

            return outBuffer;
        }

        /**
         * <param name="headerPtr"> A pointer to the ZRA header to extract the meta section from</param>
         * <returns>A byte array containing the ZRA meta section.</returns>
         */
        public static byte[] GetZraMetaSection(IntPtr headerPtr)
        {
            ulong metaSize = LibZra.ZraGetMetadataSize(headerPtr);

            if (metaSize == 0)
                return null;

            byte[] metaBuffer = new byte[metaSize];
            LibZra.ZraGetMetadata(headerPtr, metaBuffer);

            return metaBuffer;
        }

        /**
         * <summary>
         * Throws an exception if the ZraStatusCode within the ZraStatus structure is not success.
         * </summary>
         * <param name="zraStatus">The ZraStatus structure to check</param>
         */
        public static void ThrowIfError(this ZraStatus zraStatus)
        {
            if (zraStatus.ZraStatusCode == ZraStatusCode.Success) return;

            IntPtr errorStringPtr = LibZra.ZraGetErrorString(zraStatus);
            throw new ZraException(zraStatus, Marshal.PtrToStringUTF8(errorStringPtr));
        }
    }
}