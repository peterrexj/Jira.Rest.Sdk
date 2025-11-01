using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jira.Rest.Sdk.Dtos
{
    public class Description
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("content")]
        public List<Content> Content { get; set; }

        /// <summary>
        /// Converts the ADF (Atlassian Document Format) description to a pretty-printed plain text string.
        /// </summary>
        /// <returns>A formatted string representation of the description content.</returns>
        public override string ToString()
        {
            if (Content == null || !Content.Any())
                return string.Empty;

            var result = new StringBuilder();
            
            foreach (var contentItem in Content)
            {
                AppendContentToString(contentItem, result, 0);
            }

            return result.ToString().Trim();
        }

        /// <summary>
        /// Recursively processes ADF content and appends formatted text to the StringBuilder.
        /// </summary>
        /// <param name="content">The content item to process.</param>
        /// <param name="sb">The StringBuilder to append to.</param>
        /// <param name="indentLevel">The current indentation level for nested content.</param>
        private void AppendContentToString(Content content, StringBuilder sb, int indentLevel)
        {
            if (content == null) return;

            var indent = new string(' ', indentLevel * 2);

            switch (content.Type?.ToLower())
            {
                case "paragraph":
                    if (content.NestedContent != null && content.NestedContent.Any())
                    {
                        foreach (var nestedContent in content.NestedContent)
                        {
                            AppendContentToString(nestedContent, sb, indentLevel);
                        }
                        sb.AppendLine(); // Add line break after paragraph
                    }
                    break;

                case "text":
                    if (!string.IsNullOrEmpty(content.Text))
                    {
                        sb.Append(indent + content.Text);
                    }
                    break;

                case "heading":
                    if (content.NestedContent != null && content.NestedContent.Any())
                    {
                        sb.Append(indent + "# ");
                        foreach (var nestedContent in content.NestedContent)
                        {
                            AppendContentToString(nestedContent, sb, 0); // No additional indent for heading text
                        }
                        sb.AppendLine();
                        sb.AppendLine(); // Extra line break after heading
                    }
                    break;

                case "bulletlist":
                case "orderedlist":
                    if (content.NestedContent != null && content.NestedContent.Any())
                    {
                        foreach (var listItem in content.NestedContent)
                        {
                            AppendContentToString(listItem, sb, indentLevel);
                        }
                        sb.AppendLine(); // Add line break after list
                    }
                    break;

                case "listitem":
                    sb.Append(indent + "• ");
                    if (content.NestedContent != null && content.NestedContent.Any())
                    {
                        foreach (var nestedContent in content.NestedContent)
                        {
                            AppendContentToString(nestedContent, sb, 0); // No additional indent for list item text
                        }
                    }
                    sb.AppendLine();
                    break;

                case "codeblock":
                    sb.AppendLine(indent + "```");
                    if (content.NestedContent != null && content.NestedContent.Any())
                    {
                        foreach (var nestedContent in content.NestedContent)
                        {
                            AppendContentToString(nestedContent, sb, indentLevel + 1);
                        }
                    }
                    sb.AppendLine(indent + "```");
                    break;

                case "blockquote":
                    sb.Append(indent + "> ");
                    if (content.NestedContent != null && content.NestedContent.Any())
                    {
                        foreach (var nestedContent in content.NestedContent)
                        {
                            AppendContentToString(nestedContent, sb, 0); // No additional indent for blockquote text
                        }
                    }
                    sb.AppendLine();
                    break;

                default:
                    // Handle unknown content types by processing nested content
                    if (content.NestedContent != null && content.NestedContent.Any())
                    {
                        foreach (var nestedContent in content.NestedContent)
                        {
                            AppendContentToString(nestedContent, sb, indentLevel);
                        }
                    }
                    else if (!string.IsNullOrEmpty(content.Text))
                    {
                        sb.Append(indent + content.Text);
                    }
                    break;
            }
        }
    }
}
