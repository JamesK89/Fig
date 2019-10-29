using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FigTests")]

namespace Fig
{
	internal class CharacterStream
		: BaseBuffer<char>, IDisposable
	{
		private const int DefaultBufferSize = 256;

		public CharacterStream(
			TextReader source)
			: base(DefaultBufferSize)
		{
			Source = source;
		}

		public TextReader Source
		{
			get;
			private set;
		}

		protected override bool ReadFromSource(out char value)
		{
			bool result = false;

			value = '\0';

			int readValue = Source.Read();

			if (readValue != -1)
			{
				result = true;
				value = (char)readValue;
			}

			return result;
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
