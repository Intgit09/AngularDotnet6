﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileuploadController : ControllerBase
    {

        [HttpPost("api/upload")]
        public async Task<IActionResult> UploadChunk()
        {
            // Get chunk information from request headers
            var chunkIndex = Request.Headers["X-Chunk-Index"];
            var totalChunks = Request.Headers["X-Total-Chunks"];
            string projectDirectory = Directory.GetCurrentDirectory();
            string uploadsDirectory = Path.Combine(projectDirectory, "Uploads");
            // Check if all chunks have been received
            if (string.IsNullOrEmpty(chunkIndex) || string.IsNullOrEmpty(totalChunks))
            {
                return BadRequest(new { error = "Invalid chunk information" });
            }

            // Retrieve the uploaded chunk from the request body
            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var chunkBytes = memoryStream.ToArray();

                // Save the chunk to a temporary location, e.g., a directory on the server
                var chunkFileName = $"{Guid.NewGuid()}.chunk";



                string filePath = Path.Combine(uploadsDirectory, chunkFileName);

                // filePath will be the complete file path relative to the current project directory
                await System.IO.File.WriteAllBytesAsync(filePath, chunkBytes);
            }

            // Check if all chunks have been received
            var currentChunkIndex = int.Parse(chunkIndex);
            var totalChunkCount = int.Parse(totalChunks);
            if (currentChunkIndex == totalChunkCount - 1)
            {
                // All chunks have been uploaded, combine the chunks into a single file
                var mergedFileName = "merged_file.ext"; // Customize the file name and extension
                string mergedFilePath = Path.Combine(uploadsDirectory, mergedFileName);

                // Get all the chunk file paths
                var chunkFiles = Directory.GetFiles(uploadsDirectory, "*.chunk");

                // Sort the chunk files based on their names
                var sortedChunkFiles = chunkFiles.OrderBy(f => f);

                // Combine the chunk files into a single file
                using (var mergedFileStream = new FileStream(mergedFilePath, FileMode.Create))
                {
                    foreach (var chunkFile in sortedChunkFiles)
                    {
                        using (var chunkFileStream = new FileStream(chunkFile, FileMode.Open))
                        {
                            await chunkFileStream.CopyToAsync(mergedFileStream);
                        }
                    }
                }

                // Delete the chunk files
                foreach (var chunkFile in sortedChunkFiles)
                {
                    System.IO.File.Delete(chunkFile);
                }

                // Perform further processing or save the merged file as needed

                return Ok(new { message = "File upload complete" });
            }

            return Ok(new { message = "Chunk uploaded successfully" });
        }


    }
}
