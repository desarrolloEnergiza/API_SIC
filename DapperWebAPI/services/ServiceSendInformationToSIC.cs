using DataAccess.Interfases;
using Newtonsoft.Json;
using System.Text;

namespace DapperWebAPI.services
{
    public interface IServiceSendInformationToSIC
    {
        public void sendInformationToSIC();
    }

    public class ServiceSendInformationToSIC : IServiceSendInformationToSIC
    {
        private readonly ILogger<ServiceSendInformationToSIC> logger;
        private readonly IRootRepo rootRepo;

        public ServiceSendInformationToSIC(ILogger<ServiceSendInformationToSIC> logger, IRootRepo rootRepo)
        {
            this.logger = logger;
            this.rootRepo = rootRepo;
        }

        public void sendInformationToSIC()
        {
            var apiKey = "https://auladigital.sence.cl/gestor/API/avance-sic/enviarAvance";
            List<string> responses = new List<string>();
            List<int> ids = new List<int>() { 213, 193 };

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromHours(2);
                List<Task<string>> responseTasks = new List<Task<string>>();

                foreach (int id in ids)
                {
                    responseTasks.Add(SendDataAsync(httpClient, apiKey, id));
                }

                Task.WhenAll(responseTasks).GetAwaiter().GetResult();

                foreach (var responseTask in responseTasks)
                {
                    try
                    {
                        string responseBody = responseTask.Result;
                        Logs.saveLog(DateTime.Now.ToString("yyyy-MM-dd HH") + "RESPONSE", responseBody);
                        responses.Add(responseBody);
                    }
                    catch (Exception ex)
                    {
                        responses.Add(ex.Message);
                    }
                }

            }
        }

        private async Task<string> SendDataAsync(HttpClient httpClient, string apiKey, int id)
        {
            try
            {
                var rootResponse = await rootRepo.GetData(id);
                var rawResponse = JsonConvert.SerializeObject(rootResponse);

                Console.WriteLine(rawResponse);

                var content = new StringContent(rawResponse, Encoding.UTF8, "application/json");

                Logs.saveLog(DateTime.Now.ToString("yyyy-MM-dd HH") + "REQUEST", " "+content+" ");

                HttpResponseMessage response = await httpClient.PostAsync(apiKey, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
