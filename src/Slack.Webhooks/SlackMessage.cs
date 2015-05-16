using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Slack.Webhooks
{
    public enum TextType
    {
        Normal = 0,
        Escaped = 1
    }
    /// <summary>
    /// Slack Message
    /// </summary>
    public class SlackMessage
    {
        
        private string _text;
        
        private bool _markdown = true;
        /// <summary>
        /// This is the text that will be posted to the channel
        /// </summary>
        public string Text 
        { 
            get{ return _text; } 
            set{ _text = TextType == TextType.Escaped ? Regex.Unescape(value) : value; } 
        }
        
        public TextType TextType { get; set; }
        /// <summary>
        /// Optional override of destination channel
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// Optional override of the username that is displayed
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Optional emoji displayed with the message
        /// </summary>
        public string IconEmoji { get; set; }
        /// <summary>
        /// Optional override markdown mode. Default: true
        /// </summary>
        public bool Mrkdwn
        {
            get { return _markdown; }
            set { _markdown = value; }
        }
        /// <summary>
        /// Optional attachment collection
        /// </summary>
        public List<SlackAttachment> Attachments { get; set; }
        
        public SlackMessage() : this(TextType.Normal)
        {
        }
        
        public SlackMessage(TextType textType)
        {
            TextType = textType;
        }
    }
}
