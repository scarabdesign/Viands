using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Viands.Support;

namespace Viands.Data
{
    public static class ViandsService
    {

        public static async Task<ViandsProductQueryResponse> FetchProductInfoByUPC(string apikey, string upc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return null;
            }

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            try
            {
                var uri = new Uri(Path.Combine(Constants.APIEndpoint, "products?apikey=" + apikey + "&upc=" + upc));
                using HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadFromJsonAsync<ViandsProductQueryResponse>();

                if (responseBody != null && responseBody.status == "ok")
                {
                    return responseBody;
                }

                Debug.WriteLine(responseBody.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        public static async Task<List<ViandsCloudBackupResponseListItem>> ListCloudBackups(string apikey)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return null;
            }

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            try
            {
                var uri = new Uri(Constants.APIEndpoint + "/backups?apikey=" + apikey);
                using HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadFromJsonAsync<ViandsCloudBackupResponse>();
                if(responseBody != null && responseBody.status == "ok")
                {
                    return responseBody.list;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        public static async Task<List<string>> ListFormattedCloudBackups(List<string> localList)
        {
            var remoteBackups = await ListCloudBackups(await LoginUtils.GetCurrentUserApiKey());
            var localBackups = localList.Where(l => !l.Contains(Support.Constants.VirtualCloudPath)).ToList();
            if (remoteBackups != null)
            {
                var remotePaths = remoteBackups.Select(r => r.backupname).ToList();
                var uniqueRemotes = remoteBackups.Where(b => !localBackups.Any(l => l.Contains(b.backupname))).Select(b => Support.Constants.VirtualCloudPath + "/" + b.backupname + ".r2");
                var notFoundOnRemote = localBackups.Where(b =>
                {
                    var fn = Path.GetFileNameWithoutExtension(b);
                    var sfn = BackupNameUpdate(fn);
                    return !remotePaths.Contains(sfn) && 
                        (BackupNameContains(fn, BackupStatusFilenames.CloudSuccess) || 
                        BackupNameContains(fn, BackupStatusFilenames.RemoteSuccess));

                }).ToList();

                if(notFoundOnRemote.Count > 0)
                {
                    notFoundOnRemote.ForEach(f =>
                    {
                        var newFileName = BackupNameUpdate(f, BackupStatusFilenames.LocalSuccess);
                        File.Move(f, newFileName);
                        var index = localBackups.IndexOf(f);
                        if(index > -1)
                        {
                            localBackups[index] = newFileName;
                        }
                    });
                }

                localBackups.AddRange(uniqueRemotes);
            }

            localBackups.Sort((a, b) => Path.GetFileName(b).CompareTo(Path.GetFileName(a)));
            return localBackups;
        }

        public static async Task<bool> ViandsCloudBackup(string apikey, string backupPath)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return false;
            }

            using var fileStream = File.OpenRead(backupPath);

            byte[] bytes;

            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            fileStream.Close();

            using var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            var filename = BackupNameUpdate(Path.GetFileName(backupPath));
            var filenamePre = filename.Replace(".zip", "");

            using var form = new MultipartFormDataContent
            {
                { fileContent, "backupdata", filename },
                { new StringContent(apikey), "apikey" },
                { new StringContent(filenamePre), "backupname" },
            };

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            try
            {
                var uri = new Uri(Constants.APIEndpoint + "/backups/create");
                var response = await client.PostAsync(uri, form);
                response.EnsureSuccessStatusCode();

                File.Move(backupPath, BackupNameUpdate(backupPath, BackupStatusFilenames.CloudSuccess));

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            File.Move(backupPath, BackupNameUpdate(backupPath, BackupStatusFilenames.CloudFailed));

            return false;
        }

        public static bool BackupNameContains(string backupPath, string status)
        {
            return backupPath.Contains("." + status);
        }

        public static string BackupNameUpdate(string backupPath, string newStatus = null)
        {
            backupPath = backupPath
                .Replace("." + BackupStatusFilenames.CloudSuccess, "")
                .Replace("." + BackupStatusFilenames.CloudFailed, "")
                .Replace("." + BackupStatusFilenames.LocalSuccess, "")
                .Replace("." + BackupStatusFilenames.LocalFailed, "")
                .Replace("." + BackupStatusFilenames.RemoteAvailable, "")
                .Replace("." + BackupStatusFilenames.RemoteSuccess, "")
                .Replace("." + BackupStatusFilenames.RemoteFailed, "");

            if(newStatus != null)
            {
                if (backupPath.Contains(".zip"))
                {
                    backupPath = backupPath.Replace(".zip", "." + newStatus + ".zip");
                }
                else
                {
                    backupPath += "." + newStatus;
                }
            }

            return backupPath;
        }

        public static async Task<bool> ViandsDeleteCloudBackup(string apikey, string backupName)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return false;
            }

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            try
            {
                var bn = BackupNameUpdate(backupName);
                var uri = new Uri(Constants.APIEndpoint + "/backups/delete?apikey=" + apikey + "&backupname=" + bn);
                var response = await client.GetAsync(uri);
                var results = response.EnsureSuccessStatusCode();
                return results.ReasonPhrase == "OK";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }

        public static async Task<bool> ViandsCloudRestore(string apikey, string backupName, bool downloadOnly)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return false;
            }

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            try
            {
                var bn = BackupNameUpdate(backupName);
                var uri = new Uri(Constants.APIEndpoint + "/backups/restore?apikey=" + apikey + "&backupname=" + bn);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var nbn = Path.Combine(Constants.DatabaseBackupFolder, BackupNameUpdate(bn, BackupStatusFilenames.CloudSuccess) + ".zip");
                using var fileStream = File.Create(nbn);
                using (var reader = new StreamReader(stream))
                {
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                }

                fileStream.Close();
                stream.Close();

                if (!downloadOnly)
                {
                    return await Constants.Database.RestoreDatabase(nbn);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }

        public static async Task<bool> ViandsRegisterUser(v_users user)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return false;
            }

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            try
            {
                var form = new Dictionary<string, string>()
                {
                    { "username", user.name },
                    { "email", user.email },
                    { "passhash", user.password },
                    { "apikey", user.apikey },
                };

                var uri = new Uri(Constants.APIEndpoint + "/users/register");
                var response = await client.PostAsync(uri, new FormUrlEncodedContent(form));
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }
    }

    public class ViandsProductQueryResponse
    {
        public string status { get; set; }
        public string upc { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string brand { get; set; }
        public string size { get; set; }
        public string ownerid { get; set; }

        public override string ToString()
        {
            return "{ " +
                "status: " + status + ", " +
                "upc: " + upc + ", " +
                "title: " + title + ", " +
                "description: " + description + ", " +
                "brand: " + brand + ", " +
                "size: " + size + ", " +
                "ownerid: " + ownerid +
            " }";
        }
    }

    public class ViandsCloudBackupResponse
    {
        public string status { get; set; }
        public List<ViandsCloudBackupResponseListItem> list { get; set; }
    }

    public class ViandsCloudBackupResponseListItem
    {
        public string backupname { get; set; }
        public string datecreated { get; set; }
    }
}
