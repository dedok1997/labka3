using System;
public static class BuiltinFunctions {
	public static void dump() {
		Console.WriteLine();
	}
	public static void dump(int v) {
		Console.WriteLine(v);
	}
	public static void dump(bool v) {
		Console.WriteLine(v ? "true" : "false");
	}
}
