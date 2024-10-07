namespace Storage.Repositories
{
    public class AzureStorageSettings
    {
        public AzureStorageSettings()
        {
        }

        public AzureStorageSettings(string connectionString, string containerName, string shareName)
        {
            ConnectionString = connectionString;
            ContainerName = containerName;
            ShareName = shareName;
        }

        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string ShareName { get; set; }

    }
}