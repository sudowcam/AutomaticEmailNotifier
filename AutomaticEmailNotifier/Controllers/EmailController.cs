using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;         // Install-Package Google.Apis.Gmail.v1
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Plus.v1;          // Install-Package Google.Apis.Plus.v1
using Google.Apis.Util.Store;
using MimeKit;                      // Install-Package MimeKit -Version 3.2.0

using AutomaticEmailNotifier.Models;
using Microsoft.AspNetCore.Mvc;


namespace AutomaticEmailNotifier.Controllers
{
    public class EmailController : Controller
    {
        static string[] Scopes = { GmailService.Scope.GmailCompose };
        static string ApplicationName = "C# ASP.NET Core Gmail API";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(CancellationToken cancellationToken, EmailModel email)
        {
            GmailService service = Initialize();
            ComposeAndSendEmail(service, email);
            return View();
        }

        static private GmailService Initialize()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(".API_Credentials/gmail_api_credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        static private IList<Message> getEmailMessages(GmailService service)
        {
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List("me");
            IList<Message> oMessage = request.Execute().Messages;
            Console.WriteLine("Messages:");
            if (oMessage != null && oMessage.Count > 0)
            {
                foreach (var item in oMessage)
                {
                    Message m2 = service.Users.Messages.Get("Me", item.Id).Execute();

                    // Get email snippet
                    //Console.WriteLine("[{0}]", m2.Snippet);
                }
            }
            else
            {
                Console.WriteLine("No Messages found.");
            }
            //Console.Read();

            return oMessage;
        }

        static private IList<Label> getEmailLabels(GmailService service)
        {
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");
            IList<Label> oLabels = request.Execute().Labels;
            Console.WriteLine("Labels:");
            if (oLabels != null && oLabels.Count > 0)
            {
                foreach (var item in oLabels)
                {
                    Console.WriteLine("{0}", item.Name);
                }
            }
            else
            {
                Console.WriteLine("No labels found.");
            }
            //Console.Read();

            return oLabels;
        }


        static private InternetAddressList getRecipientEmail(EmailModel model)
        {
            InternetAddressList recipientList = new InternetAddressList();

            string[] splitEmail = model.To.Split(";");
            char[] charToTrim = { ' ', '\t' };

            foreach (var rawEmail in splitEmail)
            { 
                string email = rawEmail.Trim(charToTrim);
                if (email.IndexOf('@') < 0)
                {
                    // not email detected
                    continue;
                }
                else
                {
                    // https://stackoverflow.com/questions/63525362/sending-emails-with-mailkit-obsolete-warnings-with-mailboxaddressstring-constr
                    string displayname = email.Split('@')[0];
                    recipientList.Add(new MailboxAddress(displayname, email));
                }
            }

            return recipientList;
        }

        static private void ComposeAndSendEmail(
            GmailService service,
            EmailModel model)
        {
            var EmailDetails = new MimeMessage();

            /// From
            var gmailProfile = service.Users.GetProfile("me").Execute();
            var userGmailAddress = gmailProfile.EmailAddress;
            var userGmailName = userGmailAddress.Split('@')[0];
            EmailDetails.From.Add(new MailboxAddress(userGmailName, userGmailAddress));

            /// To
            InternetAddressList emailList = getRecipientEmail(model);
            if (emailList.Count < 1)
            {
                //ViewBag.AlertMessage = "Invalid or no proof of payment uploaded. Only jpg, png or pdf files are accepted.";
                //ViewBag.AlertType = "Danger";
            }
            EmailDetails.To.AddRange(emailList);

            /// Subject
            EmailDetails.Subject = model.Subject;

            /// Body
            if (model.Body != null)
            {
                EmailDetails.Body = new TextPart("plain")
                {
                    Text = model.Body
                };
            }


            var ms = new MemoryStream();
            EmailDetails.WriteTo(ms);
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            Message message = new Message();
            string rawString = sr.ReadToEnd();

            byte[] raw = System.Text.Encoding.UTF8.GetBytes(rawString);
            message.Raw = System.Convert.ToBase64String(raw);
            var result = service.Users.Messages.Send(message, "me").Execute();

            Console.WriteLine(result);
            Console.WriteLine("Message was sent!");

            return;
        }

    }
}
