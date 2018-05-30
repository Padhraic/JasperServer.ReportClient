using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JasperServer.ReportClient
{
    public interface IReportClient

    {

        Task<Stream> GetAsync(string report);

        Task<Stream> GetAsync(string report, Dictionary<string, string> parameters);

        Task SaveToFileAsync(string report, string filename);

        Task SaveToFileAsync(string report, string filename, Dictionary<string, string> parameters);

    }
}