using System;
using System.Threading.Tasks;
using CommandLine;
using Keroosha.GraphQL.Web.Services;

namespace Keroosha.GraphQL.Web.Cmdlets
{
    public class CmdletServices : CmdletBase<CmdletServices.ServiceOptions>
    {
        private readonly IServiceProvider _provider;

        public CmdletServices(IServiceProvider provider)
        {
            _provider = provider;
        }

        [Verb("--Services")]
        public class ServiceOptions
        {
        }

        protected override async Task<int> Execute(ServiceOptions args)
        {
            ServiceRunner.StartServices(_provider);
            return 0;
        }
    }
}