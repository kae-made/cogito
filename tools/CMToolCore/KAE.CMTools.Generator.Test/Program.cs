// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// See https://aka.ms/new-console-template for more information

using KAE.CMTools.Core;
using KAE.CMTools.Generator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

string schemaFilePath = args[0];
string descripFilePath = args[1];
string targetDomainName = args[2];
string generatedSchemaFilePath = "";
List<string> fosInstModelPaths = new List<string>();

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

if (args.Length> 3)
{
    generatedSchemaFilePath = args[3];
    if (args.Length > 4)
    {
        for(int i=4; i<args.Length; i++)
        {
            fosInstModelPaths.Add(args[i]);
        }
    }
}

using (var formatStream = File.OpenRead(schemaFilePath))
{
    using (var schemaStream = File.OpenRead(descripFilePath))
    {
        var reader = new YamlSchemaReader() { Logger = logger };
        try
        {
            try
            {
                logger.LogInformation($"Reading Conceptual Model");
                logger.LogInformation($" Schema : {schemaFilePath}");
                logger.LogInformation($" Model  : {descripFilePath}");
                reader.Read(formatStream, schemaStream);
                logger.LogInformation("Reading Done.");

                var repository = new InstanceRepository();
                try
                {
                    logger.LogInformation("Parsing Conceptual Model...");
                    reader.Parse(repository);
                    logger.LogInformation("Parsing Done.");
                    logger.LogInformation("Validating Conceptual Model...");
                    var validatedResult = reader.Validate();
                    if (validatedResult)
                    {
                        logger.LogInformation("Validating Done.");

                        if (!string.IsNullOrEmpty(generatedSchemaFilePath))
                        {
                            if (!File.Exists(generatedSchemaFilePath))
                            {
                                logger.LogInformation($"Generating YAML Schema for Instance Model...");
                                using (var outputStream = File.OpenWrite(generatedSchemaFilePath))
                                {
                                    using (var writer = new StreamWriter(outputStream))
                                    {
                                        var generator = new DomainSchemaGenerator() { Logger = logger };
                                        generator.Generate(targetDomainName, repository, writer);
                                    }
                                }
                            }
                            foreach (var fosStateFilePath in fosInstModelPaths)
                            {
                                if (!string.IsNullOrEmpty(fosStateFilePath))
                                {
                                    using (var fosSchemaStream = File.OpenRead(generatedSchemaFilePath))
                                    {
                                        using (var fosDescripStream = File.OpenRead(fosStateFilePath))
                                        {
                                            var fosReader = new YamlFieldOfSenseReader() { Logger = logger };
                                            logger.LogInformation($"Reading Instance Model...");
                                            logger.LogInformation($"  - Schema :         {generatedSchemaFilePath}");
                                            logger.LogInformation($"  - Instance Model : {fosStateFilePath}");
                                            if (fosReader.Read(fosSchemaStream, fosDescripStream))
                                            {
                                                logger.LogInformation("Reading Done.");
                                                logger.LogInformation("Parsing Instance Model...");
                                                fosReader.Parse(repository);
                                                logger.LogInformation("Parsing Done.");

                                                logger.LogInformation("Validating Instance Model...");
                                                if (fosReader.Validate())
                                                {
                                                    logger.LogInformation("Validating Done.");
                                                }
                                                else
                                                {
                                                    logger.LogInformation("Validation Failed.");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ;
                }
            }
            catch(YamlSyntaxValidationException ex)
            {
                logger.LogInformation($"Validation Error : {ex.YamlKind.ToString()}");
                Console.Write(ex.Message);
                var innerError = ((SyntaxErrorException)(ex.innerException));
                logger.LogInformation($" - Line:{innerError.Start.Line}, Column: {innerError.Start.Column}");
            }
        }
        catch (YamlDotNet.Core.SemanticErrorException ex)
        {
            logger.LogInformation(ex.Message);
            var mark = ex.Start;
            logger.LogInformation($"Index[{mark.Index}] Line:{mark.Line}, Column:{mark.Column}");
        }
        catch (YamlDotNet.Core.SyntaxErrorException ex)
        {
            logger.LogInformation(ex.Message);
            var mark = ex.Start;
            logger.LogInformation($"[{mark.Index}] Line:{mark.Line}, Column:{mark.Column}");

        }
        catch (Newtonsoft.Json.JsonReaderException ex)
        {
            logger.LogInformation(ex.Message);
            foreach(var d in ex.Data)
            {
                
            }

            logger.LogInformation($"Line:{ex.LineNumber}, Position:{ex.LinePosition}");
            
        }
        catch (Newtonsoft.Json.Schema.JSchemaReaderException ex)
        {
            logger.LogInformation($"For '{args[0]}'");
            logger.LogInformation(ex.Message);
            logger.LogInformation($"Line:{ex.LineNumber}, Position:{ex.LinePosition}");
            ShowErrorLineAndPos(args[0], ex.LinePosition, 100);
        }
        catch (YamlSchemaValidationException ex)
        {
            logger.LogInformation(ex.Message);
            foreach (var error in ex.Errors)
            {
                logger.LogInformation(error.Message);
                logger.LogInformation(error.JsonText);
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex.Message);
        }
    }
}


void ShowErrorLineAndPos(string filePath, int position, int length)
{
    using( var stream = File.OpenRead(filePath))
    {
        using (var reader = new StreamReader(stream))
        {
            var deserializer = new DeserializerBuilder().
                WithNamingConvention(CamelCaseNamingConvention.Instance).
                WithAttemptingUnquotedStringTypeDeserialization().
                Build();

            var formatText = reader.ReadToEnd();

            var formatObject = deserializer.Deserialize<object>(formatText);
            var formatJsonText = JsonConvert.SerializeObject(formatObject);

            logger.LogInformation($"{formatJsonText.Substring(position, length)}");
        }
    }
}