using System;
using System.IO;
using System.Text;
using System.Linq;
using NUnit.Framework;

using Fig;

namespace Fig.Tests
{
	public class ParserTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void Teardown()
		{
		}

		private MemoryStream GetStream(string str)
		{
			byte[] data = Encoding.UTF8.GetBytes(str);
			return new MemoryStream(data);
		}

		[Test]
		public void SingleSection()
		{
			string testData = "ASDF{test1=2.13}";
			using (MemoryStream ms = GetStream(testData))
			{
				Config cfg = new Config();
				cfg.Load(ms);

				Assert.IsTrue(
					cfg.Root != null);

				Section asdf = cfg.Root.Child("asdf");

				Assert.IsTrue(
					asdf != null);

				Variable var = asdf.Variable("test1");

				Assert.IsTrue(
					var != null &&
					var.RealValue == 2.13d);
			}
		}

		[Test]
		public void SingleNestedSection()
		{
			string testData = @"ASDF{test1=2.13fdsa#comment#{test2='Te\'st'}}";
			using (MemoryStream ms = GetStream(testData))
			{
				Config cfg = new Config();
				cfg.Load(ms);

				Assert.IsTrue(
					cfg.Root != null);

				Section asdf = cfg.Root.Child("asdf");

				Assert.IsTrue(
					asdf != null);

				Variable var = asdf.Variable("test1");

				Assert.IsTrue(
					var != null &&
					var.RealValue == 2.13d);

				Section fdsa = asdf.Child("fdsa");

				Assert.IsTrue(
					fdsa != null);

				var = fdsa.Variable("test2");
				
				Assert.IsTrue(
					var != null &&
					string.CompareOrdinal(var.StringValue, @"Te\'st") == 0);
			}
		}

		[Test]
		public void DoubleSection()
		{
			string testData = @"ASDF{test1=2.13}fdsa#comment#{test2='Te\'st'}";
			using (MemoryStream ms = GetStream(testData))
			{
				Config cfg = new Config();
				cfg.Load(ms);

				Assert.IsTrue(
					cfg.Root != null);

				Section asdf = cfg.Root.Child("asdf");

				Assert.IsTrue(
					asdf != null);

				Variable var = asdf.Variable("test1");

				Assert.IsTrue(
					var != null &&
					var.RealValue == 2.13d);

				Section fdsa = cfg.Root.Child("fdsa");

				Assert.IsTrue(
					fdsa != null);

				var = fdsa.Variable("test2");
				
				Assert.IsTrue(
					var != null &&
					string.CompareOrdinal(var.StringValue, @"Te\'st") == 0);
			}
		}

		[Test]
		public void DoubleNestedSection()
		{
			string testData = @"ASDF{test1=2.13asection{}}fdsa#comment#{a_nother321section{_test3=321}test2='Te\'st'}";
			using (MemoryStream ms = GetStream(testData))
			{
				Config cfg = new Config();
				cfg.Load(ms);

				Assert.IsTrue(
					cfg.Root != null);

				Section asdf = cfg.Root.Child("asdf");

				Assert.IsTrue(
					asdf != null);

				Variable var = asdf.Variable("test1");

				Assert.IsTrue(
					var != null &&
					var.RealValue == 2.13d);

				Section asection = asdf.Child("asection");

				Assert.IsTrue(
					asection != null &&
					asection.Variables.Count() == 0);

				Section fdsa = cfg.Root.Child("fdsa");
				
				Assert.IsTrue(
					fdsa != null);

				var = fdsa.Variable("test2");
				
				Assert.IsTrue(
					var != null &&
					string.CompareOrdinal(var.StringValue, @"Te\'st") == 0);

				Section a_nother321section = fdsa.Child("a_nother321section");

				Assert.IsTrue(
					a_nother321section != null);

				var = a_nother321section.Variable("_test3");

				Assert.IsTrue(
					var != null &&
					var.IntegerValue == 321);
			}
		}
	}
}
