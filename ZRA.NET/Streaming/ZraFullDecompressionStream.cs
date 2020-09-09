// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2020 ZRA Contributors (https://github.com/zraorg)

using System;
using System.IO;
using System.Threading.Tasks;

namespace ZRA.NET.Streaming
{
    public class ZraFullDecompressionStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length { get; }

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        private long _position;

        private readonly IntPtr _fullDecompressor;
        private readonly IntPtr _header;
        private readonly Stream _baseStream;
        private readonly bool   _leaveOpen;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly LibZra.ReadFunction _readFunction;

        /**
         * <summary>
         * Creates a <see cref="ZraFullDecompressionStream"/> used to stream-decompress data.
         * This stream is non-seekable.
         * </summary>
         * <param name="baseStream">The underlying stream where the compressed data will be read from.</param>
         * <param name="leaveOpen">Whether to leave the underlying stream open or not when the <see cref="ZraFullDecompressionStream"/> is disposed.</param>
         * <returns><see cref="ZraFullDecompressionStream"/></returns>
         */
        public ZraFullDecompressionStream(Stream baseStream, bool leaveOpen = false)
        {
            _baseStream = baseStream;
            _leaveOpen  = leaveOpen;

            unsafe
            {
                _readFunction = (ulong offset, ulong size, byte* buffer) =>
                {
                    _baseStream.Seek((long)offset, SeekOrigin.Begin);
                    _baseStream.Read(new Span<byte>(buffer, (int)size));
                };
            }

            LibZra.ZraCreateFullDecompressor(out _fullDecompressor, _readFunction).ThrowIfError();
            _header = LibZra.ZraGetHeaderWithFullDecompressor(_fullDecompressor);
            Length = (long)LibZra.ZraGetUncompressedSizeWithHeader(_header);
        }

        public byte[] GetMetaSection() => Zra.GetZraMetaSection(_header);

        /**
         * <summary>
         * Reads a sequence of bytes from the current stream and decompresses it, then advances the position within the stream by the number of bytes read.
         * </summary>
         * <param name="buffer">An array of bytes. When this method returns, the buffer contains the bytes decompressed from the current source.</param>
         * <param name="offset">This stream is non-seekable, therefore a <see cref="ArgumentOutOfRangeException"/> is thrown if offset is non-zero.</param>
         * <param name="count">The maximum number of bytes to be read and decompressed from the current stream.</param>
         * <returns>The total number of bytes read and decompressed into the buffer.
         * This can be less than the number of bytes requested if that many bytes are not currently available,
         * or zero (0) if the end of the stream has been reached.</returns>
         */
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0) throw new ArgumentOutOfRangeException(nameof(offset), "This stream is non-seekable, therefore passing a value to the offset parameter is not allowed.");

            LibZra.ZraDecompressWithFullDecompressor(_fullDecompressor, buffer, (ulong)count, out ulong outputSize).ThrowIfError();

            _position += (long)outputSize;
            return (int)outputSize;
        }

        public new void CopyTo(Stream destination)
        {
            ulong bufferSize = LibZra.ZraGetFrameSizeWithHeader(_header);
            CopyTo(destination, (int)bufferSize);
        }

        public new Task CopyToAsync(Stream destination)
        {
            ulong bufferSize = LibZra.ZraGetFrameSizeWithHeader(_header);
            return CopyToAsync(destination, (int)bufferSize);
        }

        protected override void Dispose(bool disposing)
        {
            LibZra.ZraDeleteFullDecompressor(_fullDecompressor);

            if (!_leaveOpen)
                _baseStream?.Dispose();

            base.Dispose(disposing);
        }

        /**<summary>Not Supported.</summary>*/
        public override void Flush() => throw new NotSupportedException();

        /**<summary>Not Supported.</summary>*/
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /**<summary>Not Supported.</summary>*/
        public override void SetLength(long value) => throw new NotSupportedException();

        /**<summary>Not Supported.</summary>*/
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}