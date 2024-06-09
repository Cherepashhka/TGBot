using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TGBot.ForSkinPriceAPI
{
    public class ConnectionClient
    {
        private HttpClient _httpClient;
        private static string _address;

        public ConnectionClient()
        {
            _httpClient = new HttpClient();
            _address = Constants.address;
            _httpClient.BaseAddress = new Uri(_address);
        }

        public async Task<SkinInfo> GetSkinInfoAsync(string SkinName)
        {
            var responce = await _httpClient.GetAsync($"/APIController/GetSkinPrice?skinName={SkinName}");
            responce.EnsureSuccessStatusCode();
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<SkinInfo>(content);
            return result;
        }
        public async Task PostInPreferenceList(string SkinName)
        {
            SkinName = SkinName.Split(' ')[1];
            var serialskinname = JsonConvert.SerializeObject(SkinName);
            var responce = await _httpClient.PostAsJsonAsync($"/DatabaseController/NewPreference?serskinname=%22{SkinName}%22", serialskinname);
            responce.EnsureSuccessStatusCode();
            return;
        }
        public async Task<string> GetPreferenceList()
        {
            var responce = await _httpClient.GetAsync($"/DatabaseController/PreferenceList");
            responce.EnsureSuccessStatusCode();
            var preflist = responce.Content.ReadAsStringAsync().Result;
            return preflist;
        }
        public async Task DeleteFromPreferenceList(string SkinName)
        {
            var responce = await _httpClient.DeleteAsync($"/DatabaseController/DeletePreference?name={SkinName}");
            responce.EnsureSuccessStatusCode();
            return;
        }
        public async Task DeleteAllFromPreferenceList()
        {
            var responce = await _httpClient.DeleteAsync($"/DatabaseController/DeleteAllPreference");
            responce.EnsureSuccessStatusCode();
            return;
        }
        public async Task<string> Inventory(string steamID)
        {
            var responce = await _httpClient.GetAsync($"/APIController/Inventory?steamID={steamID}");
            responce.EnsureSuccessStatusCode();
            var result = responce.Content.ReadAsStringAsync().Result;
            return result;
        }
    }
}