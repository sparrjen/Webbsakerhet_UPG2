using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Webbsäkerhet_UPG2.Data;
using Webbsäkerhet_UPG2.Models;
using Webbsäkerhet_UPG2.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;
using System.Net.Mime;
using System.Diagnostics;

namespace Webbsäkerhet_UPG2.Controllers
{
    public class ForumFilesController : Controller
    {
        private readonly AppDbContext Db;
        private readonly long fileSizeLimit = 10 * 1048576;
        private readonly string[] permittedExtensions = { ".jpg", ".png", ".pdf" };


        public ForumFilesController(AppDbContext context)
        {
            Db = context;
        }

        // GET: ForumFiles
        public async Task<IActionResult> Index()
        {
            return View(await Db.ForumFile.ToListAsync());
        }

        [HttpPost]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile()
        {
            var theWebRequest = HttpContext.Request;

            // validation of Content-Type
            // 1. first, it must be a form-data request
            // 2. a boundary should be found in the Content-Type
            if (!theWebRequest.HasFormContentType ||
            !MediaTypeHeaderValue.TryParse(theWebRequest.ContentType, out var theMediaTypeHeader) ||
            string.IsNullOrEmpty(theMediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(theMediaTypeHeader.Boundary.Value, theWebRequest.Body);
            var section = await reader.ReadNextSectionAsync();

            // This sample try to get the first file from request and save it
            // Make changes according to your needs in actual use
            while (section != null)
            {
                var DoesItHaveContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                out var theContentDisposition);


                if (DoesItHaveContentDispositionHeader && theContentDisposition.DispositionType.Equals("form-data") &&
                !string.IsNullOrEmpty(theContentDisposition.FileName.Value))
                {
                    // Don't trust any file name, file extension, and file data from the request unless you trust them completely
                    // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
                    // In short, it is necessary to restrict and verify the upload
                    // Here, we just use the temporary folder and a random file name

                    ForumFile forumFile = new ForumFile();
                    forumFile.UntrustedName = HttpUtility.HtmlEncode(theContentDisposition.FileName.Value);
                    forumFile.TimeStamp = DateTime.UtcNow;


                    forumFile.Content =
                    await FileHelpers.ProcessStreamedFile(section, theContentDisposition,
                    ModelState, permittedExtensions, fileSizeLimit);
                    if (forumFile.Content.Length == 0)
                    {
                        return RedirectToAction("Error");
                    }
                    forumFile.Size = forumFile.Content.Length;

                    await Db.ForumFile.AddAsync(forumFile);
                    await Db.SaveChangesAsync();

                    return RedirectToAction("Index", "ForumFiles");
                }
                section = await reader.ReadNextSectionAsync();
            }

            // If the code runs to this location, it means that no files have been saved
            return BadRequest("No files data in the request.");
        }

        // GET: ForumFiles/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var forumFile = await Db.ForumFile
            .FirstOrDefaultAsync(m => m.Id == id);
            if (forumFile == null)
            {
                return NotFound();
            }
            return File(forumFile.Content, MediaTypeNames.Application.Octet, forumFile.UntrustedName);
        }

        // GET: ApplicationFiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var forumFile = await Db.ForumFile
            .FirstOrDefaultAsync(m => m.Id == id);
            if (forumFile == null)
            {
                return NotFound();
            }
            return View(forumFile);
        }

        // POST: ApplicationFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forumFile = await Db.ForumFile.FindAsync(id);
            Db.ForumFile.Remove(forumFile);
            await Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ForumFileExists(int id)
        {
            return Db.ForumFile.Any(e => e.Id == id);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
