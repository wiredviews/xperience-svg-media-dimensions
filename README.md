# Xperience Svg Media Dimensions

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.SvgMediaDimensions.svg)](https://www.nuget.org/packages/XperienceCommunity.SvgMediaDimensions)

Sets the width/height of SVG images when uploaded/updated in the Administration application of Kentico Xperience 13 sites.

With SVG width/height values, developers can ensure that when files are rendered on the live site, the `<img>` element has width/height values.

These values help the browser know [how much space to reserve in the layout](https://www.smashingmagazine.com/2020/03/setting-height-width-images-important-again/) for the image before it is downloaded from the server and rendered.

This helps improve [Core Web Vitals](https://web.dev/vitals/) by reducing [Cumulative Layout Shift](https://web.dev/cls/).

## Dependencies

This package is compatible with Kentico Xperience 13.

## How to Use?

1. First, install the NuGet package in your Kentico Xperience administration `CMSApp` project

   ```bash
   dotnet add package XperienceCommunity.SvgMediaDimensions
   ```

1. Now any valid SVG that is uploaded to the Media Library, as an Attachment, or as a Meta file (ex: SKU image) will have its width/height values set.

If there are any issues setting the dimensions for an SVG file, the Event Log will be updated with an error with the `EventCode` `SVG_DIMENSIONS_UPDATE_FAILURE`.

## How Does It Work?

A custom module intercepts insertions/updates of Media Files (`MediaLibraryInfo`, `IAttachment`, `MetaFileInfo`) and sets the width/height
values of the media before they are insert into the database, if the media file is a valid SVG.

## References

### Kentico Xperience

- [Kentico Xperience 13 - Custom Module classes](https://docs.xperience.io/custom-development/creating-custom-modules/initializing-modules-to-run-custom-code)
- [SVG.NET](https://github.com/svg-net/SVG)
- [Smashing Magazine - Setting Height And Width On Images Is Important Again](https://www.smashingmagazine.com/2020/03/setting-height-width-images-important-again/)
- [Web.dev - Web Vitals](https://web.dev/vitals/)
- [Web.dev - Cumulative Layout Shift (CLS)](https://web.dev/cls/)
