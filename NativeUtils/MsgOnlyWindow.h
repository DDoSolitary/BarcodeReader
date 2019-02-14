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

	public ref class MsgOnlyWindow {
	private:
		static const _TCHAR *const WndClassName = _T("MessageOnly");
		HWND _hWnd;
		Thread ^const _loopThread;
		ManualResetEventSlim ^const _signal;
		void MsgLoop();

	internal:
		static String ^const TlsName = "WindowObject";
		void WndProc(UINT msg, WPARAM wParam, LPARAM lParam);

	public:
		event OnHotKeyEventHandler ^OnHotKey;
		event OnClipboardUpdateEventHandler ^OnClipboardUpdate;
		static MsgOnlyWindow();
		MsgOnlyWindow();
		void RegisterHotKey(int32_t id, KeyModifier fsModifiers, uint32_t vk);
		void Close();
	};
}
