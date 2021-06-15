using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Interfaces
{
    public interface IHttpHandler
    {
        HttpResponseMessage Get(string url);

        HttpResponseMessage Post(string url, HttpContent content);

        Task<HttpResponseMessage> GetAsync(string url);

        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);

        HttpRequestHeaders DefaultRequestHeaders { get; }
    }
}