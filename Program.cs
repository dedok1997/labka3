using ExtraLab2018.Nodes;
using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;
namespace ExtraLab2018 {



	sealed class Program {
		static ProgramNode CheckedParse(string code) {
			var programNode = Parser.Parse(code);
			var code2 = programNode.FormattedString;
			var programNode2 = Parser.Parse(code2);
			var code3 = programNode2.FormattedString;
			if (code2 != code3) {
				Console.WriteLine(code2);
				Console.WriteLine(code3);
				throw new Exception("Кривой парсер или ToString у узлов");
			}
			return programNode;
		}
		static void Main(string[] args) {
			string code = File.ReadAllText(@"..\..\code.txt");
			var programNode = CheckedParse(code);
			var module = ModuleDefinition.CreateModule("out", ModuleKind.Console);
			var types = new Compiler.AllTypes(module);
			new Compiler.ProgramCompiler(types).Compile(programNode, "Entry", "Main");
			module.Write("out.exe");
			Assembly.LoadFrom("out.exe").GetType("Entry").GetMethod("Main").Invoke(null, new object[] { });
		}
	}
}
