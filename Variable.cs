using System;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FigTests")]

namespace Fig
{
	public enum VariableType
	{
		String = 0,
		Boolean,
		Integer,
		Real
	}

	public class VariableChangedEventArgs 
		: EventArgs
	{
	}

	public class Variable
	{
		public delegate void VariableChangedEventHandler(
			object sender, VariableChangedEventArgs e);

		public event VariableChangedEventHandler Changed;

		private string _asString;
		private bool _asBoolean;
		private long _asInteger;
		private double _asReal;
		private object _asOriginal;

		internal Variable(
			Section section,
			string name)
		{
			Section = section;
			Name = name;

			Reset();
		}

		internal Variable(
			Section section,
			string name,
			string value)
			: this(section, name)
		{
			Set(value);
		}

		internal Variable(
			Section section,
			string name,
			bool value)
			: this(section, name)
		{
			Set(value);
		}

		internal Variable(
			Section section,
			string name,
			long value)
			: this(section, name)
		{
			Set(value);
		}

		internal Variable(
			Section section,
			string name,
			double value)
			: this(section, name)
		{
			Set(value);
		}

		public VariableType Type
		{
			get;
			private set;
		}

		public Section Section
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public virtual string StringValue
		{
			get
			{
				return _asString;
			}
			set
			{
				Set(value);
			}
		}

		public virtual Double RealValue
		{
			get
			{
				return _asReal;
			}
			set
			{
				Set(value);
			}
		}

		public virtual bool BooleanValue
		{
			get
			{
				return _asBoolean;
			}
			set
			{
				Set(value);
			}
		}

		public virtual long IntegerValue
		{
			get
			{
				return _asInteger;
			}
			set
			{
				Set(value);
			}
		}

		public virtual object OriginalValue
		{
			get
			{
				return _asOriginal;
			}
			set
			{
				Set(value);
			}
		}

		protected virtual void Reset()
		{
			_asString = string.Empty;
			_asBoolean = false;
			_asInteger = 0;
			_asReal = 0d;
			_asOriginal = null;
		}

		public virtual Variable Set(object value)
		{
			Set(value?.ToString() ?? string.Empty);
			return this;
		}

		public virtual Variable Set(string value)
		{
			Reset();

			_asString = value;

			double.TryParse(_asString, out _asReal);

			if (!long.TryParse(_asString, out _asInteger))
			{
				_asInteger = (long)_asReal;
			}

			_asBoolean =
				(string.Compare(_asString, bool.TrueString, true) == 0) ||
				(_asInteger != 0) ||
				(Math.Abs(_asReal) > double.Epsilon);

			_asOriginal = value;

			Type = VariableType.String;

			Changed?.Invoke(this, new VariableChangedEventArgs());

			return this;
		}

		public virtual Variable Set(long value)
		{
			Reset();

			_asInteger = value;
			_asBoolean = value != 0;
			_asReal = Convert.ToDouble(_asInteger);
			_asString = Convert.ToString(_asInteger);
			_asOriginal = value;

			Type = VariableType.Integer;

			Changed?.Invoke(this, new VariableChangedEventArgs());

			return this;
		}

