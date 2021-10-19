using System;
using System.IO;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MediaLibrary;
using Svg;

namespace XperienceCommunity.SvgMediaDimensions
{
    /// <summary>
    /// Uses https://github.com/svg-net/SVG to parse SVG metadata and sets the uploaded image's
    /// metadata on the database record
    /// </summary>
    public class SvgMediaDimensionsParser
    {
        private readonly ISiteService siteService;
        private readonly IEventLogService eventLogService;

        public SvgMediaDimensionsParser(ISiteService siteService, IEventLogService eventLogService)
        {
            this.siteService = siteService;
            this.eventLogService = eventLogService;
        }

        public void SetDimensions(MetaFileInfo metaFile)
        {
            if (!string.Equals(metaFile.MetaFileExtension, ".svg", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (metaFile.MetaFileImageWidth > 0 && metaFile.MetaFileImageHeight > 0)
            {
                return;
            }

            try
            {
                byte[] binary = GetFileBinary(metaFile);

                if (binary is object)
                {
                    using (var stream = new MemoryStream(binary))
                    {
                        UpdateDimensions(stream, metaFile);
                    }
                }

                if (metaFile.MetaFileImageWidth <= 0 || metaFile.MetaFileImageHeight <= 0)
                {
                    eventLogService.LogError(
                        nameof(SvgMediaDimensionsParser),
                        "SVG_DIMENSIONS_UPDATE_FAILURE",
                        $"Could not retrieve attachment binary for meta file.{Environment.NewLine}{Environment.NewLine}{metaFile.MetaFileGUID}{Environment.NewLine}{metaFile.MetaFileName}");
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SvgMediaDimensionsParser), "SVG_DIMENSIONS_UPDATE_FAILURE", ex);
            }
        }

        private void UpdateDimensions(Stream stream, MetaFileInfo metaFile)
        {
            var svgDoc = SvgDocument.Open<SvgDocument>(stream, null);

            if (svgDoc is null)
            {
                return;
            }

            metaFile.MetaFileImageWidth = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Width).Value);
            metaFile.MetaFileImageHeight = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Height).Value);
        }

        private byte[] GetFileBinary(MetaFileInfo metaFile)
        {
            if (metaFile.MetaFileBinary is object)
            {
                return metaFile.MetaFileBinary;
            }

            return MetaFileInfoProvider.GetFile(metaFile, siteService.CurrentSite.SiteName);
        }

        public void SetDimensions(IAttachment attachment)
        {
            if (!string.Equals(attachment.AttachmentExtension, ".svg", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (attachment.AttachmentImageWidth > 0 && attachment.AttachmentImageHeight > 0)
            {
                return;
            }

            try
            {
                byte[] binary = GetFileBinary(attachment);

                if (binary is object)
                {
                    using (var stream = new MemoryStream(binary))
                    {
                        var svgDoc = SvgDocument.Open<SvgDocument>(stream, null);

                        if (svgDoc is object)
                        {
                            attachment.AttachmentImageWidth = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Width).Value);
                            attachment.AttachmentImageHeight = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Height).Value);
                        }
                    }
                }

                if (attachment.AttachmentImageWidth <= 0 || attachment.AttachmentImageHeight <= 0)
                {
                    eventLogService.LogError(
                        nameof(SvgMediaDimensionsParser),
                        "SVG_DIMENSIONS_UPDATE_FAILURE",
                        $"Could not retrieve attachment binary for attachment.{Environment.NewLine}{Environment.NewLine}{attachment.AttachmentGUID}{Environment.NewLine}{attachment.AttachmentName}");
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SvgMediaDimensionsParser), "SVG_DIMENSIONS_UPDATE_FAILURE", ex);
            }
        }

        private byte[] GetFileBinary(IAttachment attachment)
        {
            // For files with uploaded binary (new file or update)
            if (attachment.AttachmentBinary is object)
            {
                return attachment.AttachmentBinary;
            }

            var docAttachment = new DocumentAttachment(attachment);
            return AttachmentBinaryHelper.GetAttachmentBinary(docAttachment);
        }

        public void SetDimensions(MediaFileInfo mediaFile)
        {
            if (!string.Equals(mediaFile.FileExtension, ".svg", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (mediaFile.FileImageWidth > 0 && mediaFile.FileImageHeight > 0)
            {
                return;
            }

            try
            {
                if (mediaFile.FileBinaryStream.Length > 0)
                {
                    UpdateDimensions(mediaFile.FileBinaryStream, mediaFile);
                }
                else
                {
                    byte[] binary = GetFileBinary(mediaFile);

                    if (binary is object)
                    {
                        using (var stream = new MemoryStream(binary))
                        {
                            UpdateDimensions(stream, mediaFile);
                        }
                    }
                }

                if (mediaFile.FileImageWidth <= 0 || mediaFile.FileImageHeight <= 0)
                {
                    eventLogService.LogError(
                        nameof(SvgMediaDimensionsParser),
                        "SVG_DIMENSIONS_UPDATE_FAILURE",
                        $"Could not retrieve attachment binary for attachment.{Environment.NewLine}{Environment.NewLine}{mediaFile.FileGUID}{Environment.NewLine}{mediaFile.FileName}");
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SvgMediaDimensionsParser), "SVG_DIMENSIONS_UPDATE_FAILURE", ex);
            }
        }

        private void UpdateDimensions(Stream stream, MediaFileInfo mediaFile)
        {
            var svgDoc = SvgDocument.Open<SvgDocument>(stream, null);

            if (svgDoc is null)
            {
                return;
            }

            mediaFile.FileImageWidth = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Width).Value);
            mediaFile.FileImageHeight = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Height).Value);
        }

        private byte[] GetFileBinary(MediaFileInfo mediaFile)
        {
            // For files with uploaded binary (new file or update)
            if (mediaFile.FileBinary != null)
            {
                return mediaFile.FileBinary;
            }

            // For existing files
            var mediaLibrary = MediaLibraryInfo.Provider.Get(mediaFile.FileLibraryID);
            return MediaFileInfoProvider.GetFile(mediaFile, mediaLibrary.LibraryFolder, siteService.CurrentSite.SiteName);
        }
    }
}
