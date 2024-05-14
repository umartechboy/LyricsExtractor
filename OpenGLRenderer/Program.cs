using GLFW;
using SkiaSharp;
using System.Runtime.InteropServices;
namespace OpenGLRenderer
{
	public class GRContextCreator
	{
		static GRContext GenerateSkiaContext(NativeWindow nativeWindow)
		{
			var nativeContext = GetNativeContext(nativeWindow);
			var glInterface = GRGlInterface.AssembleGlInterface(nativeContext, (contextHandle, name) => Glfw.GetProcAddress(name));
			return GRContext.Create(GRBackend.OpenGL, glInterface);
		}
		static object GetNativeContext(NativeWindow nativeWindow)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return Native.GetWglContext(nativeWindow);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// XServer
				return Native.GetGLXContext(nativeWindow);
				// Wayland
				//return Native.GetEglContext(nativeWindow);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return Native.GetNSGLContext(nativeWindow);
			}

			throw new PlatformNotSupportedException();
		}
		public static GRContext Get(int width, int height)
		{
			// Set some common hints for the OpenGL profile creation
			GLFW.Glfw.WindowHint(GLFW.Hint.ClientApi, GLFW.ClientApi.OpenGL);
			GLFW.Glfw.WindowHint(GLFW.Hint.ContextVersionMajor, 3);
			GLFW.Glfw.WindowHint(GLFW.Hint.ContextVersionMinor, 3);
			GLFW.Glfw.WindowHint(GLFW.Hint.OpenglProfile, GLFW.Profile.Core);
			GLFW.Glfw.WindowHint(GLFW.Hint.Doublebuffer, true);
			GLFW.Glfw.WindowHint(GLFW.Hint.Decorated, false);
			GLFW.Glfw.WindowHint(GLFW.Hint.OpenglForwardCompatible, true);
			GLFW.Glfw.WindowHint(GLFW.Hint.TransparentFramebuffer, true);
			var window = new NativeWindow(width, height, "temp");

			GLFW.Glfw.SetWindowAttribute(window, GLFW.WindowAttribute.Decorated, false);
			GLFW.Glfw.SetWindowAttribute(window, GLFW.WindowAttribute.Resizable, true);
			GLFW.Glfw.SetWindowSize(window, window.Bounds.Width, window.Bounds.Height);
			GLFW.Glfw.SwapInterval(0); // 0 has better smoothness and lesser tearing
			window.CenterOnScreen();
			return GenerateSkiaContext(window);
		}
	}
}
