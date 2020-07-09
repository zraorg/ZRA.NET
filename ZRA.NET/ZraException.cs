// SPDX-License-Identifier: BSD-3-Clause
// Copyright © 2020 ZRA Contributors (https://github.com/zraorg)

namespace ZRA.NET
{
    public class ZraException : System.Exception
    {
        public ZraStatus ZraStatus { get; }

        public ZraException(ZraStatus zraStatus, string message = null) : base(message)
        {
            ZraStatus = zraStatus;
        }
    }
}