using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace L03
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var clientId = "48743065629-pg76tron1h14covglsbkca06jp3r30tl.apps.googleusercontent.com";
            var clientSecret = "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";

            var clientSecrets = new ClientSecrets() { ClientId = clientId, ClientSecret = clientSecret };
            var scopes = Enumerable.Empty<string>()
                .Append(DriveService.Scope.Drive)
                .Append(DriveService.Scope.DriveFile);

            var creds = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets, scopes, Environment.UserName, CancellationToken.None,
                new FileDataStore("Daimto.GoogleDrive.Auth.Store"));
            using var service = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = creds });

            var fileListRequest = service.Files.List();
            fileListRequest.Fields = "nextPageToken, files(id, name, fileExtension, mimeType, size)";
            fileListRequest.Q = "'root' in parents and mimeType != 'application/vnd.google-apps.folder'";
            fileListRequest.OrderBy = "name";

            var data = await fileListRequest.ExecuteAsync();
            Console.WriteLine(data.Files
                .Select(a => $"{a.Name}\n")
                .Aggregate((a, b) => a + b));

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine("random");
            writer.Flush();
            stream.Position = 0;

            var file = new Google.Apis.Drive.v3.Data.File();
            file.Name = "random.txt";
            file.Description = "Kosovo je Srbija";
            file.MimeType = MediaTypeNames.Text.Plain;
            file.Parents = new string[] { "root" };
            var fileCreateRequest = service.Files.Create(file, stream, MediaTypeNames.Text.Plain);
            await fileCreateRequest.UploadAsync();
        }
    }
}
