namespace AutomaticEmailNotifier.Models
{
    public class EmailModel
    {
        public string To { get; set; }

        // Temporary disabled
        //public string cc { get; set; }
        //public string bcc { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

    }
}