// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2020 ZRA Contributors (https://github.com/zraorg)

using System;
using System.Runtime.InteropServices;

namespace ZRA.NET
{
    public static class LibZra
    {
        /* ======= Library Functions ======= */

        /**
         * <summary>
         * The highest version of ZRA the library linked to this supports.
         * </summary>
         */
        [DllImport("libZRA")] public static extern ushort ZraGetVersion();

        /**
         * <param name="status">The status structure that should be described</param>
         * <returns>A pointer to a string describing the error corresponding to the code supplied.</returns>
         */
        [DllImport("libZRA")] public static extern IntPtr ZraGetErrorString(ZraStatus status);

        /**
         * <summary>
         * This function is a delegate used by the native library to stream in data.
         * </summary>
         * <param name="offset">The offset to read from.</param>
         * <param name="size">The number of bytes to read.</param>
         * <param name="buffer">A pointer to the buffer to read into.</param>
         */
        public unsafe delegate void ReadFunction(ulong offset, ulong size, byte* buffer);


        /* ======= Header Functions ======= */

        /**
         * <summary>
         * Creates a ZraHeader object from a file.
         * </summary>
         * <param name="header">A pointer to a pointer to store the ZraHeader pointer in</param>
         * <param name="readFunction">This function is used to read data from the compressed file while supplying the offset and the size, the output should be into the buffer</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraCreateHeader(out IntPtr header, [MarshalAs(UnmanagedType.FunctionPtr)] ReadFunction readFunction);

        /**
         * <summary>
         * Creates a ZraHeader object from a file.
         * </summary>
         * <param name="header">A pointer to store the ZraHeader pointer in</param>
         * <param name="buffer">A buffer containing the entire file</param>
         * <param name="size">The size of the entire file in bytes</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraCreateHeader2(out IntPtr header, byte[] buffer, ulong size);

        /**
         * <param name="header">A pointer to the ZraHeader object</param>
         * <returns>The version of the file with the corresponding header.</returns>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetVersionWithHeader(IntPtr header);

        /**
         * <param name="header">A pointer to the ZraHeader object</param>
         * <returns>The size of the entire header in bytes.</returns>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetHeaderSizeWithHeader(IntPtr header);

        /**
         * <param name="header">A pointer to the ZraHeader object</param>
         * <returns>The size of the original uncompressed data in bytes.</returns>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetUncompressedSizeWithHeader(IntPtr header);

        /**
         * <param name="header">A pointer to the ZraHeader object</param>
         * <returns>The size of a single frame in bytes.</returns>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetFrameSizeWithHeader(IntPtr header);

        /**
         * <param name="header">A pointer to the ZraHeader object</param>
         * <returns>The size of the metadata section in bytes</returns>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetMetadataSize(IntPtr header);

        /**
         * <param name="header">A pointer to the ZraHeader object</param>
         * <param name="buffer">The buffer into which the metadata section is written into, should be at least <see cref="ZraGetMetadataSize"/> bytes long</param>
         */
        [DllImport("libZRA")] public static extern void ZraGetMetadata(IntPtr header, byte[] buffer);

        /**
         * <summary>
         * Deletes a ZraHeader object
         * </summary>
         * <param name="header">A pointer to the ZraHeader object</param>
         */
        [DllImport("libZRA")] public static extern void ZraDeleteHeader(IntPtr header);


        /* ======= In-Memory Functions ======= */

