using System;
using System.Runtime.Versioning;

namespace System.Runtime.CompilerServices
{
	/// <summary>Contains generic, low-level functionality for manipulating pointers.</summary>
	// Token: 0x02000002 RID: 2
	public static class Unsafe
	{
		/// <summary>Reads a value of type <typeparamref name="T" /> from the given location.</summary>
		/// <param name="source">The location to read from.</param>
		/// <typeparam name="T">The type to read.</typeparam>
		/// <returns>An object of type <typeparamref name="T" /> read from the given location.</returns>
		// Token: 0x06000001 RID: 1 RVA: 0x000020D0 File Offset: 0x000002D0
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static T Read<T>(void* source)
		{
			return *(T*)source;
		}

		/// <summary>Reads a value of type <typeparamref name="T" /> from the given location without assuming architecture dependent alignment of the addresses.</summary>
		/// <param name="source">The location to read from.</param>
		/// <typeparam name="T">The type to read.</typeparam>
		/// <returns>An object of type <typeparamref name="T" /> read from the given location.</returns>
		// Token: 0x06000002 RID: 2 RVA: 0x000020E4 File Offset: 0x000002E4
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static T ReadUnaligned<T>(void* source)
		{
			return *(T*)source;
		}

		/// <summary>Reads a value of type <typeparamref name="T" /> from the given location without assuming architecture dependent alignment of the addresses.</summary>
		/// <param name="source">The location to read from.</param>
		/// <typeparam name="T">The type to read.</typeparam>
		/// <returns>An object of type <typeparamref name="T" /> read from the given location.</returns>
		// Token: 0x06000003 RID: 3 RVA: 0x000020FC File Offset: 0x000002FC
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadUnaligned<T>(ref byte source)
		{
			return source;
		}

		/// <summary>Writes a value of type <typeparamref name="T" /> to the given location.</summary>
		/// <param name="destination">The location to write to.</param>
		/// <param name="value">The value to write.</param>
		/// <typeparam name="T">The type of value to write.</typeparam>
		// Token: 0x06000004 RID: 4 RVA: 0x00002114 File Offset: 0x00000314
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void Write<T>(void* destination, T value)
		{
			*(T*)destination = value;
		}

		/// <summary>Writes a value of type <typeparamref name="T" /> to the given location without assuming architecture dependent alignment of the addresses.</summary>
		/// <param name="destination">The location to write to.</param>
		/// <param name="value">The value to write.</param>
		/// <typeparam name="T">The type of value to write.</typeparam>
		// Token: 0x06000005 RID: 5 RVA: 0x00002128 File Offset: 0x00000328
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void WriteUnaligned<T>(void* destination, T value)
		{
			*(T*)destination = value;
		}

		/// <summary>Writes a value of type <typeparamref name="T" /> to the given location without assuming architecture dependent alignment of the addresses.</summary>
		/// <param name="destination">The location to write to.</param>
		/// <param name="value">The value to write.</param>
		/// <typeparam name="T">The type of value to write.</typeparam>
		// Token: 0x06000006 RID: 6 RVA: 0x00002140 File Offset: 0x00000340
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUnaligned<T>(ref byte destination, T value)
		{
			destination = value;
		}

		/// <summary>Copies a value of type <typeparamref name="T" /> to the given location.</summary>
		/// <param name="destination">The location to copy to.</param>
		/// <param name="source">A reference to the value to copy.</param>
		/// <typeparam name="T">The type of value to copy.</typeparam>
		// Token: 0x06000007 RID: 7 RVA: 0x00002158 File Offset: 0x00000358
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void Copy<T>(void* destination, ref T source)
		{
			*(T*)destination = source;
		}

		/// <summary>Copies a value of type <typeparamref name="T" /> to the given location.</summary>
		/// <param name="destination">The location to copy to.</param>
		/// <param name="source">A pointer to the value to copy.</param>
		/// <typeparam name="T">The type of value to copy.</typeparam>
		// Token: 0x06000008 RID: 8 RVA: 0x00002174 File Offset: 0x00000374
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void Copy<T>(ref T destination, void* source)
		{
			destination = *(T*)source;
		}

		/// <summary>Returns a pointer to the given by-ref parameter.</summary>
		/// <param name="value">The object whose pointer is obtained.</param>
		/// <typeparam name="T">The type of object.</typeparam>
		/// <returns>A pointer to the given value.</returns>
		// Token: 0x06000009 RID: 9 RVA: 0x00002190 File Offset: 0x00000390
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void* AsPointer<T>(ref T value)
		{
			return (void*)(&value);
		}

