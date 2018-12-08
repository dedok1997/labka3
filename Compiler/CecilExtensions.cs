using Mono.Cecil;
using System.Collections.Generic;
namespace ExtraLab2018.Compiler {
	sealed class TypeReferenceEqualityComparer : IEqualityComparer<TypeReference> {
		public static readonly TypeReferenceEqualityComparer Instance = new TypeReferenceEqualityComparer();
		public static bool EqualTo(TypeReference x, TypeReference y) {
			return x == y || x != null && y != null && x.FullName == y.FullName;
		}
		public bool Equals(TypeReference x, TypeReference y) {
			return EqualTo(x, y);
		}
		public int GetHashCode(TypeReference obj) {
			return obj.FullName.GetHashCode();
		}
	}
	static class CecilExtensions {
		public static bool EqualTo(this TypeReference a, TypeReference b) {
			return TypeReferenceEqualityComparer.EqualTo(a, b);
		}
	}
}