        /**
         * <param name="inputSize">The size of the input being compressed</param>
         * <param name="frameSize">The size of a single frame</param>
         * <returns>The worst-case size of the compressed output.</returns>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetCompressedOutputBufferSize(ulong inputSize, ulong frameSize);

        /**
         * <summary>
         * Compresses the supplied buffer with specified parameters in-memory into the specified buffer
         * </summary>
         * <param name="inputBuffer"> A pointer to the uncompressed source data</param>
         * <param name="inputSize"> The size of the uncompressed source data</param>
         * <param name="outputBuffer"> A pointer to the buffer to write compressed data into (Size should be at least <see cref="ZraGetCompressedOutputBufferSize"/> bytes)</param>
         * <param name="outputSize"> The size of the compressed output</param>
         * <param name="compressionLevel"> The ZSTD compression level to compress the buffer with</param>
         * <param name="frameSize"> The size of a single frame which can be decompressed individually (This does not always equate to a single ZSTD frame)</param>
         * <param name="checksum"> If ZSTD should add a checksum over all blocks of data that'll be compressed</param>
         * <param name="metaBuffer"> A buffer containing the metadata</param>
         * <param name="metaSize"> The size of the metadata</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraCompressBuffer(byte[] inputBuffer, ulong inputSize, byte[] outputBuffer, out ulong outputSize, byte compressionLevel = 0, uint frameSize = 16384, bool checksum = false, byte[] metaBuffer = null, ulong metaSize = 0);

        /**
         * <summary>
         * Decompresses the entirety of the supplied compressed buffer in-memory into the specified buffer
         * </summary>
         * <param name="inputBuffer"> A pointer to the compressed data</param>
         * <param name="inputSize"> The size of the compressed data</param>
         * <param name="outputBuffer"> A pointer to the buffer to write uncompressed data into (Size should be at least <see cref="ZraGetUncompressedSizeWithHeader"/> bytes)</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraDecompressBuffer(byte[] inputBuffer, ulong inputSize, byte[] outputBuffer);

        /**
         * <summary>
         * Decompresses a specific region of the supplied compressed buffer in-memory into the specified buffer
         * </summary>
         * <param name="inputBuffer"> A pointer to the compressed data</param>
         * <param name="inputSize"> The size of the compressed data</param>
         * <param name="outputBuffer"> A pointer to the buffer to write uncompressed data into (Size should be adequate)</param>
         * <param name="offset"> The corresponding offset in the uncompressed buffer</param>
         * <param name="size"> The amount of bytes to decompress from the supplied offset</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraDecompressRA(byte[] inputBuffer, ulong inputSize, byte[] outputBuffer,  ulong offset, ulong size);


        /* ======= Streaming Compressor ======= */

        /**
         * <summary>
         * Creates a ZraCompressor object with the specified parameters
         * </summary>
         * <param name="compressor"> A pointer to a pointer to store the ZraCompressor pointer in</param>
         * <param name="size"> The exact size of the overall stream</param>
         * <param name="compressionLevel"> The level of ZSTD compression to use</param>
         * <param name="frameSize"> The size of a single frame which can be decompressed individually</param>
         * <param name="checksum"> If ZSTD should add a checksum over all blocks of data that'll be compressed</param>
         * <param name="metaBuffer"> A buffer containing the metadata</param>
         * <param name="metaSize"> The size of the metadata</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraCreateCompressor(out IntPtr compressor, ulong size, byte compressionLevel = 0, uint frameSize = 16384, bool checksum = false, byte[] metaBuffer = null, ulong metaSize = 0);

        /**
         * <summary>
         * Deletes a ZraCompressor object
         * </summary>
         * <param name="compressor">A pointer to the ZraCompressor object</param>
         */
        [DllImport("libZRA")] public static extern void ZraDeleteCompressor(IntPtr compressor);

        /**
         * <param name="compressor"> A pointer to the ZraCompressor object</param>
         * <param name="inputSize"> The size of the input being compressed</param>
         * <returns>The worst-case size of the compressed output</returns>
         * <remarks>This is not the same as <see cref="ZraGetCompressedOutputBufferSize"/></remarks>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetOutputBufferSizeWithCompressor(IntPtr compressor, ulong inputSize);

        /**
         * <summary>
         * Compresses a partial stream of contiguous data into the specified buffer.
         * </summary>
         * <param name="compressor"> A pointer to the ZraCompressor object</param>
         * <param name="inputBuffer"> A pointer to the contiguous partial compressed data</param>
         * <param name="inputSize"> The size of the partial compressed data</param>
         * <param name="outputBuffer"> A pointer to the buffer to write the corresponding uncompressed data into (Size should be at least <see cref="ZraGetOutputBufferSizeWithCompressor"/> bytes)</param>
         * <param name="outputSize"> The size of the compressed output</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraCompressWithCompressor(IntPtr compressor, byte[] inputBuffer, ulong inputSize, byte[] outputBuffer, out ulong outputSize);

        /**
         * <param name="compressor">A pointer to the ZraCompressor object</param>
         * <returns>The size of the full header from the compressor in bytes.</returns>
         */
        [DllImport("libZRA")] public static extern ulong ZraGetHeaderSizeWithCompressor(IntPtr compressor);

