using System;
using Xunit;
using System.Text.RegularExpressions;

namespace Slack.Webhooks.Tests
{
	public class SlackMessageTests
	{
		[Fact]
		public void SlackMessage_should_return_escaped_text()
		{
			// arrange
			var text = "This is a\\nMultiline message";
			var message = new SlackMessage();
			
			// act
			message.Text = text;
			
			// assert
			Assert.Equal(text, message.Text);
		}
		
		[Fact]
		public void SlackMessage_should_unescape_verbatim_string()
		{
			// arrange
			var text = @"This is a\nMultiline message";
			var message = new SlackMessage(TextType.Escaped);
			
			// act
			message.Text = text;
			
			// assert
			Assert.Equal(message.Text, Regex.Unescape(text));
		}
	}
}