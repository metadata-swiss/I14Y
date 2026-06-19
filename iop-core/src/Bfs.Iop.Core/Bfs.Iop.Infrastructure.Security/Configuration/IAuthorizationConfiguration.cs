using Bfs.Iop.Infrastructure.Security.Configuration;

namespace Bfs.Iop.Infrastructure.Security;

internal interface IAuthorizationConfiguration
{
    string Audience { get; }

    string[] Issuers { get; }
}
