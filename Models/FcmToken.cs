using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PetFinderAPI.App_Data;

namespace PetFinderAPI.Models
{
    public class FcmToken
    {

        public int Id { get; set; }
        public String Token { get; set; }
        public String Serial { get; set; }
        [Required(ErrorMessage = "Post is requied!")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public FcmToken()
        {
        }

        public static List<FcmToken> GetTokenForUser(int UserId)
        {
            using (var ctx = new PetContext())
            {
                return ctx.Tokens.Where(t => t.UserId == UserId).ToList();

            }
        }
        public static List<FcmToken> GetTokenForSerial(string serial)
        {
            using (var ctx = new PetContext())
            {
                return ctx.Tokens.Where(t => t.Serial == serial).ToList();

            }
        }

        public static void SendNotificationToUser(string notiBody, int userId)
        {
            var listToken = GetTokenForUser(userId);
            //TODO: Send msg
            Console.Write("FCM: ");

            if (listToken.Count > 0)
            {
                HttpClient client = new HttpClient();

                for (int i = 0; i < listToken.Count; i++)
                {
                    var token = listToken[i].Token;

                    var requestBody = new Dictionary<string, dynamic>
                    {
                        { "to", token },
                        { "notification",  new Dictionary<string, dynamic>
                                            {
                                                { "title", "MeoWoof"},
                                                {"body", notiBody },
                                                {"content_available", true },
                                                {"priority", "high" },
                                            }
                        },
                        { "data",  new Dictionary<string, dynamic>
                                            {
                                                { "title", "MeoWoof"},
                                                {"body", notiBody },
                                                {"content_available", true },
                                                {"priority", "high" },
                                            }
                        },
                    };

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=AAAAjrkUoZg:APA91bFkS32d1Uzz07h0g7uX_QSTu12AGs4nkBt6HJ2O4yYnteNnj3ukv5QUcTHuQzjVBG94xLUoEELDMX_PPQtQqrnxh1MQQ_idRBSD8U_ZzMUz4RLk8SOaR188mtGbaIGh0NNb9BYE");
                    client.DefaultRequestHeaders
                            .Accept
                            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var httpRequestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://fcm.googleapis.com/fcm/send"),
                        Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"),
                    };

                    var response = client.SendAsync(httpRequestMessage).Result;

                    Console.WriteLine(response.StatusCode.ToString());
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                }
            }
        }
    }
}
