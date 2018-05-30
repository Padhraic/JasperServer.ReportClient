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
    public class ReportClient

    {

        private readonly Uri _uri;
        private readonly AuthenticationHeaderValue _authentication;


        /// <summary>
        /// Util to retrieve a report from JasperServer.
        /// </summary>
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
        public ReportClient(string url, string username, string password) : this(new Uri(url), username, password)
        {
            
        } 
        
        public ReportClient(Uri uri, string username, string password)
        {
            if(string.IsNullOrEmpty(username))
                throw new NullReferenceException("Username cannot be blank or null.");
            if(string.IsNullOrEmpty(password))
                throw new NullReferenceException("Password cannot be blank of null.");
            
            _uri = uri;
            if(!_uri.IsAbsoluteUri)
                throw new NullReferenceException("Url must be an absolute Url.");
            
            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            var encoded = Convert.ToBase64String(byteArray);
            this._authentication = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);
        }

        /// <summary>
        /// Retrieve the stream of report asked using parameters.
        /// </summary>
        /// <param name="report">Report path at server</param>
        /// <param name="parameters">Parameters of report</param>
        /// <returns>Stream of report asked using parameters</returns>
        public async Task<Stream> GetAsync(string report, Dictionary<string, string> parameters = null)
        {
            if (string.IsNullOrEmpty(report)) {
                throw new NullReferenceException("Username should not be empty.");
            }

            var extUri = new Uri(_uri,report);
            
            if(parameters != null)
            {
                var builder = new UriBuilder(extUri)
                {
                    Query = string.Join("&",
                        parameters.Select(pair => string.Concat(pair.Key, "=", HttpUtility.UrlEncode(pair.Value)))
                            .ToArray())
                };
                extUri = builder.Uri;
            };
            return await this.GetStreamAsync(extUri);
        }

        private async Task<Stream> GetStreamAsync(Uri uri)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = _authentication;
            return await client.GetStreamAsync(uri);
        }

        /// <summary>
        /// Save report to a file path using parameters.
        /// </summary>
        /// <param name="report">Report path at server</param>
        /// <param name="filename">File full path to output stream</param>
        /// <param name="parameters">Parameters of report</param>
        public async Task SaveToFileAsync(string report, string filename, Dictionary<string, string> parameters = null)
        {
            var stream = await this.GetAsync(report, parameters);
            stream.Seek(0, SeekOrigin.Begin);
            Stream output = File.OpenWrite(filename);
            stream.CopyTo(output);
            output.Close();
        }
    }
}