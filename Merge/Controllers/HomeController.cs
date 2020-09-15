using Pdftools.Pdf;
using Pdftools.PdfSplMrg;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace Merge.Controllers
{
    public class HomeController : Controller
    {

        private string license = ConfigurationManager.AppSettings["outdoc:license"];

        private PDFCopyOptions copyOptions = PDFCopyOptions.ePdfCopyAssociatedFiles |
                                            PDFCopyOptions.ePdfCopyAssociatedFiles |
                                            PDFCopyOptions.ePdfCopyFormFields |
                                            PDFCopyOptions.ePdfCopyLinks |
                                            PDFCopyOptions.ePdfCopyLogicalStructure |
                                            PDFCopyOptions.ePdfCopyNamedDestinations |
                                            PDFCopyOptions.ePdfCopyOutlines |
                                            PDFCopyOptions.ePdfMergeOCGs;
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload()
        {

            var files = new List<string>();
            var resultFileName = "MergedFile.pdf";
            var downloadPath = string.Empty;
            byte[] RestResultBytes;

            if (Request.Files.Count > 1)
            {
                resultFileName = (Guid.NewGuid()).ToString() + "_merged.pdf";//Request.Files[0].FileName.Replace(".pdf","");
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = (Guid.NewGuid()).ToString() + ".pdf";
                        var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);
                        file.SaveAs(path);
                        files.Add(path);
                    }

                }

                OutDoc.SetLicenseKey(license);
                using (OutDoc output = new OutDoc())
                {
                    if (!output.CreateInMemory())
                    {
                        return Json(new { success = false, message = "Server Error" });
                    }

                    foreach (var file in files)
                    {
                        try
                        {
                            using (InDoc input1 = new InDoc())
                            {
                                if (!input1.Open(file, string.Empty))
                                {
                                    JsonResult res = null;
                                    if (input1.ErrorCode == PDFErrorCode.PDF_E_PASSWORD)
                                    {
                                        res = new JsonResult()
                                        {
                                            Data = new { code = "pass" }
                                        };

                                    }

                                    // TODO some exeption
                                    input1.Close();
                                    output.Close();

                                    return res;
                                }

                                // Set the default document attributes to input 1
                                output.CopyAttributes(input1);
                                output.CopyPages2(input1, 1, -1, copyOptions);
                                input1.Close();
                            }
                        }
                        finally
                        {
                            System.IO.File.Delete(file);
                        }
                    }

                    output.Title = "MergedFile";
                    RestResultBytes = output.GetPdf();
                    output.Close();
                }

                downloadPath = Path.Combine(Server.MapPath("~/Downloads/"), resultFileName);
                System.IO.File.WriteAllBytes(downloadPath, RestResultBytes);
                return Json(new { success = true, fileName = resultFileName });
            }
            return Json(new { success = false, error="filecount" });

        }

        [HttpPost]
        public ActionResult download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return View("Error");

           
            var path = Path.Combine(Server.MapPath("~/Downloads/"), fileName);
            if (System.IO.File.Exists(path))
            {
                var file = System.IO.File.ReadAllBytes(path);
                return File(file, MediaTypeNames.Application.Pdf, "Download File.pdf");
                //return File(path, MediaTypeNames.Application.Pdf,"MergedFile.pdf");
            }

            return View("Error");
        }
    }
}