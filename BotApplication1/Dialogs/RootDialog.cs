using BotApplication1.DTO;
using BotApplication1.Helpers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApplication1.Dialogs
{
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        public RootDialog() : base(new LuisService(new LuisModelAttribute("TODO", "TODO")))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("CarFaster")]
        public async Task CarFaster(IDialogContext context, LuisResult result)
        {
            await IoTHelper.SendAsync("s+20");
            await context.PostAsync($"Vroooom, the car goes faster");
            context.EndConversation("Conversation Ended");

        }


        [LuisIntent("CarSlower")]
        public async Task CarSlower(IDialogContext context, LuisResult result)
        {
            await IoTHelper.SendAsync("s-20");
            await context.PostAsync($"Pressing the brake");
            context.EndConversation("Conversation Ended");

        }


        [LuisIntent("CarStop")]
        public async Task CarStop(IDialogContext context, LuisResult result)
        {
            await IoTHelper.SendAsync("s0");
            await context.PostAsync($"Car halted !!");
            context.EndConversation("Conversation Ended");

        }

        [LuisIntent("CarStart")]
        public async Task CarStart(IDialogContext context, LuisResult result)
        {
            await IoTHelper.SendAsync("s50");
            await context.PostAsync("3");
            await context.PostAsync("2");
            await context.PostAsync("1");

            await context.PostAsync("GO !!!!!!");
            context.EndConversation("Conversation Ended");

        }

        private int questionNumber;

        private Dictionary<string, string> questions = new Dictionary<string, string>()
            {
                { "email", "Wich email address would you like to use for your account ?" },
                { "nickname", "Which nickname would you like to use ?" },
               // { "LastName","Provide a last name" }
            };

        [LuisIntent("RegisterUser")]
        public async Task RegisterUser(IDialogContext context, LuisResult result)
        {
            var questionsToAsk = new Dictionary<string, string>(questions);

            if (result.Entities == null || result.Entities.Any(x => x.Type == "builtin.email"))
            {
                context.PrivateConversationData.SetValue(questionsToAsk.ElementAt(0).Key,
                    result.Entities.First(x => x.Type == "builtin.email").Entity);

                questionsToAsk.Remove("Email");
            }

            if (result.Entities == null || result.Entities.Any(x => x.Type == "nickname"))
            {
                context.PrivateConversationData.SetValue("nickname",
                    result.Entities.First(x => x.Type == "nickname").Entity);

                questionsToAsk.Remove("nickname");
            }

            if (questionsToAsk.Count > 0)
            {
                context.PrivateConversationData.SetValue("questionsToAsk",
                    questionsToAsk);
                PromptDialog.Text(context, OnQuestionReply, questionsToAsk.ElementAt(0).Value);
            }
            else
            {
                await FinishRegistration(context);
            }
        }




        private async Task OnQuestionReply(IDialogContext context, IAwaitable<string> result)
        {
            var answer = await result;
            //store the value

            var questionsToAsk = context.PrivateConversationData.GetValue<Dictionary<string, string>>("questionsToAsk");
            context.PrivateConversationData.SetValue(questionsToAsk.ElementAt(questionNumber).Key, answer);

            questionNumber++;
            if (questionNumber < questionsToAsk.Keys.Count)
            {
                PromptDialog.Text(context, OnQuestionReply, questionsToAsk.ElementAt(questionNumber).Value);
            }
            else
            {
                await FinishRegistration(context);
            }
        }

        private async Task FinishRegistration(IDialogContext context)
        {
            try
            {
                await FireBaseHelper.registerUserInFireBase(new User
                {
                    email = context.PrivateConversationData.GetValue<string>("email")
                ,
                    // lastname = context.PrivateConversationData.GetValue<string>("LastName")
                    //  ,
                    nickname = context.PrivateConversationData.GetValue<string>("nickname")
                });

                await context.PostAsync($"Registration successfull with following details: ");
                foreach (string key in questions.Keys)
                {
                    await context.SayAsync($"{key}: {context.PrivateConversationData.GetValue<string>(key)}",
                        $"{key}: {context.PrivateConversationData.GetValue<string>(key)}");
                    //await context.PostAsync($"{key}: { context.PrivateConversationData.GetValue<string>(key)}");
                }
                context.EndConversation("Conversation Ended");
            }
            catch (Exception e)
            {
                await context.PostAsync($"Error occured: {e.Message}");
            }
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.EndConversation("Conversation Ended");
        }
    }
}