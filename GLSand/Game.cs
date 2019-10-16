using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace GLSand
{
	public class Game : GameWindow
	{
		public static Random Rand { get; set; }
		public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
		{
			Rand = new Random();
		}

		float[] vertices = {
			 1f,  1f, 0.0f, 1.0f, 1.0f, // top right
			 1f, -1f, 0.0f, 1.0f, 0.0f, // bottom right
			-1f, -1f, 0.0f, 0.0f, 0.0f, // bottom left
			-1f,  1f, 0.0f, 0.0f, 1.0f  // top left
		};

		uint[] indices = {  // note that we start from 0!
			0, 1, 3,   // first triangle
			1, 2, 3    // second triangle
		};

		int VertexBufferObject;

		int VertexArrayObject;

		int ElementBufferObject;

		int RenderTarget0;
		int RenderTarget1;

		int computeShader;

		Shader shader;
		Texture texture;
		int renderedTexture0;
		int renderedTexture1;
		Texture noiseTexture;

		int destTex;

		bool firstframe = true;
		bool rt0 = true;

		Point texSize = new Point(256, 256);

		float time;

		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(0.0f, 0f, 0f, 1.0f);

			

			// Vertex Buffer Object initialization
			VertexBufferObject = GL.GenBuffer();

			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

			


			// Element Buffer Object initialization
			ElementBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

			shader = new Shader("shader.vert", "shader.frag");
			shader.Use();

			computeShader = SetupComputeProgram(computeShader);

			texture = new Texture(256, 256, new Color(255, 255, 255, 255), true);
			texture.Use();

			noiseTexture = new Texture(256, 256, new Color(255, 255, 255, 255), false);
			noiseTexture.Use();

			RenderTarget0 = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderTarget0);

			renderedTexture0 = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, renderedTexture0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 256, 256, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);


			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTexture0, 0);

			

			RenderTarget1 = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderTarget1);

			renderedTexture1 = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, renderedTexture1);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 256, 256, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);


			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTexture1, 0);

			DrawBuffersEnum[] DrawBuffers = new DrawBuffersEnum[1] { DrawBuffersEnum.ColorAttachment0 };
			GL.DrawBuffers(1, DrawBuffers); // "1" is the size of DrawBuffers

			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
				Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());

			//Vertex array object initialization

			VertexArrayObject = GL.GenVertexArray();

			GL.BindVertexArray(VertexArrayObject);

			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

			// We bind the EBO here too, just like with the VBO in the previous tutorial.
			// Now, the EBO will be bound when we bind the VAO.
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			int texCoordLocation = shader.GetAttribLocation("aTexCoord");
			GL.EnableVertexAttribArray(texCoordLocation);
			GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

			base.OnLoad(e);
		}

		protected override void OnUnload(EventArgs e)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.DeleteBuffer(VertexBufferObject);

			shader.Dispose();
			texture.Dispose();

			base.OnUnload(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			KeyboardState input = Keyboard.GetState();

			if (input.IsKeyDown(Key.Escape))
			{
				Exit();
			}

			if (input.IsKeyDown(Key.Space))
			{
				firstframe = true;
			}

			time += 0.016f;

			

			base.OnUpdateFrame(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if(rt0) GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderTarget0);
			else	GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderTarget1);
			GL.Viewport(0, 0, Width, Height);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.BindVertexArray(VertexArrayObject);

			
			if (firstframe)
			{
				firstframe = false;
				shader.Use();
				noiseTexture.Use();
			}
			else
			{
				GL.UseProgram(computeShader);
				if(rt0) GL.BindTexture(TextureTarget.Texture2D, renderedTexture1);
				else	GL.BindTexture(TextureTarget.Texture2D, renderedTexture0);
				GL.Uniform1(GL.GetUniformLocation(computeShader, "destTex"), 0);
				if(rt0) GL.Uniform1(GL.GetUniformLocation(computeShader, "sourceTex"), renderedTexture1);
				else	GL.Uniform1(GL.GetUniformLocation(computeShader, "sourceTex"), renderedTexture0);
				GL.Uniform1(GL.GetUniformLocation(computeShader, "roll"), time);
				GL.Uniform1(GL.GetUniformLocation(computeShader, "firstframe"), firstframe ? 1 : 0);
				GL.DispatchCompute(texSize.X / 16, texSize.Y / 16, 1);
				texture.Use();
				shader.Use();
			}



			GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Viewport(0, 0, Width, Height);

			if (rt0) GL.BindTexture(TextureTarget.Texture2D, renderedTexture0);
			else GL.BindTexture(TextureTarget.Texture2D, renderedTexture1);
			shader.Use();

			GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

			Context.SwapBuffers();

			rt0 = !rt0;
			base.OnRenderFrame(e);
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
			base.OnResize(e);
		}

		private int SetupComputeProgram(int texHandle)
		{
			// Creating the compute shader, and the program object containing the shader
			int progHandle = GL.CreateProgram();
			int cs = GL.CreateShader(ShaderType.ComputeShader);
			string source;

			using (StreamReader reader = new StreamReader("shader.comp", Encoding.UTF8))
			{
				source = reader.ReadToEnd();
			}


			GL.ShaderSource(cs, source);

			GL.CompileShader(cs);

			int rvalue;
			GL.GetShader(cs, ShaderParameter.CompileStatus, out rvalue);
			if (rvalue != (int)All.True)
			{
				Console.WriteLine(GL.GetShaderInfoLog(cs));
			}
			GL.AttachShader(progHandle, cs);

			GL.LinkProgram(progHandle);
			GL.GetProgram(progHandle, GetProgramParameterName.LinkStatus, out rvalue);
			if (rvalue != (int)All.True)
			{
				Console.WriteLine(GL.GetProgramInfoLog(progHandle));
			}

			GL.UseProgram(progHandle);

			GL.Uniform1(GL.GetUniformLocation(progHandle, "destTex"), 0);

			//checkErrors("Compute shader");
			return progHandle;
		}
	}
}
