mergeInto(LibraryManager.library, {
  VeQue_RegisterPageHidden: function (callback) {
    if (window.__veQuePageHiddenBound) return;
    window.__veQuePageHiddenBound = true;

    var notify = function () {
      {{{ makeDynCall('v', 'callback') }}}();
    };
    document.addEventListener('visibilitychange', function () {
      if (document.visibilityState === 'hidden') notify();
    });
    window.addEventListener('pagehide', notify);
  },

  // Màn sao lưu bằng HTML để chép, dán bằng thao tác gốc của điện thoại và hiện đúng chữ có dấu.
  VeQue_ShowBackup: function (codePtr, importCallback) {
    var OVERLAY_ID = 'veque-backup';
    if (document.getElementById(OVERLAY_ID)) return;

    var code = UTF8ToString(codePtr);

    var overlay = document.createElement('div');
    overlay.id = OVERLAY_ID;
    overlay.style.cssText = 'position:fixed;inset:0;z-index:2147483647;display:flex;align-items:center;' +
      'justify-content:center;box-sizing:border-box;touch-action:auto;background:rgba(0,0,0,.6);' +
      'padding:max(16px,env(safe-area-inset-top)) max(16px,env(safe-area-inset-right)) ' +
      'max(16px,env(safe-area-inset-bottom)) max(16px,env(safe-area-inset-left));' +
      'font-family:system-ui,-apple-system,sans-serif;color:#3b2a1a;';

    // Không để thao tác trên màn này lọt xuống game.
    ['pointerdown', 'pointerup', 'pointermove', 'mousedown', 'mouseup', 'mousemove',
      'touchstart', 'touchend', 'touchmove', 'wheel', 'keydown', 'keyup', 'keypress'].forEach(function (name) {
      overlay.addEventListener(name, function (event) { event.stopPropagation(); });
    });

    var box = document.createElement('div');
    box.style.cssText = 'width:100%;max-width:420px;max-height:100%;overflow:auto;box-sizing:border-box;' +
      'padding:16px;border-radius:8px;background:#f4e8c8;';

    var title = document.createElement('div');
    title.textContent = 'Sao lưu tiến trình';
    title.style.cssText = 'font-size:20px;font-weight:700;margin-bottom:8px;';

    var hint = document.createElement('div');
    hint.textContent = 'Sao chép mã này và cất ở nơi an toàn. Để lấy lại tiến trình, dán mã đã cất vào ô rồi bấm Nhập mã.';
    hint.style.cssText = 'font-size:15px;line-height:1.4;margin-bottom:8px;';

    // Cỡ chữ 16px để iOS không tự phóng to trang khi chạm vào ô.
    var area = document.createElement('textarea');
    area.value = code;
    area.rows = 6;
    area.spellcheck = false;
    area.setAttribute('autocapitalize', 'off');
    area.setAttribute('autocomplete', 'off');
    area.setAttribute('autocorrect', 'off');
    area.style.cssText = 'width:100%;box-sizing:border-box;font-size:16px;font-family:monospace;' +
      'word-break:break-all;padding:8px;border:1px solid #8a6a45;border-radius:4px;';

    var status = document.createElement('div');
    status.style.cssText = 'min-height:22px;font-size:15px;margin:8px 0;';
    status.setAttribute('role', 'status');

    var row = document.createElement('div');
    row.style.cssText = 'display:flex;gap:8px;';

    var makeButton = function (label) {
      var button = document.createElement('button');
      button.type = 'button';
      button.textContent = label;
      button.style.cssText = 'flex:1;min-height:44px;font-size:15px;border:0;border-radius:6px;' +
        'background:#8a5a33;color:#fff;';
      row.appendChild(button);
      return button;
    };

    var IMPORT_LABEL = 'Nhập mã';
    var copyButton = makeButton('Sao chép');
    var importButton = makeButton(IMPORT_LABEL);
    var closeButton = makeButton('Đóng');

    var close = function () {
      overlay.remove();
    };

    var copyFallback = function () {
      area.focus();
      area.select();
      area.setSelectionRange(0, area.value.length);
      var copied = false;
      try {
        copied = document.execCommand('copy');
      } catch (error) {
        copied = false;
      }
      status.textContent = copied ? 'Đã sao chép.' : 'Không tự chép được, hãy giữ tay vào ô để chép.';
    };

    copyButton.addEventListener('click', function () {
      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(area.value).then(function () {
          status.textContent = 'Đã sao chép.';
        }, copyFallback);
      } else {
        copyFallback();
      }
    });

    // Bấm lần đầu chỉ cảnh báo, vì nhập mã sẽ ghi đè tiến trình hiện tại.
    var armed = false;
    var disarm = function () {
      armed = false;
      importButton.textContent = IMPORT_LABEL;
    };
    area.addEventListener('input', disarm);

    importButton.addEventListener('click', function () {
      var value = area.value.trim();
      if (value === code) {
        status.textContent = 'Đây là mã hiện tại. Hãy dán mã đã cất vào ô.';
        return;
      }
      if (!armed) {
        armed = true;
        importButton.textContent = 'Xác nhận';
        status.textContent = 'Tiến trình hiện tại sẽ bị thay thế. Bấm Xác nhận để tiếp tục.';
        return;
      }
      disarm();

      var size = lengthBytesUTF8(value) + 1;
      var pointer = _malloc(size);
      stringToUTF8(value, pointer, size);
      var result = {{{ makeDynCall('ii', 'importCallback') }}}(pointer);
      _free(pointer);

      if (result === 0) {
        close();
      } else if (result === 2) {
        status.textContent = 'Mã từ bản game mới hơn. Hãy tải lại trang rồi thử lại.';
      } else {
        status.textContent = 'Mã không hợp lệ. Hãy kiểm tra lại đã chép đủ mã chưa.';
      }
    });

    closeButton.addEventListener('click', close);

    box.appendChild(title);
    box.appendChild(hint);
    box.appendChild(area);
    box.appendChild(status);
    box.appendChild(row);
    overlay.appendChild(box);
    document.body.appendChild(overlay);
  }
});
