# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This repository contains SVG.NET, a C# SVG rendering library and editor framework. The project consists of two main components:

1. **Core SVG Library** (`Svg/`) - A comprehensive SVG parsing and rendering library
2. **SVG Editor Framework** (`Svg.Editor.*`) - A cross-platform SVG editor built on top of the core library

## Common Commands

### Building the Project

Use Cake build system for building:
```bash
# Build both solutions (SVG library and editor)
./build.ps1 -Target Build

# Build with specific configuration
./build.ps1 -Target Build -Configuration Release

# Clean build directories
./build.ps1 -Target Clean
```

### Running Tests

```bash
# Run all tests
./build.ps1 -Target Test

# Run specific test project using dotnet
dotnet test Svg.Tests.Win/Svg.Tests.Win.csproj
dotnet test Svg.Editor.Core.Tests/Svg.Editor.Core.Tests.csproj
```

### Building NuGet Packages

```bash
# Build and package for NuGet
./build.ps1 -Target NuGet -nuget_version 2.4.4.12
```

### Working with Individual Projects

```bash
# Build main SVG library
dotnet build Svg/Svg.csproj

# Build editor core
dotnet build Svg.Editor.Core/Svg.Editor.Core.csproj

# Run editor tests
dotnet test Svg.Editor.Core.Tests/Svg.Editor.Core.Tests.csproj
```

## Project Architecture

### Core SVG Library (`Svg/`)

The main SVG library is organized into functional areas:

- **Basic Shapes/**: Core SVG shape implementations (SvgCircle, SvgRectangle, etc.)
- **Document Structure/**: SVG document hierarchy (SvgDocument, SvgGroup, SvgFragment)
- **Rendering/**: Cross-platform rendering abstractions and SkiaSharp implementation
- **Paths/**: SVG path parsing and manipulation
- **Painting/**: Paint servers, gradients, and color handling
- **Text/**: SVG text rendering and font handling
- **Transforms/**: SVG transformations (translate, rotate, scale, etc.)
- **Filter Effects/**: SVG filters and effects
- **Css/**: CSS parsing and styling support
- **External/**: Third-party libraries (ExCSS, Fizzler)

Key classes:
- `SvgDocument`: Main entry point for loading/creating SVG documents
- `SvgRenderer`: Core rendering engine using SkiaSharp
- `SvgElement`: Base class for all SVG elements

### SVG Editor Framework (`Svg.Editor.*`)

The editor is built using a tool-based architecture:

- **Svg.Editor.Core/**: Core editor logic and tool framework
  - **Tools/**: Individual editing tools (SelectionTool, LineTool, TextTool, etc.)
  - **Gestures/**: Input gesture recognition
  - **Services/**: Platform abstraction services
  - **UndoRedo/**: Command pattern implementation for undo/redo

- **Svg.Editor.Forms/**: Xamarin.Forms UI components
- **Svg.Editor.Views/**: Native platform implementations

Key concepts:
- **ITool**: Interface for all editing tools
- **SvgDrawingCanvas**: Main canvas control for editing
- **IGestureRecognizer**: Cross-platform gesture handling
- **IUndoRedoService**: Undo/redo functionality

### Multi-Platform Support

The library targets multiple platforms:
- .NET Standard 2.0
- .NET Framework 4.6.2
- MonoAndroid (Xamarin.Android)
- Xamarin.iOS
- UWP (Universal Windows Platform)

Platform-specific code is organized in `Platforms/` directories with conditional compilation.

## Key Dependencies

- **SkiaSharp**: Core graphics rendering
- **System.Reactive**: Reactive programming for editor
- **Xamarin.Forms**: Cross-platform UI framework
- **ExCSS**: CSS parsing
- **Fizzler**: CSS selector implementation

## Development Notes

### SVG Rendering Pipeline

1. **Parse**: SVG XML is parsed into element tree
2. **Style**: CSS styles are applied to elements
3. **Layout**: Element bounds and transformations are calculated
4. **Render**: Elements are rendered using SkiaSharp graphics

### Editor Tool System

Tools follow a consistent pattern:
- Inherit from `ToolBase` or `UndoableToolBase`
- Implement gesture handling (tap, drag, etc.)
- Use command pattern for undoable operations
- Integrate with selection and transformation systems

### Testing Strategy

- Unit tests in `Svg.Tests.Win/` and `Svg.Editor.Core.Tests/`
- W3C SVG test suite integration in `SvgW3CTestSuite.*`
- Performance tests for rendering operations
- Cross-platform compatibility tests

## Common Patterns

### Adding New SVG Elements

1. Create element class inheriting from appropriate base (e.g., `SvgVisualElement`)
2. Implement rendering logic in `Render()` method
3. Add element registration in `SvgElementFactory`
4. Add unit tests for parsing and rendering

### Adding New Editor Tools

1. Create tool class implementing `ITool`
2. Handle relevant gestures (tap, drag, etc.)
3. Implement undo/redo commands if needed
4. Register tool in `ToolFactoryProvider`
5. Add tool icon as embedded resource

### Performance Considerations

- SVG parsing and rendering can be expensive for complex documents
- Use caching for frequently accessed elements
- Optimize rendering by minimizing state changes
- Consider using background threads for heavy operations