		public virtual Variable Set(ulong value)
		{
			Set(Convert.ToInt64(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Set(int value)
		{
			Set(Convert.ToInt64(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Set(uint value)
		{
			Set(Convert.ToInt64(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Set(short value)
		{
			Set(Convert.ToInt64(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Set(ushort value)
		{
			Set(Convert.ToInt64(value));
			_asOriginal = value;
			return this;
		}
		
		public virtual Variable Set(byte value)
		{
			Set(Convert.ToInt64(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Set(sbyte value)
		{
			Set(Convert.ToInt64(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Set(bool value)
		{
			Reset();

			_asBoolean = value;
			_asInteger = value ? 1 : 0;
			_asReal = value ? 1d : 0d;
			_asString = value ? bool.TrueString : bool.FalseString;
			_asOriginal = value;

			Type = VariableType.Boolean;

			Changed?.Invoke(this, new VariableChangedEventArgs());

			return this;
		}

		public virtual Variable Set(double value)
		{
			Reset();

			_asReal = value;
			_asInteger = Convert.ToInt64(_asReal);
			_asBoolean = Math.Abs(_asReal) > double.Epsilon;
			_asString = Convert.ToString(_asReal);
			_asOriginal = value;

			Type = VariableType.Real;

			Changed?.Invoke(this, new VariableChangedEventArgs());

			return this;
		}

		public virtual Variable Set(float value)
		{
			Set(Convert.ToDouble(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Set(decimal value)
		{
			Set(Convert.ToDouble(value));
			_asOriginal = value;
			return this;
		}

		public virtual Variable Get(out object value)
		{
			value = _asOriginal;
			return this;
		}

		public virtual Variable Get(out string value)
		{
			value = _asString;
			return this;
		}

		public virtual Variable Get(out double value)
		{
			value = _asReal;
			return this;
		}

		public virtual Variable Get(out decimal value)
		{
			value = Convert.ToDecimal(_asReal);
			return this;
		}

		public virtual Variable Get(out float value)
		{
			value = Convert.ToSingle(_asReal);
			return this;
		}

		public virtual Variable Get(out bool value)
		{
			value = _asBoolean;
			return this;
		}

		public virtual Variable Get(out long value)
		{
			value = _asInteger;
			return this;
		}

		public virtual Variable Get(out ulong value)
		{
			value = Convert.ToUInt64(_asInteger);
			return this;
		}
		
		public virtual Variable Get(out int value)
		{
			value = Convert.ToInt32(_asInteger);
			return this;
		}

		public virtual Variable Get(out uint value)
		{
			value = Convert.ToUInt32(_asInteger);
			return this;
		}

		public virtual Variable Get(out short value)
		{
			value = Convert.ToInt16(_asInteger);
			return this;
		}

		public virtual Variable Get(out ushort value)
		{
			value = Convert.ToUInt16(_asInteger);
			return this;
		}

		public virtual Variable Get(out byte value)
		{
			value = Convert.ToByte(_asInteger);
			return this;
		}

		public virtual Variable Get(out sbyte value)
		{
			value = Convert.ToSByte(_asInteger);
			return this;
		}

		public virtual string AsString()
		{
			return _asString;
		}

		public virtual long AsInteger()
		{
			return _asInteger;
		}

		public virtual double AsReal()
		{
			return _asReal;
		}

		public virtual bool AsBoolean()
		{
			return _asBoolean;
		}

		public virtual object AsOriginal()
		{
			return _asOriginal;
		}

		public virtual string ToString(bool heirarchy)
		{
			StringBuilder sb = new StringBuilder();

			if (heirarchy)
			{
				Section parent = Section;

				while (parent != null)
				{
					if (sb.Length > 0)
					{
						sb.Insert(0, ".");
					}

					sb.Insert(0, parent.Name);

					parent = Section.Parent;
				}
			}

			sb.Append(Name);
			sb.Append(" = ");
			sb.Append(_asString);

			return _asString;
		}

		public override string ToString()
		{
			return this.ToString(true);
		}

		public static implicit operator bool(Variable variable)
		{
			return variable._asBoolean;
		}

		public static implicit operator long(Variable variable)
		{
			return variable._asInteger;
		}

		public static implicit operator ulong(Variable variable)
		{
			return Convert.ToUInt64(variable._asInteger);
		}

		public static implicit operator int(Variable variable)
		{
			return Convert.ToInt32(variable._asInteger);
		}

		public static implicit operator uint(Variable variable)
		{
			return Convert.ToUInt32(variable._asInteger);
		}

		public static implicit operator short(Variable variable)
		{
			return Convert.ToInt16(variable._asInteger);
		}
		
		public static implicit operator ushort(Variable variable)
		{
			return Convert.ToUInt16(variable._asInteger);
		}

		public static implicit operator byte(Variable variable)
		{
			return Convert.ToByte(variable._asInteger);
		}
		
		public static implicit operator sbyte(Variable variable)
		{
			return Convert.ToSByte(variable._asInteger);
		}

		public static implicit operator string(Variable variable)
		{
			return variable._asString;
		}

		public static implicit operator double(Variable variable)
		{
			return variable._asReal;
		}

		public static implicit operator float(Variable variable)
		{
			return Convert.ToSingle(variable._asReal);
		}
	}
}
