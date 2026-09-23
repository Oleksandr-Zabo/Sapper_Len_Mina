# Sapper

A WPF Minesweeper-style application built with .NET 10. The application reads a binary grid from `input.txt`, calculates the distance from every cell to the nearest mine using a graph and multi-source breadth-first search (BFS), displays the result with colored cells, and writes the distance matrix to `output.txt`.

## Requirements

- .NET 10 SDK
- Windows, because the project uses WPF
- Visual Studio 2026 or another .NET-compatible IDE

## Input format

Place an `input.txt` file next to the application executable. The first line contains `N` and `M`, followed by `N × M` binary values:

```text
4 5
0 0 1 0 0
0 0 0 0 0
1 0 0 0 0
0 0 0 0 0
```

`N` and `M` must be between 1 and 100. At least one cell must contain `1`, which represents a mine. Values may be separated by spaces or written as compact binary rows.

## Output format

The application automatically writes `output.txt` next to the executable. It contains `N` rows with `M` space-separated numbers. Each number is the Manhattan distance to the nearest mine; mine cells have distance `0`.

For the example above, the output is:

```text
2 1 0 1 2
1 2 1 2 3
0 1 2 3 4
1 2 3 4 5
```

## Features

- Explicit grid graph where each cell is a vertex.
- Edges connect orthogonally adjacent cells.
- Multi-source BFS starts from every mine simultaneously.
- Mines are displayed as red symbols.
- Distance values use different colors based on their range.
- Input validation and error messages.
- Buttons for reloading `input.txt` and saving `output.txt`.

## Run

1. Build the project:

   ```powershell
   dotnet build Sapper.csproj
   ```

2. Put `input.txt` in the output directory, for example:

   `bin\Debug\net10.0-windows\input.txt`

3. Run the application:

   ```powershell
   dotnet run --project Sapper.csproj
   ```

The board is loaded automatically when the application starts. The `output.txt` file is generated after successful loading.
