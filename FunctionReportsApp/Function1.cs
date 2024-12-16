using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Extensibility.Rendering;
using GrapeCity.ActiveReports.Rendering.IO;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using static GrapeCity.Enterprise.Data.DataEngine.DataProcessing.DataProcessor;
using System.Reflection;
using System;

namespace FunctionReportsApp
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> log)
        {
            _logger = log;
        }

        [FunctionName("Function1")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "datajson" }, Summary = "Run function", Description = "This function processes a request and returns a PDF.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "datajson", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **DataJson** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/pdf", bodyType: typeof(byte[]), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            //クエリパラメータを取得
            string datajson = req.Query["datajson"];

            //リクエストボディを取得
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            // クエリパラメータが空の場合、リクエストボディのdatajsonを使用  
            if (datajson == null && data != null)
            {
                // datajsonが存在するか確認し、文字列に変換する  
                datajson = JsonConvert.SerializeObject(data.datajson);
            }

            //ActionResultを設定
            ActionResult result;

            try
            {
                //アセンブリ取得
                var assembly = Assembly.GetExecutingAssembly();

                //レポートファイルのストリームを取得
                var stream = assembly.GetManifestResourceStream("FunctionReportsApp.Invoice_bluegray.rdlx");
                var streamReader = new StreamReader(stream);

                //レポートファイルを読み込み
                GrapeCity.ActiveReports.PageReport pageReport = new PageReport(streamReader);
                //フォントの設定
                pageReport.FontResolver = new EmbeddedFontResolver();

                //レポートにパラメータを設定
                pageReport.Document.Parameters[0].Values[0].Value = datajson;

                //PDF出力設定
                GrapeCity.ActiveReports.Export.Pdf.Page.Settings pdfSetting = new GrapeCity.ActiveReports.Export.Pdf.Page.Settings();
                GrapeCity.ActiveReports.Export.Pdf.Page.PdfRenderingExtension pdfRenderingExtension = new GrapeCity.ActiveReports.Export.Pdf.Page.PdfRenderingExtension();
                GrapeCity.ActiveReports.Rendering.IO.MemoryStreamProvider outputProvider = new GrapeCity.ActiveReports.Rendering.IO.MemoryStreamProvider();

                //PDFレンダリング
                pageReport.Document.Render(pdfRenderingExtension, outputProvider, pdfSetting);

                //PDFファイルをMemoryStreamに変換
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                outputProvider.GetPrimaryStream().OpenStream().CopyTo(ms);

                //ActionResultにPDFファイルを設定
                result = new FileContentResult(ms.ToArray(), "application/pdf")
                {
                    FileDownloadName = "report.pdf"
                };
            }
            catch (Exception ex)    //エラー処理
            {
                //エラー時はステータスコード500を返す
                result = new StatusCodeResult(500);

                //エラーログを出力
                _logger.LogInformation(ex.Message);
            }

            return result;
        }
    }
}

