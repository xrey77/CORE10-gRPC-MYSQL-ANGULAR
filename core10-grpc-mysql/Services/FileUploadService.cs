//Services/FileUploadService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;

public class FileUploadService : global::Fileupload.FileUploadService.FileUploadServiceBase 
{
    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public override async Task<global::Fileupload.UploadImageResponse> UploadImage(
        IAsyncStreamReader<global::Fileupload.UploadImageRequest> requestStream, 
        ServerCallContext context)
    {
        string fileName = string.Empty;
        string userFolder = "users";
        string saveDirectory = Path.Combine(_env.WebRootPath, userFolder);

        // Ensure the wwwroot/users directory exists
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        string fullFilePath = string.Empty;
        FileStream fileStream = null;

        try
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                if (fileStream == null)
                {
                    fileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.FileName)}"; // Security: Unique filename
                    fullFilePath = Path.Combine(saveDirectory, fileName);
                    fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write);
                }

                if (request.ChunkData != null)
                {
                    // Write the binary payload to the wwwroot file stream
                    var chunkBytes = request.ChunkData.ToByteArray();
                    await fileStream.WriteAsync(chunkBytes, 0, chunkBytes.Length);
                }
            }

            if (fileStream != null)
            {
                await fileStream.FlushAsync();
            }

            return new global::Fileupload.UploadImageResponse 
            {                 
                Message = "Image uploaded successfully.", 
                FileUrl = $"/{userFolder}/{fileName}" 
            };
        }
        catch (Exception ex)
        {
            return new global::Fileupload.UploadImageResponse 
            { 
                Success = false, 
                Message = $"Error uploading image: {ex.Message}" 
            };
        }
        finally
        {
            if (fileStream != null)
            {
                await fileStream.DisposeAsync();
            }
        }
    }
}
