﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Dia2Lib;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.BinaryParsers.ProgramDatabase
{
    /// <summary>
    /// The main class
    /// </summary>
    public sealed class Pdb : IDisposable
    {
        private IDiaSession _session;
        private readonly Lazy<Symbol> _globalScope;

        /// <summary>
        /// Load debug info fr PE or PDB, using symbolPath to help find symbols
        /// </summary>
        /// <param name="pePath">The path to the portable executable.</param>
        /// <param name="localSymbolDirectories">An option collection of local directories that should be examined for PDBs.</param>
        /// <param name="symbolPath">The symsrv.dll symbol path.</param>
        public Pdb(string pePath, string symbolPath = null, string localSymbolDirectories = null)
        {
            _globalScope = new Lazy<Symbol>(GetGlobalScope, LazyThreadSafetyMode.ExecutionAndPublication);
            _writableSegmentIds = new Lazy<HashSet<uint>>(GenerateWritableSegmentSet);
            _executableSectionContribCompilandIds = new Lazy<HashSet<uint>>(GenerateExecutableSectionContribIds);
            Init(pePath, symbolPath, localSymbolDirectories);
        }

        /// <summary>
        /// Load debug info
        /// </summary>
        /// <param name="peOrPdbPath"></param>
        /// <param name="symbolPath"></param>
        private void Init(string peOrPdbPath, string symbolPath, string localSymbolDirectories)
        {
            try
            {
                PlatformSpecificHelpers.ThrowIfNotOnWindows();
                WindowsNativeLoadPdbUsingDia(peOrPdbPath, symbolPath, localSymbolDirectories);
            }
            catch (PlatformNotSupportedException ex)
            {
                throw new PdbParseException(BinaryParsersResources.PdbPlatformUnsupported, ex);
            }
            catch (COMException ce)
            {
                throw new PdbParseException(ce);
            }
        }

        private void WindowsNativeLoadPdbUsingDia(string pePath, string symbolPath, string localSymbolDirectories)
        {
            IDiaDataSource diaSource;
            Environment.SetEnvironmentVariable("_NT_SYMBOL_PATH", "");
            Environment.SetEnvironmentVariable("_NT_ALT_SYMBOL_PATH", "");

            try
            {
                diaSource = MsdiaComWrapper.GetDiaSource();
                diaSource.loadDataForExe(pePath, localSymbolDirectories, IntPtr.Zero);
            }
            catch
            {
                diaSource = null;
            }

            if (diaSource == null)
            {
                diaSource = MsdiaComWrapper.GetDiaSource();
                diaSource.loadDataForExe(pePath, symbolPath, IntPtr.Zero);
            }

            diaSource.openSession(out _session);
        }

        /// <summary>
        /// Returns the symbol for the global scope. Does not give up ownership of the symbol; callers
        /// must NOT dispose it.
        /// </summary>
        /// <value>The global scope.</value>
        public Symbol GlobalScope
        {
            get
            {
                return _globalScope.Value;
            }
        }

        public bool IsStripped
        {
            get
            {
                return GlobalScope.IsStripped;
            }
        }


        /// <summary>
        /// Get the list of modules in this executable
        /// </summary>
        public DisposableEnumerable<Symbol> CreateObjectModuleIterator()
        {
            return this.GlobalScope.CreateChildIterator(SymTagEnum.SymTagCompiland);
        }

        /// <summary>
        /// Returns global variables defined in this executable
        /// </summary>
        public DisposableEnumerable<Symbol> CreateGlobalVariableIterator()
        {
            return this.GlobalScope.CreateChildIterator(SymTagEnum.SymTagData);
        }

        /// <summary>
        /// Returns global functions defined in this executable
        /// </summary>
        public DisposableEnumerable<Symbol> CreateGlobalFunctionIterator()
        {
            return this.GlobalScope.CreateChildIterator(SymTagEnum.SymTagFunction);
        }

        /// <summary>
        /// Returns global functions defined in this executable that meet the supplied filter
        /// </summary>
        public DisposableEnumerable<Symbol> CreateGlobalFunctionIterator(string functionName, NameSearchOptions searchOptions)
        {
            return this.GlobalScope.CreateChildren(SymTagEnum.SymTagFunction, functionName, searchOptions);
        }

        public DisposableEnumerable<SourceFile> CreateSourceFileIterator()
        {
            return this.CreateSourceFileIterator(null);
        }

        public DisposableEnumerable<SourceFile> CreateSourceFileIterator(Symbol inObjectModule)
        {
            return new DisposableEnumerable<SourceFile>(this.CreateSourceFileIteratorImpl(inObjectModule.UnderlyingSymbol));
        }

        private IEnumerable<SourceFile> CreateSourceFileIteratorImpl(IDiaSymbol inObjectModule)
        {
            IDiaEnumSourceFiles sourceFilesEnum = null;
            try
            {
                _session.findFile(inObjectModule, null, 0, out sourceFilesEnum);

                while (true)
                {
                    sourceFilesEnum.Next(1, out IDiaSourceFile sourceFile, out uint celt);
                    if (celt != 1) break;

                    yield return new SourceFile(sourceFile);
                }
            }
            finally
            {
                if (sourceFilesEnum != null)
                {
                    Marshal.ReleaseComObject(sourceFilesEnum);
                }
            }
        }

        private T CreateDiaTable<T>() where T : class
        {
            IDiaEnumTables enumTables = null;

            try
            {
                _session.getEnumTables(out enumTables);
                if (enumTables == null)
                {
                    return null;
                }

                // GetEnumerator() fails in netcoreapp2.0--need to iterate without foreach.
                for (int i = 0; i < enumTables.Count; i++)
                {
                    IDiaTable table = enumTables.Item(i);
                    if (!(table is T result))
                    {
                        Marshal.ReleaseComObject(table);
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            finally
            {
                if (enumTables != null)
                {
                    Marshal.ReleaseComObject(enumTables);
                }
            }

            return null;
        }

        private readonly Lazy<HashSet<uint>> _writableSegmentIds;

        private HashSet<uint> GenerateWritableSegmentSet()
        {
            var result = new HashSet<uint>();
            IDiaEnumSegments enumSegments = null;

            try
            {
                enumSegments = CreateDiaTable<IDiaEnumSegments>();
            }
            catch (NotImplementedException) { }

            try
            {
                // GetEnumerator() fails in netcoreapp2.0--need to iterate without foreach.
                for (uint i = 0; i < (uint)enumSegments.Count; i++)
                {
                    IDiaSegment segment = enumSegments.Item(i);
                    try
                    {
                        if (segment.write != 0)
                        {
                            result.Add(segment.addressSection);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(segment);
                    }
                }
            }
            finally
            {
                if (enumSegments != null)
                {
                    Marshal.ReleaseComObject(enumSegments);
                }
            }

            return result;
        }

        public bool IsSegmentWithIdWritable(uint addressSection)
        {
            return _writableSegmentIds.Value.Contains(addressSection);
        }

        private readonly Lazy<HashSet<uint>> _executableSectionContribCompilandIds;

        private HashSet<uint> GenerateExecutableSectionContribIds()
        {
            var result = new HashSet<uint>();
            IDiaEnumSectionContribs enumSectionContribs = null;

            try
            {
                enumSectionContribs = CreateDiaTable<IDiaEnumSectionContribs>();
            }
            catch (NotImplementedException) { }

            if (enumSectionContribs == null)
            {
                return result;
            }

            try
            {
                // GetEnumerator() fails in netcoreapp2.0--need to iterate without foreach.
                for (uint i = 0; i < (uint)enumSectionContribs.Count; i++)
                {
                    IDiaSectionContrib sectionContrib = enumSectionContribs.Item(i);
                    try
                    {
                        if (sectionContrib.execute != 0)
                        {
                            result.Add(sectionContrib.compilandId);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(sectionContrib);
                    }
                }
            }
            finally
            {
                if (enumSectionContribs != null)
                {
                    Marshal.ReleaseComObject(enumSectionContribs);
                }
            }
            return result;
        }

        public bool CompilandWithIdIsInExecutableSectionContrib(uint segmentId)
        {
            return _executableSectionContribCompilandIds.Value.Contains(segmentId);
        }

        public bool ContainsExecutableSectionContribs()
        {
            return _executableSectionContribCompilandIds.Value.Count != 0;
        }

        /// <summary>
        /// Returns the location of the PDB for this module
        /// </summary>
        public string PdbLocation
        {
            get
            {
                return _session.globalScope.symbolsFileName;
            }
        }

        public void Dispose()
        {
            if (_globalScope.IsValueCreated)
            {
                _globalScope.Value.Dispose();
            }

            if (_session != null)
            {
                Marshal.ReleaseComObject(_session);
            }
        }

        private Symbol GetGlobalScope()
        {
            return Symbol.Create(_session.globalScope);
        }
    }
}
