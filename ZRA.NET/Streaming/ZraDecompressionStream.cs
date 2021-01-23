// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2020 ZRA Contributors (https://github.com/zraorg)

using System;
using System.IO;

namespace ZRA.NET.Streaming
{
    public class ZraDecompressionStream : Stream
    {
        public IntPtr ZraHeader { get; }
        
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length { get; }
        public override long Position { get; set; }

        private readonly IntPtr _decompressor;
        private readonly Stream _baseStream;
        private readonly bool   _leaveOpen;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly LibZra.ReadFunction _readFunction;

        /**
         * <summary>
         * Creates a <see cref="ZraDecompressionStream"/> used to stream-decompress data.
         * </summary>
         * <param name="baseStream">The underlying stream where the compressed data will be read from.</param>
         * <param name="maxCacheSize">The maximum size of the file cache, if the uncompressed segment read goes above this then it'll be read into it's own buffer</param>
         * <param name="leaveOpen">Whether to leave the underlying stream open or not when the <see cref="ZraDecompressionStream"/> is disposed.</param>
         * <remarks>The cache is to preallocate buffers that are passed into readFunction, so that there isn't constant reallocation.</remarks>
         * <returns><see cref="ZraDecompressionStream"/></returns>
         */
        public unsafe ZraDecompressionStream(Stream baseStream, ulong maxCacheSize = 1024 * 1024 * 20, bool leaveOpen = false)
        {
            _baseStream   = baseStream;
            _leaveOpen    = leaveOpen;
            _readFunction = (ulong offset, ulong size, byte* buffer) =>
            {
                _baseStream.Seek((long)offset, SeekOrigin.Begin);
                _baseStream.Read(new Span<byte>(buffer, (int)size));
            };

            LibZra.ZraCreateDecompressor(out _decompressor, _readFunction, maxCacheSize).ThrowIfError();
            ZraHeader = LibZra.ZraGetHeaderWithDecompressor(_decompressor);
            Length    = (long)LibZra.ZraGetUncompressedSizeWithHeader(ZraHeader);
        }

        /**
         * <summary>
         * Reads a sequence of bytes from the current stream and decompresses it, then advances the position within the stream by the number of bytes read.
         * </summary>
         * <param name="buffer">An array of bytes. When this method returns,
         * the buffer contains the specified byte array with the values between offset and (offset + count - 1)
         * replaced by the bytes decompressed from the current source.</param>
         * <param name="offset">The zero-based byte offset in buffer at which to begin storing the data decompressed from the current stream.</param>
         * <param name="count">The maximum number of bytes to be read and decompressed from the current stream.</param>
         * <returns>The total number of bytes read and decompressed into the buffer.
         * This can be less than the number of bytes requested if that many bytes are not currently available,
         * or zero (0) if the end of the stream has been reached.</returns>
         */
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer, offset, count);

        /**
         * <summary>
         * Reads a sequence of bytes from the current stream and decompresses it, then advances the position within the stream by the number of bytes read.
         * </summary>
         * <param name="buffer">An span of bytes. When this method returns,
         * the buffer contains the specified byte array with the values between offset and (offset + count - 1)
         * replaced by the bytes decompressed from the current source.</param>
         */
        public override int Read(Span<byte> buffer) => Read(buffer, 0, buffer.Length);

        /**
         * <summary>
         * Reads a sequence of bytes from the current stream and decompresses it, then advances the position within the stream by the number of bytes read.
         * </summary>
         * <param name="buffer">An span of bytes. When this method returns,
         * the buffer contains the specified byte array with the values between offset and (offset + count - 1)
         * replaced by the bytes decompressed from the current source.</param>
         * <param name="offset">The zero-based byte offset in buffer at which to begin storing the data decompressed from the current stream.</param>
         * <param name="count">The maximum number of bytes to be read and decompressed from the current stream.</param>
         * <returns>The total number of bytes read and decompressed into the buffer.
         * This can be less than the number of bytes requested if that many bytes are not currently available,
         * or zero (0) if the end of the stream has been reached.</returns>
         */
        public unsafe int Read(Span<byte> buffer, int offset, int count)
        {
            long readOffset = Position + offset;

            if (readOffset + count > Length)
            {
                count = (int)(Length - readOffset);
            }

            fixed (byte* bufferPtr = buffer)
            {
                LibZra.ZraDecompressWithDecompressor(_decompressor, (ulong)readOffset, (ulong)count, bufferPtr).ThrowIfError();
            }

            Position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                default:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return Position;
        }

        protected override void Dispose(bool disposing)
        {
            LibZra.ZraDeleteDecompressor(_decompressor);

            if (!_leaveOpen)
            {
                _baseStream?.Dispose();
            }

            base.Dispose(disposing);
        }

        /**<summary>Not Supported.</summary>*/
        public override void Flush() => throw new NotSupportedException();

        /**<summary>Not Supported.</summary>*/
        public override void SetLength(long value) => throw new NotSupportedException();

        /**<summary>Not Supported.</summary>*/
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}