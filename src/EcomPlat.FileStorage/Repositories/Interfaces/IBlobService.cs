using Azure.Storage.Blobs;

namespace EcomPlat.FileStorage.Repositories.Interfaces
{
    public interface IBlobService
    {
        string BlobPrefix { get; }

        BlobContainerClient? GetContainerReference(string containerName);
    }
}