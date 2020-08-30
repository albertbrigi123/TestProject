using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using TestProject;

namespace Project.Controllers
{
    public class FileController : ApiController
    {
        [HttpPost]
        [Route("api/FileAPI/UploadFile")]
        public HttpResponseMessage UploadFile()
        {

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count == 0)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            HttpPostedFile postedFile = httpRequest.Files[0];
            byte[] fileData = null;
            using (BinaryReader binaryReader = new BinaryReader(postedFile.InputStream))
            {
                fileData = binaryReader.ReadBytes(postedFile.ContentLength);
            }

            //Insert the File to Database Table.
            FilesEntities fileEntities = new FilesEntities();
            DataFile file = new DataFile
            {
                Name = Path.GetFileName(postedFile.FileName),
                ContentType = postedFile.ContentType,
                Data = fileData
            };
            fileEntities.DataFiles.Add(file);
            fileEntities.SaveChanges();
            
            return Request.CreateResponse(HttpStatusCode.OK, new { id = file.id, Name = file.Name });
        }

        [HttpPost]
        [Route("api/FileAPI/GetFiles")]
        public HttpResponseMessage GetFiles()
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            FilesEntities filesEntities = new FilesEntities();
            var files = from file in filesEntities.DataFiles
                        select new { id = file.id, Name = file.Name };

            return Request.CreateResponse(HttpStatusCode.OK, files);
        }

        [HttpGet]
        [Route("api/FileAPI/GetFile")]
        public HttpResponseMessage GetFile(int fileId)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            FilesEntities filesEntities = new FilesEntities();
            DataFile file = filesEntities.DataFiles.ToList().Find(p => p.id == fileId);

            response.Content = new ByteArrayContent(file.Data);
            response.Content.Headers.ContentLength = file.Data.LongLength;

            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = file.Name;

            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            return response;
        }

        [HttpGet]
        [Route("api/FileAPI/DeleteFile")]
        public HttpResponseMessage DeleteFile(int fileId)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            FilesEntities fileEntities = new FilesEntities();

            var file = new DataFile { id = fileId };
            fileEntities.DataFiles.Attach(file);
            fileEntities.DataFiles.Remove(file);
            fileEntities.SaveChanges();

            return response;
        }

        [HttpPost]
        [Route("api/FileAPI/ChangeFileName")]
        public HttpResponseMessage ChangeFileName(String fname, String newfname)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            FilesEntities fileEntities = new FilesEntities();
            var files = fileEntities.DataFiles.Where(x => x.Name.IndexOf(fname) != -1).ToList();

            files.ForEach(m => m.Name = newfname);
            fileEntities.SaveChanges();

            return response;
        }

        [HttpPost]
        [Route("api/FileAPI/SearchFiles")]

        public HttpResponseMessage SearchFiles(String fname)
        {
            FilesEntities entities = new FilesEntities();

            var files = from file in entities.DataFiles
                        where file.Name == fname
                        select file;

            return Request.CreateResponse(HttpStatusCode.OK, files);
        }
    }
}