		/// <summary>Returns the size of an object of the given type parameter.</summary>
		/// <typeparam name="T">The type of object whose size is retrieved.</typeparam>
		/// <returns>The size of an object of type <typeparamref name="T" />.</returns>
		// Token: 0x0600000A RID: 10 RVA: 0x000021A0 File Offset: 0x000003A0
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOf<T>()
		{
			return sizeof(T);
		}

		/// <summary>Copies bytes from the source address to the destination address.</summary>
		/// <param name="destination">The destination address to copy to.</param>
		/// <param name="source">The source address to copy from.</param>
		/// <param name="byteCount">The number of bytes to copy.</param>
		// Token: 0x0600000B RID: 11 RVA: 0x000021B4 File Offset: 0x000003B4
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void CopyBlock(void* destination, void* source, uint byteCount)
		{
			cpblk(destination, source, byteCount);
		}

		/// <summary>Copies bytes from the source address to the destination address.</summary>
		/// <param name="destination">The destination address to copy to.</param>
		/// <param name="source">The source address to copy from.</param>
		/// <param name="byteCount">The number of bytes to copy.</param>
		// Token: 0x0600000C RID: 12 RVA: 0x000021C8 File Offset: 0x000003C8
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyBlock(ref byte destination, ref byte source, uint byteCount)
		{
			cpblk(ref destination, ref source, byteCount);
		}

		/// <summary>Copies bytes from the source address to the destination address without assuming architecture dependent alignment of the addresses.</summary>
		/// <param name="destination">The destination address to copy to.</param>
		/// <param name="source">The source address to copy from.</param>
		/// <param name="byteCount">The number of bytes to copy.</param>
		// Token: 0x0600000D RID: 13 RVA: 0x000021DC File Offset: 0x000003DC
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void CopyBlockUnaligned(void* destination, void* source, uint byteCount)
		{
			cpblk(destination, source, byteCount);
		}

		/// <summary>Copies bytes from the source address to the destination address without assuming architecture dependent alignment of the addresses.</summary>
		/// <param name="destination">The destination address to copy to.</param>
		/// <param name="source">The source address to copy from.</param>
		/// <param name="byteCount">The number of bytes to copy.</param>
		// Token: 0x0600000E RID: 14 RVA: 0x000021F4 File Offset: 0x000003F4
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount)
		{
			cpblk(ref destination, ref source, byteCount);
		}

		/// <summary>Initializes a block of memory at the given location with a given initial value.</summary>
		/// <param name="startAddress">The address of the start of the memory block to initialize.</param>
		/// <param name="value">The value to initialize the block to.</param>
		/// <param name="byteCount">The number of bytes to initialize.</param>
		// Token: 0x0600000F RID: 15 RVA: 0x0000220C File Offset: 0x0000040C
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void InitBlock(void* startAddress, byte value, uint byteCount)
		{
			initblk(startAddress, value, byteCount);
		}

		/// <summary>Initializes a block of memory at the given location with a given initial value.</summary>
		/// <param name="startAddress">The address of the start of the memory block to initialize.</param>
		/// <param name="value">The value to initialize the block to.</param>
		/// <param name="byteCount">The number of bytes to initialize.</param>
		// Token: 0x06000010 RID: 16 RVA: 0x00002220 File Offset: 0x00000420
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void InitBlock(ref byte startAddress, byte value, uint byteCount)
		{
			initblk(ref startAddress, value, byteCount);
		}

