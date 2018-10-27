using BotApplication1.DTO;
using Firebase.Database.Client;
using Firebase.Database.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BotApplication1.Helpers
{
    public static class FireBaseHelper
    {
        public static  async Task registerUserInFireBase(User user)
        {
            var options = await getFirebaseClientOptions();
            using (var fbClient = new FirebaseClient(ConfigurationManager.AppSettings["FireBaseClientUrl"].ToString(), options))
            {
                //var scores = await fbClient.Child("scores").OnceAsync<Dictiona‌​ry<string, string>>();
                await fbClient.Child("users").PostAsync(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            }
        }

        private static async Task<FirebaseOptions> getFirebaseClientOptions()
        {
            var fbAuthClient = new Firebase.Auth.Provider.FirebaseAuthProvider(new Firebase.Auth.Config.FirebaseConfig(ConfigurationManager.AppSettings["FireBaseApiKey"].ToString()));
            var token =  await fbAuthClient.SignInWithEmailAndPasswordAsync(ConfigurationManager.AppSettings["FireBaseLoginMail"].ToString(), ConfigurationManager.AppSettings["FireBaseLoginPassword"].ToString());

            var options = new Firebase.Database.Options.FirebaseOptions
            {
                AuthTokenAsync = () => Task.FromResult(token.FirebaseToken)
            };

            return options;
        }

    }
}