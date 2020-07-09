// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2020 ZRA Contributors (https://github.com/zraorg)

using System.Runtime.InteropServices;

namespace ZRA.NET
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ZraStatus
    {
        public ZraStatusCode ZraStatusCode;  // The status code from ZRA
        public int           ZstdStatusCode; // The status code from ZSTD
    }
}