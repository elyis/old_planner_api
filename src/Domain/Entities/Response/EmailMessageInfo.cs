using MimeKit;

namespace old_planner_api.src.Domain.Entities.Response
{
    public class EmailMessageInfo
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public string Body { get; set; }
        public int Index { get; set; }

        public EmailMessageInfo(MimeMessage message, int index)
        {
            
            Subject = message.Subject;
            From = message.From.Mailboxes.Select(m => m.Address).FirstOrDefault();
            To = message.To.Mailboxes.Select(m => m.Address).FirstOrDefault();
            Date = message.Date.DateTime;
            Body = message.TextBody ?? message.HtmlBody;
            Index = index;
        }
    }
}