using Bfs.Iop.Admin.Models.EIAM.SOAP.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.ExternalClients.EIAM;

internal interface IEIAMSoapApiClient
{
    Task<IList<UserReturn>> SearchUsersByQuery(string query);
}
