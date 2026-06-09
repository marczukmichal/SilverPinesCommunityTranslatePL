using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Serialization;

public static class CustomFormatTextureSerializer
{
	private const uint Version = 1u;

	public static byte[] SerializeToCustomBytesPNGFormat(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		if (!texture.isReadable)
		{
			throw new InvalidOperationException("Texture '" + texture.name + "' is not readable. Enable Read/Write or create a readable copy.");
		}
		byte[] raw = texture.EncodeToPNG();
		return texture.SerializeToCustomBytesFormat(raw);
	}

	public static byte[] SerializeToCustomBytesJPGFormat(this Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		if (!texture.isReadable)
		{
			throw new InvalidOperationException("Texture '" + texture.name + "' is not readable. Enable Read/Write or create a readable copy.");
		}
		byte[] raw = texture.EncodeToJPG();
		return texture.SerializeToCustomBytesFormat(raw);
	}

	private static byte[] SerializeToCustomBytesFormat(this Texture2D texture, byte[] raw)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		if (!texture.isReadable)
		{
			throw new InvalidOperationException("Texture '" + texture.name + "' is not readable. Enable Read/Write or create a readable copy.");
		}
		int num = 0;
		num += 4;
		num += 4;
		num += 4;
		num += 4;
		num++;
		num += 4;
		using MemoryStream memoryStream = new MemoryStream(raw.Length + num);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(1u);
		binaryWriter.Write(texture.width);
		binaryWriter.Write(texture.height);
		binaryWriter.Write((int)texture.graphicsFormat);
		binaryWriter.Write(texture.isDataSRGB);
		binaryWriter.Write(raw.Length);
		binaryWriter.Write(raw);
		return memoryStream.ToArray();
	}

	public static void LoadFromCustomBytesFormat(this Texture2D texture, byte[] bytes)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		if (bytes == null || bytes.Length == 0)
		{
			throw new ArgumentException("Empty texture bytes.", "bytes");
		}
		using MemoryStream input = new MemoryStream(bytes);
		using BinaryReader binaryReader = new BinaryReader(input);
		uint num = binaryReader.ReadUInt32();
		if (num != 1)
		{
			throw new InvalidDataException($"Unsupported texture version: {num}. Upgrading needs implementing here if required.");
		}
		int width = binaryReader.ReadInt32();
		int height = binaryReader.ReadInt32();
		GraphicsFormat format = (GraphicsFormat)binaryReader.ReadInt32();
		bool flag = binaryReader.ReadBoolean();
		int num2 = binaryReader.ReadInt32();
		if (num2 <= 0)
		{
			throw new InvalidDataException("Invalid raw texture length.");
		}
		byte[] array = binaryReader.ReadBytes(num2);
		if (array.Length != num2)
		{
			throw new EndOfStreamException("Unexpected end of texture data.");
		}
		texture.Reinitialize(width, height, format, hasMipMap: false);
		if (texture.isDataSRGB != flag)
		{
			Debug.LogError("Miss match with sRGB of saved texture and texture to load into. Values will look different.");
		}
		texture.LoadImage(array);
		texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
	}
}
