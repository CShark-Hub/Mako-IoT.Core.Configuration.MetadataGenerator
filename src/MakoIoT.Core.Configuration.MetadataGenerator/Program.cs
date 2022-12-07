// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using MakoIoT.ConfigurationApi.Model;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

if (args.Length != 1)
{
    Console.WriteLine($"Usage: MakoIoT.Core.Configuration.MetadataGenerator YourAssemblyWithConfiguration.dll");
    return;
}

var dc = new CSharpDecompiler(args[0],
    new DecompilerSettings(LanguageVersion.CSharp10_0));

var candidates = dc.TypeSystem.MainModule.TypeDefinitions
    .Where(t => t.FullName.Contains(".Configuration.")).ToList();

var sectionMetadata = new List<ConfigSectionMetadata>();


foreach (var t in candidates)
{
    if (TryFindChildByType<TypeDeclaration>(dc.DecompileType(t.FullTypeName).Children, out var typeDecl))
    {
        if (TryFindChildByType<Attribute>(typeDecl.Attributes, out var att))
        {
            if (TryGetAttributeValue("SectionMetadata", typeDecl.Attributes, out var sectionProps))
            {
                var section = new ConfigSectionMetadata
                {
                    Name = typeDecl.Members.OfType<PropertyDeclaration>().Where(p => p.Name == "SectionName")
                        .Select(p => p.ExpressionBody).OfType<PrimitiveExpression>()
                        .Select(p => (string)p.Value).SingleOrDefault(),
                    Label = (string)sectionProps[0],
                    IsHidden = (bool)sectionProps[1],
                    Parameters = typeDecl.Members.OfType<PropertyDeclaration>().Select(prop =>
                    {
                        if (TryGetAttributeValue("ParameterMetadata", prop.Attributes, out var attValues))
                        {
                            return new ConfigParamMetadata
                            {
                                Name = prop.Name,
                                Label = (string)attValues[0],
                                Type = (string)attValues[1] == ""
                                    ? (prop.ReturnType is PrimitiveType rtpt ? rtpt.Keyword : "string")
                                    : (string)attValues[1],
                                IsHidden = (bool)attValues[2],
                                IsSecret = (bool)attValues[3]
                            };
                        }

                        return null;
                    }).Where(i => i != null).ToArray()
                };
                sectionMetadata.Add(section);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Class name: {t.Name}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(JsonSerializer.Serialize(section));
            }
        }
    }
}



bool TryFindChildByType<T>(IEnumerable<AstNode> nodes, out T node) where T:class
{
    node = default(T)!;
    foreach (var m in nodes)
    {
        if (m is T td)
        {
            node = td;
            return true;
        }

        if (TryFindChildByType<T>(m.Children, out node))
            return true;
    }

    return false;
}


bool TryGetAttributeValue(string attributeName, AstNodeCollection<AttributeSection> attributeSections, out object[] values)
{
    values = Array.Empty<object>();
    var att = attributeSections.SingleOrDefault(a => a.FirstChild?.FirstChild is SimpleType st
                                                     && st.Identifier == attributeName)
        ?.FirstChild;
    if (att == null)
        return false;

    values = att.Children.OfType<PrimitiveExpression>().Select(p => p.Value).ToArray();
    return true;
}