        /**
         * <summary>
         * Writes the header of the ZRA file into the specified buffer, this should only be read in after compression has been completed.
         * </summary>
         * <param name="compressor">A pointer to the ZraCompressor object</param>
         * <param name="outputBuffer">A pointer to the buffer to write the header into (Size should be at least <see cref="ZraGetHeaderSizeWithCompressor"/> bytes)</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraGetHeaderWithCompressor(IntPtr compressor, byte[] outputBuffer);


        /* ======= Streaming Random-Access Decompressor ======= */

        /**
         * <summary>
         * Creates a ZraDecompressor object with the specified parameters.
         * </summary>
         * <param name="decompressor"> A pointer to a pointer to store the ZraDecompressor pointer in</param>
         * <param name="readFunction"> This function is used to read data from the compressed file while supplying the offset and the size, the output should be into the buffer</param>
         * <param name="maxCacheSize"> The maximum size of the file cache, if the uncompressed segment read goes above this then it'll be read into it's own buffer</param>
         * <remarks>The cache is to preallocate buffers that are passed into readFunction, so that there isn't constant reallocation.</remarks>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraCreateDecompressor(out IntPtr decompressor, [MarshalAs(UnmanagedType.FunctionPtr)] ReadFunction readFunction, ulong maxCacheSize = 1024 * 1024 * 20);

        /**
         * <summary>
         * Deletes a ZraDecompressor object
         * </summary>
         * <param name="decompressor">A pointer to the ZraDecompressor object</param>
         */
        [DllImport("libZRA")] public static extern void ZraDeleteDecompressor(IntPtr decompressor);

        /**
         * <param name="decompressor">A pointer to the ZraDecompressor object</param>
         * <returns>A pointer to the header object created by the decompressor internally, so that it won't have to be constructed redundantly</returns>
         * <remarks>The lifetime of the object is directly tied to that of the Decompressor, do not manually delete it</remarks>
         */
        [DllImport("libZRA")] public static extern IntPtr ZraGetHeaderWithDecompressor(IntPtr decompressor);

        /**
         * <summary>
         * Decompresses data from a slice of corresponding to the original uncompressed file into the specified buffer.
         * </summary>
         * <param name="decompressor">A pointer to the ZraDecompressor object</param>
         * <param name="offset"> The offset of the data to decompress in the original file</param>
         * <param name="size"> The size of the data to decompress in the original file</param>
         * <param name="outputBuffer"> A pointer to the buffer to write the decompressed output into (Size should be adequate)</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraDecompressWithDecompressor(IntPtr decompressor, ulong offset, ulong size, byte[] outputBuffer);


        /* ======= Streaming Full Decompressor ======= */

        /**
         * <summary>
         * Creates a ZraFullDecompressor object with the specified parameters
         * </summary>
         * <param name="fullDecompressor"> A pointer to store the ZraFullDecompressor pointer in</param>
         * <param name="readFunction"> This function is used to read data from the compressed file while supplying the offset and the size, the output should be into the buffer</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraCreateFullDecompressor(out IntPtr fullDecompressor, [MarshalAs(UnmanagedType.FunctionPtr)] ReadFunction readFunction);

        /**
         * <summary>
         * Deletes a ZraFullDecompressor object
         * </summary>
         * <param name="fullDecompressor">A pointer to the ZraFullDecompressor object</param>
         */
        [DllImport("libZRA")] public static extern void ZraDeleteFullDecompressor(IntPtr fullDecompressor);

        /**
         * <param name="fullDecompressor">A pointer to the ZraFullDecompressor object</param>
         * <returns>The header object created by the decompressor internally, so that it won't have to be constructed redundantly</returns>
         * <remarks>The lifetime of the object is directly tied to that of the FullDecompressor, do not manually delete it</remarks>
         */
        [DllImport("libZRA")] public static extern IntPtr ZraGetHeaderWithFullDecompressor(IntPtr fullDecompressor);

        /**
         * <summary>
         * Decompresses as much data as possible into the supplied output buffer
         * </summary>
         * <param name="fullDecompressor">A pointer to the ZraFullDecompressor object</param>
         * <param name="outputBuffer"> The buffer to write the decompressed output into</param>
         * <param name="outputCapacity"> The size of the output buffer, it should be at least <see cref="ZraGetFrameSizeWithHeader"/> bytes</param>
         * <returns>A <see cref="ZraStatus"/> structure which contains the result code of the completed operation.</returns>
         */
        [DllImport("libZRA")] public static extern ZraStatus ZraDecompressWithFullDecompressor(IntPtr fullDecompressor, byte[] outputBuffer, ulong outputCapacity);
    }
}