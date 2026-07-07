using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Microsoft.Extensions.Logging;

namespace KAE.CMTools.Generator
{
    internal class YamlValidator
    {
        public List<YamlValidateError> ValidationErrors { get => validationErrors; }
        public JObject ValidatedDescripJson { get => validatedDescripJson; }
        public int errorShowChars { get; set; }

        public ILogger Logger { get => logger; set => logger = value; }

        public YamlValidator(Stream schemaStream, Stream descripStream)
        {
            this.schemaStream = schemaStream;
            this.descripStream = descripStream;
            this.validationErrors = new List<YamlValidateError>();
            errorShowChars = 50;
        }

        public bool Validate()
        {
            string schemaText = "";
            string descripYamlText = "";
            using (var reader = new StreamReader(schemaStream))
            {
                schemaText = reader.ReadToEnd();
            }
            using (var reader = new StreamReader(descripStream))
            {
                descripYamlText = reader.ReadToEnd();
            }

            IList<string> erros = new List<string>();

            var deserializer = new DeserializerBuilder().
                WithNamingConvention(CamelCaseNamingConvention.Instance).
                WithAttemptingUnquotedStringTypeDeserialization().
                Build();

            try
            {
                var descripYamlObject = deserializer.Deserialize<object>(descripYamlText);
                string descripJsonYaml = JsonConvert.SerializeObject(descripYamlObject);
                try
                {
                    var schemaObject = deserializer.Deserialize<object>(schemaText);
                    var schemaJsonText = JsonConvert.SerializeObject(schemaObject);

                    try
                    {
                        JSchema schema = JSchema.Parse(schemaJsonText);

                        JObject parsedDescripYamlObject = JObject.Parse(descripJsonYaml);

                        var validated = parsedDescripYamlObject.IsValid(schema, out erros);
                        if (!validated)
                        {
                            foreach (var error in erros)
                            {
                                string posStr = "position ";
                                var pos = error.LastIndexOf(posStr);
                                var periodPos = error.LastIndexOf('.');
                                var posPos = error.Substring(pos + posStr.Length, periodPos - pos - posStr.Length);
                                int errorPosition = int.Parse(posPos);
                                int showChars = errorShowChars;

                                if (descripJsonYaml.Length < errorPosition + errorShowChars)
                                {
                                    showChars = descripJsonYaml.Length - errorPosition;
                                }
                                string errorMessaget = descripJsonYaml.Substring(errorPosition, showChars);
                                validationErrors.Add(new YamlValidateError(false, error, errorMessaget, 1, errorPosition));
                            }
                        }
                        validatedDescripJson = parsedDescripYamlObject;
                    }
                    catch (Newtonsoft.Json.Schema.JSchemaReaderException ex)
                    {
                        string missingDescrip = "..." + schemaJsonText.Substring(ex.LinePosition, 100) + "...";
                        validationErrors.Add(new YamlValidateError(true, ex.Message, missingDescrip, ex.LineNumber, ex.LinePosition, ex));
                    }
                }
                catch (YamlDotNet.Core.SyntaxErrorException ex)
                {
                    validationErrors.Add(new YamlValidateError(true, ex.Message, "", ex.Start.Line, ex.Start.Column, ex));
                }
            }
            catch (YamlDotNet.Core.SyntaxErrorException ex)
            {
                validationErrors.Add(new YamlValidateError(false, ex.Message, "", ex.Start.Line, ex.Start.Column, ex));
            }
            return validationErrors.Count == 0;
        }

        public void ShowErrors()
        {
            if (this.validationErrors.Count == 0)
            {
                logger.Log( LogLevel.Information, "No problem.");
                return;
            }
            logger.LogInformation("Validation Error :");
            int no = 1;
            foreach (var error in this.ValidationErrors)
            {
                logger.LogInformation($"[{no++}] {error.Subject}");
                if (error.Line != 0 && error.Column != 0)
                {
                    logger.LogInformation($" Line:{error.Line}, Postion:{error.Column}");
                }
                if (!string.IsNullOrEmpty(error.Message))
                {
                    logger.LogInformation($" {error.Message}");
                }
                if (error.Exception != null)
                {
                    logger.LogInformation(error.Exception.ToString());
                }
            }

        }

        private Stream schemaStream;
        private Stream descripStream;
        private List<YamlValidateError> validationErrors;
        private JObject validatedDescripJson;
        private ILogger logger;

        public class YamlValidateError()
        {
            public bool IsSchema { get;set; }
            public Exception Exception { get; set; }
            public string Subject { get; set; }
            public string Message { get; set; }

            public long Line { get; set; }
            public long Column { get; set; }

            public YamlValidateError(bool isSchema, string subject, string message, long line, long column, Exception ex = null) : this()
            {
                IsSchema = isSchema;
                Exception = ex;
                Subject = subject;
                Message = message;
                Line = line;
                Column = column;
            }
        }
    }
}
