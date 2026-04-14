# L5X Module Parser

A C# console application for parsing **Rockwell Studio 5000 / ControlLogix `.L5X` files** and generating a **CSV module report**.

This project reads an exported `.L5X` file, finds all XML `<Module>` elements, extracts their attributes, collects descendant `<Port>` attributes, and writes the flattened result to a CSV file.

## What this tool does

The parser creates **one CSV row per module** and dynamically builds the CSV headers from all attributes found in the file.

It extracts:

- All attributes on each `<Module>` element
- All attributes on descendant `<Port>` elements
- Optional direct text content from `<Port>` elements when present

Port data is flattened into columns using this naming pattern:

```text
Port[1].Address
Port[1].Type
Port[2].Address
```

## Example workflow

1. Load an `.L5X` file from disk
2. Parse the XML using `XDocument`
3. Find all `<Module>` elements in a namespace-safe way
4. Collect module-level attributes
5. Collect port-level attributes
6. Build a union of all discovered column names
7. Export the results to CSV

## Current project structure

Main logic is contained in a single file:

- `L5X_Module_Parser.cs`

Core methods:

- `Main(...)` - validates file paths, loads XML, extracts module data, writes CSV
- `AddPortAttributes(...)` - flattens `<Port>` data into CSV-ready columns
- `WriteCsv(...)` - writes headers and rows to the output file
- `EscapeCsv(...)` - safely escapes CSV values

## Requirements

- **Visual Studio 2026**
- **.NET 10.0**
- Windows file system access to the source `.L5X` file and destination CSV path

## Namespaces used

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
```

## Input and output

### Input

A Rockwell `.L5X` file, for example:

```text
D:\C_SharpDev\PLC_Program_L5XFormat.L5X
```

### Output

A CSV report file, for example:

```text
D:\C_SharpDev\PLC_Program_ModulesReport.csv
```

## How to run

At the moment, the application uses **hardcoded full paths** inside `Main(...)`.

### Option 1: Run from Visual Studio

1. Open the solution in Visual Studio 2026
2. Confirm the project targets `.NET 10.0`
3. Update the input and output paths in `Program.Main`
4. Build and run the project

### Option 2: Run from the command line

```bash
dotnet run
```

## Example output columns

Depending on the `.L5X` content, the CSV may contain columns such as:

- `Name`
- `CatalogNumber`
- `Vendor`
- `ProductType`
- `Inhibited`
- `Port[1].Address`
- `Port[1].Type`
- `Port[2].Address`

Because headers are built dynamically, the exact columns depend on the modules and ports found in the source file.

## Design notes

### Namespace-safe XML parsing

The code matches XML elements using `e.Name.LocalName == "Module"` and `e.Name.LocalName == "Port"`.

This makes the parser more robust when `.L5X` files include XML namespaces.

### Dynamic schema generation

Not all modules contain the same attributes. Instead of hardcoding CSV headers, the code builds the final header list from the union of keys across all rows.

### CSV escaping

The application properly escapes values that contain:

- commas
- double quotes
- carriage returns
- line feeds

This improves compatibility with Excel and other CSV readers.

## Limitations in the current version

- File paths are hardcoded
- All parsing logic is in one source file
- No command-line argument support yet
- No automated tests yet
- No filtering options for specific module types or chassis sections
- `Descendants()` can be broader than necessary for very large files

## Recommended next improvements

Here are good next steps for evolving the project:

1. Accept input and output paths from command-line arguments
2. Add exception handling for CSV write failures
3. Split parsing and export logic into separate classes
4. Add unit tests with small sample `.L5X` files
5. Add optional filtering by module name, vendor, or catalog number
6. Add logging or summary statistics
7. Add support for reporting only Ethernet/IP port data

## Suggested repository description

**Parse Rockwell Studio 5000 L5X module data and export module/port attributes to CSV using C# and .NET 10.**

## Suggested topics/tags

- `csharp`
- `dotnet`
- `net10`
- `visual-studio`
- `automation`
- `controllogix`
- `studio5000`
- `l5x`
- `plc`
- `xml`
- `csv`
- `rockwell`

## Notes for contributors

If you modify the parser, please keep these goals in mind:

- Preserve namespace-safe XML matching
- Keep CSV output backward compatible when possible
- Document any new flattened column naming rules
- Add a sample `.L5X` file for testing if a new XML pattern is supported

## License

Add your preferred license here, for example:

- MIT
- Apache-2.0
- Proprietary / internal use only

