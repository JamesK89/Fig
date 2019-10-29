using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FigTests")]

namespace Fig
{
	public enum Tokens : byte
	{
		None = 0,

		BraceLeft,
		BraceRight,

		BracketLeft,
		BracketRight,

		ParenthesisLeft,
		ParenthesisRight,

		String,

		Literal,

		Number,

		Equality,
		Period,
		Comma
	}

	public struct Token
	{
		public static Token Empty = 
			new Token() { Type = Tokens.None, Value = null };

		public Tokens Type;
		public string Value;

		public override string ToString()
		{
			return $"[{Type.ToString("G")}] {Value}";
		}
	}
}
