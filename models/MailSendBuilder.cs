using System.Collections.Generic;
using Microsoft.Graph;

namespace Eagels.MailSender;

public class MailSendBuilder
{
    private string _sendFrom;
    private string _sendTo;
    private string _content;
    private bool _hasQrCode;
    private string _subject;
    private MessageAttachmentsCollectionPage _attachments = new MessageAttachmentsCollectionPage();

    public MailSendBuilder(string sendFrom)
    {
        _sendFrom = sendFrom;
    }
    public MailSendBuilder AddSendTo(string sendTo)
    {
        _sendTo = sendTo;
        return this;
    }
    public MailSendBuilder AddContent(string content)
    {
        _content = content;
        return this;
    }
    public MailSendBuilder AddQRCode(byte[] qrCode)
    {
        _hasQrCode = true;
        _attachments.Add(new FileAttachment()
        {
            ContentType = "image/png",
            ContentBytes = qrCode,
            ContentId = "qrcode",
            Name = "Einzahlungschein.png"
        });
        return this;
    }
    public MailSendBuilder AddSubject(string subject)
    {
        _subject = subject;
        return this;
    }
    public MailSendBuilder AddAttachment(FileAttachment attachement) {
        _attachments.Add(attachement);
        return this;
    }
    public MailSendBuilder AddAttachment(string name, string contenType, byte[] content)
    {
        _attachments.Add(new FileAttachment()
        {
            ContentBytes = content,
            Name = name,
            ContentType = contenType
        });
        return this;
    }
    public MailSendBuilder ReplacePlaceHolders(string placeholder, string value)
    {
        _content = _content.Replace(placeholder, value);
        return this;
    }

    public Message BuildMailMessage()
    {
        string content = _hasQrCode ? _content + "<img src='cid:qrcode' width='50%' height='50%'/>" : _content;
        MessageAttachmentsCollectionPage attachments = new MessageAttachmentsCollectionPage();
        return new Message()
        {
            Attachments = _attachments,
            Subject = _subject,
            Body = new ItemBody()
            {
                ContentType = BodyType.Html,
                Content = content
            },
            ToRecipients = new Recipient[]{new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = _sendTo
                    }
                }
            }
        };
    }
    public string GetSendFrom() {
        return _sendFrom;
    }
}