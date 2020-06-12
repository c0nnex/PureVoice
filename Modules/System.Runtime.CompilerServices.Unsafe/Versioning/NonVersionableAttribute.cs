using System;

namespace System.Runtime.Versioning
{
	// Token: 0x02000003 RID: 3
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	internal sealed class NonVersionableAttribute : Attribute
	{
	}
}
