using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.PolicyInjection.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Switchboard
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

    public static class Unity
    {
        private static readonly Microsoft.Practices.Unity.IUnityContainer UnityContainer;

        static Unity()
        {
            UnityContainer = new Microsoft.Practices.Unity.UnityContainer();
            UnityContainer.AddNewExtension<Microsoft.Practices.Unity.InterceptionExtension.Interception>();
            UnityContainer.AddNewExtension<Microsoft.Practices.EnterpriseLibrary.Common.Configuration.Unity.EnterpriseLibraryCoreExtension>();

            IConfigurationSource configSource = ConfigurationSourceFactory.Create();
            PolicyInjectionSettings policyInjectionsettings = (PolicyInjectionSettings)configSource.GetSection("policyInjection");

            if (policyInjectionsettings!= null)
            {
                policyInjectionsettings.ConfigureContainer(UnityContainer, configSource);
            }

            /*
            var configurator = new UnityContainerConfigurator(container);

            // Read the configuration files and set up the container.
            EnterpriseLibraryContainer.ConfigureContainer(configurator,
                                       ConfigurationSourceFactory.Create());
            // The container is now ready to resolve Enterprise Library objects
            */
        }

        public static T Resolve<T>(string name)
        {
            return UnityContainer.Resolve<T>(name);
        }
    }
}
