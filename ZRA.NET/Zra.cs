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
         * <returns>A byte array containing the newly compressed data.</returns>
         */
        public static ReadOnlySpan<byte> Compress(ReadOnlySpan<byte> inBuffer, byte compressionLevel = 9, uint frameSize = 131072) =>
            Compress(inBuffer, ReadOnlySpan<byte>.Empty, compressionLevel, frameSize);

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
        public static ReadOnlySpan<byte> Compress(ReadOnlySpan<byte> inBuffer, ReadOnlySpan<byte> metaBuffer, byte compressionLevel = 9, uint frameSize = 131072)
        {
            ulong outputSizeMax = LibZra.ZraGetCompressedOutputBufferSize((ulong)inBuffer.Length, frameSize);
            Span<byte> outBuffer = new byte[outputSizeMax];
            ulong outputSize = Compress(inBuffer, outBuffer, metaBuffer, compressionLevel, frameSize);

            return outBuffer.Slice(0, (int)outputSize);
        }

        /**
         * <summary>
         * Compresses the supplied buffer with specified parameters in-memory.
         * </summary>
         * <param name="inBuffer"> A byte array containing the data to be compressed.</param>
         * <param name="outBuffer"> A byte array containing the newly compressed data.</param>
         * <param name="compressionLevel"> The ZSTD compression level to compress the buffer with</param>
         * <param name="frameSize"> The size of a single frame which can be decompressed individually (This does not always equate to a single ZSTD frame)</param>
         * <returns>A byte array containing the newly compressed data.</returns>
         */
        public static ulong Compress(ReadOnlySpan<byte> inBuffer, Span<byte> outBuffer, byte compressionLevel = 9, uint frameSize = 131072) =>
            Compress(inBuffer, outBuffer, ReadOnlySpan<byte>.Empty, compressionLevel, frameSize);

        /**
         * <summary>
         * Compresses the supplied buffer with specified parameters in-memory.
         * </summary>
         * <param name="inBuffer"> A byte array containing the data to be compressed.</param>
         * <param name="outBuffer"> A byte array containing the newly compressed data.</param>
         * <param name="compressionLevel"> The ZSTD compression level to compress the buffer with</param>
         * <param name="frameSize"> The size of a single frame which can be decompressed individually (This does not always equate to a single ZSTD frame)</param>
         * <param name="checksum"> If ZSTD should add a checksum over all blocks of data that'll be compressed</param>
         * <param name="metaBuffer"> A buffer containing the metadata</param>
         */
        public static unsafe ulong Compress(ReadOnlySpan<byte> inBuffer, Span<byte> outBuffer, ReadOnlySpan<byte> metaBuffer, byte compressionLevel = 9, uint frameSize = 131072, bool checksum = true)
        {
            fixed (byte* inBufferPtr = inBuffer, outBufferPtr = outBuffer, metaPtr = metaBuffer)
            {
                LibZra.ZraCompressBuffer(inBufferPtr, (ulong)inBuffer.Length, outBufferPtr, out ulong outputSize, compressionLevel, frameSize, checksum, metaPtr, (ulong)metaBuffer.Length).ThrowIfError();
                return outputSize;
            }
        }

        /**
         * <summary>
         * Decompresses the entirety of the supplied compressed byte array in-memory.
         * </summary>
         * <param name="inBuffer"> A span array containing the data to be decompressed.</param>
         * <returns>A byte array containing the newly decompressed data.</returns>
         */
        public static unsafe ReadOnlySpan<byte> Decompress(ReadOnlySpan<byte> inBuffer)
        {
            fixed (byte* inBufferPtr = inBuffer)
            {
                LibZra.ZraCreateHeader2(out IntPtr headerPtr, inBufferPtr, (ulong)inBuffer.Length).ThrowIfError();
                
                try
                {
                    ulong outputSize = LibZra.ZraGetUncompressedSizeWithHeader(headerPtr);

                    Span<byte> outBuffer = new byte[outputSize];
                    Decompress(inBuffer, outBuffer);

                    return outBuffer;
                }
                finally
                {
                    LibZra.ZraDeleteHeader(headerPtr);
                }
            }
        }

        /**
         * <summary>
         * Decompresses the entirety of the supplied compressed byte array in-memory.
         * </summary>
         * <param name="inBuffer"> A byte span containing the data to be decompressed.</param>
         * <param name="outBuffer"> A byte span containing the newly decompressed data.</param>
         */
        public static unsafe void Decompress(ReadOnlySpan<byte> inBuffer, Span<byte> outBuffer)
        {
            fixed (byte* inBufferPtr = inBuffer, outBufferPtr = outBuffer)
            {
                LibZra.ZraDecompressBuffer(inBufferPtr, (ulong)inBuffer.Length, outBufferPtr).ThrowIfError();
            }
        }

        /**
         * <summary>
         * Decompresses a specific region of the supplied compressed buffer in-memory into the specified buffer
         * </summary>
         * <param name="inBuffer"> A byte span containing the compressed data</param>
         * <param name="offset"> The corresponding offset in the uncompressed buffer</param>
         * <param name="length"> The amount of bytes to decompress from the supplied offset</param>
         * <returns>A byte array containing the newly decompressed data.</returns>
         */
        public static unsafe ReadOnlySpan<byte> DecompressSection(ReadOnlySpan<byte> inBuffer, ulong offset, ulong length)
        {
            fixed (byte* inBufferPtr = inBuffer)
            {
                LibZra.ZraCreateHeader2(out IntPtr headerPtr, inBufferPtr, (ulong)inBuffer.Length).ThrowIfError();

                try
                {
                    ulong outputSize = LibZra.ZraGetUncompressedSizeWithHeader(headerPtr);

                    Span<byte> outBuffer = new byte[outputSize];
                    DecompressSection(inBuffer, outBuffer, offset, length);

                    return outBuffer;
                }
                finally
                {
                    LibZra.ZraDeleteHeader(headerPtr);
                }
            }
        }

        /**
         * <summary>
         * Decompresses a specific region of the supplied compressed buffer in-memory into the specified buffer
         * </summary>
         * <param name="inBuffer"> A byte span containing the compressed data</param>
         * <param name="outBuffer"> A byte span containing the newly decompressed data.</param>
         * <param name="offset"> The corresponding offset in the uncompressed buffer</param>
         * <param name="length"> The amount of bytes to decompress from the supplied offset</param>
         */
        public static unsafe void DecompressSection(ReadOnlySpan<byte> inBuffer, Span<byte> outBuffer, ulong offset, ulong length)
        {
            fixed (byte* inBufferPtr = inBuffer, outBufferPtr = outBuffer)
            {
                LibZra.ZraDecompressRA(inBufferPtr, (ulong)inBuffer.Length, outBufferPtr, offset, length).ThrowIfError();
            }
        }

        /**
         * <param name="headerPtr"> A pointer to the ZRA header to extract the meta section from</param>
         * <returns>A byte array containing the ZRA meta section.</returns>
         */
        public static ReadOnlySpan<byte> GetZraMetaSection(IntPtr headerPtr)
        {
            ulong metaSize = LibZra.ZraGetMetadataSize(headerPtr);

            if (metaSize == 0)
            {
                return ReadOnlySpan<byte>.Empty;
            }

            Span<byte> metaBufferPtr = new byte[metaSize];
            GetZraMetaSection(headerPtr, metaBufferPtr);

            return metaBufferPtr;
        }

        /**
         * <param name="headerPtr"> A pointer to the ZRA header to extract the meta section from</param>
         * <param name="headerBuffer">A byte span containing the ZRA meta section.</param>
         */
        public static unsafe void GetZraMetaSection(IntPtr headerPtr, Span<byte> headerBuffer)
        {
            fixed (byte* metaBufferPtr = headerBuffer)
            {
                LibZra.ZraGetMetadata(headerPtr, metaBufferPtr);
            }
        }

        public static string GetErrorString(ZraStatus zraStatus)
        {
            IntPtr errorStringPtr = LibZra.ZraGetErrorString(zraStatus);

            return Marshal.PtrToStringUTF8(errorStringPtr);
        }

        /**
         * <summary>
         * Throws an exception if the ZraStatusCode within the ZraStatus structure is not success.
         * </summary>
         * <param name="zraStatus">The ZraStatus structure to check</param>
         */
        public static void ThrowIfError(this ZraStatus zraStatus)
        {
            if (zraStatus.ZraStatusCode != ZraStatusCode.Success)
            {
                throw new ZraException(zraStatus);
            }
        }
    }
}