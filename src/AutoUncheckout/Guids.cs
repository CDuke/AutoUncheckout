// Guids.cs
// MUST match guids.h
using System;

namespace KulikovDenis.AutoUncheckout
{
    static class GuidList
    {
        public const string guidAutoUncheckoutPkgString = "1954660f-1535-4694-9e6d-c27e8eb476a4";
        public const string guidAutoUncheckoutCmdSetString = "d859909f-3a82-408f-b5d0-833a39b8de2e";

        public static readonly Guid guidAutoUncheckoutCmdSet = new Guid(guidAutoUncheckoutCmdSetString);
    };
}