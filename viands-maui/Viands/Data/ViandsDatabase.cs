using SQLite;
using System.Diagnostics;
using System.IO.Compression;
using Viands.Support;

namespace Viands.Data
{
    public class ViandsDatabase
    {

        public SQLiteAsyncConnection connection;
        public ViandsDatabase() { }

        public async Task InitComponents(bool force = false)
        {
            if (connection is not null)
            {
                if (!force)
                {
                    Debug.WriteLine("Database already initialized");
                    return;
                }

                await connection.CloseAsync();
                connection = null;
            }
                
            connection = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags, false);
            await BuildDatabase();
        }

        public async Task BuildDatabase()
        {
            var types = new List<Type>()
            {
                typeof(v_users), typeof(v_lists), 
                typeof(v_listitems), typeof(v_producttypes), 
                typeof(v_products), typeof(v_prices),
                typeof(v_locations), typeof(v_settings)
            };

            var tasks = types.Select(BuildTable);
            await Task.WhenAll(tasks);
            Debug.WriteLine("Database initialized");
        }

        public async Task ClearDatabase()
        {
            //disconnect main connection
            await connection?.CloseAsync();

            //delete database file
            File.Delete(Constants.DatabasePath);

            //re-init db
            await InitComponents(true);
        }

        public async Task BackupDatabase(string apikey, bool upload)
        {
            try
            {
                //get backup path and name
                var filename = DateTime.Now.ToString("s").Replace(":", "_");
                var backupName = Constants.DatabaseBackupPath(filename);

                //get sync connection to current db
                using var mainDB = new SQLiteConnection(Constants.DatabasePath, false);

                //backup all from main to new backup
                SQLiteCommand sqlCmd = mainDB.CreateCommand($"VACUUM INTO '{backupName}'");
                sqlCmd.ExecuteNonQuery();
                mainDB.Close();

                var zipFileName = backupName + ".zip";

                //create zip file
                using (var zipFile = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                {
                    //add backup to zip
                    zipFile.CreateEntryFromFile(backupName, Constants.DatabaseFilename);
                }

                //delete new backup file, leaving zip
                File.Delete(backupName);

                if(upload)
                {
                    await ViandsService.ViandsCloudBackup(apikey, zipFileName);
                }
                else
                {
                    var fileNameSuccess = ViandsService.BackupNameUpdate(zipFileName, BackupStatusFilenames.LocalSuccess);
                    File.Move(zipFileName, fileNameSuccess);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task<bool> RestoreDatabase(string backupname)
        {
            try
            {
                //get backup path
                if (!File.Exists(backupname))
                {
                    return false;
                }

                //disconnect main connection
                await connection?.CloseAsync();

                //rename current database (to make it undoable in the future)
                if (File.Exists(Constants.DatabasePath))
                {
                    var renamepath = Path.Combine(Constants.DatabaseFolder, "_" + Constants.DatabaseFilename);
                    File.Move(Constants.DatabasePath, renamepath, true);
                }

                //extract backed up database from zip
                using var stream = File.OpenRead(backupname);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
                archive.ExtractToDirectory(Constants.DatabaseFolder, true);

                //re-init db
                await InitComponents(true);

                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }

        public void DeleteBackup(string backuppath)
        {
            try
            {
                if (File.Exists(backuppath))
                {
                    File.Delete(backuppath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task BuildTable(Type T)
        { 
            try
            {
                var result = await connection.CreateTableAsync(T);
                Debug.WriteLine(T.Name + ": " + result);
            }
            catch(Exception e)
            {
                ErrorHandler.LogError(e);
            }
        }

        public async static Task<int> Update(dynamic row)
        {
            try
            {
                await Constants.Database?.connection?.UpdateAsync(row);
                return row.id;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
            }
            return 0;
        }

        public async static Task<int> Insert(dynamic row)
        {
            try
            {
                await Constants.Database?.connection?.InsertAsync(row);
                return row.id;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
            }
            return 0;
        }

        public async static Task<int> Delete(dynamic row)
        {
            try
            {
                await Constants.Database?.connection?.DeleteAsync(row);
                return row.id;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
            }
            return 0;
        }

        public async static Task<int> GetMaxOrder(string table)
        {

            try
            {
                return await Constants.Database?.connection.ExecuteScalarAsync<int>("SELECT Max(\"order\") FROM [" + table + "] ", new object[0]);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
            }
            return 0;
            
        }
    }
}
