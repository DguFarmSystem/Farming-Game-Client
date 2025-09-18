mergeInto(LibraryManager.library, {
  GetCookieJS: function (namePtr) {
    var name = UTF8ToString(namePtr);
    var value = "";
    var arr = (document.cookie || "").split("; ");
    for (var i = 0; i < arr.length; i++) {
      var kv = arr[i].split("=");
      if (kv[0] === name) {
        value = decodeURIComponent(arr[i].substring(name.length + 1));
        break;
      }
    }

    return stringToNewUTF8(value || "");
  },

  GetLocalStorageJS: function (keyPtr) {
    var key = UTF8ToString(keyPtr);
    var v = "";
    try { v = localStorage.getItem(key) || ""; } catch(e) {}
    return stringToNewUTF8(v);
  }
});