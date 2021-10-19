using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MediaLibrary;
using XperienceCommunity.SvgMediaDimensions;

[assembly: RegisterModule(typeof(SvgMediaDimensionsModule))]

namespace XperienceCommunity.SvgMediaDimensions
{
    public class SvgMediaDimensionsModule : Module
    {
        public SvgMediaDimensionsModule() : base(nameof(SvgMediaDimensionsModule))
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            MediaFileInfo.TYPEINFO.Events.Insert.Before += MediaFile_BeforeSave;
            MediaFileInfo.TYPEINFO.Events.Update.Before += MediaFile_BeforeSave;

            AttachmentInfo.TYPEINFO.Events.Insert.Before += Attachment_BeforeSave;
            AttachmentInfo.TYPEINFO.Events.Update.Before += Attachment_BeforeSave;

            MetaFileInfo.TYPEINFO.Events.Insert.Before += MetaFile_BeforeSave;
            MetaFileInfo.TYPEINFO.Events.Update.Before += MetaFile_BeforeSave;
        }

        private void MetaFile_BeforeSave(object sender, ObjectEventArgs e)
        {
            if (!(e.Object is MetaFileInfo metaFile))
            {
                return;
            }

            var parser = new SvgMediaDimensionsParser(SiteService, EventLogService);

            parser.SetDimensions(metaFile);
        }

        private void Attachment_BeforeSave(object sender, ObjectEventArgs e)
        {
            if (!(e.Object is IAttachment attachment))
            {
                return;
            }

            var parser = new SvgMediaDimensionsParser(SiteService, EventLogService);

            parser.SetDimensions(attachment);
        }

        private void MediaFile_BeforeSave(object sender, ObjectEventArgs e)
        {
            if (!(e.Object is MediaFileInfo mediaFile))
            {
                return;
            }

            var parser = new SvgMediaDimensionsParser(SiteService, EventLogService);

            parser.SetDimensions(mediaFile);
        }

        private ISiteService siteService;

        private ISiteService SiteService
        {
            get
            {
                if (siteService is null)
                {
                    siteService = Service.Resolve<ISiteService>();
                }

                return siteService;
            }
        }

        private IEventLogService eventLogService;

        private IEventLogService EventLogService
        {
            get
            {
                if (eventLogService is null)
                {
                    eventLogService = Service.Resolve<IEventLogService>();
                }

                return eventLogService;
            }
        }
    }
}
