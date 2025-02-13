﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;

using Dia2Lib;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.BinaryParsers
{
    public class PEBinaryTests
    {
        internal static string BaselineTestsDataDirectory = GetTestDirectory(@"Test.FunctionalTests.BinSkim.Driver" + Path.DirectorySeparatorChar + "BaselineTestsData");

        internal static string GetTestDirectory(string relativeDirectory)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            dirPath = Path.Combine(dirPath, string.Format(@"..{0}..{0}..{0}..{0}src{0}", Path.DirectorySeparatorChar));
            dirPath = Path.GetFullPath(dirPath);
            return Path.Combine(dirPath, relativeDirectory);
        }
    
        [Fact]
        public void PEBinary_PdbAvailable()
        {
            string fileName = Path.Combine(BaselineTestsDataDirectory, "Native_x64_VS2013_Default.dll");
            using (PEBinary peBinary = new PEBinary(new Uri(fileName)))
            {
                peBinary.Pdb.Should().NotBeNull();
                peBinary.StrippedPdb.Should().BeNull();
                peBinary.PdbParseException.Should().BeNull();
            }
        }

        [Fact]
        public void PEBinary_NoPdbAvailable()
        {
            string fileName = Path.Combine(BaselineTestsDataDirectory, "Native_x86_VS2013_PdbMissing.exe");
            using (PEBinary peBinary = new PEBinary(new Uri(fileName)))
            {
                peBinary.Pdb.Should().BeNull();
                peBinary.StrippedPdb.Should().BeNull();
                peBinary.PdbParseException.Should().NotBeNull();
            }
        }

        [Fact]
        public void PEBinary_PdbIsStripped()
        {
            string fileName = Path.Combine(BaselineTestsDataDirectory, "Native_x86_VS2017_15.5.4_PdbStripped.dll");
            using (PEBinary peBinary = new PEBinary(new Uri(fileName)))
            {
                peBinary.Pdb.Should().BeNull();
                peBinary.StrippedPdb.Should().NotBeNull();
                peBinary.PdbParseException.Should().NotBeNull();
            }
        }

        [Fact]
        public void PEBinary_CanCreateIDiaSourceFromMsdia()
        {
            Action action = () => { IDiaDataSource source = ProgramDatabase.MsdiaComWrapper.GetDiaSource(); };
            action.ShouldNotThrow();
        }
    }
}
