using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FigTests")]

namespace Fig
{
	internal class TokenStream
		: BaseBuffer<Token>, IDisposable
	{
		private const int DefaultBufferSize = 16;

		private static Regex NumberRegex = new Regex(
			@"^-?([0-9]+(?:\.[0-9]+)?|\.[0-9]+)",
			RegexOptions.Compiled | RegexOptions.Singleline);

		private static Regex StringRegex = new Regex(
			@"^(?<q>""|')(?<value>(?:\\\k<q>|(?!\k<q>).)+)\k<q>",
			RegexOptions.Compiled | RegexOptions.Singleline);

		private static Regex LiteralRegex = new Regex(
			@"^(?:(?:_[0-9]+)|_?[a-zA-Z]+)[\-_a-zA-Z0-9]*",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static IDictionary<char, Tokens> BraceTokens =
			new Dictionary<char, Tokens>() {
				{ '{', Tokens.BraceLeft },
				{ '}', Tokens.BraceRight }
			};

		private static IDictionary<char, Tokens> BracketTokens =
			new Dictionary<char, Tokens>() {
				{ '[', Tokens.BracketLeft },
				{ ']', Tokens.BracketRight }
			};

		private static IDictionary<char, Tokens> ParenthesisTokens =
			new Dictionary<char, Tokens>() {
				{ '(', Tokens.ParenthesisLeft },
				{ ')', Tokens.ParenthesisRight }
			};

		private const string NumberCharacters =
			"01234567890.-";
		private const string StringCharacters = 
			"\"'";
		private const string BraceCharacters = 
			"{}";
		private const string BracketCharacters = 
			"[]";
		private const string ParenthesisCharacters = 
			"()";
		private const string WhiteSpaceCharacters = 
			" \t\v\r\n";
		private const string LiteralCharacters =
			"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
			"abcdefghijklmnopqrstuvwxyz" +
			"01234567890" +
			"_-";

		private const char PeriodCharacter = 
			'.';
		private const char CommaCharacter = 
			',';
		private const char EqualityCharacter = 
			'=';
		private const char CommentCharacter = '#';

		public TokenStream(CharacterStream source)
			: base(DefaultBufferSize)
		{
			Source = source;
		}

		public CharacterStream Source
		{
			get;
			private set;
		}

		protected override bool ReadFromSource(out Token value)
		{
			value = Token.Empty;

			SkipIgnored();

			bool result = 
				ReadBrace(out value) ||
				ReadBracket(out value) ||
				ReadParenthesis(out value) ||
				ReadEquality(out value) ||
				ReadNumber(out value) ||
				ReadPeriod(out value) ||
				ReadComma(out value) ||
				ReadLiteral(out value) ||
				ReadString(out value);

			return result;
		}

		private bool ReadWhiteSpace()
		{
			bool result = false;

			while (
				Source.Peek(out char @char) &&
				MightBeWhiteSpace(@char))
			{
				result = true;

				if (!Source.Read(out char @ws))
				{
					break;
				}
			};

			return result;
		}

		private bool ReadComment()
		{
			bool result = false;

			if (Source.Peek(out char bchar) &&
				MightBeComment(bchar))
			{
				Source.Read(1);

				while (Source.Peek(out char mchar) &&
					   !MightBeComment(mchar))
				{
					Source.Read(1);
				}

				if (Source.Peek(out char echar))
				{
					Source.Read(1);
				}

				result = true;
			}

			return result;
		}

		private void SkipIgnored()
		{
			ReadWhiteSpace();

			while (ReadComment())
			{
				ReadWhiteSpace();
			}
		}

		private bool ReadCharacter(
			out Token token,
			Func<char, bool> isFunc,
			Tokens type)
		{
			bool result = false;

			token = Token.Empty;

			if (Source.Peek(out char @char) &&
				isFunc.Invoke(@char))
			{
				result = true;
				token.Value = @char.ToString();
				token.Type = type;
				Source.Read(1);
			}

			return result;
		}
		
		private bool ReadCharacters(
			out Token token,
			Func<char, bool> isFunc,
			IDictionary<char, Tokens> chars)
		{
			bool result = false;

			token = Token.Empty;

			if (chars != null &&
				Source.Peek(out char @char) &&
				isFunc.Invoke(@char))
			{
				var selected = 
					chars.Where((o) => o.Key == @char).Select((o) => o);

				var enumerator = selected?.GetEnumerator() ?? null;

				if (selected != null && enumerator != null &&
					enumerator.MoveNext())
				{
					result = true;
					token.Type = enumerator.Current.Value;
				}

				if (result)
				{
					token.Value = @char.ToString();
					Source.Read(1);
				}
			}

			return result;
		}

		private bool ReadBrace(out Token token)
		{
			return ReadCharacters(
				out token,
				MightBeBrace,
				BraceTokens);
		}

		private bool ReadBracket(out Token token)
		{
			return ReadCharacters(
				out token,
				MightBeBracket,
				BracketTokens);
		}

		private bool ReadParenthesis(out Token token)
		{
			return ReadCharacters(
				out token,
				MightBeParenthesis,
				ParenthesisTokens);
		}
		
		private bool ReadComma(out Token token)
		{
			return ReadCharacter(
				out token,
				MightBeComma,
				Tokens.Comma);
		}

		private bool ReadPeriod(out Token token)
		{
			return ReadCharacter(
				out token,
				MightBePeriod,
				Tokens.Period);
		}

		private bool ReadEquality(out Token token)
		{
			return ReadCharacter(
				out token,
				MightBeEquality,
				Tokens.Equality);
		}
		
		private bool ReadNumber(out Token token)
		{
			bool result = false;

			token = Token.Empty;

			StringBuilder sb = new StringBuilder();
			char[] charsRead = null;

			int idx = 0;

			while ((charsRead = Source.Peek(idx++, 1)) != null &&
					charsRead.Length == 1 &&
					MightBeNumber(charsRead[0]))
			{
				sb.Append(charsRead[0]);
			}

			string charString = sb.ToString();

			if (!string.IsNullOrWhiteSpace(charString))
			{
				Match m = NumberRegex.Match(charString);

				if (m != null && m.Success)
				{
					result = true;
					token.Type = Tokens.Number;
					token.Value = m.Value;
					Source.Read(m.Length);
				}
			}

			return result;
		}

		private bool ReadLiteral(out Token token)
		{
			bool result = false;

			token = Token.Empty;

			StringBuilder sb = new StringBuilder();
			char[] charsRead = null;

			int idx = 0;

			while ((charsRead = Source.Peek(idx++, 1)) != null &&
					charsRead.Length == 1 &&
					MightBeLiteral(charsRead[0]))
			{
				sb.Append(charsRead[0]);
			}

			string charString = sb.ToString();

			if (!string.IsNullOrWhiteSpace(charString))
			{
				Match m = LiteralRegex.Match(charString);

				if (m != null && m.Success)
				{
					result = true;
					token.Type = Tokens.Literal;
					token.Value = m.Value;
					Source.Read(m.Length);
				}
			}

			return result;
		}

		private bool ReadString(out Token token)
		{
			bool result = false;

			token = Token.Empty;

			StringBuilder sb = new StringBuilder();
			char[] charsRead = null;

			int idx = 0;

			bool inString = false;
			bool skipString = false;
			char stringChar = '\0';

			while ((charsRead = Source.Peek(idx++, 1)) != null &&
					charsRead.Length == 1 &&
					(inString || (!inString && MightBeString(charsRead[0]))))
			{
				char @char = charsRead[0];

				if (!inString)
				{
					sb.Append(@char);
					stringChar = @char;
					inString = true;
				}
				else
				{
					sb.Append(@char);

					if (skipString)
					{
						skipString = false;
					}
					else if (@char == '\\' && !skipString)
					{
						skipString = true;
					}
					else if (@char == stringChar)
					{
						break;
					}
				}
			}

			string charString = sb.ToString();

			if (!string.IsNullOrWhiteSpace(charString))
			{
				Match m = StringRegex.Match(charString);

				if (m != null && m.Success)
				{
					result = true;
					token.Type = Tokens.String;
					token.Value = m.Groups["value"]?.Value ?? string.Empty;
					Source.Read(m.Length);
				}
			}

			return result;
		}

		private bool MightBeComment(char ch)
		{
			return (ch == CommentCharacter);
		}

		private bool MightBeNewLine(char ch)
		{
			return (ch == '\r' || ch == '\n');
		}

		private bool MightBeWhiteSpace(char ch)
		{
			return WhiteSpaceCharacters.Contains(ch);
		}

		private bool MightBeNumber(char ch)
		{
			return NumberCharacters.Contains(ch);
		}

		private bool MightBeBrace(char ch)
		{
			return BraceCharacters.Contains(ch);
		}

		private bool MightBeLiteral(char ch)
		{
			return LiteralCharacters.Contains(ch);
		}

		private bool MightBeString(char ch)
		{
			return StringCharacters.Contains(ch);
		}

		private bool MightBePeriod(char ch)
		{
			return (ch == PeriodCharacter);
		}

		private bool MightBeComma(char ch)
		{
			return (ch == CommaCharacter);
		}
		
		private bool MightBeBracket(char ch)
		{
			return BracketCharacters.Contains(ch);
		}

		private bool MightBeParenthesis(char ch)
		{
			return ParenthesisCharacters.Contains(ch);
		}

		private bool MightBeEquality(char ch)
		{
			return (ch == EqualityCharacter);
		}

		protected override void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					if (Source != null)
					{
						Source.Dispose();
						Source = null;
					}
				}

				base.Dispose(disposing);
			}
		}
	}
}
