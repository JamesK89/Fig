using System;
using System.IO;

using Fig;

namespace Demo
{
	public class DemoConfig
	{
		public DemoConfig()
		{
			Config = new Config();
			Config.ResolveLiteral += ResolveLiteral;
			Demonstration = new DemonstrationSection(Config.Root);
		}

		public DemoConfig(string fileName)
			: this()
		{
			if (File.Exists(fileName))
			{
				Config.Load(fileName);
			}
		}

		private void ResolveLiteral(
			object sender,
			ResolveLiteralEventArgs e)
		{
			if (string.Equals(
					e.Value,
					"SomeValue",
					StringComparison.InvariantCultureIgnoreCase))
			{
				e.Variable.Set(Math.PI);
				e.Success = true;
			}
		}

		public Config Config
		{
			get;
			private set;
		}

		public DemonstrationSection Demonstration
		{
			get;
			private set;
		}
	}

	public class DemonstrationSection
	{
		public DemonstrationSection(Section parent)
		{
			Section = parent.AddChild("Demonstration");

			TestVariable =
				Section.AddVariable("TestVariable").Set("Hello World!");
		}

		public Section Section
		{
			get;
			private set;
		}

		public Variable TestVariable
		{
			get;
			private set;
		}
	}

	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			DemoConfig cfg = new DemoConfig("Demo.cfg");
			
			PrintVariable(cfg.Demonstration.TestVariable);

			string input = string.Empty;

			while ((input = Console.ReadLine()) != null)
			{
				if (input.Length < 1)
				{
					break;
				}
				else
				{
					cfg.Demonstration.TestVariable.Set(input);
					PrintVariable(cfg.Demonstration.TestVariable);
				}
			}
		}

		public static void PrintVariable(Variable var)
		{
			Console.WriteLine(var.Name);
			Console.WriteLine($"\tString: {var.StringValue}");
			Console.WriteLine($"\tInteger: {var.IntegerValue}");
			Console.WriteLine($"\tReal: {var.RealValue}");
			Console.WriteLine($"\tBoolean: {var.BooleanValue}");
		}
	}
}