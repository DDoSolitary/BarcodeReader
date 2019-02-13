#include "pch.h"
#include "Utils.h"

namespace NativeUtils {
	void Win32Assert(bool assertion) {
		if (!assertion) throw gcnew Win32Exception();
	}

	LRESULT CALLBACK WndProcWrapper(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam) {
		Utils::WndProc(hWnd, msg, wParam, lParam);
		return DefWindowProc(hWnd, msg, wParam, lParam);
	}

	void Utils::WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam) {
		switch (msg) {
		case WM_HOTKEY:
			OnHotKey(static_cast<int32_t>(wParam));
			break;
		case WM_CLIPBOARDUPDATE:
			OnClipboardUpdate();
			break;
		default:
			break;
		}
	}

	void Utils::MessageLoop() {
		MSG msg;
		BOOL res;
		while ((res = GetMessage(&msg, _hWnd, 0, 0)) != 0) {
			Win32Assert(res != -1);
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	static Utils::Utils() {
		WNDCLASS cls = {};
		cls.lpfnWndProc = &WndProcWrapper;
		const auto clsName = _T("MessageOnly");
		cls.lpszClassName = clsName;
		if (RegisterClass(&cls) == 0) throw gcnew Win32Exception();
		_hWnd = CreateWindow(clsName, nullptr, 0, 0, 0, 0, 0, HWND_MESSAGE, nullptr, nullptr, nullptr);
		Win32Assert(_hWnd != nullptr && AddClipboardFormatListener(_hWnd));
		_msgLoopThread = gcnew Thread(gcnew ThreadStart(MessageLoop));
		_msgLoopThread->Start();
	}

	void Utils::RegisterHotKey(int32_t id, KeyModifier fsModfiers, uint32_t vk) {
		Win32Assert(::RegisterHotKey(_hWnd, id, static_cast<uint32_t>(fsModfiers), vk));
	}
}
