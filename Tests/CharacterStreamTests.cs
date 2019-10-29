using System;
using System.IO;
using System.Text;
using NUnit.Framework;

using Fig;

namespace Fig.Tests
{
	public class CharacterStreamTests
	{
		private const string TestString = 
			"The quick fox jumped over the lazy brown dog";

		MemoryStream _stream;
		TextReader _reader;
		
		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream(
				Encoding.UTF8.GetBytes(
					TestString));

			_reader = new StreamReader(_stream);
		}

		[TearDown]
		public void Teardown()
		{
			_stream.Dispose();
			_reader.Dispose();
		}

		[Test]
		public void ReadCharacters()
		{
			string stringToRead = "The quick";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);
		}

		[Test]
		public void ReadCharactersAtEnd()
		{
			string stringToRead = "The quick fox jumped over the lazy brown ";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);

			charsRead = null;
			charsRead = fcs.Read(4);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == 3 &&
				string.CompareOrdinal(
					new string(charsRead), "dog") == 0);
		}

		[Test]
		public void PeekCharactersAtEnd()
		{
			string stringToRead = "The quick fox jumped over the lazy brown ";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);

			charsRead = null;
			charsRead = fcs.Peek(4);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == 3 &&
				string.CompareOrdinal(
					new string(charsRead), "dog") == 0);
		}

		[Test]
		public void PeekCharactersAtEndWithOffset()
		{
			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsRead = fcs.Peek(TestString.Length - 3, 4);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == 3 &&
				string.CompareOrdinal(
					new string(charsRead), "dog") == 0);

			charsRead = null;
			charsRead = fcs.Peek(5);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == 5 &&
				string.CompareOrdinal(
					new string(charsRead), "The q") == 0);
		}

		[Test]
		public void PeekCharacters()
		{
			string stringToRead = "The quick";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsPeeked = fcs.Peek(stringToRead.Length);

			Assert.IsTrue(
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);
		}

		[Test]
		public void PeekCharactersWithOffset()
		{
			string stringToRead = "quick";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsPeeked = fcs.Peek(4, stringToRead.Length);

			Assert.IsTrue(
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);
		}

		[Test]
		public void PeekCharactersWithOffsetAndPeekAgain()
		{
			string stringToRead = "quick";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsPeeked = fcs.Peek(4, stringToRead.Length);

			Assert.IsTrue(
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);

			Assert.IsTrue(fcs.Peek(out char @char) && @char == 'T');
		}

		[Test]
		public void PeekAndReadCharacters()
		{
			string stringToRead = "The quick";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsPeeked = fcs.Peek(stringToRead.Length);

			Assert.IsTrue(
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);

			char[] charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);

			charsRead = null;
			stringToRead = " fox jumped";

			charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);

			charsPeeked = null;
			stringToRead = " over the lazy";

			charsPeeked = fcs.Peek(stringToRead.Length);

			Assert.IsTrue(
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);
		}

		[Test]
		public void PeekWithOffsetAndReadCharacters()
		{
			string stringToRead = "quick";

			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsPeeked = fcs.Peek(4, stringToRead.Length);

			Assert.IsTrue(
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);

			stringToRead = "The quick";

			char[] charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);

			charsRead = null;
			stringToRead = " fox jumped";

			charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);

			charsPeeked = null;
			stringToRead = " over the lazy";

			charsPeeked = fcs.Peek(stringToRead.Length);

			Assert.IsTrue(
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);
		}

		[Test]
		public void EndOfStream()
		{
			string stringToRead = "The quick fox jumped over the ";
			CharacterStream fcs = new CharacterStream(_reader);

			char[] charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				!fcs.EndOfSource &&
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);
			
			stringToRead = "lazy brown dog";

			char[] charsPeeked = fcs.Peek(stringToRead.Length);
			
			Assert.IsTrue(
				!fcs.EndOfSource &&
				charsPeeked != null &&
				charsPeeked.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsPeeked), stringToRead) == 0);

			charsRead = fcs.Read(stringToRead.Length);

			Assert.IsTrue(
				!fcs.EndOfSource &&
				charsRead != null &&
				charsRead.Length == stringToRead.Length &&
				string.CompareOrdinal(
					new string(charsRead), stringToRead) == 0);

			bool peekResult = fcs.Peek(out char pch);

			Assert.IsTrue(!peekResult && !fcs.EndOfSource && pch == '\0');

			bool readResult = fcs.Read(out char rch);

			Assert.IsTrue(!readResult && fcs.EndOfSource && rch == '\0');
		}
	}
}
