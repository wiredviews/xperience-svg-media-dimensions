# Xperience SVG Media Dimensions

[![GitHub Actions CI: Build](https://github.com/wiredviews/xperience-svg-media-dimensions/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-svg-media-dimensions/actions/workflows/ci.yml)

[![Publish Packages to NuGet](https://github.com/wiredviews/xperience-svg-media-dimensions/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-svg-media-dimensions/actions/workflows/publish.yml)

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

1. (optional) The module can be disabled site-wide through the `SvgMediaDimensions_Enabled` CMS Setting. You will need to add
a [custom settings to Xperience](https://docs.xperience.io/custom-development/creating-custom-modules/adding-custom-website-settings) to toggle the module off. The module defaults to enabled if no settings value is found.

1. (optional) To have Xperience treat `.svg` files as media in a predictable way, add the following app setting to your `web.config`, which [changes the default image extensions](https://docs.xperience.io/configuring-xperience/configuring-the-environment-for-content-editors/configuring-media-libraries/configuring-supported-file-types-in-media-libraries) to include `.svg`.

    ```xml
    <add key="CMSImageExtensions" value="bmp;gif;ico;png;wmf;jpg;jpeg;tiff;tif;svg" />
    ```

    As noted in the Xperience documentation, updating the application's image settings to include the `.svg` file type won't enable the display of SVG files to work everywhere in the CMS. Specifically the media library file preview will show a broken image.

    To fix this, you can edit the file `CMS\CMSModules\MediaLibrary\Controls\MediaLibrary\MediaFileEdit.ascx.cs` and change the `SetupFile()` method to look something like this when handling the rendering of images:

    ```csharp
    if (ImageHelper.IsImage(FileInfo.FileExtension))
   {
      // Ensure max side size 200
      int[] maxsize = ImageHelper.EnsureImageDimensions(0, 0, 200, FileInfo.FileImageWidth, FileInfo.FileImageHeight);
      imagePreview.Width = maxsize[0];
      imagePreview.Height = maxsize[1];

      // Do not add 'maxsidesize' or 'width'/'height' query parameters to the image URL
      // for SVG images, as these settings are not compatible or applicable
      if (FileInfo.FileExtension.EndsWith("svg"))
      {
         imagePreview.URL = permanentUrl;
         imagePreview.SizeToURL = false;
      }
      else
      {
         imagePreview.URL = URLHelper.AddParameterToUrl(permanentUrl, "maxsidesize", "200");
      }

      imagePreview.URL = URLHelper.AddParameterToUrl(imagePreview.URL, "chset", Guid.NewGuid().ToString());

      plcImagePreview.Visible = true;
      plcMediaPreview.Visible = false;

      pnlPrew.Visible = true;
   }
   ```

## How Does It Work?

A custom module intercepts insertions/updates of Media Files (`MediaLibraryInfo`, `IAttachment`, `MetaFileInfo`) and sets the width/height
values of the media before they are inserted into the database, if the media file is a valid SVG.

## References

### Kentico Xperience

- [Kentico Xperience 13 - Custom Module classes](https://docs.xperience.io/custom-development/creating-custom-modules/initializing-modules-to-run-custom-code)
- [SVG.NET](https://github.com/svg-net/SVG)
- [Smashing Magazine - Setting Height And Width On Images Is Important Again](https://www.smashingmagazine.com/2020/03/setting-height-width-images-important-again/)
- [Web.dev - Web Vitals](https://web.dev/vitals/)
- [Web.dev - Cumulative Layout Shift (CLS)](https://web.dev/cls/)
