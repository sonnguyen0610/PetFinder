using System.Web.Http;

namespace PetFinderAPI
{
    public static class WebApiConfig
    {


        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services  
            // Configure Web API to use only bearer token authentication.  
            config.Filters.Add(new AuthorizeAttribute());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
