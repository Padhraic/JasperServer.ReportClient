using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace JasperServer.ReportClient
{
    public class ReportClient : IReportClient

    {

        private readonly ReportClientOptions _options;
        private readonly AuthenticationHeaderValue _authentication;



        /// <summary>
        /// Util to retrieve a report from JasperServer.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password for Username</param>
        /// <param name="baseurl">JasperReports</param>
        /// <example>
        /// JasperserverRestClient jasperserverRestClient = new JasperserverRestClient("username", "password", "http://localhost:8080/jasperserver/rest_v2/reports");
        /// Stream stream = jasperserverRestClient.Get("/reports/SGV/Total.pdf");
        /// // or
        /// JasperserverRestClient jasperserverRestClient = new JasperserverRestClient("username", "password", "http://localhost:8080/jasperserver/rest_v2/reports");
        /// Dictionary<string, string> parameters = new Dictionary<string, string>();
        /// parameters.Add("PARAM1", "VALUE1");
        /// parameters.Add("PARAM2", "VALUE2");
        /// 
        /// Stream stream = jasperserverRestClient.Get("/reports/SGV/Total.pdf", parameters);
        /// </example>
        public ReportClient(ReportClientOptions options)
        {
            _options = options;

            if (String.IsNullOrEmpty(_options.Username)) {
                throw new NullReferenceException("Username should not be empty.");
            }

            if (String.IsNullOrEmpty(_options.Password)) {
                throw new NullReferenceException("Password should not be empty.");
            }

            if (String.IsNullOrEmpty(_options.BaseUrl)) {
                throw new NullReferenceException("Base URL should not be empty.");
            }

            var byteArray = Encoding.ASCII.GetBytes(_options.Username + ":" + _options.Password);
            var encoded = Convert.ToBase64String(byteArray);
            this._authentication = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);
        }

        /// <summary>
        /// Retrieve the stream of report asked.
        /// </summary>
        /// <param name="report">Report path at server</param>
        /// <returns>Stream of report asked</returns>
        public async Task<Stream> GetAsync(string report)
        {
            return await this.GetAsync(report, null);
        }

        /// <summary>
        /// Retrieve the stream of report asked using parameters.
        /// </summary>
        /// <param name="report">Report path at server</param>
        /// <param name="parameters">Parameters of report</param>
        /// <returns>Stream of report asked using parameters</returns>
        public async Task<Stream> GetAsync(string report, Dictionary<string, string> parameters)
        {
            Contract.Requires(_authentication != null);
            Contract.Requires(!string.IsNullOrEmpty(report));
            
            var url = _options.BaseUrl + report;
            if (parameters == null || parameters.Count <= 0) return await this.GetTaskAsync(url);
            var builder = new UriBuilder(url)
            {
                Query = string.Join("&",
                    parameters.Select(pair => string.Concat(pair.Key, "=", HttpUtility.UrlEncode(pair.Value)))
                        .ToArray())
            };
            return await this.GetTaskAsync(builder.Uri.AbsoluteUri);
        }

        /// <summary>
        /// Retrieve a task of url.
        /// </summary>
        /// <param name="url">Url to retrieve content</param>
        private async Task<Stream> GetTaskAsync(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = _authentication;
            return await client.GetStreamAsync(url);
        }

        /// <summary>
        /// Save report to a file path.
        /// </summary>
        /// <param name="report">Report path at server</param>
        /// <param name="filename">File full path to output stream</param>
        public async Task SaveToFileAsync(string report, string filename)
        {
            await this.SaveToFileAsync(report, null, filename);
        }

        /// <summary>
        /// Save report to a file path using parameters.
        /// </summary>
        /// <param name="report">Report path at server</param>
        /// <param name="parameters">Parameters of report</param>
        /// <param name="filename">File full path to output stream</param>
        public async Task SaveToFileAsync(string report, Dictionary<string, string> parameters, string filename)
        {
            var stream = await this.GetAsync(report, parameters);
            stream.Seek(0, SeekOrigin.Begin);
            Stream output = File.OpenWrite(filename);
            stream.CopyTo(output);
            output.Close();
        }
    }
}