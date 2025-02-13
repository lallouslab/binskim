# BinSkim Release History

## **v1.6.0** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.6.0
* FEATURE: Update to final SARIF v2 (version 2.1.16). This enables results caching when passing --hashes on the command-line, a significant performance improvement when recursively analyzing directories with multiple copies of scan targets.
* BUG FIX: Fix typo in BA2021.DoNotMarkWritableSectionsAsExecutable output.
* PERFORMANCE: Eliminate PDB loading for all non-mixed-mode for managed assemblies, including IL Library (ahead of time compiled) binaries.
* FALSE NEGATIVE FIX: Verify that a PDB placed alongside a binary actually matches the binary under analysis
* FEATURE: Provide --local-symbol-directories argument to specify additional (local, non-symbol-server) PDB look-up locations
* FALSE POSITIVE FIX: Skip PDB-driven analysis for the generated .NET core native bootstrap exe (which is not user-controllable code).

## **v1.6.0-beta.3** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.6.0-beta.3)
* Drop Spectre analysis to warning

## **v1.6.0-beta.2** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.6.0-beta.2)
* Fix Linux NuGet packaging to include BinSkim executable missing in 1.6.0-beta.1
* Update to pre-release SARIF v2 output format (sarif-2.0.0-csd.2.beta.2019-01-24)
* Provide for SARIF v1 or v2 file format export. Default is v1 until SARIF v2 is final.

## **v1.6.0-beta.1** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.6.0-beta.1)
* **Breaking** Output is now Sarif V2-CSD1 compliant rather than Sarif V1  

## **v1.5.1** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.5.1
* Fix Linux NuGet packaging to include BinSkim executable missing in 1.5.0.

## **v1.5.0** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.5.0
* Cross platform (Windows/Linux) support.
* **Possibly Breaking:** New Results: Identify and fire configuration errors when located PDBs are stripped
* **Possibly Breaking:** New Results: False negative removed for BA2015.EnableHighEntropyVA:  Correctly flags an AnyCPU binary with HighEntropyVA and Prefer32Bit disabled
* **Possibly Breaking:** New Rules: New rules for ELF Binaries (BA3001.EnablePieOnExecutables, BA3002.DoNotMarkStackAsExecutable, BA3003.EnableStackProtector, BA3010.EnableReadOnlyRelocations, and BA3030.UseCheckedFunctionsWithGcc)
* **Possibly Breaking:** New Rules: Provide preliminary BA2024.EnableSpectreMitigations analysis

## **v1.4.5** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.4.5
* Correct signing check pass message to reflect actual analysis
* Sign all BinSkim binaries

## **v1.4.4** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.4.4
* Do not fire BA2001.LoadImageAboveFourGigabyteAddressId for ILOnly 64-bit assemblies

## **v1.4.3** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.4.3
* Fix rich return code return functionality when core command-line parsing breaks
* Export configuration knob to adjust EnableControlFlowGuard linker version check
* Loosen SignSecurely rule to prevent errors on WinTrustVerify errors CERT_E_UNTRUSTEDROOT and CERT_E_CHAINING

## **v1.4.2** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.4.2
* Add 'rich' return code (a bitfield value of observed runtime conditions) via SARIF SDK --rich-return-code arg

## **v1.4.1** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.4.1
* Add response file support
* Add __vcrt_trace_logging_provider::_TlgWrite exception to BA2014.DoNotDisableStackProtectionForFunctions

## **v1.4.0** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.4.0
* Fix rule crash on firing 'not applicable' message for control flow guard check
* Add BinScope readable rule name information to SARIF log file output
* Fix reporting errors when flagging binaries signed with weak cryptogrphic algorithms
* Drop required compiler tools version to 17.0.65501.17013
* Make minimum required linker configurable for EnableControlFlowGuard check

## **v1.3.9** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.3.9
* Fix false positives of BA2008:EnableControlFlowGuard firing on x86 kernel mode binaries
* Eliminate high-entropy VA analysis for binaries with no entry points
* Update various checks to eliminate noise analyzing boot binaries

## **v1.3.8** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.3.8
* Update Sarif dependency to 1.5.40
* --config argument is now optional
* Fix false positives of BA2008:EnableControlFlowGuard firing against MC++ mixed mode binaries
* Fix false positives of BA2008:EnableControlFlowGuard firing against resource-only dll that include exported API forwarders (but no code)
* XML-based configuration now functional
* Eliminated compiler tool version false positives for Intel compiler and MASM

## **v1.3.7** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.3.7
* Update Sarif dependency to 1.5.38
* More incidental reporting improvements

## **v1.3.6** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.3.6
* Update Sarif dependency to 1.5.36
* Improves output in error cases

## **v1.3.5** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.3.5
* Fix false positives in 'sign securely' analysis for multi-signed binaries
* Eliminate noise in stack protection analysis against .NET native binaries
* Update Sarif dependency to 1.5.28

## **v1.3.4-beta** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.3.4-beta) 
* Force load PDBs in some circumstances where they have failed to do so

## **v1.3.3-beta** [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BinSkim/1.3.3-beta) 
* Update Sarif dependency to Sarif SDK/Driver 1.5.22-beta (Sarif JSON format 1.0.0)