using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace HLP.OrganizePlanilha.UI.Web.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts/desktop")
                .Include("~/Scripts/jquery-{version}.js",
                         "~/Scripts/jquery-ui-{version}.js",
                         "~/Scripts/bootstrap.min.js",
                         "~/Scripts/bootbox.min.js",                         
                         "~/Scripts/jquery.maskedinput-{version}.js",
                         "~/Scripts/jquery.validate.min.js",
                         "~/Scripts/jquery.validate.unobtrusive.min.js")
                         .IncludeDirectory("~/Scripts", ".js"));

            bundles.Add(new StyleBundle("~/bundles/css/desktop")
                .Include( "~/Content/bootstrap.min.css",
                          "~/Content/bootstrap-theme.min.css",
                          "~/Content/Site.css",
                          "~/Content/themes/pepper-grinder.css")
                          .IncludeDirectory("~/Content",".css"));
        }
    }
}