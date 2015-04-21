using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Session;
using Nancy.TinyIoc;
using WebServer.Database;

namespace WebServer
{
    public class CustomRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return Globals.RootPath;
        }
    }

    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            CookieBasedSessions.Enable(pipelines);
        }

        protected override IRootPathProvider RootPathProvider
        {
            get 
            { 
                return new CustomRootPathProvider(); 
            }
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("dist", @"dist"));

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("resources", @"resources"));

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("controls", @"controls"));

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("admin", @"admin"));

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("admin_reporting", @"admin/reporting"));

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("viewreporting", @"viewreporting"));

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("report_resources", @"report_resources"));

            base.ConfigureConventions(conventions);
        }
    }
}
