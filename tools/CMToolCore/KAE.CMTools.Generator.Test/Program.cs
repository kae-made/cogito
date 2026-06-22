// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// See https://aka.ms/new-console-template for more information

using KAE.CMTools.Core;
using KAE.CMTools.Generator;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using (var formatStream = File.OpenRead(args[0]))
{
    using (var schemaStream = File.OpenRead(args[1]))
    {
        var reader = new YamlSchemaReader();
        try
        {
            try
            {
                reader.Read(formatStream, schemaStream);

                var repository = new InstanceRepository();
                try
                {
                    reader.Parse(repository);
                    var validatedResult = reader.Validate();
                }
                catch (Exception ex)
                {
                    ;
                }
            }
            catch(YamlSyntaxValidationException ex)
            {
                Console.WriteLine($"Validation Error : {ex.YamlKind.ToString()}");
                Console.Write(ex.Message);
                var innerError = ((SyntaxErrorException)(ex.innerException));
                Console.WriteLine($" - Line:{innerError.Start.Line}, Column: {innerError.Start.Column}");
            }
        }
        catch (YamlDotNet.Core.SemanticErrorException ex)
        {
            Console.WriteLine(ex.Message);
            var mark = ex.Start;
            Console.WriteLine($"Index[{mark.Index}] Line:{mark.Line}, Column:{mark.Column}");
        }
        catch (YamlDotNet.Core.SyntaxErrorException ex)
        {
            Console.WriteLine(ex.Message);
            var mark = ex.Start;
            Console.WriteLine($"[{mark.Index}] Line:{mark.Line}, Column:{mark.Column}");

        }
        catch (Newtonsoft.Json.JsonReaderException ex)
        {
            Console.WriteLine(ex.Message);
            foreach(var d in ex.Data)
            {
                
            }

            Console.WriteLine($"Line:{ex.LineNumber}, Position:{ex.LinePosition}");
            
        }
        catch (Newtonsoft.Json.Schema.JSchemaReaderException ex)
        {
            Console.WriteLine($"For '{args[0]}'");
            Console.WriteLine(ex.Message);
            Console.WriteLine($"Line:{ex.LineNumber}, Position:{ex.LinePosition}");
            ShowErrorLineAndPos(args[0], ex.LinePosition, 100);
        }
        catch (YamlSchemaValidationException ex)
        {
            Console.WriteLine(ex.Message);
            foreach (var error in ex.Errors)
            {
                Console.WriteLine(error.Message);
                Console.WriteLine(error.JsonText);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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

            Console.WriteLine($"{formatJsonText.Substring(position, length)}");
        }
    }
}