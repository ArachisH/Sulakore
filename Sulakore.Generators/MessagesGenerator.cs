using System.IO;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Runtime.Serialization.Json;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Sulakore.Generators
{
    [Generator]
    public class MessagesGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            string messagesJsonPath = context.AdditionalFiles.Single(at => at.Path.EndsWith("messages.json")).Path;
            using var messagesStream = new MemoryStream(File.ReadAllBytes(messagesJsonPath));

            var messagesSerializer = new DataContractJsonSerializer(typeof(Messages));
            var messages = (Messages)messagesSerializer.ReadObject(messagesStream);

            context.AddSource("Outgoing.cs", SourceText.From(CreateMessagesSource(messages.Outgoing, true), Encoding.UTF8));
            context.AddSource("Incoming.cs", SourceText.From(CreateMessagesSource(messages.Incoming, false), Encoding.UTF8));
        }
        public void Initialize(GeneratorInitializationContext context)
        { }

        private static string CreateMessagesSource(Message[] messages, bool isOutgoing)
        {
            string className = isOutgoing ? "Outgoing" : "Incoming";
            string isOutgoingString = isOutgoing.ToString().ToLowerInvariant();

            using var text = new StringWriter();
            using var indentedText = new IndentedTextWriter(text);

            indentedText.WriteLine("namespace Sulakore.Habbo");
            indentedText.WriteLine('{');
            indentedText.Indent++;

            indentedText.WriteLine($"public sealed class {className} : Identifiers");
            indentedText.WriteLine('{');
            indentedText.Indent++;

            indentedText.Write("#region Message Properties");
            foreach (Message message in messages)
            {
                indentedText.WriteLine();
                message.BackingFieldName = $"_{char.ToLower(message.Name[0]) + message.Name.Substring(1)}";
                indentedText.WriteLine($"private HMessage {message.BackingFieldName};");
                indentedText.WriteLine($"public HMessage {message.Name}");
                indentedText.WriteLine('{');

                indentedText.Indent++;
                indentedText.WriteLine($"get => {message.BackingFieldName};");
                indentedText.WriteLine($"init => Register(value, \"{message.Name}\", ref {message.BackingFieldName});");
                indentedText.Indent--;

                indentedText.WriteLine('}');
            }
            indentedText.WriteLine("#endregion");

            indentedText.WriteLine();
            indentedText.WriteLine($"public {className}() : base({isOutgoingString}) {{ }}");
            indentedText.WriteLine($"public {className}(IGame game) : base({messages.Length}, {isOutgoingString})");
            indentedText.WriteLine('{');
            indentedText.Indent++;

            foreach (Message message in messages)
            {
                indentedText.Write($"{message.BackingFieldName} = ResolveMessage(game, \"{message.Name}\", {message.UnityId}, ");
                if (string.IsNullOrWhiteSpace(message.UnityStructure))
                {
                    indentedText.Write("null");
                }
                else indentedText.Write("\"" + message.UnityStructure + "\"");

                foreach (uint postShuffleHash in message.PostShuffleHashes)
                {
                    indentedText.Write(", ");
                    indentedText.Write(postShuffleHash);
                }
                indentedText.WriteLine(");");
            }

            indentedText.Indent--;
            indentedText.WriteLine('}');

            indentedText.Indent--;
            indentedText.WriteLine('}');

            indentedText.Indent--;
            indentedText.WriteLine('}');

            indentedText.Flush();
            return text.ToString();
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}