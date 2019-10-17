using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace GLSand
{
	public class Texture
	{
		public int Handle { get; set; }

		public Texture(int width, int height, Color color, bool write)
		{
			Handle = GL.GenTexture();
			Use();

			byte[] pixels = new byte[width * height * 4];
			for(int i = 0; i < width * height * 4; i += 4)
			{
				bool val = Game.Rand.Next(8) == 0;
				pixels[i] = val ? color.R : (byte)0;
				pixels[i+1] = val ? color.G : (byte)0;
				pixels[i+2] = val ? color.B : (byte)0;
				pixels[i+3] = color.A;
			}

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

			if (write)
			{
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

				GL.BindImageTexture(0, Handle, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
			}
			else
			{
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
			}
		}

		public void Use()
		{
			GL.BindTexture(TextureTarget.Texture2D, Handle);
		}

		bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				GL.DeleteProgram(Handle);

				disposedValue = true;
			}
		}

		~Texture()
		{
			GL.DeleteTexture(Handle);
		}


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
