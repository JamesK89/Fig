using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FigTests")]

namespace Fig
{
	public class Section
	{
		private List<Section> _children;
		private List<Variable> _variables;

		protected Section()
		{
			Name = string.Empty;
			Parent = null;
			_children = new List<Section>();
			_variables = new List<Variable>();
		}

		internal Section(
			Section parent,
			string name)
			: this()
		{
			Name = name;
			Parent = parent;
		}

		public Section Parent
		{
			get;
			protected set;
		}

		public string Name
		{
			get;
			protected set;
		}

		public IEnumerable<Section> Children
		{
			get => _children;
		}

		public IEnumerable<Variable> Variables
		{
			get => _variables;
		}

		public Variable Variable(string path)
		{
			string[] split = path.Trim('.').Split('.');
			Variable result = null;

			if (split != null)
			{
				if (split.Length == 1)
				{
					result = _variables
						.Select((o) => o)
						.Where((o) => 
							o.Name?.Equals(split[0].Trim(),
								StringComparison.InvariantCultureIgnoreCase) ??
								false)
						.FirstOrDefault();
				}
				else if (split.Length > 1)
				{
					Section sec = Child(split[0]);
					result = sec.Variable(string.Join(".", split.Skip(1)));
				}
			}

			return result;
		}

		public Section Child(string path)
		{
			string[] split = path.Trim('.').Split('.');
			Section result = null;

			if (split != null)
			{
				if (split.Length == 1)
				{
					result = _children
						.Select((o) => o)
						.Where((o) =>
							o.Name?.Equals(split[0].Trim(),
								StringComparison.InvariantCultureIgnoreCase) ??
								false)
						.FirstOrDefault();
				}
				else if (split.Length > 1)
				{
					Section sec = Child(split[0]);
					result = sec.Child(string.Join(".", split.Skip(1)));
				}
			}

			return result;
		}

		public Variable AddVariable(string name)
		{
			Variable var = Variable(name);

			if (var == null)
			{
				var = new Variable(this, name);
				_variables.Add(var);
			}

			return var;
		}

		public void RemoveVariable(string name)
		{
			Variable var = Variable(name);

			if (var != null)
			{
				int idx = _variables.IndexOf(var);

				if (idx > -1)
				{
					_variables.RemoveAt(idx);
				}
			}
		}

		public Section AddChild(string name)
		{
			Section sec = Child(name);

			if (sec == null)
			{
				sec = new Section(this, name);
				_children.Add(sec);
			}

			return sec;
		}

		public void RemoveChild(string name)
		{
			Section sec = Child(name);

			if (sec != null)
			{
				int idx = _children.IndexOf(sec);

				if (idx > -1)
				{
					_children.RemoveAt(idx);
				}
			}
		}

		private string ToString(int depth)
		{
			StringBuilder sb = new StringBuilder();

			string outerTabString = new string('\t', depth);
			string innerTabString = new string('\t', depth + 1);

			sb.Append(outerTabString);
			sb.AppendLine(Name);
			sb.Append(outerTabString);
			sb.AppendLine("{");

			foreach (Variable var in _variables)
			{
				sb.Append(innerTabString);
				sb.AppendLine(var.ToString(false));
			}

			foreach (Section child in _children)
			{
				sb.Append(child.ToString(depth + 1));
			}
			
			sb.Append(outerTabString);
			sb.AppendLine("}");

			return base.ToString();
		}

		public override string ToString()
		{
			return this.ToString(0);
		}
	}
}
