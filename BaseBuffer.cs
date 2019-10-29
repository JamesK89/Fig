using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FigTests")]

namespace Fig
{
	internal abstract class BaseBuffer<T>
		: IDisposable
	{
		private int _bufferLength;

		protected BaseBuffer(
			int defaultBufferSize)
		{
			Buffer = new T[defaultBufferSize];
			ClearBuffer();
		}

		protected T[] Buffer
		{
			get;
			private set;
		}

		protected int BufferPosition
		{
			get;
			set;
		}

		protected int BufferLength
		{
			get
			{
				return _bufferLength;
			}
			set
			{
				ResizeBuffer(_bufferLength);
			}
		}

		public bool EndOfSource
		{
			get;
			private set;
		}

		protected virtual bool ReadFromSource(out T @value)
		{
			throw new NotImplementedException();
		}

		private void ClearBuffer()
		{
			if (Buffer != null)
			{
				for (int i = 0; i < Buffer.Length; i++)
				{
					Buffer[i] = default(T);
				}
			}

			BufferPosition = 0;
			_bufferLength = 0;
		}

		private void ResizeBuffer(
			int newSize)
		{
			T[] newBuffer = new T[newSize];

			if (Buffer != null)
			{
				T[] oldBuffer = Buffer;

				Buffer = newBuffer;

				ClearBuffer();

				int copyLength = Math.Min(oldBuffer.Length, newBuffer.Length);

				Array.Copy(oldBuffer, newBuffer, copyLength);
			}
		}

		private void AppendBuffer(
			T value)
		{
			AppendBuffer(new T[] { value });
		}

		private void AppendBuffer(
			IEnumerable<T> values)
		{
			foreach (T t in values)
			{
				Buffer[_bufferLength++] = t;

				if (_bufferLength >= Buffer.Length)
				{
					ResizeBuffer(_bufferLength * 2);
				}
			}
		}

		public T[] Read(
			int count)
		{
			return Read(count, false);
		}

		protected T[] Read(
			int count,
			bool peeking)
		{
			List<T> result = new List<T>();

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException(
					nameof(count),
					"Count must be greater than, or equal to, one");
			}

			for (int i = 0; i < count; i++)
			{
				if (BufferPosition < BufferLength)
				{
					result.Add(Buffer[BufferPosition++]);
				}
				else
				{
					T value = default(T);

					if (ReadFromSource(out value))
					{
						result.Add(value);
						AppendBuffer(value);
						BufferPosition++;
					}
					else
					{
						if (!peeking)
						{
							EndOfSource = true;
						}

						break;
					}
				}
			}

			if (!peeking && BufferPosition >= BufferLength)
			{
				ClearBuffer();
			}

			return result.ToArray();
		}

		public bool Read(
			out T value)
		{
			bool result = false;
			
			value = default(T);

			T[] readValues = Read(1);

			if ((readValues != null)  && 
				(readValues.Length == 1))
			{
				result = true;
				value = readValues[0];
			}

			return result;
		}

		public T[] Peek(
			int count)
		{
			return Peek(0, count);
		}

		public T[] Peek(
			int offset,
			int count)
		{
			T[] result = null;

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(offset),
					"Offset must be greater than, or equal to, zero");
			}
			else
			{
				int oldBufferPos = BufferPosition;

				int targetStart = (BufferPosition + offset);
				int targetEnd = targetStart + count;
				int targetLength = targetEnd - targetStart;

				if (targetEnd > BufferLength)
				{
					BufferPosition = BufferLength;
					Read(targetEnd - BufferLength, true);
				}

				int actualCount = count;

				if (targetEnd > BufferLength)
				{
					actualCount = BufferLength	- targetStart;
				}

				result = new T[actualCount];
				Array.Copy(Buffer, targetStart, result, 0, actualCount);

				BufferPosition = oldBufferPos;
			}

			return result;
		}

		public bool Peek(
			out T value)
		{
			bool result = false;

			value = default(T);

			T[] readValues = Peek(1);

			if ((readValues != null)  && 
				(readValues.Length == 1))
			{
				result = true;
				value = readValues[0];
			}

			return result;
		}

		#region IDisposable Support
		protected bool Disposed
		{
			get;
			private set;
		} = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					Buffer = null;
					BufferPosition = 0;
					_bufferLength = 0;
				}
				
				Disposed = true;
			}
		}
		
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
