// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2020 ZRA Contributors (https://github.com/zraorg)

namespace ZRA.NET
{
    public enum ZraStatusCode : int
    {
        Success                = 0, // The operation was successful
        ZStdError              = 1, // An error was returned by ZStandard
        ZraVersionLow          = 2, // The version of ZRA is too low to decompress this archive
        HeaderInvalid          = 3, // The header in the supplied buffer was invalid
        HeaderIncomplete       = 4, // The header hasn't been fully written before getting retrieved
        OutOfBoundsAccess      = 5, // The specified offset and size are past the data contained within the buffer
        OutputBufferTooSmall   = 6, // The output buffer is too small to contain the output (Supply null output buffer to get size)
        CompressedSizeTooLarge = 7, // The compressed output's size exceeds the maximum limit
        InputFrameSizeMismatch = 8, // The input size is not divisible by the frame size and it isn't the final frame
    }
}