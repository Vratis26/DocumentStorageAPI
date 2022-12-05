using DocumentStorageAPI.DB;
using DocumentStorageAPI.Models;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static DocumentStorageAPI.Models.Definitions;

namespace DocumentStorageAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DocumentsController : Controller
    {
        private DocumentDbContext _db;

        public DocumentsController(DocumentDbContext db)
        {
            _db = db;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                string contentType = this.Request.Headers.Accept;
                if (contentType == null)
                    return StatusCode(415);

                DocumentOutputType documentType;
                switch (contentType)
                {
                    case "application/xml":
                        documentType = DocumentOutputType.XML;
                        break;
                    case "application/json":
                        documentType = DocumentOutputType.JSON;
                        break;
                    case "application/messagepack":
                        documentType = DocumentOutputType.MESSAGEPACK;
                        break;
                    default:
                        documentType = DocumentOutputType.NONE;
                        break;
                }

                if (documentType == DocumentOutputType.NONE)
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType);

                Document? document = await _db.Document.FirstOrDefaultAsync(d => d.Id == id);
                if (document == null)
                    return NotFound(id);

                List<Tag> tags = await _db.Tag.Where(t => t.Id == id).ToListAsync();

                DocumentFileGet documentFile = new DocumentFileGet { Id = id, Data = JsonConvert.DeserializeObject<JObject>(document.Data), Tags = new List<string>() };
                foreach (Tag tag in tags)
                {
                    documentFile.Tags.Add(tag.Name);
                }

                string json = JsonConvert.SerializeObject(documentFile);

                switch (documentType)
                {
                    case DocumentOutputType.JSON:
                        return Ok(json);
                        break;
                    case DocumentOutputType.XML:
                        XmlDocument xml = (XmlDocument)JsonConvert.DeserializeXmlNode(json, "document");
                        return Ok(xml.InnerXml);
                    case DocumentOutputType.MESSAGEPACK:
                        
                        var mp = MessagePackSerializer.ConvertFromJson(json);
                        return Ok(mp);
                }
            }
            catch(Exception ex) { }
            

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DocumentFile json)
        {
            try
            {
                _db.Document.Add(new Document { Id = json.Id, Data = json.Data.ToString() });
                foreach (string tag in json.Tags)
                {
                    _db.Tag.Add(new Tag { Id = json.Id, Name = tag });
                }

                await _db.SaveChangesAsync();

                return Ok();
            }catch(Exception ex) { }

            return BadRequest();
        }
        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> Put(string id, [FromBody] DocumentFile json)
        {
            try
            {
                Document? document = await _db.Document.FirstOrDefaultAsync(d => d.Id == id);
                if (document == null)
                    return NotFound(id);

                List<Tag> tags = await _db.Tag.Where(t => t.Id == id).ToListAsync();
                document.Data = json.Data.ToString();
                foreach(Tag tag in tags)
                {
                    var tmp = json.Tags.FirstOrDefault(t => t == tag.Name);
                    if (tmp == null)
                        _db.Tag.Remove(tag);
                    else
                        json.Tags.Remove(tmp);
                }

                foreach(string tag in json.Tags)
                {
                    _db.Add(new Tag { Id = id, Name = tag });
                }
                _db.SaveChanges();

                return Ok();
            }catch(Exception ex) { }
            return BadRequest();
        }
    }
}
