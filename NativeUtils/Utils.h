#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Threading;

namespace NativeUtils {
	public delegate void OnHotKeyEventHandler(int32_t id);
	public delegate void OnClipboardUpdateEventHandler();

	[FlagsAttribute]
	public enum class KeyModifier : uint32_t {
		Alt = MOD_ALT,
		Control = MOD_CONTROL,
		NoRepeat = MOD_NOREPEAT,
		Shift = MOD_SHIFT,
		Win = MOD_WIN
	};

	public ref class Utils abstract sealed {
	private:
		static HWND _hWnd;
		static Thread ^_msgLoopThread;
		static void MessageLoop();

	internal:
		static void WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

	public:
		static event OnHotKeyEventHandler ^OnHotKey;
		static event OnClipboardUpdateEventHandler ^OnClipboardUpdate;
		static Utils();
		static void RegisterHotKey(int32_t id, KeyModifier fsModfiers, uint32_t vk);
	};
}
