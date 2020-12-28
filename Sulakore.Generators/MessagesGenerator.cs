using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Sulakore.Generators
{
    [Generator]
    public class MessagesGenerator : ISourceGenerator
    {
        // TODO: idk, fix these errors I guess
        public record Message(bool IsUnity, bool IsOutgoing, string Name, string FlashName, string Hash, short Id);

        public void Execute(GeneratorExecutionContext context)
        {
            IEnumerable<Message> unityMessages = ParseMessages(context.AdditionalFiles.Single(at => at.Path.EndsWith("UnityMessages.ini")).Path, true).OrderBy(m => m.Name);
            IEnumerable<Message> flashMessages = ParseMessages(context.AdditionalFiles.Single(at => at.Path.EndsWith("FlashMessages.ini")).Path, false).OrderBy(m => m.Name);
            // TODO: Merge messages if names are already identical.

            var inis = context.AdditionalFiles.Where(file => file.Path.EndsWith("Messages.ini"));
            context.AddSource("Messages.cs", SourceText.From($@"
namespace Sulakore.Habbo.Messages
{{
    public sealed class Outgoing : Identifiers
    {{
        #region Unity Messages
        {GeneratePropertyDeclarations(unityMessages, true)}
        #endregion

        #region Flash Messages
        {GeneratePropertyDeclarations(flashMessages, true)}
        #endregion

        public Outgoing()
            : base(true)
        {{ }}
    }}
    public sealed class Incoming : Identifiers
    {{
        #region Unity Messages
        {GeneratePropertyDeclarations(unityMessages, false)}
        #endregion

        #region Flash Messages
        {GeneratePropertyDeclarations(flashMessages, false)}
        #endregion

        public Incoming()
            : base(false)
        {{ }}
    }}
}}", Encoding.UTF8));
        }
        public void Initialize(GeneratorInitializationContext context)
        { }

        public static IEnumerable<Message> ParseMessages(string path, bool isUnity)
        {
            bool isOutgoing = true;
            foreach (string line in File.ReadLines(path))
            {
                string[] items = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length != 2)
                {
                    if (line.Equals("[outgoing]", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isOutgoing = true;
                    }
                    else if (line.Equals("[incoming]", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isOutgoing = false;
                    }
                    continue;
                }

                short id = -1;
                string name = items[0].Trim(), flashName = null, hash = null;

                if (isUnity)
                {
                    string[] subItems = items[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    id = short.Parse(subItems[0]);
                    if (subItems.Length > 1)
                    {
                        flashName = subItems[1].Trim();
                    }
                }
                else if (items[1].Length >= 32) // MD5 Hash
                {
                    hash = items[1].Trim();
                }
                yield return new Message(isUnity, isOutgoing, name, flashName, hash, id);
            }
        }

        public static string GeneratePropertyAssignments(IEnumerable<Message> messages, bool isDeclaringOutgoing)
        {
            return "";
        }
        public static string GeneratePropertyDeclarations(IEnumerable<Message> messages, bool isDeclaringOutgoing)
        {
            string declarationSource = null;
            foreach (Message message in messages)
            {
                if (!message.IsOutgoing) continue;
                declarationSource += $"        public HMessage {message.Name} {{ get; }}\r\n";
            }
            return declarationSource;
        }
    }
}