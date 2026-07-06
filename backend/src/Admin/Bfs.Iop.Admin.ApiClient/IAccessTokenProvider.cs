using System.Threading.Tasks;

namespace Bfs.Iop.Admin.ApiClient
{
    public interface IAccessTokenProvider
    {
        string AccessToken { get; set; }

        void Clear();

        Task LoginUser(string username, string password);
    }
}