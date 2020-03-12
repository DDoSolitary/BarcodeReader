#include "pch.h"
#include "MsgOnlyWindow.h"

#pragma warning(disable: 4244)

namespace NativeUtils {
	void Win32Assert(bool assertion) {
		if (!assertion) throw gcnew Win32Exception(GetLastError());
	}

	void ShowError(LPCTSTR msg) {
		MessageBox(nullptr, msg, _T("Error - BarcodeReader"), MB_OK | MB_ICONWARNING | MB_TASKMODAL | MB_SETFOREGROUND | MB_TOPMOST);

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
			if (!::RegisterHotKey(_hWnd, wParam, lParam & 0xffff, lParam >> 16)) {
				ShowError(_T("Failed to register the hotkey."));
			}
			break;
		case WM_DESTROY:
			PostQuitMessage(0);
			break;
		default:
			break;
		}
	}

	void MsgOnlyWindow::MsgLoop() {
		try {
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
		} catch (Exception ^e) {
			auto msg = "Error occurred when listening to hotkeys and clipboard updates:" + Environment::NewLine + e->Message;
			pin_ptr<const wchar_t> cMsg = PtrToStringChars(msg);
			ShowError(cMsg);
		} finally {
			_hWnd = nullptr;
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
		if (_hWnd) {
			Win32Assert(PostMessage(_hWnd, WM_CLOSE, 0, 0));
		}
		delete _signal;
	}
}
