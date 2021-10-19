# Xperience Svg Media Dimensions

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.SvgMediaDimensions.svg)](https://www.nuget.org/packages/XperienceCommunity.SvgMediaDimensions)

A Kentico Xperience Form Control Extender that sync the Form Control value to/from Page CustomData fields

## Dependencies

This package is compatible with Kentico Xperience 13.

## How to Use?

1. First, install the NuGet package in your Kentico Xperience administration `CMSApp` project

   ```bash
   dotnet add package XperienceCommunity.SvgMediaDimensions
   ```

## How Does It Work?

A custom module intercepts insertions/updates of Media Files (`MediaLibraryInfo`, `IAttachment`, `MetaFileInfo`) and sets the width/height
values of the media before they are insert into the database, if the media file is a valid SVG.

## References

### Kentico Xperience

- [SVG.NET](https://github.com/svg-net/SVG)
