using System;
using System.IO;
using Newtonsoft.Json;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Core.Persistence
{
    public class Serialize
    {
        private readonly string mBasePath;
        private readonly string mSaveFilePath;

        public Serialize(string gameName = "Rache_der_RETI")
        {
            mBasePath = Path.Combine(Globals.mAppDataFilePath, gameName);
            mSaveFilePath = Path.Combine(mBasePath, "JSON");
            CreateSaveFolders();
        }

        private void CreateSaveFolders()
        {
            CreateFolder(mBasePath);
            CreateFolder(mSaveFilePath);
        }

        private static void CreateFolder(string folderPath)
        {
            DirectoryInfo createFDirectoryInfo = new DirectoryInfo(folderPath);
            if (createFDirectoryInfo.Exists)
            {
                return;
            }
            createFDirectoryInfo.Create();
        }
        public void DeleteFile(string fileName)
        {
            var filePath = Path.Combine(mSaveFilePath, fileName + ".json");
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("An IO exception has occurred while attempting to delete the file:");
                Console.WriteLine(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("You do not have permission to delete the file:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception has occurred while attempting to delete the file:");
                Console.WriteLine(ex.Message);
            }
        }
        public void SerializeObject(object obj, string fileName)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            var objectJson = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            File.WriteAllText(Path.Combine(mSaveFilePath, fileName + ".json"), objectJson);
        }

        public object PopulateObject(object obj, string fileName, JsonSerializerSettings settings)
        {
            var filePath = Path.Combine(mSaveFilePath, fileName + ".json");
            if (!File.Exists(filePath))
            {
                return null;
            }
            using StreamReader r = new StreamReader(filePath);
            string json = r.ReadToEnd();
            JsonConvert.PopulateObject(json, obj, settings);
            return obj;
        }

        public object DeserializeObject(Type objectType, string fileName)
        {
            var filePath = Path.Combine(mSaveFilePath, fileName + ".json");
            if (!File.Exists(filePath))
            {
                return null;
            }
            using var file = File.OpenText(filePath);
            JsonSerializer serializer = new JsonSerializer();
            var loadedObject = serializer.Deserialize(file, objectType);
            return loadedObject;
        }


    }
}
