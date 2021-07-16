using Azure;
using Azure.Storage.Blobs;
using Lab5.Data;
using Lab5.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.Controllers
{
    public class AnswerImages : Controller
    {
        private readonly AnswerImageDataContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string earthContainerName = "earthimages";
        private readonly string computerContainerName = "computerimages";
        private string containerName;

        public AnswerImages(AnswerImageDataContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.AnswerImages.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile answerImage)
        {

            BlobContainerClient containerClient;
            try
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
                containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
            }
            catch (RequestFailedException)
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }

            try
            {
                var blockBlob = containerClient.GetBlobClient(answerImage.FileName);
                if (await blockBlob.ExistsAsync())
                {
                    await blockBlob.DeleteAsync();
                }

                using (var memoryStream = new MemoryStream())
                {
                    await answerImage.CopyToAsync(memoryStream);

                    memoryStream.Position = 0;

                    await blockBlob.UploadAsync(memoryStream);
                    memoryStream.Close();
                }

                var image = new AnswerImage();
                image.Url = blockBlob.Uri.AbsoluteUri;
                image.FileName = answerImage.FileName;

                _context.AnswerImages.Add(image);
                _context.SaveChanges();
            }
            catch (RequestFailedException)
            {
                View("Error");
            }

            return RedirectToAction("Index");
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.AnswerImages
                .FirstOrDefaultAsync(m => m.AnswerImageId == id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _context.AnswerImages.FindAsync(id);


            BlobContainerClient containerClient;
            try
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            try
            {
              
                var blockBlob = containerClient.GetBlobClient(image.FileName);
                if (await blockBlob.ExistsAsync())
                {
                    await blockBlob.DeleteAsync();
                }

                _context.AnswerImages.Remove(image);
                await _context.SaveChangesAsync();

            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            return RedirectToAction("Index");
        }
    }
}