using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;
using HttpDeleteAttribute = Microsoft.AspNetCore.Mvc.HttpDeleteAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using HttpPutAttribute = Microsoft.AspNetCore.Mvc.HttpPutAttribute;

namespace WindowsService_HostAPI
{
    public class ValuesController : ApiController
    {
        [HttpGet]
        public JsonResult Get(Guid id)
        {
            DirToCheck dir = SelfHostService.dirToCheckList.Find(x => x.Id == id);
            if (dir != null)
            {
                if (dir.inProccess)
                    return new JsonResult("In process");
                else
                    return new JsonResult(dir);
            }
            return new JsonResult("Wasn't found");
        }

        public class DirRequest
        {
            public string path;
            public DateTime time;
        }

        [HttpPost]
        public JsonResult Post([FromBody] DirRequest path)
        {
            string filePath = "";
            try
            {
                filePath = Environment.ExpandEnvironmentVariables(path.path);
            }
            catch
            {
                return new JsonResult(path);
            }
            if (!Directory.Exists(filePath))
                return new JsonResult("Path doesn't exists");
            DirToCheck dir = new DirToCheck(Guid.NewGuid(), filePath);
            SelfHostService.dirToCheckList.Add(dir);
            Check(dir);
            return new JsonResult(dir.Id);
        }

        [HttpPut]
        public JsonResult Put()
        {
            return new JsonResult("Put request");
        }

        [HttpDelete]
        public JsonResult Delete(Guid id)
        {
            return new JsonResult("Delete request");
        }

        async void Check(DirToCheck directorie)
        {
            await Task.Run(() =>
            {
                string filePath = directorie.DirectoryRoute;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Tuple<List<string>, int> thisDir = GetFiles(directorie.DirectoryRoute, 0);
                directorie.NumFiles = thisDir.Item1.Count() + thisDir.Item2;
                foreach (string file in thisDir.Item1)
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        FileInfo fInfo = new FileInfo(file);
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Contains(@"%userprofile%\Documents"))
                            {
                                directorie.RmDetects++;
                                break;
                            }
                            if (line.Contains("Rundl32 sus.dll SusEntry"))
                            {
                                directorie.Rundll32Detects++;
                                break;
                            }
                            if (fInfo.Extension == ".js" && line.Contains("<script>evil_script()</script>"))
                            {
                                directorie.JsDetects++;
                                break;
                            }
                        }
                    }
                }
                stopwatch.Stop();
                directorie.inProccess = false;
                directorie.TimeProcessing = stopwatch.Elapsed;
            });
        }

        Tuple<List<string>, int> GetFiles(string path, int errors)
        {
            var files = new List<string>();

            try
            {
                files.AddRange(Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly));
                foreach (var directory in Directory.GetDirectories(path))
                {
                    Tuple<List<string>, int> thisDir = GetFiles(directory, errors);
                    files.AddRange(thisDir.Item1);
                    errors += thisDir.Item2;
                }
            }
            catch (UnauthorizedAccessException)
            {
                errors++;
            }

            return Tuple.Create(files, errors);
        }
    }
}