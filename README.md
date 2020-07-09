<p align="center">
<b>ZRA.NET</b> is a <b><a href="https://github.com/zraorg/ZRA">ZStandard Random Access</a></b> (ZRA) wrapper library for .NET Standard 2.1, written in C#
<br>
<b>ZRA</b> allows random access inside an archive compressed using <a href="https://github.com/facebook/zstd">ZStandard</a>
<br>
<a href="https://github.com/zraorg/ZRA.NET/actions"><img align="center" alt="ZRA.NET Build" src="https://github.com/zraorg/ZRA.NET/workflows/ZRA.NET%20Build/badge.svg"/></a>
<a href="https://www.nuget.org/packages/ZRA.NET"><img align="center" alt="Nuget" src="https://img.shields.io/nuget/v/ZRA.NET?logo=nuget"></a>
<a href="https://github.com/zraorg/ZRA/actions"><img align="center" alt="ZRA Build" src="https://github.com/zraorg/ZRA/workflows/C/C++%20CI/badge.svg"/></a>
</p>

***
### Format
See [ZRA#Format](https://github.com/zraorg/ZRA/blob/master/README.md#format) for more info
### Usage
#### Compression
* In-Memory
  ```csharp
  byte[] compressedData = Zra.Compress(dataToCompress, compressionLevel: 9, frameSize: 131072);
  ```
* Streaming
  ```csharp
  using (ZraCompressionStream compressionStream = new ZraCompressionStream(outStream, (ulong)inStream.Length, compressionLevel: 9, frameSize: 131072))
  {
      inStream.CopyTo(compressionStream);
      compressionStream.Flush(); // You must call Flush() to write the ZRA header to the output stream.
  }
  ```
#### Decompression (Entire File)
* In-Memory
  ```csharp
  byte[] decompressedData = Zra.Decompress(compressedData);
  ```
* Streaming
  ```csharp
  using (ZraDecompressionStream decompressionStream = new ZraDecompressionStream(inStream))
  {
      decompressionStream.CopyTo(outStream);
  }
  ```
#### Decompression (Random-Access)
NOTE: `offset` and `count` are of the original uncompressed data.
* In-Memory
  ```csharp
  byte[] decompressedData = Zra.DecompressSection(compressedData, offset, count);
  ```
* Streaming
  ```csharp
  using (ZraDecompressionStream decompressionStream = new ZraDecompressionStream(inStream))
  {
      byte[] decompressedSection = new byte[count];
      decompressionStream.Read(decompressedSection, offset, count);
  }
  ```
***
### License
We use a simple 3-clause BSD license located at [LICENSE](LICENSE.md) for easy integration into projects while being compatible with the libraries we utilize