		/// <summary>Initializes a block of memory at the given location with a given initial value without assuming architecture dependent alignment of the address.</summary>
		/// <param name="startAddress">The address of the start of the memory block to initialize.</param>
		/// <param name="value">The value to initialize the block to.</param>
		/// <param name="byteCount">The number of bytes to initialize.</param>
		// Token: 0x06000011 RID: 17 RVA: 0x00002234 File Offset: 0x00000434
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount)
		{
			initblk(startAddress, value, byteCount);
		}

		/// <summary>Initializes a block of memory at the given location with a given initial value without assuming architecture dependent alignment of the address.</summary>
		/// <param name="startAddress">The address of the start of the memory block to initialize.</param>
		/// <param name="value">The value to initialize the block to.</param>
		/// <param name="byteCount">The number of bytes to initialize.</param>
		// Token: 0x06000012 RID: 18 RVA: 0x0000224C File Offset: 0x0000044C
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
		{
			initblk(ref startAddress, value, byteCount);
		}

		/// <summary>Casts the given object to the specified type.</summary>
		/// <param name="o">The object to cast.</param>
		/// <typeparam name="T">The type which the object will be cast to.</typeparam>
		/// <returns>The original object, casted to the given type.</returns>
		// Token: 0x06000013 RID: 19 RVA: 0x00002264 File Offset: 0x00000464
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T As<T>(object o) where T : class
		{
			return o;
		}

		/// <summary>Reinterprets the given location as a reference to a value of type <typeparamref name="T" />.</summary>
		/// <param name="source">The location of the value to reference.</param>
		/// <typeparam name="T">The type of the interpreted location.</typeparam>
		/// <returns>A reference to a value of type <typeparamref name="T" />.</returns>
		// Token: 0x06000014 RID: 20 RVA: 0x00002274 File Offset: 0x00000474
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static ref T AsRef<T>(void* source)
		{
			return ref *(T*)source;
		}

		/// <summary>Reinterprets the given read-only reference as a reference.</summary>
		/// <param name="source">The read-only reference to reinterpret.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>A reference to a value of type <typeparamref name="T" />.</returns>
		// Token: 0x06000015 RID: 21 RVA: 0x00002284 File Offset: 0x00000484
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AsRef<T>(in T source)
		{
			return ref source;
		}

		/// <summary>Reinterprets the given reference as a reference to a value of type <typeparamref name="TTo" />.</summary>
		/// <param name="source">The reference to reinterpret.</param>
		/// <typeparam name="TFrom">The type of reference to reinterpret.</typeparam>
		/// <typeparam name="TTo">The desired type of the reference.</typeparam>
		/// <returns>A reference to a value of type <typeparamref name="TTo" />.</returns>
		// Token: 0x06000016 RID: 22 RVA: 0x00002294 File Offset: 0x00000494
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref TTo As<TFrom, TTo>(ref TFrom source)
		{
			return ref source;
		}

		/// <summary>Returns a <see langword="mutable ref" /> to a boxed value.</summary>
		/// <param name="box">The value to unbox.</param>
		/// <typeparam name="T">The type to be unboxed.</typeparam>
		/// <returns>A <see langword="mutable ref" /> to the boxed value <paramref name="box" />.</returns>
		/// <exception cref="T:System.NullReferenceException">
		///   <paramref name="box" /> is <see langword="null" />, and <typeparamref name="T" /> is a non-nullable value type.</exception>
		/// <exception cref="T:System.InvalidCastException">
		///   <paramref name="box" /> is not a boxed value type.
		/// -or-
		/// <paramref name="box" /> is not a boxed <typeparamref name="T" />.</exception>
		/// <exception cref="T:System.TypeLoadException">
		///   <typeparamref name="T" /> cannot be found.</exception>
		// Token: 0x06000017 RID: 23 RVA: 0x000022A4 File Offset: 0x000004A4
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Unbox<T>(object box) where T : struct
		{
			return ref (T)box;
		}

		/// <summary>Adds an element offset to the given reference.</summary>
		/// <param name="source">The reference to add the offset to.</param>
		/// <param name="elementOffset">The offset to add.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>A new reference that reflects the addition of offset to pointer.</returns>
		// Token: 0x06000018 RID: 24 RVA: 0x000022B8 File Offset: 0x000004B8
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Add<T>(ref T source, int elementOffset)
		{
			return ref source + (IntPtr)elementOffset * (IntPtr)sizeof(T);
		}

		/// <summary>Adds an element offset to the given void pointer.</summary>
		/// <param name="source">The void pointer to add the offset to.</param>
		/// <param name="elementOffset">The offset to add.</param>
		/// <typeparam name="T">The type of void pointer.</typeparam>
		/// <returns>A new void pointer that reflects the addition of offset to the specified pointer.</returns>
		// Token: 0x06000019 RID: 25 RVA: 0x000022D0 File Offset: 0x000004D0
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void* Add<T>(void* source, int elementOffset)
		{
			return (void*)((byte*)source + (IntPtr)elementOffset * (IntPtr)sizeof(T));
		}

		/// <summary>Adds an element offset to the given reference.</summary>
		/// <param name="source">The reference to add the offset to.</param>
		/// <param name="elementOffset">The offset to add.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>A new reference that reflects the addition of offset to pointer.</returns>
		// Token: 0x0600001A RID: 26 RVA: 0x000022E8 File Offset: 0x000004E8
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Add<T>(ref T source, IntPtr elementOffset)
		{
			return ref source + elementOffset * (IntPtr)sizeof(T);
		}

		/// <summary>Adds a byte offset to the given reference.</summary>
		/// <param name="source">The reference to add the offset to.</param>
		/// <param name="byteOffset">The offset to add.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>A new reference that reflects the addition of byte offset to pointer.</returns>
		// Token: 0x0600001B RID: 27 RVA: 0x00002300 File Offset: 0x00000500
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
		{
			return ref source + byteOffset;
		}

		/// <summary>Subtracts an element offset from the given reference.</summary>
		/// <param name="source">The reference to subtract the offset from.</param>
		/// <param name="elementOffset">The offset to subtract.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>A new reference that reflects the subtraction of offset from pointer.</returns>
		// Token: 0x0600001C RID: 28 RVA: 0x00002310 File Offset: 0x00000510
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Subtract<T>(ref T source, int elementOffset)
		{
			return ref source - (IntPtr)elementOffset * (IntPtr)sizeof(T);
		}

		/// <summary>Subtracts an element offset from the given void pointer.</summary>
		/// <param name="source">The void pointer to subtract the offset from.</param>
		/// <param name="elementOffset">The offset to subtract.</param>
		/// <typeparam name="T">The type of the void pointer.</typeparam>
		/// <returns>A new void pointer that reflects the subtraction of offset from the specified pointer.</returns>
		// Token: 0x0600001D RID: 29 RVA: 0x00002328 File Offset: 0x00000528
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void* Subtract<T>(void* source, int elementOffset)
		{
			return (void*)((byte*)source - (IntPtr)elementOffset * (IntPtr)sizeof(T));
		}

		/// <summary>Subtracts an element offset from the given reference.</summary>
		/// <param name="source">The reference to subtract the offset from.</param>
		/// <param name="elementOffset">The offset to subtract.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>A new reference that reflects the subtraction of offset from pointer.</returns>
		// Token: 0x0600001E RID: 30 RVA: 0x00002340 File Offset: 0x00000540
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Subtract<T>(ref T source, IntPtr elementOffset)
		{
			return ref source - elementOffset * (IntPtr)sizeof(T);
		}

		/// <summary>Subtracts a byte offset from the given reference.</summary>
		/// <param name="source">The reference to subtract the offset from.</param>
		/// <param name="byteOffset">The offset to subtract.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>A new reference that reflects the subtraction of byte offset from pointer.</returns>
		// Token: 0x0600001F RID: 31 RVA: 0x00002358 File Offset: 0x00000558
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset)
		{
			return ref source - byteOffset;
		}

		/// <summary>Determines the byte offset from origin to target from the given references.</summary>
		/// <param name="origin">The reference to origin.</param>
		/// <param name="target">The reference to target.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>Byte offset from origin to target i.e. <paramref name="target" /> - <paramref name="origin" />.</returns>
		// Token: 0x06000020 RID: 32 RVA: 0x00002368 File Offset: 0x00000568
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntPtr ByteOffset<T>(ref T origin, ref T target)
		{
			return ref target - ref origin;
		}

		/// <summary>Determines whether the specified references point to the same location.</summary>
		/// <param name="left">The first reference to compare.</param>
		/// <param name="right">The second reference to compare.</param>
		/// <typeparam name="T">The type of reference.</typeparam>
		/// <returns>
		///   <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> point to the same location; otherwise, <see langword="false" />.</returns>
		// Token: 0x06000021 RID: 33 RVA: 0x00002378 File Offset: 0x00000578
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreSame<T>(ref T left, ref T right)
		{
			return ref left == ref right;
		}

		/// <summary>Returns a value that indicates whether a specified reference is greater than another specified reference.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <typeparam name="T">The type of the reference.</typeparam>
		/// <returns>
		///   <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		// Token: 0x06000022 RID: 34 RVA: 0x0000238C File Offset: 0x0000058C
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAddressGreaterThan<T>(ref T left, ref T right)
		{
			return ref left != ref right;
		}

		/// <summary>Returns a value that indicates whether a specified reference is less than another specified reference.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <typeparam name="T">The type of the reference.</typeparam>
		/// <returns>
		///   <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		// Token: 0x06000023 RID: 35 RVA: 0x000023A0 File Offset: 0x000005A0
		[NonVersionable]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAddressLessThan<T>(ref T left, ref T right)
		{
			return ref left < ref right;
		}
	}
}
