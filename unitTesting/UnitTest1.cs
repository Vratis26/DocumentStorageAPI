using DocumentStorageAPI.Controllers;
using DocumentStorageAPI.DB;
using DocumentStorageAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace unitTesting
{
    public class UnitTest1
    {
        [Fact]
        public async void PostFile()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);
            var controller = new DocumentsController(context);
            List<string> tags = new List<string>();
            tags.Add(".net");
            tags.Add("book");
            var jsonObject = new JsonObject(
                new[]
                {
                    KeyValuePair.Create<string, JsonNode?>("foo", "abcd"),
                    KeyValuePair.Create<string, JsonNode?>("some", "data"),
                }
            );

            DocumentFile document = new DocumentFile { Id = "1", Tags = tags, Data = jsonObject };
            Document documentToCompare = new Document { Id = "1", Data = jsonObject.ToString() };
            var result = await controller.Post(document);
            Assert.IsType<OkResult>(result);
            List<Tag> tagsFromDb = context.Tag.ToList();
            List<string> tagsStringFromDb = new List<string>();
            foreach(Tag tag in tagsFromDb)
            {
                tagsStringFromDb.Add(tag.Name);
            }
            Assert.Equal(tagsStringFromDb, tags);

            Document? fromDb = context.Document.FirstOrDefault(d => d.Id == "1");
            Assert.Equal(fromDb.Id, documentToCompare.Id);
            Assert.Equal(fromDb.Data, documentToCompare.Data);
        }

        [Fact]
        public async void PutFileOk()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);

            context.Document.Add(new Document { Id = "1", Data = "{\"some\":\"data\"}" });
            context.Tag.Add(new Tag { Id = "1", Name = "abcd" });
            await context.SaveChangesAsync();

            var controller = new DocumentsController(context);
            List<string> tags = new List<string>();
            tags.Add("book");
            tags.Add(".net");
            var jsonObject = new JsonObject(
                new[]
                {
                    KeyValuePair.Create<string, JsonNode?>("foo", "abcd"),
                    KeyValuePair.Create<string, JsonNode?>("some", "data"),
                }
            );

            DocumentFile document = new DocumentFile { Id = "1", Tags = tags, Data = jsonObject };
            var result = await controller.Put("1", document);
            Assert.IsType<OkResult>(result);
            Document documentToCompare = new Document { Id = "1", Data = jsonObject.ToString() };
            List<Tag> tagsFromDb = context.Tag.ToList();
            List<string> tagsStringFromDb = new List<string>();
            foreach (Tag tag in tagsFromDb)
            {
                Assert.Contains(tag.Name, tags);
            }

            Document? fromDb = context.Document.FirstOrDefault(d => d.Id == "1");
            Assert.Equal(fromDb.Id, documentToCompare.Id);
            Assert.Equal(fromDb.Data, documentToCompare.Data);
        }

        [Fact]
        public async void PutFileBadRequest()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);

            context.Document.Add(new Document { Id = "2", Data = "{\"some\":\"data\"}" });
            context.Tag.Add(new Tag { Id = "2", Name = "abcd" });
            await context.SaveChangesAsync();

            var controller = new DocumentsController(context);
            List<string> tags = new List<string>();
            tags.Add("book");
            tags.Add(".net");
            var jsonObject = new JsonObject(
                new[]
                {
                    KeyValuePair.Create<string, JsonNode?>("foo", "abcd"),
                    KeyValuePair.Create<string, JsonNode?>("some", "data"),
                }
            );

            DocumentFile document = new DocumentFile { Id = "1", Tags = tags, Data = jsonObject };
            var result = await controller.Put("1", document);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void GetFileJson()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);

            context.Document.Add(new Document { Id = "2", Data = "{\"some\":\"data\"}" });
            context.Tag.Add(new Tag { Id = "2", Name = "abcd" });
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Accept"] = "application/json";

            var controller = new DocumentsController(context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext,
                }
            };

            var result = controller.Get("2").Result as ObjectResult;
            Assert.Equal(result.StatusCode, 200);
            Assert.Equal(result.Value, "{\"Id\":\"2\",\"Tags\":[\"abcd\"],\"Data\":{\"some\":\"data\"}}");
        }

        [Fact]
        public async void GetFileXml()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);

            context.Document.Add(new Document { Id = "2", Data = "{\"some\":\"data\"}" });
            context.Tag.Add(new Tag { Id = "2", Name = "abcd" });
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Accept"] = "application/xml";

            var controller = new DocumentsController(context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext,
                }
            };

            var result = controller.Get("2").Result as ObjectResult;
            Assert.Equal(result.StatusCode, 200);
            Assert.Equal(result.Value, "<document><Id>2</Id><Tags>abcd</Tags><Data><some>data</some></Data></document>");
        }

        [Fact]
        public async void GetFileMessagePack()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);

            context.Document.Add(new Document { Id = "2", Data = "{\"some\":\"data\"}" });
            context.Tag.Add(new Tag { Id = "2", Name = "abcd" });
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Accept"] = "application/messagepack";

            var controller = new DocumentsController(context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext,
                }
            };

            var result = controller.Get("2").Result as ObjectResult;
            Assert.Equal(result.StatusCode, 200);
            byte[] toCompare = new byte[] { 131, 162, 73, 100, 161, 50, 164, 84, 97, 103, 115, 145, 164, 97, 98, 99, 100, 164, 68, 97, 116, 97, 129, 164, 115, 111, 109, 101, 164, 100, 97, 116, 97 };
            Assert.Equal(result.Value, toCompare);
        }

        [Fact]
        public async void GetFileBadAcceptHeader()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);

            context.Document.Add(new Document { Id = "2", Data = "{\"some\":\"data\"}" });
            context.Tag.Add(new Tag { Id = "2", Name = "abcd" });
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Accept"] = "application/text";

            var controller = new DocumentsController(context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext,
                }
            };

            var result = controller.Get("2").Result as StatusCodeResult;
            Assert.Equal(result.StatusCode, 415);
        }

        [Fact]
        public async void GetFileNotFound()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "Documents");
            var context = new DocumentDbContext(optionsBuilder.Options);

            context.Document.Add(new Document { Id = "2", Data = "{\"some\":\"data\"}" });
            context.Tag.Add(new Tag { Id = "2", Name = "abcd" });
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Accept"] = "application/json";

            var controller = new DocumentsController(context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext,
                }
            };

            var result = controller.Get("1").Result as ObjectResult;
            Assert.Equal(result.StatusCode, 404);
        }
    }
}