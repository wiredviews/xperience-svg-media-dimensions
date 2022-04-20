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
    /// width/height so it can persisted in the database.
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

        public bool SetDimensions(MetaFileInfo metaFile)
        {
            if (!string.Equals(metaFile.MetaFileExtension, ".svg", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (metaFile.MetaFileImageWidth > 0 && metaFile.MetaFileImageHeight > 0)
            {
                return false;
            }

            try
            {
                bool hasUpdated = false;

                if (metaFile.InputStream?.Length > 0)
                {
                    hasUpdated = UpdateDimensions(metaFile.InputStream, metaFile);

                    if (metaFile.InputStream.CanSeek)
                    {
                        // If we don't reset the stream, then Xperience won't have any
                        // data to read to populate the file in external (Azure/AWS) storage
                        metaFile.InputStream.Seek(0, SeekOrigin.Begin);
                    }
                }
                else
                {
                    byte[] binary = GetFileBinary(metaFile);

                    if (binary is object)
                    {
                        using (var stream = new MemoryStream(binary))
                        {
                            hasUpdated = UpdateDimensions(stream, metaFile);
                        }
                    }
                    else
                    {
                        eventLogService.LogError(
                            nameof(SvgMediaDimensionsParser),
                            "SVG_DIMENSIONS_UPDATE_FAILURE",
                            $"Could not retrieve the SVG binary for the meta file.{Environment.NewLine}{Environment.NewLine}{metaFile.MetaFileGUID}{Environment.NewLine}{metaFile.MetaFileName}");

                        return false;
                    }
                }

                if (!hasUpdated)
                {
                    eventLogService.LogError(
                        nameof(SvgMediaDimensionsParser),
                        "SVG_DIMENSIONS_UPDATE_FAILURE",
                        $"Could not update the dimensions for the SVG meta file.{Environment.NewLine}{Environment.NewLine}{metaFile.MetaFileGUID}{Environment.NewLine}{metaFile.MetaFileName}");

                    return false;
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SvgMediaDimensionsParser), "SVG_DIMENSIONS_UPDATE_FAILURE", ex, additionalMessage: $"Meta File: {metaFile.MetaFileGUID}");

                return false;
            }

            return true;
        }

        private bool UpdateDimensions(Stream stream, MetaFileInfo metaFile)
        {
            var svgDoc = SvgDocument.Open<SvgDocument>(stream, null);

            if (svgDoc is null)
            {
                return false;
            }

            int width = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Width).Value);
            int height = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Height).Value);

            if (width <= 0 || height <= 0)
            {
                return false;
            }

            metaFile.MetaFileImageWidth = width;
            metaFile.MetaFileImageHeight = height;

            return true;
        }

        private byte[] GetFileBinary(MetaFileInfo metaFile)
        {
            if (metaFile.MetaFileBinary is object)
            {
                return metaFile.MetaFileBinary;
            }

            return MetaFileInfoProvider.GetFile(metaFile, siteService.CurrentSite.SiteName);
        }

        public bool SetDimensions(IAttachment attachment)
        {
            if (!string.Equals(attachment.AttachmentExtension, ".svg", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (attachment.AttachmentImageWidth > 0 && attachment.AttachmentImageHeight > 0)
            {
                return false;
            }

            try
            {
                bool hasUpdated = false;
                byte[] binary = GetFileBinary(attachment);

                if (binary is object)
                {
                    using (var stream = new MemoryStream(binary))
                    {
                        var svgDoc = SvgDocument.Open<SvgDocument>(stream, null);

                        if (svgDoc is object)
                        {
                            int width = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Width).Value);
                            int height = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Height).Value);

                            if (width >= 0 && height >= 0)
                            {
                                attachment.AttachmentImageWidth = width;
                                attachment.AttachmentImageHeight = height;

                                hasUpdated = true;
                            }

                            if (!hasUpdated)
                            {
                                eventLogService.LogError(
                                    nameof(SvgMediaDimensionsParser),
                                    "SVG_DIMENSIONS_UPDATE_FAILURE",
                                    $"Could not update the dimensions for the SVG attachment.{Environment.NewLine}{Environment.NewLine}{attachment.AttachmentGUID}{Environment.NewLine}{attachment.AttachmentName}");

                                return false;
                            }
                        }
                        else
                        {
                            eventLogService.LogError(
                                nameof(SvgMediaDimensionsParser),
                                "SVG_DIMENSIONS_UPDATE_FAILURE",
                                $"Could not retrieve the SVG binary for the attachment.{Environment.NewLine}{Environment.NewLine}{attachment.AttachmentGUID}{Environment.NewLine}{attachment.AttachmentName}");

                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SvgMediaDimensionsParser), "SVG_DIMENSIONS_UPDATE_FAILURE", ex, additionalMessage: $"Attachment: {attachment.AttachmentGUID}");

                return false;
            }

            return true;
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

        public bool SetDimensions(MediaFileInfo mediaFile)
        {
            if (!string.Equals(mediaFile.FileExtension, ".svg", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (mediaFile.FileImageWidth > 0 && mediaFile.FileImageHeight > 0)
            {
                return false;
            }

            try
            {
                bool hasUpdated = false;

                if (mediaFile.FileBinaryStream?.Length > 0)
                {
                    hasUpdated = UpdateDimensions(mediaFile.FileBinaryStream, mediaFile);

                    if (mediaFile.FileBinaryStream.CanSeek)
                    {
                        // If we don't reset the stream, then Xperience won't have any
                        // data to read to populate the file in external (Azure/AWS) storage
                        mediaFile.FileBinaryStream.Seek(0, SeekOrigin.Begin);
                    }
                }
                else
                {
                    byte[] binary = GetFileBinary(mediaFile);

                    if (binary is object)
                    {
                        using (var stream = new MemoryStream(binary))
                        {
                            hasUpdated = UpdateDimensions(stream, mediaFile);
                        }
                    }
                    else
                    {
                        eventLogService.LogError(
                            nameof(SvgMediaDimensionsParser),
                            "SVG_DIMENSIONS_UPDATE_FAILURE",
                            $"Could not retrieve the SVG binary for the media file.{Environment.NewLine}{Environment.NewLine}{mediaFile.FileGUID}{Environment.NewLine}{mediaFile.FileName}");

                        return false;
                    }
                }

                if (!hasUpdated)
                {
                    eventLogService.LogError(
                        nameof(SvgMediaDimensionsParser),
                        "SVG_DIMENSIONS_UPDATE_FAILURE",
                        $"Could not update the dimensions for the SVG media file.{Environment.NewLine}{Environment.NewLine}{mediaFile.FileGUID}{Environment.NewLine}{mediaFile.FileName}");

                    return false;
                }
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(SvgMediaDimensionsParser), "SVG_DIMENSIONS_UPDATE_FAILURE", ex, additionalMessage: $"Media File: {mediaFile.FileGUID}");

                return false;
            }

            return true;
        }

        private bool UpdateDimensions(Stream stream, MediaFileInfo mediaFile)
        {
            var svgDoc = SvgDocument.Open<SvgDocument>(stream, null);

            if (svgDoc is null)
            {
                return false;
            }

            int width = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Width).Value);
            int height = (int)Math.Round(new SvgUnit(SvgUnitType.Pixel, svgDoc.Height).Value);

            if (width <= 0 || height <= 0)
            {
                return false;
            }

            mediaFile.FileImageWidth = width;
            mediaFile.FileImageHeight = height;

            return true;
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
