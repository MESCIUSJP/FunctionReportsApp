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
            //�N�G���p�����[�^���擾
            string datajson = req.Query["datajson"];

            //���N�G�X�g�{�f�B���擾
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            // �N�G���p�����[�^����̏ꍇ�A���N�G�X�g�{�f�B��datajson���g�p  
            if (datajson == null && data != null)
            {
                // datajson�����݂��邩�m�F���A������ɕϊ�����  
                datajson = JsonConvert.SerializeObject(data.datajson);
            }

            //ActionResult��ݒ�
            ActionResult result;

            try
            {
                //�A�Z���u���擾
                var assembly = Assembly.GetExecutingAssembly();

                //���|�[�g�t�@�C���̃X�g���[�����擾
                var stream = assembly.GetManifestResourceStream("FunctionReportsApp.Invoice_bluegray.rdlx");
                var streamReader = new StreamReader(stream);

                //���|�[�g�t�@�C����ǂݍ���
                GrapeCity.ActiveReports.PageReport pageReport = new PageReport(streamReader);
                //�t�H���g�̐ݒ�
                pageReport.FontResolver = new EmbeddedFontResolver();

                //���|�[�g�Ƀp�����[�^��ݒ�
                pageReport.Document.Parameters[0].Values[0].Value = datajson;

                //PDF�o�͐ݒ�
                GrapeCity.ActiveReports.Export.Pdf.Page.Settings pdfSetting = new GrapeCity.ActiveReports.Export.Pdf.Page.Settings();
                GrapeCity.ActiveReports.Export.Pdf.Page.PdfRenderingExtension pdfRenderingExtension = new GrapeCity.ActiveReports.Export.Pdf.Page.PdfRenderingExtension();
                GrapeCity.ActiveReports.Rendering.IO.MemoryStreamProvider outputProvider = new GrapeCity.ActiveReports.Rendering.IO.MemoryStreamProvider();

                //PDF�����_�����O
                pageReport.Document.Render(pdfRenderingExtension, outputProvider, pdfSetting);

                //PDF�t�@�C����MemoryStream�ɕϊ�
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                outputProvider.GetPrimaryStream().OpenStream().CopyTo(ms);

                //ActionResult��PDF�t�@�C����ݒ�
                result = new FileContentResult(ms.ToArray(), "application/pdf")
                {
                    FileDownloadName = "report.pdf"
                };
            }
            catch (Exception ex)    //�G���[����
            {
                //�G���[���̓X�e�[�^�X�R�[�h500��Ԃ�
                result = new StatusCodeResult(500);

                //�G���[���O���o��
                _logger.LogInformation(ex.Message);
            }

            return result;
        }
    }
}

