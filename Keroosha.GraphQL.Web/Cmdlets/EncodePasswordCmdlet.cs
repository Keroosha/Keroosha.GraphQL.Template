using System;
using System.Threading.Tasks;
using CommandLine;
using Keroosha.GraphQL.Web.Auth;

namespace Keroosha.GraphQL.Web.Cmdlets
{
    public class EncodePasswordCmdletBase : CmdletBase<EncodePasswordCmdletBase.EncodePassOptions>
    {
        [Verb("--Encodepass")]
        public class EncodePassOptions
        {
            [Value(0)] public string Password { get; set; }
        }

        protected override Task<int> Execute(EncodePassOptions args)
        {
            if (args.Password == null)
            {
                Console.WriteLine("Enter:");
                args.Password = Console.ReadLine();
            }

            Console.WriteLine(PasswordToolkit.EncodeSshaPassword(args.Password));
            return Task.FromResult(0);
        }
    }
}