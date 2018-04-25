using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System;
using log4net;
using SPI_Service_Alarm.Model;
using System.Configuration;
using System.Reflection;
using Newtonsoft.Json;

namespace SPI_Service_Alarm
{
    public class HttpRequestOtherAPI
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public List<Tag> GetAPITagsAlarme()
        {
            try
            {
                HttpRequestOtherAPI restComunication = new HttpRequestOtherAPI();

                StringBuilder urlBuilder = new StringBuilder();
                urlBuilder.Append(ConfigurationManager.AppSettings["GetTagsAlarm"]);

                var content = RequestOtherAPI("get", urlBuilder.ToString(), "xpto", "application/json", null);

                List<Tag> tagList = JsonConvert.DeserializeObject<List<Tag>>(content);

                return tagList;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return null;
            }
        }

        public ThingAlarm GetAPIAlarm(int thingId)
        {
            try
            {
                HttpRequestOtherAPI restComunication = new HttpRequestOtherAPI();

                StringBuilder urlBuilder = new StringBuilder();
                urlBuilder.Append(ConfigurationManager.AppSettings["GetAlarm"]);
                urlBuilder.Append(thingId.ToString());

                var content = RequestOtherAPI("get", urlBuilder.ToString(), "xpto", "application/json", null);

                List<ThingAlarm> alarm = JsonConvert.DeserializeObject<List<ThingAlarm>>(content);

                if(alarm != null || alarm.Count > 0)
                    return alarm.FirstOrDefault();
                
                return null;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return null;
            }
        }

        public ThingAlarm PostAPIAlarm(Alarm alarm)
        {
            try
            {
                HttpRequestOtherAPI restComunication = new HttpRequestOtherAPI();

                StringBuilder urlBuilder = new StringBuilder();
                urlBuilder.Append(ConfigurationManager.AppSettings["GetAlarm"]);

                var alarmJson = JsonConvert.SerializeObject(alarm);

                var content = RequestOtherAPI("Post", urlBuilder.ToString(), "xpto", "application/json", alarmJson.ToString());

                ThingAlarm alarmReturn = JsonConvert.DeserializeObject<ThingAlarm>(content);

                if (alarmReturn != null)
                    return alarmReturn;

                return null;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return null;
            }
        }

        public ThingGroup GetAPIThingGroup(int thingGroupId)
        {
            try
            {
                HttpRequestOtherAPI restComunication = new HttpRequestOtherAPI();

                StringBuilder urlBuilder = new StringBuilder();
                urlBuilder.Append(ConfigurationManager.AppSettings["GetThingGroup"]);
                urlBuilder.Append(thingGroupId.ToString());

                var content = RequestOtherAPI("get", urlBuilder.ToString(), "xpto", "application/json", null);

                ThingGroup thingGroup = JsonConvert.DeserializeObject<ThingGroup>(content);

                return thingGroup;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return null;
            }
        }

        private string RequestOtherAPI(string method, string url, string authHeader, string contentType, string data = null)
        {

            string content = null;

            WebRequest webRequest = null;
            WebResponse webResponse = null;
            HttpWebResponse httpResponse = null;
            Stream dataStream = null;
            StreamReader streamReader = null;

            try
            {
                webRequest = WebRequest.Create(url);
                webRequest.Method = method;
                webRequest.ContentType = contentType;
                webRequest.Headers.Add(HttpRequestHeader.Authorization, authHeader);

                // If there is data to send,
                // do appropriate logic
                if (!string.IsNullOrEmpty(data))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    webRequest.ContentLength = byteArray.Length;
                    dataStream = webRequest.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                webResponse = webRequest.GetResponse();

                httpResponse = (HttpWebResponse)webResponse;

                dataStream = webResponse.GetResponseStream();

                streamReader = new StreamReader(dataStream);

                content = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                _log.Error(" URL: " + url +" - ERRO: " + ex.Message);
            }
            finally
            {
                if (streamReader != null) streamReader.Close();
                if (dataStream != null) dataStream.Close();
                if (httpResponse != null) httpResponse.Close();
                if (webResponse != null) webResponse.Close();
                if (webRequest != null) webRequest.Abort();
            }

            return content;
        }
    }
}
