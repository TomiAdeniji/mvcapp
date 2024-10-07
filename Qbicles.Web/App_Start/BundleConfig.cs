using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Optimization;

namespace Qbicles.Web
{
    internal class AsIsBundleOrderer : IBundleOrderer
    {
        public virtual IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }

    internal static class BundleExtensions
    {
        public static Bundle ForceOrdered(this Bundle sb)
        {
            sb.Orderer = new AsIsBundleOrderer();
            return sb;
        }
    }

    public class BundleConfig
    {
        private static bool isDebug = Debugger.IsAttached;

        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jqueryStyle").Include(
                    "~/Content/DesignStyle/js/select2.full.min.js",
                    "~/Content/DesignStyle/js/jquery.datetimepicker.full.min.js",
                   "~/Content/DesignStyle/js/html5tooltips.js"

            ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/DesignStyle/css/select2.min.css",
                      //Tooltips
                      "~/Content/DesignStyle/css/html5tooltips.css",
                      "~/Content/DesignStyle/css/html5tooltips.animation.css",
                      "~/Content/DesignStyle/css/animate.min.css",
                      //Core
                      "~/Content/DesignStyle/css/app.css",

                      "~/Content/DesignStyle/css/owl.carousel.min.css",
                      "~/Content/DesignStyle/css/owl.theme.default.min.css",
                      "~/Content/DesignStyle/css/bootstrap-tagsinput.css",
                      "~/Content/DesignStyle/css/skins/skin-default.css",
                       //< !--Form builder-- >
                       "~/Content/DesignStyle/css/form-builder.min.css",
                        "~/Content/DesignStyle/css/form-render.min.css",
                        "~/Content/DesignStyle/css/jquery.datetimepicker.min.css",
                        "~/Content/DesignStyle/css/jquery.fancybox.css",
                        "~/Content/DesignStyle/css/daterangepicker.css"
                      ));
            //login =============

            bundles.Add(new ScriptBundle("~/bundles/LogInjqueryStyle").Include(
                    "~/Content/DesignStyle/js/jquery.min.js").Include(
                    "~/Scripts/bootstrap.min.js",
                   "~/Content/DesignStyle/js/html5tooltips.js",
                   "~/Content/DesignStyle/js/jquery.validate.min.js",
                    "~/Content/DesignStyle/js/select2.full.min.js",
                   "~/Content/DesignStyle/js/app.js"
            ).ForceOrdered());

            bundles.Add(new StyleBundle("~/Content/LogIncss").Include(
                     "~/Content/bootstrap.min.css",
                      //Tooltips
                      "~/Content/DesignStyle/css/html5tooltips.css",
                      "~/Content/DesignStyle/css/html5tooltips.animation.css",
                      "~/Content/DesignStyle/css/animate.min.css",
                      //Core
                      "~/Content/DesignStyle/css/app.css",
                      "~/Content/DesignStyle/css/owl.carousel.min.css",
                      "~/Content/DesignStyle/css/owl.theme.default.min.css",
                      "~/Content/DesignStyle/css/bootstrap-tagsinput.css",
                      "~/Content/DesignStyle/css/skins/skin-default.css"
                      ).Include("~/Content/DesignStyle/fa/css/font-awesome.min.css", new CssRewriteUrlTransform()));

            //use on _Layout ====================================================================
            bundles.Add(new StyleBundle("~/Content/qbiclescss")
                .Include(
                        "~/Content/DesignStyle/css/bootstrap.min.css",
                        "~/Content/DesignStyle/css/app.css",
                        "~/Content/DesignStyle/css/jstree.min.css",
                        "~/Content/DesignStyle/css/html5tooltips.css",
                        "~/Content/DesignStyle/css/html5tooltips.animation.css",
                        "~/Content/" + (isDebug ? "Site.css" : "Site.min.css"),
                        "~/Content/DesignStyle/css/datepicker.min.css",
                        "~/Content/DesignStyle/css/skins/skin-default.css",
                        "~/Content/DesignStyle/css/form-builder.min.css",
                        "~/Content/DesignStyle/css/form-render.min.css",
                        "~/Content/DesignStyle/css/select2.min.css",
                        "~/Content/DesignStyle/css/jquery.datetimepicker.min.css",
                        "~/Content/DesignStyle/css/jquery.fancybox.css",
                        "~/Content/DesignStyle/css/daterangepicker.min.css",
                        "~/Content/DesignStyle/css/datepicker.min.css",
                        "~/Content/DesignStyle/css/bootstrap-timepicker.min.css",
                        "~/Content/DesignStyle/css/animate.min.css",
                        "~/Content/DesignStyle/css/monthly.min.css",
                        "~/Content/DesignStyle/css/bootstrap-toggle.min.css",
                        "~/Content/DesignStyle/css/owl.carousel.min.css",
                        "~/Content/DesignStyle/css/owl.theme.default.min.css",
                        "~/Content/DesignStyle/css/bootstrap-tagsinput.css",
                        "~/Content/DesignStyle/css/datatables.min.css",
                        "~/Content/DesignStyle/css/rowReorder.dataTables.min.css",
                        "~/Content/DesignStyle/css/tui-calendar.css",
                        "~/Scripts/toastr/toastr.min.css",
                        "~/Content/DesignStyle/css/pagination.css"
            ).Include("~/Content/DesignStyle/fa/css/font-awesome.min.css", new CssRewriteUrlTransform())
            .Include("~/Content/DesignStyle/css/v4-shims.min.css", new CssRewriteUrlTransform())
            .Include("~/Content/DesignStyle/css/all.min.css", new CssRewriteUrlTransform())
            );//fix BundleConfig minify font-awesome

            bundles.Add(new ScriptBundle("~/bundles/qbiclesjs").Include(
                    "~/Content/DesignStyle/js/jquery.min.js",
                    "~/Content/DesignStyle/js/jstree.min.js",
                    "~/Content/DesignStyle/js/jquery-ui.min.js",
                    "~/Scripts/bootstrap.min.js",
                    "~/Content/DesignStyle/js/loadingoverlay.min.js",
                    "~/Content/DesignStyle/js/" + (isDebug ? "app.js" : "app.min.js"),
                    "~/Content/DesignStyle/js/html5tooltips.js",
                    "~/Content/DesignStyle/js/jquery.validate.min.js",
                    "~/Content/DesignStyle/js/" + (isDebug ? "formvalidate.js" : "formvalidate.min.js"),
                    "~/Content/DesignStyle/js/" + (isDebug ? "jquery.fancybox.js" : "jquery.fancybox.min.js"),
                    "~/Content/DesignStyle/js/jquery.waypoints.min.js",
                    "~/Content/DesignStyle/js/jquery.datetimepicker.full.min.js",
                    "~/Content/DesignStyle/js/jquery.jscroll.min.js",
                    "~/Content/DesignStyle/js/" + (isDebug ? "moment.js" : "moment.min.js"),
                    "~/Content/DesignStyle/js/" + (isDebug ? "daterangepicker.js" : "daterangepicker.min.js"),
                    "~/Content/DesignStyle/js/owl.carousel.min.js",
                    "~/Content/DesignStyle/js/" + (isDebug ? "bootstrap-tagsinput.js" : "bootstrap-tagsinput.min.js"),
                    "~/Scripts/dev/" + (isDebug ? "modal-controller.js" : "modal-controller.min.js"),
                    "~/Scripts/dev/" + (isDebug ? "qbicle.formatnumber.js" : "qbicle.formatnumber.js"),
                    "~/Scripts/dev/" + (isDebug ? "common-controller.js" : "common-controller.js"),
                    "~/Scripts/dev/" + (isDebug ? "s3.upload.medial.js" : "s3.upload.medial.min.js"),
                    "~/Scripts/dev/" + (isDebug ? "approval.status.style.js" : "approval.status.style.min.js"),
                    "~/Scripts/dev/" + (isDebug ? "moduleSelected.js" : "moduleSelected.min.js"),
                    "~/Content/DesignStyle/js/" + (isDebug ? "typeahead.js" : "typeahead.min.js"),
                    "~/Content/DesignStyle/js/datatables.min.js",
                    "~/Content/DesignStyle/js/dataTables.bootstrap.min.js",
                    "~/Content/DesignStyle/js/select2.full.min.js",
                    "~/Scripts/toastr/toastr.min.js",
                    "~/Content/DesignStyle/js/dataTables.rowReorder.min.js",
                    "~/Content/DesignStyle/js/bootstrap-toggle.min.js",
                    "~/Scripts/toastr/cleanBookNotification.min.js",
                    "~/Scripts/signalr/js/jquery.signalR-2.4.1.min.js",
                    "~/Content/DesignStyle/js/lodash/lodash.min.js",
                    "~/Content/DesignStyle/js/jquery.slimscroll.min.js",
                    "~/Content/DesignStyle/js/jquery.countdown.min.js",
                    "~/Scripts/DesignStyle/bootbox/" + (isDebug ? "bootbox.js" : "bootbox.min.js"),
                    "~/Content/DesignStyle/js/datepicker.min.js",
                    "~/Content/DesignStyle/js/datepicker-en.js",
                    "~/Content/DesignStyle/js/pagination.min.js"
            ).ForceOrdered()
            );
            var extJsErrorMessages = new ScriptBundle("~/Scripts/ErrorMessages")
                    .Include("~/Scripts/dev/ErrorMessages.js");
            extJsErrorMessages.Transforms.Clear();
            extJsErrorMessages.Transforms.Add(new Helper.JsTranslator());
            extJsErrorMessages.Transforms.Add(new JsMinify());

            bundles.Add(extJsErrorMessages);

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}