using System;
using System.IO;
using System.Text;
using NUnit.Framework;

using Fig;

namespace Fig.Tests
{
	public class TokenStreamTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void Teardown()
		{
		}

		private void AssertToken(Token tok, Tokens type, string value)
		{
			Assert.IsTrue(
				tok.Type == type,
				$"Unexpected token {tok.Type.ToString("G")} " +
				$"but was expecting {type.ToString("G")}");
			Assert.IsTrue(
				string.CompareOrdinal(tok.Value, value) == 0,
				$"Token value '{tok.Value}' did not match " +
				$"expected '{value}'");
		}
		
		[Test,
			TestCase("1234", null, Tokens.Number,
				TestName = "Integer, Positive"),
			TestCase("-1234", null, Tokens.Number,
				TestName = "Integer, Negative"),
			TestCase("12.34", null, Tokens.Number,

				TestName = "Real, Positive"),
			TestCase("-12.34", null, Tokens.Number,
				TestName = "Real, Negative"),

			TestCase(".34", null, Tokens.Number,
				TestName = "Partial Real, Positive"),
			TestCase("-.534", null, Tokens.Number,
				TestName = "Partial Real, Negative"),

			TestCase("{", null, Tokens.BraceLeft,
				TestName = "Brace, Left"),
			TestCase("}", null, Tokens.BraceRight,
				TestName = "Brace, Right"),

			TestCase("[", null, Tokens.BracketLeft,
				TestName = "Bracket, Left"),
			TestCase("]", null, Tokens.BracketRight,
				TestName = "Bracket, Right"),
			
			TestCase("(", null, Tokens.ParenthesisLeft,
				TestName = "Parenthesis, Left"),
			TestCase(")", null, Tokens.ParenthesisRight,
				TestName = "Parenthesis, Right"),

			TestCase("=", null, Tokens.Equality,
				TestName = "Equality"),
			TestCase(".", null, Tokens.Period,
				TestName = "Period"),
			TestCase(",", null, Tokens.Comma,
				TestName = "Comma"),

			TestCase("ASDF", null, Tokens.Literal,
				TestName = "Literal, Upper Case"),
			TestCase("asdf", null, Tokens.Literal,
				TestName = "Literal, Lower Case"),
			TestCase("aSDf", null, Tokens.Literal,
				TestName = "Literal, Mixed Case"),
			TestCase("_9-0", null, Tokens.Literal,
				TestName = "Literal, Underscore"),

			TestCase("\"asdf\"", "asdf", Tokens.String,
				TestName = "String, Double Quote"),
			TestCase("\"as\\\"df\"", "as\\\"df", Tokens.String,
				TestName = "String, Double Quote Escaped"),
			TestCase("'asdf'", "asdf", Tokens.String,

				TestName = "String, Single Quote"),
			TestCase("'as\\'df'", "as\\'df", Tokens.String,
				TestName = "String, Single Quote Escaped")]
		public void TokenReadSimple(string what, string expected, Tokens type)
		{
			byte[] stringBytes = Encoding.UTF8.GetBytes(what);
			Token tok = Token.Empty;

			if (expected == null)
			{
				expected = what;
			}

			using (MemoryStream ms = new MemoryStream(stringBytes))
			{
				using (StreamReader sr = new StreamReader(ms))
				{
					CharacterStream cs = new CharacterStream(sr);
					TokenStream ts = new TokenStream(cs);

					Assert.IsTrue(
						ts.Read(out tok), "Failed to read token");
					AssertToken(tok, type, expected);
				}
			}
		}

		[Test]
		public void EndOfStream()
		{
			byte[] stringBytes = Encoding.UTF8.GetBytes(
				"({[=1.23,.}'Te\\'st'ABCDEF");
			Token tok = Token.Empty;

			using (MemoryStream ms = new MemoryStream(stringBytes))
			{
				using (StreamReader sr = new StreamReader(ms))
				{
					CharacterStream cs = new CharacterStream(sr);
					TokenStream ts = new TokenStream(cs);
					
					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.ParenthesisLeft &&
						!ts.EndOfSource);

					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.BraceLeft &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.BracketLeft &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.Equality &&
						!ts.EndOfSource);

					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.Number &&
						string.Compare(tok.Value, "1.23", true) == 0 &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.Comma &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.Period &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.BraceRight &&
						!ts.EndOfSource);

					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.String &&
						string.Compare(tok.Value, "Te\\'st", true) == 0 &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						ts.Read(out tok) &&
						tok.Type == Tokens.Literal &&
						string.Compare(tok.Value, "ABCDEF", true) == 0 &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						!ts.Peek(out tok) &&
						tok.Type == Tokens.None &&
						!ts.EndOfSource);
					
					Assert.IsTrue(
						!ts.Read(out tok) &&
						tok.Type == Tokens.None &&
						ts.EndOfSource);
				}
			}
		}

		[Test]
		public void PeakAndReadComment()
		{
			byte[] stringBytes = Encoding.UTF8.GetBytes(
				"# This is a comment#\n'Test String' # Another comment");
			Token tok = Token.Empty;

			using (MemoryStream ms = new MemoryStream(stringBytes))
			{
				using (StreamReader sr = new StreamReader(ms))
				{
					CharacterStream cs = new CharacterStream(sr);
					TokenStream ts = new TokenStream(cs);

					Assert.IsTrue(
						ts.Read(out tok), "Failed to read token");
					AssertToken(tok, Tokens.String, "Test String");

					Assert.IsTrue(
						!ts.Read(out tok) && ts.EndOfSource);
				}
			}
		}

		[Test]
		public void ReadAndPeakAndRead()
		{
			byte[] stringBytes = Encoding.UTF8.GetBytes(
				"123-.27'Test'");
			Token tok = Token.Empty;

			using (MemoryStream ms = new MemoryStream(stringBytes))
			{
				using (StreamReader sr = new StreamReader(ms))
				{
					CharacterStream cs = new CharacterStream(sr);
					TokenStream ts = new TokenStream(cs);

					Assert.IsTrue(
						ts.Read(out tok), "Failed to read token");
					AssertToken(tok, Tokens.Number, "123");

					Assert.IsTrue(
						ts.Peek(out tok), "Failed to read token");
					AssertToken(tok, Tokens.Number, "-.27");

					Assert.IsTrue(
						ts.Read(out tok), "Failed to read token");
					AssertToken(tok, Tokens.Number, "-.27");

					Assert.IsTrue(
						ts.Read(out tok), "Failed to read token");
					AssertToken(tok, Tokens.String, "Test");
				}
			}
		}
	}
}
