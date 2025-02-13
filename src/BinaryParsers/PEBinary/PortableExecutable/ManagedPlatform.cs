﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.BinaryParsers.PortableExecutable
{
    public enum ManagedPlatform
    {
        Unknown,
        DotNetFramework,
        DotNetCore,
        DotNetStandard
    }
}
