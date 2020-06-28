# Sending emails in ASP.NET Core
**Source** : https://www.codewithmukesh.com/blog/send-emails-with-aspnet-core/
```c#
var client = new RestClient("http://127.0.0.1:5000/mail/send");
client.Timeout = -1;
var request = new RestRequest(Method.POST);
request.AddParameter("Receiver", "yourmail@domain.com");
request.AddParameter("Subject", "Test");
request.AddParameter("Body", "hi! this should work.");
request.AddFile("Attachments", "path/to/file1");
request.AddFile("Attachments", "path/to/file2");
IRestResponse response = client.Execute(request);
Console.WriteLine(response.Content);
```