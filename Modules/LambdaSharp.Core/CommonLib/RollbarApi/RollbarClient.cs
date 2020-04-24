/*
 * LambdaSharp (λ#)
 * Copyright (C) 2018-2020
 * lambdasharp.net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LambdaSharp.Core.RollbarApi {

    public class RollbarClientException : Exception {

        //--- Constructors ---
        public RollbarClientException(string message) : base(message) { }
    }

    public class RollbarResponse {

        //--- Properties ---
        [JsonProperty("err")]
        public int Error { get; set; }

        [JsonProperty("result")]
        public object Result { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class RollbarCreateProjectRequest {

        //--- Properties ---
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RollbarProject {

        //--- Properties ---
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("date_created")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonProperty("date_modified")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Modified { get; set; }
    }

    public class RollbarProjectToken {

        //--- Properties ---
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("date_created")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Created { get; set; }

        [JsonProperty("date_modified")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Modified { get; set; }
    }

    public class RollbarClient {

        //--- Class Fields ---
        public static HttpClient HttpClient = new HttpClient();

        //--- Fields ---
        private readonly string _accountReadAccessToken;
        private readonly string _accountWriteAccessToken;
        private readonly Action<string> _logInfo;

        //--- Constructors ---
        public RollbarClient(string accountReadAccessToken, string accountWriteAccessToken, Action<string> logInfo) {
            _accountReadAccessToken = accountReadAccessToken;
            _accountWriteAccessToken = accountWriteAccessToken;
            _logInfo = logInfo;
        }

        //--- Properties ---
        public bool HasTokens => (_accountReadAccessToken != null) && (_accountWriteAccessToken != null);

        //--- Methods ---
        public string SendRollbarPayload(Rollbar rollbar) {

            // send payload to rollbar
            var payload = JsonConvert.SerializeObject(rollbar, Formatting.None, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var request = (HttpWebRequest)WebRequest.Create("https://api.rollbar.com/api/1/item/");
            request.ContentType = "application/json";
            request.Method = "POST";
            request.ContentLength = payloadBytes.Length;
            using(var stream = request.GetRequestStream()) {
                stream.Write(payloadBytes, 0, payloadBytes.Length);
            }
            var response = request.GetResponse();
            var responseCode = ((HttpWebResponse)response).StatusCode;
            using(var stream = response.GetResponseStream()) {
                if(stream == null) {
                    return responseCode.ToString();
                }
                using(var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task<RollbarProject> CreateProject(string projectName) {
            LogInfo($"create rollbar project {projectName}");
            var httpResponse = await HttpClient.SendAsync(new HttpRequestMessage {
                RequestUri = new Uri("https://api.rollbar.com/api/1/projects/"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new RollbarCreateProjectRequest {
                    AccessToken = _accountWriteAccessToken,
                    Name = projectName
                }), Encoding.UTF8, "application/json")
            });
            var result = JsonConvert.DeserializeObject<RollbarResponse>(await httpResponse.Content.ReadAsStringAsync());
            if((httpResponse.StatusCode == (HttpStatusCode)422) && (result.Message == "Project with this name already exists")) {
                return await FindProjectByName(projectName) ?? throw new RollbarClientException($"could not find project: {projectName}");
            }
            if(!httpResponse.IsSuccessStatusCode) {
                throw new RollbarClientException($"http operation failed: {httpResponse.StatusCode}");
            }
            if(result.Error != 0) {
                throw new RollbarClientException($"rollbar operation failed (error {result.Error}): {result.Message}");
            }
            return JsonConvert.DeserializeObject<RollbarProject>(JsonConvert.SerializeObject(result.Result));
        }

        public async Task<IEnumerable<RollbarProject>> ListAllProjects() {
            LogInfo($"list all rollbar projects");
            var httpResponse = await HttpClient.SendAsync(new HttpRequestMessage {
                RequestUri = new Uri($"https://api.rollbar.com/api/1/projects/?access_token={_accountReadAccessToken}"),
                Method = HttpMethod.Get
            });
            if(!httpResponse.IsSuccessStatusCode) {
                throw new RollbarClientException($"http operation failed: {httpResponse.StatusCode}");
            }
            var result = JsonConvert.DeserializeObject<RollbarResponse>(await httpResponse.Content.ReadAsStringAsync());
            if(result.Error != 0) {
                throw new RollbarClientException($"rollbar operation failed (error {result.Error}): {result.Message}");
            }
            var list = JsonConvert.DeserializeObject<List<RollbarProject>>(JsonConvert.SerializeObject(result.Result));
            return list.Where(project => project.Name != null).ToArray();
        }

        public async Task<RollbarProject> FindProjectByName(string projectName) {

            // Rollbar has a 32-character limit on project names
            if(projectName.Length > 32) {
                projectName = projectName.Substring(0, 32);
            }
            var allProjects = await ListAllProjects();
            return allProjects.FirstOrDefault(project => projectName.Equals(project.Name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<RollbarProject> GetProject(int projectId) {
            LogInfo($"get rollbar project {projectId}");
            var httpResponse = await HttpClient.SendAsync(new HttpRequestMessage {
                RequestUri = new Uri($"https://api.rollbar.com/api/1/project/{projectId}?access_token={_accountReadAccessToken}"),
                Method = HttpMethod.Get
            });
            if(!httpResponse.IsSuccessStatusCode) {
                throw new RollbarClientException($"http operation failed: {httpResponse.StatusCode}");
            }
            var result = JsonConvert.DeserializeObject<RollbarResponse>(await httpResponse.Content.ReadAsStringAsync());
            if(result.Error != 0) {
                throw new RollbarClientException($"rollbar operation failed (error {result.Error}): {result.Message}");
            }
            return JsonConvert.DeserializeObject<RollbarProject>(JsonConvert.SerializeObject(result.Result));
        }

        public async Task DeleteProject(int projectId) {
            LogInfo($"delete rollbar project {projectId}");
            var httpResponse = await HttpClient.SendAsync(new HttpRequestMessage {
                RequestUri = new Uri($"https://api.rollbar.com/api/1/project/{projectId}?access_token={_accountWriteAccessToken}"),
                Method = HttpMethod.Delete
            });
            if(!httpResponse.IsSuccessStatusCode) {
                throw new RollbarClientException($"http operation failed: {httpResponse.StatusCode}");
            }
            var result = JsonConvert.DeserializeObject<RollbarResponse>(await httpResponse.Content.ReadAsStringAsync());
            if(result.Error != 0) {
                throw new RollbarClientException($"rollbar operation failed (error {result.Error}): {result.Message}");
            }
        }

        public async Task<IEnumerable<RollbarProjectToken>> ListProjectTokens(int projectId) {
            LogInfo($"list rollbar project tokens {projectId}");
            var httpResponse = await HttpClient.SendAsync(new HttpRequestMessage {
                RequestUri = new Uri($"https://api.rollbar.com/api/1/project/{projectId}/access_tokens?access_token={_accountReadAccessToken}"),
                Method = HttpMethod.Get
            });
            if(!httpResponse.IsSuccessStatusCode) {
                throw new RollbarClientException($"http operation failed: {httpResponse.StatusCode}");
            }
            var result = JsonConvert.DeserializeObject<RollbarResponse>(await httpResponse.Content.ReadAsStringAsync());
            if(result.Error != 0) {
                throw new RollbarClientException($"rollbar operation failed (error {result.Error}): {result.Message}");
            }
            return JsonConvert.DeserializeObject<List<RollbarProjectToken>>(JsonConvert.SerializeObject(result.Result));
        }

        private void LogInfo(string message) => _logInfo?.Invoke(message);
    }
}