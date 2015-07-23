﻿using System;
using System.Collections.Generic;
using OrientDb.Http.RequestBuilders;
using Microsoft.Framework.ConfigurationModel;
using Xunit;

namespace OrientDb.Http.Tests.RequestBuilders
{
	public class CommandRequestBuilderTests : RequestBuilderTestsBase<string, CommandOptions>
	{
		// in aspnet beta5, JsonConfigurationSource doesn't work.
		// a hard coded configuration will be used before beta6.
		private static readonly IConfiguration DefaultConfiguration =
			new Configuration(
				new MemoryConfigurationSource(
					new Dictionary<string, string>
					{
						{ "orientDb:http:_default:baseUrl", "http://localhost:2480" },
						{ "orientDb:http:_default:authorization", "Basic YWRtaW46YWRtaW4=" },
						{ "orientDb:http:_default:databaseName", "test" },
					}));

		public override RequestBuilder<string, CommandOptions> CreateNullParameteredSut()
		{
			return new CommandRequestBuilder(null);
		}

		public override RequestBuilder<string, CommandOptions> CreateDefaultSut()
		{
			return new CommandRequestBuilder(new DbContextOptions(DefaultConfiguration));
		}

		[Fact]
		public void ApiName_is_command()
		{
			var sut = CreateDefaultSut();
			Assert.Equal("command", sut.ApiName);
		}

		[Fact]
		public void GuardClause_GetMessage_with_null_options()
		{
			var sut = CreateDefaultSut();
			Assert.Throws<ArgumentNullException>(() => sut.GetMessage("", null));
		}

		[Fact]
		public void GetMessage_returns_request_without_options()
		{
			var sut = CreateDefaultSut();
			var request = sut.GetMessage("");
			Assert.NotNull(request);
		}

		[Fact]
		public void GetMessage_returns_request_with_options()
		{
			var sut = CreateDefaultSut();
			var request = sut.GetMessage("", new CommandOptions());
			Assert.NotNull(request);
		}

		[Fact]
		public void Content_is_textplain()
		{
			var sut = CreateDefaultSut();
			var request = sut.GetMessage("", new CommandOptions());
			Assert.Equal("text/plain", request.Content.Headers.ContentType.MediaType);
		}

		[Theory]
		[InlineData("")]
		[InlineData("select from V")]
		[InlineData("irrelevant string will be ok.")]
		[InlineData(
			@"anything,
				even multiline will be ok.")]
		public void Content_is_payload(string value)
		{
			var sut = CreateDefaultSut();
			var request = sut.GetMessage(value);
			var actual = request.Content.ReadAsStringAsync().Result;
			Assert.Equal(value, actual);
		}
	}
}
