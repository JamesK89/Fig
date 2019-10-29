using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FigTests")]

namespace Fig
{
	public class ParserException : Exception
	{
		public ParserException(Tokens[] expected, Tokens got)
			: base($"Unexpected token {got.ToString("G")} when " +
				   ((expected?.Length ?? 0) > 1 ? "one of " : "") +
				   $"{TokensToString(expected)} was expected")
		{
		}

		public ParserException(Tokens expected, Tokens got)
			: base($"Unexpected token {got.ToString("G")} when " +
				   $"{expected.ToString("G")} was expected")
		{
		}

		private static string TokensToString(IEnumerable<Tokens> tokens)
		{
			StringBuilder sb = new StringBuilder();
			
			foreach (Tokens type in tokens)
			{
				if (sb.Length > 0)
				{
					sb.Append(", ");
				}

				sb.Append(type.ToString("G"));
			}

			return sb.ToString();
		}
	}

	public class UnknownLiteralException : Exception
	{
		public UnknownLiteralException(
			string value)
			: base($"Unknown literal {value}")
		{
		}

		public UnknownLiteralException(
			Variable var, string value)
			: base($"Attempt to assign unknown " +
				   $"literal '{value}' to variable '{var.Name}'")
		{
		}
	}

	public class ResolveLiteralEventArgs 
		: EventArgs
	{
		public ResolveLiteralEventArgs(
			Variable var,
			string value)
		{
			Variable = var;
			Value = value;
			Success = false;
		}

		public Variable Variable
		{
			get;
			private set;
		}

		public string Value
		{
			get;
			private set;
		}

		public bool Success
		{
			get;
			set;
		}
	}

	public sealed class Config
	{
		public delegate void ResolveLiteralEventHandler(
			object sender,
			ResolveLiteralEventArgs e);

		public event ResolveLiteralEventHandler ResolveLiteral;

		private Token[] _tokens;
		private int _position;

		public Config()
		{
			Root = new Section(null, string.Empty);
			_position = 0;
			_tokens = null;
		}

		public Section Root
		{
			get;
			private set;
		}

		public void Load(TextReader reader)
		{
			using (CharacterStream cs = new CharacterStream(reader))
			{
				using (TokenStream ts = new TokenStream(cs))
				{
					_tokens = ReadTokenStream(ts);
					ReadInnerSection(Root, true);
				}
			}
		}

		public void Load(Stream source)
		{
			using (StreamReader sr = new StreamReader(source))
			{
				Load(sr);
			}
		}

		public void Load(string fileName)
		{
			using (FileStream fs = new FileStream(fileName,
				FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				Load(fs);
			}
		}

		private Token[] ReadTokenStream(TokenStream ts)
		{
			List<Token> result = new List<Token>();

			while (ts != null && !ts.EndOfSource)
			{
				Token[] tokens = ts.Read(1);

				if (tokens != null &&
					tokens.Length > 0)
				{
					result.Add(tokens[0]);
				}
				else
				{
					break;
				}
			}

			return result.ToArray();
		}		

		private bool ReadToken(
			out Token tok,
			params Tokens[] types)
		{
			bool result = false;
			tok = Token.Empty;

			if (_position < _tokens.Length)
			{
				tok = _tokens[_position++];

				result = (types == null ||
						  types.Contains(tok.Type));
			}

			return result;
		}

		private bool ReadToken(
			out Token tok,
			Tokens type)
		{
			return ReadToken(out tok, new Tokens[] { type });
		}

		private bool ReadToken(params Tokens[] types)
		{
			Token tok = Token.Empty;
			return ReadToken(out tok, types);
		}

		private bool ReadToken(Tokens type)
		{
			Token tok = Token.Empty;
			return ReadToken(out tok, type);
		}

		private bool AssertToken(
			out Token tok,
			params Tokens[] types)
		{
			bool result = false;

			if (!ReadToken(out tok, types))
			{
				throw new ParserException(types, tok.Type);
			}
			else if (!types.Contains(tok.Type))
			{
				throw new ParserException(types, tok.Type);
			}
			else
			{
				result = true;
			}

			return result;
		}


		private bool AssertToken(
			out Token tok,
			Tokens type)
		{
			return AssertToken(out tok, new Tokens[] { type });
		}

		private bool AssertToken(
			params Tokens[] types)
		{
			Token tok = Token.Empty;
			return AssertToken(out tok, types);
		}

		private bool AssertToken(
			Tokens type)
		{
			Token tok = Token.Empty;
			return AssertToken(out tok, type);
		}

		private bool ReadVariable(Section parent)
		{
			bool result = false;

			int oldPos = _position;

			Token nameToken = Token.Empty;
			Token valueToken = Token.Empty;

			if (ReadToken(out nameToken, Tokens.Literal) &&
				ReadToken(Tokens.Equality) &&
				AssertToken(out valueToken,
					Tokens.Literal, Tokens.Number, Tokens.String))
			{
				string value = valueToken.Value;
				Variable var = parent.AddVariable(nameToken.Value);

				if (valueToken.Type == Tokens.Literal)
				{
					ResolveLiteralEventArgs args =
						new ResolveLiteralEventArgs(var, value);

					ResolveLiteral?.Invoke(this, args);

					if (!args.Success)
					{
						throw new UnknownLiteralException(
							var, value);
					}
				}
				else
				{
					parent.AddVariable(nameToken.Value)
							.Set(value);
				}

				result = true;
			}

			if (!result)
			{
				_position = oldPos;
			}

			return result;
		}

		private bool ReadSection(Section parent)
		{
			bool result = false;

			int oldPos = _position;

			Token nameToken = Token.Empty;

			if (ReadToken(out nameToken, Tokens.Literal) &&
				ReadToken(Tokens.BraceLeft))
			{
				Section sec = parent.AddChild(nameToken.Value);

				ReadInnerSection(sec, false);

				AssertToken(Tokens.BraceRight);

				result = true;
			}

			if (!result)
			{
				_position = oldPos;
			}

			return result;
		}

		private bool ReadInnerSection(Section parent, bool nobrace)
		{
			bool result = false;
			bool success = false;

			int oldPos = _position;

			if (!nobrace)
			{
				AssertToken(
					Tokens.Literal,
					Tokens.BraceRight);
			}
			else
			{
				AssertToken(Tokens.Literal);
			}

			_position = oldPos;

			do
			{
				success =
					ReadVariable(parent) ||
					ReadSection(parent);

				result = result || success;
			} while (success);

			return result;
		}
	}
}
