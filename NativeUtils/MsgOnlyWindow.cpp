#include "pch.h"
#include "MsgOnlyWindow.h"

#pragma warning(disable: 4244)

namespace NativeUtils {
	void Win32Assert(bool assertion) {
		if (!assertion) throw gcnew Win32Exception();
	}

	LRESULT CALLBACK WndProcWrapper(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam) {
		auto slot = Thread::GetNamedDataSlot(MsgOnlyWindow::TlsName);
		static_cast<MsgOnlyWindow ^>(Thread::GetData(slot))->WndProc(msg, wParam, lParam);
		return DefWindowProc(hWnd, msg, wParam, lParam);
	}

	void MsgOnlyWindow::WndProc(UINT msg, WPARAM wParam, LPARAM lParam) {
		switch (msg) {
		case WM_HOTKEY:
			this->OnHotKey(wParam);
			break;
		case WM_CLIPBOARDUPDATE:
			this->OnClipboardUpdate();
			break;
		case WM_USER:
			Win32Assert(::RegisterHotKey(_hWnd, wParam, lParam & 0xffff, lParam >> 16));
			break;
		case WM_DESTROY:
			PostQuitMessage(0);
			break;
		default:
			break;
		}
	}

	void MsgOnlyWindow::MsgLoop() {
		_hWnd = CreateWindow(WndClassName, nullptr, 0, 0, 0, 0, 0, HWND_MESSAGE, nullptr, nullptr, nullptr);
		Win32Assert(_hWnd != nullptr && AddClipboardFormatListener(_hWnd));
		_signal->Set();
		Thread::SetData(Thread::GetNamedDataSlot(TlsName), this);
		MSG msg;
		BOOL res;
		while ((res = GetMessage(&msg, nullptr, 0, 0)) != 0) {
			Win32Assert(res != -1);
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	static MsgOnlyWindow::MsgOnlyWindow() {
		WNDCLASS cls = {};
		cls.lpfnWndProc = &WndProcWrapper;
		cls.lpszClassName = WndClassName;
		Win32Assert(RegisterClass(&cls) != 0);
	}

	MsgOnlyWindow::MsgOnlyWindow() :
		_loopThread(gcnew Thread(gcnew ThreadStart(this, &MsgOnlyWindow::MsgLoop))),
		_signal(gcnew ManualResetEventSlim(false))
	{
		_loopThread->Start();
		_signal->Wait();
	}

	void MsgOnlyWindow::RegisterHotKey(int32_t id, KeyModifier fsModifiers, uint32_t vk) {
		Win32Assert(PostMessage(_hWnd, WM_USER, id, (vk << 16) + static_cast<uint32_t>(fsModifiers)));
	}

	void MsgOnlyWindow::Close() {
		Win32Assert(PostMessage(_hWnd, WM_CLOSE, 0, 0));
		delete _signal;
	}
}
