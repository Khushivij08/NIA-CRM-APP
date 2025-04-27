using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NIA_CRM.Models
{
    public class NAICSApiHelper
    {
        private readonly HttpClient _client;

        public NAICSApiHelper(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<NaicsRecord>> GetNAICSCodesAsync()
        {
            string apiUrl = "https://api.census.gov/data/2017/naics?get=NAICS2017_LABEL,NAICS2017_CODE&for=sector:*&key=YOUR_API_KEY";
            HttpResponseMessage response = await _client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string jsonResult = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<List<string>>>(jsonResult);

                var naicsRecords = new List<NaicsRecord>();

                for (int i = 1; i < data.Count; i++) // Skip the header row
                {
                    naicsRecords.Add(new NaicsRecord
                    {
                        Label = data[i][0],
                        Code = data[i][1],
                        Sector = data[i][2]
                    });
                }

                return naicsRecords;
            }

            return null;
        }
    }

    public class NaicsRecord
    {
        public string Label { get; set; }
        public string Code { get; set; }
        public string Sector { get; set; }
    }
}
