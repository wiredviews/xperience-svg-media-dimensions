using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MediaLibrary;
using CMS.Tests;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace XperienceCommunity.SvgMediaDimensions.Tests
{
    public class SvgMediaDimensionsParserTests : UnitTests
    {
        private IEventLogService eventLogService;
        private ISiteService siteService;

        [SetUp]
        public void Setup()
        {
            Fake<MetaFileInfo>();
            Fake<MediaFileInfo>();
            Fake<AttachmentInfo>();

            eventLogService = Substitute.For<IEventLogService>();
            siteService = Substitute.For<ISiteService>();
        }

        [Test]
        public void Files_With_Non_Svg_Extensions_Will_Not_Be_Updated()
        {
            var metaFile = new MetaFileInfo
            {
                MetaFileExtension = ".jpg"
            };
            var mediaFile = new MediaFileInfo
            {
                FileExtension = ".jpg"
            };
            var attachment = new AttachmentInfo
            {
                AttachmentExtension = ".jpg"
            };

            var sut = new SvgMediaDimensionsParser(siteService, eventLogService);

            bool metaUpdated = sut.SetDimensions(metaFile);

            metaUpdated.Should().BeFalse();

            bool mediaUpdated = sut.SetDimensions(mediaFile);

            mediaUpdated.Should().BeFalse();

            bool attachmentUpdated = sut.SetDimensions(attachment);

            attachmentUpdated.Should().BeFalse();
        }
    }
}
